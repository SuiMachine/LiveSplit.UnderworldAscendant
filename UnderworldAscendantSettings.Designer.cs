namespace LiveSplit.UnderworldAscendant
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
            this.gbStartSplits = new System.Windows.Forms.GroupBox();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.CB_SplitOnLevelChange = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.NumUpDn_RescansLimit = new System.Windows.Forms.NumericUpDown();
            this.gbStartSplits.SuspendLayout();
            this.tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumUpDn_RescansLimit)).BeginInit();
            this.SuspendLayout();
            // 
            // gbStartSplits
            // 
            this.gbStartSplits.Controls.Add(this.NumUpDn_RescansLimit);
            this.gbStartSplits.Controls.Add(this.label1);
            this.gbStartSplits.Controls.Add(this.CB_SplitOnLevelChange);
            this.gbStartSplits.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbStartSplits.Location = new System.Drawing.Point(3, 3);
            this.gbStartSplits.Name = "gbStartSplits";
            this.gbStartSplits.Size = new System.Drawing.Size(470, 50);
            this.gbStartSplits.TabIndex = 5;
            this.gbStartSplits.TabStop = false;
            this.gbStartSplits.Text = "Options";
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.gbStartSplits, 0, 0);
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(476, 173);
            this.tlpMain.TabIndex = 0;
            // 
            // CB_SplitOnLevelChange
            // 
            this.CB_SplitOnLevelChange.AutoSize = true;
            this.CB_SplitOnLevelChange.Location = new System.Drawing.Point(6, 19);
            this.CB_SplitOnLevelChange.Name = "CB_SplitOnLevelChange";
            this.CB_SplitOnLevelChange.Size = new System.Drawing.Size(130, 17);
            this.CB_SplitOnLevelChange.TabIndex = 0;
            this.CB_SplitOnLevelChange.Text = "Split on Level Change";
            this.CB_SplitOnLevelChange.UseVisualStyleBackColor = true;
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
            // NumUpDn_RescansLimit
            // 
            this.NumUpDn_RescansLimit.Location = new System.Drawing.Point(367, 18);
            this.NumUpDn_RescansLimit.Name = "NumUpDn_RescansLimit";
            this.NumUpDn_RescansLimit.Size = new System.Drawing.Size(97, 20);
            this.NumUpDn_RescansLimit.TabIndex = 3;
            this.NumUpDn_RescansLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
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
            this.tlpMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.NumUpDn_RescansLimit)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbStartSplits;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox CB_SplitOnLevelChange;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.NumericUpDown NumUpDn_RescansLimit;
    }
}
