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
            this.MSILASMPathBox = new System.Windows.Forms.TextBox();
            this.BrowseTDLBut = new System.Windows.Forms.Button();
            this.BrowseMSILASMBut = new System.Windows.Forms.Button();
            this.SaveBut = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.MSILDASMPathBox = new System.Windows.Forms.TextBox();
            this.BrowseMSILDASMBut = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tdlPathBox
            // 
            this.tdlPathBox.Location = new System.Drawing.Point(99, 12);
            this.tdlPathBox.Name = "tdlPathBox";
            this.tdlPathBox.Size = new System.Drawing.Size(148, 20);
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
            this.MSILPath.Size = new System.Drawing.Size(35, 13);
            this.MSILPath.TabIndex = 3;
            this.MSILPath.Text = "MSIL:";
            // 
            // MSILASMPathBox
            // 
            this.MSILASMPathBox.Location = new System.Drawing.Point(99, 58);
            this.MSILASMPathBox.Name = "MSILASMPathBox";
            this.MSILASMPathBox.Size = new System.Drawing.Size(148, 20);
            this.MSILASMPathBox.TabIndex = 2;
            // 
            // BrowseTDLBut
            // 
            this.BrowseTDLBut.Location = new System.Drawing.Point(253, 10);
            this.BrowseTDLBut.Name = "BrowseTDLBut";
            this.BrowseTDLBut.Size = new System.Drawing.Size(75, 23);
            this.BrowseTDLBut.TabIndex = 4;
            this.BrowseTDLBut.Text = "Browse...";
            this.BrowseTDLBut.UseVisualStyleBackColor = true;
            this.BrowseTDLBut.Click += new System.EventHandler(this.BrowseTDLBut_Click);
            // 
            // BrowseMSILASMBut
            // 
            this.BrowseMSILASMBut.Location = new System.Drawing.Point(253, 56);
            this.BrowseMSILASMBut.Name = "BrowseMSILASMBut";
            this.BrowseMSILASMBut.Size = new System.Drawing.Size(75, 23);
            this.BrowseMSILASMBut.TabIndex = 5;
            this.BrowseMSILASMBut.Text = "Browse...";
            this.BrowseMSILASMBut.UseVisualStyleBackColor = true;
            this.BrowseMSILASMBut.Click += new System.EventHandler(this.BrowseMSILASMBut_Click);
            // 
            // SaveBut
            // 
            this.SaveBut.Location = new System.Drawing.Point(128, 110);
            this.SaveBut.Name = "SaveBut";
            this.SaveBut.Size = new System.Drawing.Size(75, 23);
            this.SaveBut.TabIndex = 6;
            this.SaveBut.Text = "Save";
            this.SaveBut.UseVisualStyleBackColor = true;
            this.SaveBut.Click += new System.EventHandler(this.SaveBut_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Assembler:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Disassembler:";
            // 
            // MSILDASMPathBox
            // 
            this.MSILDASMPathBox.Location = new System.Drawing.Point(99, 84);
            this.MSILDASMPathBox.Name = "MSILDASMPathBox";
            this.MSILDASMPathBox.Size = new System.Drawing.Size(148, 20);
            this.MSILDASMPathBox.TabIndex = 8;
            // 
            // BrowseMSILDASMBut
            // 
            this.BrowseMSILDASMBut.Location = new System.Drawing.Point(253, 82);
            this.BrowseMSILDASMBut.Name = "BrowseMSILDASMBut";
            this.BrowseMSILDASMBut.Size = new System.Drawing.Size(75, 23);
            this.BrowseMSILDASMBut.TabIndex = 10;
            this.BrowseMSILDASMBut.Text = "Browse...";
            this.BrowseMSILDASMBut.UseVisualStyleBackColor = true;
            this.BrowseMSILDASMBut.Click += new System.EventHandler(this.BrowseMSILDASMBut_Click);
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(340, 262);
            this.Controls.Add(this.BrowseMSILDASMBut);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.MSILDASMPathBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SaveBut);
            this.Controls.Add(this.BrowseMSILASMBut);
            this.Controls.Add(this.BrowseTDLBut);
            this.Controls.Add(this.MSILPath);
            this.Controls.Add(this.MSILASMPathBox);
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
        private System.Windows.Forms.TextBox MSILASMPathBox;
        private System.Windows.Forms.Button BrowseTDLBut;
        private System.Windows.Forms.Button BrowseMSILASMBut;
        private System.Windows.Forms.Button SaveBut;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox MSILDASMPathBox;
        private System.Windows.Forms.Button BrowseMSILDASMBut;
    }
}