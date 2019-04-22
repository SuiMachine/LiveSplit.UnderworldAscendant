﻿namespace LiveSplit.UnderworldAscendant
{
    partial class UnderworldAscendantSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnderworldAscendantSettings));
            this.gbStartSplits = new System.Windows.Forms.GroupBox();
            this.CB_Autostart_on_LevelLoad = new System.Windows.Forms.CheckBox();
            this.NumUpDn_RescansLimit = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.CB_SplitOnLevelChange = new System.Windows.Forms.CheckBox();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.L_InjectionStatus = new System.Windows.Forms.Label();
            this.gbStartSplits.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumUpDn_RescansLimit)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbStartSplits
            // 
            this.gbStartSplits.Controls.Add(this.CB_Autostart_on_LevelLoad);
            this.gbStartSplits.Controls.Add(this.NumUpDn_RescansLimit);
            this.gbStartSplits.Controls.Add(this.label1);
            this.gbStartSplits.Controls.Add(this.CB_SplitOnLevelChange);
            this.gbStartSplits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbStartSplits.Location = new System.Drawing.Point(3, 3);
            this.gbStartSplits.Name = "gbStartSplits";
            this.gbStartSplits.Size = new System.Drawing.Size(470, 69);
            this.gbStartSplits.TabIndex = 5;
            this.gbStartSplits.TabStop = false;
            this.gbStartSplits.Text = "Options";
            // 
            // CB_Autostart_on_LevelLoad
            // 
            this.CB_Autostart_on_LevelLoad.AutoSize = true;
            this.CB_Autostart_on_LevelLoad.Location = new System.Drawing.Point(6, 19);
            this.CB_Autostart_on_LevelLoad.Name = "CB_Autostart_on_LevelLoad";
            this.CB_Autostart_on_LevelLoad.Size = new System.Drawing.Size(131, 17);
            this.CB_Autostart_on_LevelLoad.TabIndex = 4;
            this.CB_Autostart_on_LevelLoad.Text = "Autostart on level load";
            this.CB_Autostart_on_LevelLoad.UseVisualStyleBackColor = true;
            // 
            // NumUpDn_RescansLimit
            // 
            this.NumUpDn_RescansLimit.Location = new System.Drawing.Point(367, 18);
            this.NumUpDn_RescansLimit.Name = "NumUpDn_RescansLimit";
            this.NumUpDn_RescansLimit.Size = new System.Drawing.Size(97, 20);
            this.NumUpDn_RescansLimit.TabIndex = 3;
            this.NumUpDn_RescansLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(289, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Rescans limit:";
            // 
            // CB_SplitOnLevelChange
            // 
            this.CB_SplitOnLevelChange.AutoSize = true;
            this.CB_SplitOnLevelChange.Location = new System.Drawing.Point(6, 42);
            this.CB_SplitOnLevelChange.Name = "CB_SplitOnLevelChange";
            this.CB_SplitOnLevelChange.Size = new System.Drawing.Size(130, 17);
            this.CB_SplitOnLevelChange.TabIndex = 0;
            this.CB_SplitOnLevelChange.Text = "Split on Level Change";
            this.CB_SplitOnLevelChange.UseVisualStyleBackColor = true;
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.groupBox1, 0, 1);
            this.tlpMain.Controls.Add(this.groupBox2, 0, 2);
            this.tlpMain.Controls.Add(this.gbStartSplits, 0, 0);
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tlpMain.Size = new System.Drawing.Size(476, 275);
            this.tlpMain.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.L_InjectionStatus);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 78);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(470, 82);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Injection status (for debugging):";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(3, 166);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(470, 63);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Notes / Known issues:";
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(464, 44);
            this.label3.TabIndex = 0;
            this.label3.Text = resources.GetString("label3.Text");
            // 
            // L_InjectionStatus
            // 
            this.L_InjectionStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L_InjectionStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.L_InjectionStatus.Location = new System.Drawing.Point(3, 16);
            this.L_InjectionStatus.Name = "L_InjectionStatus";
            this.L_InjectionStatus.Size = new System.Drawing.Size(464, 63);
            this.L_InjectionStatus.TabIndex = 0;
            this.L_InjectionStatus.Text = "INJ";
            this.L_InjectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UnderworldAscendantSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlpMain);
            this.Name = "UnderworldAscendantSettings";
            this.Size = new System.Drawing.Size(476, 382);
            this.gbStartSplits.ResumeLayout(false);
            this.gbStartSplits.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumUpDn_RescansLimit)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbStartSplits;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox CB_SplitOnLevelChange;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.NumericUpDown NumUpDn_RescansLimit;
        private System.Windows.Forms.CheckBox CB_Autostart_on_LevelLoad;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label L_InjectionStatus;
    }
}
