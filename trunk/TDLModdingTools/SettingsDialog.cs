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

        private void SaveBut_Click(object sender, EventArgs e)
        {
            Settings set = Settings.Singleton();

            set.setSetting("TDL_Path", tdlPathBox.Text);
            set.setSetting("IL_DSAM_Path", MSILPathBox.Text);

            MessageBox.Show("Settings Saved");
            this.Close();
        }
    }
}
