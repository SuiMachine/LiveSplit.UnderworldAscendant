using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private Task _thread;
        private CancellationTokenSource _cancelSource;
        private SynchronizationContext _uiThread;
        private List<int> _ignorePIDs;
        private UnderworldAscendantSettings _settings;

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
        IntPtr LevelSystemInstancePointer = IntPtr.Zero;

        void MemoryReadThread()
        {
            Debug.WriteLine("[NoLoads] MemoryReadThread");

            while (!_cancelSource.IsCancellationRequested)
            {
                try
                {
                    Debug.WriteLine("[NoLoads] Waiting for UA.exe...");

                    Process game;
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
                    }

                    Debug.WriteLine("[NoLoads] Got games process!");

                    uint frameCounter = 0;

                    while (!game.HasExited)
                    {
                        if (!isLevelSystemHooked)
                        {
                            #region Hooking
                            if (failedScansCount > 3)
                            {
                                _ignorePIDs.Add(game.Id);
                                MessageBox.Show("Failed to find the pattern during the 3 scan loops and the process is now going to be ignored");
                            }
                            //Hook only if the process is at least 15s old (since it takes forever with allocating stuff)
                            else if (game.UserProcessorTime >= TimeSpan.FromSeconds(15))
                            {
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
                                    "48 8B 45 F0 " +
                                    "48 8B 75 F8");

                                LevelSystemInstancePointer = game.AllocateMemory(IntPtr.Size);
                                Debug.WriteLine("[NOLOADS] injectedPtrForLevelSystemPtr allocated at: " + LevelSystemInstancePointer.ToString("X8"));
                                var injectedPtrForLevelSystemBytes = BitConverter.GetBytes(LevelSystemInstancePointer.ToInt64());

                                var functionAddress = IntPtr.Zero;
                                var contentOfAHook = new List<byte>
                                {
                                        0x48, 0x8B, 0xEC,           //mov rbp,rsp
				                        0x48, 0x83, 0xEC, 0x30,     //sub rsp,30 
				                        0x48, 0x89, 0x75, 0xF8,     //mov [rbp-08],rsi
				                        0x48, 0x8B, 0xF1            //mov rsi,rcx
		                        };
                                contentOfAHook.AddRange(new byte[] { 0x48, 0xB8 });         //mov rax,....
                                contentOfAHook.AddRange(injectedPtrForLevelSystemBytes);    //address for rax^^
                                contentOfAHook.AddRange(new byte[] { 0x48, 0x89, 0x08 });  //mov [rax], rcx
                                contentOfAHook.AddRange(new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 }); //14 nops for jmp back (actually needs I think 2 less)

                                Debug.WriteLine("[NOLOADS] Scanning for signature (LevelSystem:Update)");
                                foreach (var page in game.MemoryPages())
                                {
                                    var scanner = new SignatureScanner(game, page.BaseAddress, (int)page.RegionSize);
                                    if ((functionAddress = scanner.Scan(sigScanTarget)) != IntPtr.Zero)
                                    {
                                        break;
                                    }
                                }

                                if (functionAddress == IntPtr.Zero)
                                {
                                    failedScansCount++;
                                }
                                else
                                {
                                    Debug.WriteLine("[NOLOADS] FOUND SIGNATURE AT: 0x" + functionAddress.ToString("X8"));
                                    var allocation = game.AllocateMemory(contentOfAHook.Count);
                                    game.Suspend();

                                    try
                                    {
                                        var oInitPtr = game.WriteBytes(allocation, contentOfAHook.ToArray());
                                        var detourInstalled = game.WriteDetour(functionAddress, 14, allocation);
                                        var returnInstalled = game.WriteJumpInstruction(allocation + contentOfAHook.Count - 15, functionAddress + 14);
                                        isLevelSystemHooked = true;
                                    }
                                    catch
                                    {
                                        throw;
                                    }
                                    finally
                                    {
                                        game.Resume();
                                    }
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            currentLevelName = game.ReadString(game.ReadPointer(game.ReadPointer(LevelSystemInstancePointer) + 0x50) + 0x14, ReadStringType.UTF16, 20);
                            isLoading = !(game.ReadValue<bool>(game.ReadPointer(LevelSystemInstancePointer) + 0xBA));

                            if (isLoading != prevIsLoading || currentLevelName != prevLevelName)
                            {
                                if (isLoading || currentLevelName == "MainMenu" || currentLevelName == "Intro")
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
                                    }
                                }
                                Debug.WriteLine("IsLoading = " + isLoading + ", LevelName = " + currentLevelName);
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

            return game;
        }
    }
}
