using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TDLModdingTools
{
    public partial class SettingsDialog : Form
    {
        public SettingsDialog()
        {
            InitializeComponent();
        }

        public new DialogResult ShowDialog()
        {
            //Reload the settings into the dialogs
            Settings set = Settings.Singleton();
            
            tdlPathBox.Text = set.getSetting("TDL_Path");
            MSILASMPathBox.Text = set.getSetting("IL_ASM_Path");
            MSILDASMPathBox.Text = set.getSetting("IL_DASM_Path");

            return base.ShowDialog();
        }   

        private void SaveBut_Click(object sender, EventArgs e)
        {
            Settings set = Settings.Singleton();

            //Update all settings, don't save until the last one is set
            set.setSetting("TDL_Path", tdlPathBox.Text.Substring(0, tdlPathBox.Text.LastIndexOf('\\') + 1), false);
            set.setSetting("IL_ASM_Path", MSILASMPathBox.Text, false);
            set.setSetting("IL_DASM_Path", MSILDASMPathBox.Text, false);
            set.setSetting("IL_PEVerify_Path", tdlPathBox.Text.Substring(0, tdlPathBox.Text.LastIndexOf('\\') + 1) + "PEVerify.exe", true);

            MessageBox.Show("Settings Saved");
            this.Close();
        }

        //Actually browsing for the file
        private string ShowOpenDialog(TextBox dirBox, string file)
        {
            string dir;
            try
            {
                dir = dirBox.Text.Substring(0, dirBox.Text.LastIndexOf('\\'));
            }
            catch
            {
                dir = "";
            }

            OpenFileDialog dia = new OpenFileDialog();

            dia.RestoreDirectory = false;
            dia.InitialDirectory = dir;
            dia.Filter = file + "|" + file;

            if (dia.ShowDialog() == DialogResult.OK)
                return dia.FileName;
            else
                return null;
        }

        //Browse button handlers
        private void BrowseTDLBut_Click(object sender, EventArgs e)
        {
            string retValue = ShowOpenDialog(tdlPathBox, "TDL.exe");
            if(retValue != null)
                tdlPathBox.Text = retValue;
        }

        private void BrowseMSILASMBut_Click(object sender, EventArgs e)
        {
            string retValue = ShowOpenDialog(MSILASMPathBox, "ilasm.exe");
            if (retValue != null)
                MSILASMPathBox.Text = retValue;
        }

        private void BrowseMSILDASMBut_Click(object sender, EventArgs e)
        {
            string retValue = ShowOpenDialog(MSILDASMPathBox, "ildasm.exe");
            if (retValue != null)
                MSILDASMPathBox.Text = retValue;
        }
    }
}
