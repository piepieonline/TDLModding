namespace TDLModdingTools
{
    partial class SettingsDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tdlPathBox = new System.Windows.Forms.TextBox();
            this.lblTDLPath = new System.Windows.Forms.Label();
            this.MSILPath = new System.Windows.Forms.Label();
            this.MSILPathBox = new System.Windows.Forms.TextBox();
            this.BrowseTDLBut = new System.Windows.Forms.Button();
            this.BrowseMSILBut = new System.Windows.Forms.Button();
            this.SaveBut = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tdlPathBox
            // 
            this.tdlPathBox.Location = new System.Drawing.Point(87, 12);
            this.tdlPathBox.Name = "tdlPathBox";
            this.tdlPathBox.Size = new System.Drawing.Size(160, 20);
            this.tdlPathBox.TabIndex = 0;
            // 
            // lblTDLPath
            // 
            this.lblTDLPath.AutoSize = true;
            this.lblTDLPath.Location = new System.Drawing.Point(12, 15);
            this.lblTDLPath.Name = "lblTDLPath";
            this.lblTDLPath.Size = new System.Drawing.Size(56, 13);
            this.lblTDLPath.TabIndex = 1;
            this.lblTDLPath.Text = "TDL Path:";
            // 
            // MSILPath
            // 
            this.MSILPath.AutoSize = true;
            this.MSILPath.Location = new System.Drawing.Point(12, 41);
            this.MSILPath.Name = "MSILPath";
            this.MSILPath.Size = new System.Drawing.Size(60, 13);
            this.MSILPath.TabIndex = 3;
            this.MSILPath.Text = "MSIL Path:";
            // 
            // MSILPathBox
            // 
            this.MSILPathBox.Location = new System.Drawing.Point(87, 38);
            this.MSILPathBox.Name = "MSILPathBox";
            this.MSILPathBox.Size = new System.Drawing.Size(160, 20);
            this.MSILPathBox.TabIndex = 2;
            // 
            // BrowseTDLBut
            // 
            this.BrowseTDLBut.Location = new System.Drawing.Point(253, 10);
            this.BrowseTDLBut.Name = "BrowseTDLBut";
            this.BrowseTDLBut.Size = new System.Drawing.Size(75, 23);
            this.BrowseTDLBut.TabIndex = 4;
            this.BrowseTDLBut.Text = "Browse...";
            this.BrowseTDLBut.UseVisualStyleBackColor = true;
            // 
            // BrowseMSILBut
            // 
            this.BrowseMSILBut.Location = new System.Drawing.Point(253, 36);
            this.BrowseMSILBut.Name = "BrowseMSILBut";
            this.BrowseMSILBut.Size = new System.Drawing.Size(75, 23);
            this.BrowseMSILBut.TabIndex = 5;
            this.BrowseMSILBut.Text = "Browse...";
            this.BrowseMSILBut.UseVisualStyleBackColor = true;
            // 
            // SaveBut
            // 
            this.SaveBut.Location = new System.Drawing.Point(132, 101);
            this.SaveBut.Name = "SaveBut";
            this.SaveBut.Size = new System.Drawing.Size(75, 23);
            this.SaveBut.TabIndex = 6;
            this.SaveBut.Text = "Save";
            this.SaveBut.UseVisualStyleBackColor = true;
            this.SaveBut.Click += new System.EventHandler(this.SaveBut_Click);
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(340, 262);
            this.Controls.Add(this.SaveBut);
            this.Controls.Add(this.BrowseMSILBut);
            this.Controls.Add(this.BrowseTDLBut);
            this.Controls.Add(this.MSILPath);
            this.Controls.Add(this.MSILPathBox);
            this.Controls.Add(this.lblTDLPath);
            this.Controls.Add(this.tdlPathBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsDialog";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "Settings";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tdlPathBox;
        private System.Windows.Forms.Label lblTDLPath;
        private System.Windows.Forms.Label MSILPath;
        private System.Windows.Forms.TextBox MSILPathBox;
        private System.Windows.Forms.Button BrowseTDLBut;
        private System.Windows.Forms.Button BrowseMSILBut;
        private System.Windows.Forms.Button SaveBut;
    }
}