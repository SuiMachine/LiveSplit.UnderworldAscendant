using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.UnderworldAscendant
{
    class GameMemory
    {

        public event EventHandler OnLoadStarted;
        public event EventHandler OnLoadFinished;
        public event EventHandler OnLevelChanged;
        public event EventHandler OnFirstLevelLoad;

        private Task _thread;
        private CancellationTokenSource _cancelSource;
        private SynchronizationContext _uiThread;
        private List<int> _ignorePIDs;
        private UnderworldAscendantSettings _settings;

        IntPtr LevelSystemInstancePointer = IntPtr.Zero;
        IntPtr originalFunctionAddress = IntPtr.Zero;
        IntPtr codeDetour = IntPtr.Zero;

        private byte[] OriginalInstructionBytesBefore14 = new byte[] {
            0x48, 0x8B, 0xEC,           //mov rbp,rsp
            0x48, 0x83, 0xEC, 0x30,     //sub rsp,30 
            0x48, 0x89, 0x75, 0xF8,     //mov [rbp-08],rsi
            0x48, 0x8B, 0xF1            //mov rsi,rcx
        };

        private byte[] OriginalInstructionBytesV14 = new byte[] {
            0x48, 0x8B, 0x89, 0xB8, 0x00, 0x00, 0x00,   //mov rcx,[rcx+000000B8]
            0x33, 0xD2,                                 //xor edx,edx
            0x89, 0x31,                                 //mov [rcx],esi
            0x8D, 0x4A, 0x05                            //lea ecx,[rdx+05]
        };

        private string[] LevelsExcludedFromAutosplitting = new string[]
        {
            "MainMenu",
            "IntroSequence",
            "Credits",
            ""
        };

        public GameMemory(UnderworldAscendantSettings componentSettings)
        {
            _settings = componentSettings;
            _ignorePIDs = new List<int>();
        }

        public void StartMonitoring()
        {
            if (_thread != null && _thread.Status == TaskStatus.Running)
            {
                throw new InvalidOperationException();
            }
            if (!(SynchronizationContext.Current is WindowsFormsSynchronizationContext))
            {
                throw new InvalidOperationException("SynchronizationContext.Current is not a UI thread.");
            }

            _uiThread = SynchronizationContext.Current;
            _cancelSource = new CancellationTokenSource();
            _thread = Task.Factory.StartNew(MemoryReadThread);
        }

        public void Stop()
        {
            if (_cancelSource == null || _thread == null || _thread.Status != TaskStatus.Running)
            {
                return;
            }

            if(originalFunctionAddress != IntPtr.Zero && game != null && !game.HasExited && isLevelSystemHooked)
            {
                Debug.WriteLine("[NOLOADS] Restoring original function.");
                game.Suspend();
                game.WriteBytes(originalFunctionAddress, OriginalInstructionBytesBefore14);
                game.FreeMemory(codeDetour);
                game.FreeMemory(LevelSystemInstancePointer);
                game.Resume();

            }

            _cancelSource.Cancel();
            _thread.Wait();


        }

        int failedScansCount = 0;
        bool isLevelSystemHooked = false;
        bool isLoading = false;
        bool prevIsLoading = false;
        string currentLevelName = "";
        string prevLevelName = "";
        bool loadingStarted = false;

        //Used in displaying status in Settings
        enum InjectionStatus
        {
            NoProcess,
            FoundProcessWaiting,
            Scanning,
            FailedScanning,
            FailedToInject,
            UnknownLibrarySize,
            Injected
        }

        InjectionStatus lastInjectionStatus = InjectionStatus.NoProcess;


        //This is a size of Assembly-CSharp.dll, not main module!
        enum CsharpAssemblySizes
        {
            v1_00 = 5490688,
            v1_02 = 5471232,
            v1_1 = 5386752,
            Newest
        }

        enum GameAssemblySizes
        {
            v1_4 = 43638784
        }

        enum GameVersions
        {
            v1_00,
            v1_02,
            v1_1,
            v1_3,
            v1_4,
            Unsupported
        }

        GameVersions gameVersion = GameVersions.Unsupported;

        Process game;

        void MemoryReadThread()
        {
            Debug.WriteLine("[NoLoads] MemoryReadThread");

            while (!_cancelSource.IsCancellationRequested)
            {
                try
                {
                    Debug.WriteLine("[NoLoads] Waiting for UA.exe...");

                    while ((game = GetGameProcess()) == null)
                    {
                        Thread.Sleep(250);
                        if (_cancelSource.IsCancellationRequested)
                        {
                            return;
                        }

                        isLoading = true;

                        if (isLoading != prevIsLoading)
                        {
                            loadingStarted = true;

                            // pause game timer
                            _uiThread.Post(d =>
                            {
                                if (OnLoadStarted != null)
                                {
                                    OnLoadStarted(this, EventArgs.Empty);
                                }
                            }, null);
                        }

                        prevIsLoading = true;

                        SetInjectionLabelInSettings(InjectionStatus.NoProcess, IntPtr.Zero);
                    }

                    Debug.WriteLine("[NoLoads] Got games process!");

                    uint frameCounter = 0;

                    while (!game.HasExited)
                    {
                        if (!isLevelSystemHooked)
                        {
                            #region Hooking
                            #region BeforeIntroductionOfILCPP-internals
                            if (gameVersion < GameVersions.v1_4)
                            {
                                if (_settings.RescansLimit != 0 && failedScansCount >= _settings.RescansLimit)
                                {
                                    var result = MessageBox.Show("Failed to find the pattern during the 3 scan loops. Want to retry scans?", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
                                    if (result == DialogResult.Cancel)
                                    {
                                        _ignorePIDs.Add(game.Id);
                                    }
                                    else
                                        failedScansCount = 0;
                                    //Should refresh game pages... hopefully. The memory pages extansion is really poop.
                                    game = null;

                                    SetInjectionLabelInSettings(InjectionStatus.FailedScanning, IntPtr.Zero);
                                }
                                //Hook only if the process is at least 15s old (since it takes forever with allocating stuff)
                                else if (game.UserProcessorTime >= TimeSpan.FromSeconds(15))
                                {
                                    SetInjectionLabelInSettings(InjectionStatus.Scanning, IntPtr.Zero);
                                    var sigScanTarget = new SigScanTarget(
                                        "48 8B EC " +
                                        "48 83 EC 30 " +
                                        "48 89 75 F8 " +
                                        "48 8B F1 " +
                                        "48 8B 46 10 " +
                                        "48 85 C0 " +
                                        "?? ?? " +
                                        "48 8B 46 10 " +
                                        "48 8B C8 " +
                                        "48 89 45 F0 " +
                                        "FF 50 18 " +
                                        "48 8B 45 F0 ");


                                    LevelSystemInstancePointer = game.AllocateMemory(IntPtr.Size);
                                    Debug.WriteLine("[NOLOADS] injectedPtrForLevelSystemPtr allocated at: " + LevelSystemInstancePointer.ToString("X8"));
                                    var injectedPtrForLevelSystemBytes = BitConverter.GetBytes(LevelSystemInstancePointer.ToInt64());

                                    originalFunctionAddress = IntPtr.Zero;
                                    var contentOfAHook = new List<byte>();
                                    contentOfAHook.AddRange(OriginalInstructionBytesBefore14);
                                    contentOfAHook.AddRange(new byte[] { 0x48, 0xB8 });         //mov rax,....
                                    contentOfAHook.AddRange(injectedPtrForLevelSystemBytes);    //address for rax^^
                                    contentOfAHook.AddRange(new byte[] { 0x48, 0x89, 0x08 });  //mov [rax], rcx
                                    contentOfAHook.AddRange(new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 }); //14 nops for jmp back (actually needs I think 2 less)

                                    Debug.WriteLine("[NOLOADS] Scanning for signature (LevelSystem:Update)");
                                    foreach (var page in game.MemoryPages())
                                    {
                                        var scanner = new SignatureScanner(game, page.BaseAddress, (int)page.RegionSize);
                                        if ((originalFunctionAddress = scanner.Scan(sigScanTarget)) != IntPtr.Zero)
                                        {
                                            break;
                                        }
                                    }

                                    if (originalFunctionAddress == IntPtr.Zero)
                                    {
                                        failedScansCount++;
                                        Debug.WriteLine("[NOLOADS] Failed scans: " + failedScansCount);
                                        game.FreeMemory(LevelSystemInstancePointer);
                                    }
                                    else
                                    {
                                        Debug.WriteLine("[NOLOADS] FOUND SIGNATURE AT: 0x" + originalFunctionAddress.ToString("X8"));
                                        codeDetour = game.AllocateMemory(contentOfAHook.Count);
                                        game.Suspend();

                                        try
                                        {
                                            var oInitPtr = game.WriteBytes(codeDetour, contentOfAHook.ToArray());
                                            var detourInstalled = game.WriteDetour(originalFunctionAddress, 14, codeDetour);
                                            var returnInstalled = game.WriteJumpInstruction(codeDetour + contentOfAHook.Count - 15, originalFunctionAddress + 14);
                                            isLevelSystemHooked = true;
                                            SetInjectionLabelInSettings(InjectionStatus.Injected, LevelSystemInstancePointer);
                                        }
                                        catch
                                        {
                                            SetInjectionLabelInSettings(InjectionStatus.FailedToInject, IntPtr.Zero);
                                            throw;
                                        }
                                        finally
                                        {
                                            game.Resume();
                                        }
                                    }
                                }
                                else
                                    SetInjectionLabelInSettings(InjectionStatus.FoundProcessWaiting, IntPtr.Zero);
                            }
                            #endregion
                            #region ILCPPInternals
                            else
                            {
                                LevelSystemInstancePointer = game.AllocateMemory(IntPtr.Size);
                                Debug.WriteLine("[NOLOADS] injectedPtrForLevelSystemPtr allocated at: " + LevelSystemInstancePointer.ToString("X8"));
                                var injectedPtrForLevelSystemBytes = BitConverter.GetBytes(LevelSystemInstancePointer.ToInt64());

                                originalFunctionAddress = game.ModulesWow64Safe().First(x => x.ModuleName.ToLower() == "gameassembly.dll").BaseAddress  + 0x120D382;
                                var contentOfAHook = new List<byte>();
                                contentOfAHook.AddRange(OriginalInstructionBytesV14);
                                contentOfAHook.AddRange(new byte[] { 0x48, 0xB8 });         //mov rax,....
                                contentOfAHook.AddRange(injectedPtrForLevelSystemBytes);    //address for rax^^
                                contentOfAHook.AddRange(new byte[] { 0x48, 0x89, 0x38 });  //mov [rax], rdi (rdi is base of an object)
                                contentOfAHook.AddRange(new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 }); //14 nops for jmp back (actually needs I think 2 less)

                                //Thankfully no longer need to do sig scans... fuck sigscans
                                Debug.WriteLine("[NOLOADS] INJECTING AT: 0x" + originalFunctionAddress.ToString("X8"));
                                codeDetour = game.AllocateMemory(contentOfAHook.Count);
                                game.Suspend();

                                try
                                {
                                    var oInitPtr = game.WriteBytes(codeDetour, contentOfAHook.ToArray());
                                    var detourInstalled = game.WriteDetour(originalFunctionAddress, OriginalInstructionBytesV14.Length, codeDetour);
                                    var returnInstalled = game.WriteJumpInstruction(codeDetour + contentOfAHook.Count - 15, originalFunctionAddress + OriginalInstructionBytesV14.Length);
                                    isLevelSystemHooked = true;
                                    SetInjectionLabelInSettings(InjectionStatus.Injected, LevelSystemInstancePointer);
                                }
                                catch
                                {
                                    SetInjectionLabelInSettings(InjectionStatus.FailedToInject, IntPtr.Zero);
                                    throw;
                                }
                                finally
                                {
                                    game.Resume();
                                }
                            }
                            #endregion

                            #endregion
                        }
                        else
                        {
                            switch(gameVersion)
                            {
                                case GameVersions.v1_00:
                                    currentLevelName = game.ReadString(game.ReadPointer(game.ReadPointer(LevelSystemInstancePointer) + 0x50) + 0x14, ReadStringType.UTF16, 30);
                                    isLoading = !(game.ReadValue<bool>(game.ReadPointer(LevelSystemInstancePointer) + 0xB2));
                                    break;
                                case GameVersions.v1_02:
                                    currentLevelName = game.ReadString(game.ReadPointer(game.ReadPointer(LevelSystemInstancePointer) + 0x50) + 0x14, ReadStringType.UTF16, 30);
                                    isLoading = !(game.ReadValue<bool>(game.ReadPointer(LevelSystemInstancePointer) + 0xB2));
                                    break;
                                case GameVersions.v1_1:
                                    currentLevelName = game.ReadString(game.ReadPointer(game.ReadPointer(LevelSystemInstancePointer) + 0x50) + 0x14, ReadStringType.UTF16, 30);
                                    isLoading = !(game.ReadValue<bool>(game.ReadPointer(LevelSystemInstancePointer) + 0xB2));
                                    break;
                                case GameVersions.v1_3:
                                    currentLevelName = game.ReadString(game.ReadPointer(game.ReadPointer(LevelSystemInstancePointer) + 0x50) + 0x14, ReadStringType.UTF16, 30);
                                    isLoading = !(game.ReadValue<bool>(game.ReadPointer(LevelSystemInstancePointer) + 0xBA));
                                    break;
                                default:
                                    currentLevelName = game.ReadString(game.ReadPointer(game.ReadPointer(LevelSystemInstancePointer) + 0x50) + 0x14, ReadStringType.UTF16, 30);
                                    isLoading = !(game.ReadValue<bool>(game.ReadPointer(LevelSystemInstancePointer) + 0x62));
                                    break;
                            }

                            if (isLoading != prevIsLoading || currentLevelName != prevLevelName)
                            {
#if DEBUG
                                if (currentLevelName != prevLevelName)
                                    Debug.WriteLine("Level changed from " + prevLevelName + " -> " + currentLevelName);
#endif

                                if (isLoading || (currentLevelName != null && LevelsExcludedFromAutosplitting.Contains(currentLevelName)))
                                {
                                    Debug.WriteLine(String.Format("[NoLoads] Load Start - {0}", frameCounter));

                                    loadingStarted = true;

                                    // pause game timer
                                    _uiThread.Post(d =>
                                    {
                                        if (OnLoadStarted != null)
                                        {
                                            OnLoadStarted(this, EventArgs.Empty);
                                        }
                                    }, null);
                                }
                                else
                                {


                                    Debug.WriteLine(String.Format("[NoLoads] Load End - {0}", frameCounter));

                                    if (loadingStarted)
                                    {
                                        loadingStarted = false;

                                        // unpause game timer
                                        _uiThread.Post(d =>
                                        {
                                            if (OnLoadFinished != null)
                                            {
                                                OnLoadFinished(this, EventArgs.Empty);
                                            }
                                        }, null);

                                        _uiThread.Post(d =>
                                        {
                                            if(OnFirstLevelLoad != null)
                                            {
                                                OnFirstLevelLoad(this, EventArgs.Empty);
                                            }
                                        }, null);
                                    }
                                }

                                if (currentLevelName != prevLevelName && prevLevelName != null && currentLevelName != null && !LevelsExcludedFromAutosplitting.Contains(currentLevelName) && !LevelsExcludedFromAutosplitting.Contains(prevLevelName))
                                {
                                    _uiThread.Post(d =>
                                    {
                                        if (OnLevelChanged != null)
                                        {
                                            OnLevelChanged(this, EventArgs.Empty);
                                        }
                                    }, null);
                                }
                            }

                            prevIsLoading = isLoading;
                            prevLevelName = currentLevelName;
                            frameCounter++;

                            Thread.Sleep(15);

                            if (_cancelSource.IsCancellationRequested)
                            {
                                return;
                            }
                        }

                    }

                    // pause game timer on exit or crash
                    _uiThread.Post(d =>
                    {
                        if (OnLoadStarted != null)
                        {
                            OnLoadStarted(this, EventArgs.Empty);
                        }
                    }, null);
                    isLoading = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    Thread.Sleep(1000);
                }
            }
        }


        Process GetGameProcess()
        {
            Process game = Process.GetProcesses().FirstOrDefault(p => p.ProcessName.ToLower() == "ua" && !p.HasExited && !_ignorePIDs.Contains(p.Id));

            if (game == null)
            {
                LevelSystemInstancePointer = IntPtr.Zero;
                failedScansCount = 0;
                isLevelSystemHooked = false;
                return null;
            }

            if (game.MainWindowTitle == null || game.MainWindowTitle == "")
            {
                return null;
            }

            if (game.MainWindowTitle != "Underworld Ascendant")
            {
                _ignorePIDs.Add(game.Id);
                return null;
            }

            var assemblyCsharpPath = Path.Combine(Path.GetDirectoryName(game.MainModule.FileName), "UA_Data", "Managed", "Assembly-CSharp.dll");
            if(File.Exists(assemblyCsharpPath))
            {
                FileInfo info = new FileInfo(assemblyCsharpPath);
                switch (info.Length)
                {
                    case ((long)CsharpAssemblySizes.v1_00):
                        gameVersion = GameVersions.v1_00;
                        break;
                    case ((long)CsharpAssemblySizes.v1_02):
                        gameVersion = GameVersions.v1_02;
                        break;
                    case ((long)CsharpAssemblySizes.v1_1):
                        gameVersion = GameVersions.v1_1;
                        break;
                    default:
                        gameVersion = GameVersions.v1_3;
                        break;

                }
            }
            else if (FindModuleByName(game, "gameassembly.dll", out ProcessModule gameAssemblyModule))
            {
                switch(gameAssemblyModule.ModuleMemorySize)
                {
                    case (int)GameAssemblySizes.v1_4:
                        gameVersion = GameVersions.v1_4;
                        break;
                    default:
                        SetInjectionLabelInSettings(InjectionStatus.UnknownLibrarySize, IntPtr.Zero);
                        gameVersion = GameVersions.Unsupported;
                        _ignorePIDs.Add(game.Id);
                        return null;
                }
            }
            else
            {
                _ignorePIDs.Add(game.Id);
                return null;
            }


            return game;
        }

        private void SetInjectionLabelInSettings(InjectionStatus currentInjectionStatus, IntPtr injectedPointer)
        {
            if(lastInjectionStatus != currentInjectionStatus || currentInjectionStatus == InjectionStatus.Scanning)
            {
                //UI is on different thread, invoke is required
                if (_settings.L_InjectionStatus.InvokeRequired)
                    _settings.L_InjectionStatus.Invoke(new Action(() => SetInjectionLabelInSettings(currentInjectionStatus, injectedPointer)));
                else
                {
                    switch (currentInjectionStatus)
                    {
                        case (InjectionStatus.NoProcess):
                            _settings.L_InjectionStatus.ForeColor = System.Drawing.Color.Black;
                            _settings.L_InjectionStatus.Text = "No process found";
                            break;
                        case (InjectionStatus.FoundProcessWaiting):
                            _settings.L_InjectionStatus.ForeColor = System.Drawing.Color.DarkBlue;
                            _settings.L_InjectionStatus.Text = "Found process! Waiting for it to mature (15 seconds).";
                            break;
                        case (InjectionStatus.Scanning):
                            _settings.L_InjectionStatus.ForeColor = System.Drawing.Color.Blue;
                            _settings.L_InjectionStatus.Text = string.Format("Scanning... ({0} failed scans).", failedScansCount);
                            break;
                        case (InjectionStatus.FailedScanning):
                            _settings.L_InjectionStatus.ForeColor = System.Drawing.Color.Red;
                            _settings.L_InjectionStatus.Text = "Scan failed!";
                            break;
                        case (InjectionStatus.FailedToInject):
                            _settings.L_InjectionStatus.ForeColor = System.Drawing.Color.Red;
                            _settings.L_InjectionStatus.Text = "Failed to inject code!";
                            break;
                        case (InjectionStatus.UnknownLibrarySize):
                            _settings.L_InjectionStatus.ForeColor = System.Drawing.Color.Red;
                            _settings.L_InjectionStatus.Text = "Unknown module size (likely the game has been patched). Tell Sui!";
                            break;
                        case (InjectionStatus.Injected):
                            _settings.L_InjectionStatus.ForeColor = System.Drawing.Color.Green;
                            _settings.L_InjectionStatus.Text = "Successfully injected code. Ptr copy at: 0x" + injectedPointer.ToString("X8");
                            break;
                    }

                    lastInjectionStatus = currentInjectionStatus;
                }
            }
        }

        private bool FindModuleByName(Process proc, string moduleName, out ProcessModule module)
        {
            var modules = proc.Modules;
            for(int i=0; i<modules.Count; i++)
            {
                if(modules[i].ModuleName.ToLower() == moduleName)
                {
                    module = modules[i];
                    return true;
                }
            }
            module = null;
            return false;
        }
    }
}
