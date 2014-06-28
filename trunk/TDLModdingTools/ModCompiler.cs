using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

//Very simple form, just so modders can easily create mod scripts
namespace TDLModdingTools
{
    public partial class ModCompiler : Form
    {
        private const string UPDATED_STRING = "UPDATED: ";

        public ModCompiler()
        {
            InitializeComponent();
        }

        private void mSILEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new MSILEditor().Show();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void optionsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SettingsDialog dia = new SettingsDialog();
            dia.ShowDialog();
        }

        private void ModCompiler_Load(object sender, EventArgs e)
        {
            string modsPath = Settings.Singleton().getSetting("TDL_Path") + "TDL_Data\\Mods\\";
            foreach(string modDir in Directory.EnumerateDirectories(modsPath))
            {
                string modName = modDir.Substring(modDir.LastIndexOf('\\') + 1);
                string modCsFileName = modDir + "\\" + modName + ".cs";
                string modDllFileName = modDir + "\\" + modName + ".dll";

                if(modName == "Default")
                    continue;

                //If the mod doesn't have a code file, ignore it
                if (!File.Exists(modCsFileName))
                    continue;

                //If the mod doesn't have a compiled version, or has been updated, mark it
                if (!File.Exists(modDllFileName) || File.GetLastWriteTime(modCsFileName).CompareTo(File.GetLastWriteTime(modDllFileName)) > 0)
                    modName = UPDATED_STRING + modName;

                modListBox.Items.Add(modName);
            }
        }

        private void modListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(modListBox.SelectedIndex != -1)
                modListBox.SetItemChecked(modListBox.SelectedIndex, !modListBox.GetItemChecked(modListBox.SelectedIndex));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string assemblies = "";
            //Load of the assemblies included with TDL, except mscorlib
            foreach(string assembly in Directory.EnumerateFiles(Settings.Singleton().getSetting("TDL_Path") + "TDL_Data\\Managed\\", "*.dll"))
            {
                if (assembly.Contains("mscorlib.dll"))
                    continue;
                //Should be safe spliting with |, not used by windows
                assemblies += assembly + "|";
            }


            string compileResponse = "";
            for (int i = 0; i < modListBox.CheckedItems.Count; i++ )
            {
                //Remove UPDATED markers
                string mod = (string)modListBox.CheckedItems[i];
                if (mod.Contains(UPDATED_STRING))
                    mod = mod.Substring(UPDATED_STRING.Length);

                //Actually compile it
                string modPath = Settings.Singleton().getSetting("TDL_Path") + "TDL_Data\\Mods\\" + mod + "\\";
                CSharpCodeProvider csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v2.0" } });
                CompilerParameters parameters = new CompilerParameters(assemblies.Split('|'), mod + ".dll", false);
                parameters.OutputAssembly = modPath + mod + ".dll";
                CompilerResults results = csc.CompileAssemblyFromSource(parameters, File.ReadAllText(modPath + mod + ".cs"));
                List<CompilerError> errors =  results.Errors.Cast<CompilerError>().ToList();
                
                //If we compiled it successfully, remove the updated marker
                if(errors.Count == 0)
                {
                    //Should really deselect those that compile, however, for now, we can't
                    //Otherwise the loop to compile them all will break
                    string currItem = (string)modListBox.CheckedItems[i];
                    int realI = modListBox.CheckedIndices[i];
                    if (currItem.Contains(UPDATED_STRING))
                    {
                        modListBox.Items.RemoveAt(realI);
                        modListBox.Items.Insert(realI, currItem.Substring(UPDATED_STRING.Length));
                        modListBox.SetItemChecked(realI, true);
                    }
                    else
                    {
                        //modListBox.SetItemChecked(realI, false);
                    }
                    compileResponse += mod + ".cs compiled successfully.\n";
                }
                else
                {
                    //Otherwise, list the errors
                    compileResponse += mod + ":\n";
                    errors.ForEach(error => compileResponse += "\t" + error.ErrorText + "\n");
                }
            }

            //...A better option could be made here
            MessageBox.Show(compileResponse);
        }
    }
}
