using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TDLModdingTools
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            new Settings(Application.StartupPath);

            if (!Settings.Singleton().isSetup())
            {
                SettingsDialog dia = new SettingsDialog();
                if(dia.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }
            switch (Settings.Singleton().getSetting("DefaultStart"))
            {
                case "MSIL Editor":
                    Application.Run(new MSILEditor());
                    break;
                case "Mod Compiler":
                default:
                    Application.Run(new ModCompiler());
                    break;
            }
        }
    }
}
