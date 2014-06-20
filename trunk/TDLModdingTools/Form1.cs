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
using System.Xml.Serialization;

using System.Diagnostics;

using System.Text.RegularExpressions;

namespace TDLModdingTools
{
    public partial class Form1 : Form
    {
        private string openedFileName;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            new Settings(Application.StartupPath);

            if (Settings.Singleton().getSetting("IL_DASM_Path") == "")
            {
                SettingsDialog dia = new SettingsDialog();
                dia.ShowDialog();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDia = new OpenFileDialog();
            openDia.RestoreDirectory = false;
            openDia.Filter = "Executables/Libraries (*.exe; *.dll)|*.exe;*.dll|All Files (*.*)|*.*";
            openDia.InitialDirectory = Settings.Singleton().getSetting("TDL_Path") + @"TDL_Data\Managed\";

            if (openDia.ShowDialog() != DialogResult.OK)
                return;

            String fileToDasm = openDia.FileName;
            openedFileName = openDia.SafeFileName.Substring(0, openDia.SafeFileName.LastIndexOf('.'));

            ilCodeViewBox.Text = runExternalTool(Settings.Singleton().getSetting("IL_DASM_Path"), " \"" + fileToDasm + "\"" + @" /TEXT");
            codeToTreeView();
        }

        private void codeToTreeView()
        {
            treeReady = false;
            treeView1.Nodes.Clear();
            string[] lines = ilCodeViewBox.Lines;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimStart(new char[] { '\t', ' ' });

                //if (i == 200379)
                //    Debug.Print(line);

                //if (line.StartsWith("IL_") && line.Substring(7, 1) == ":")
                //    ilCodeViewBox.Lines[i] = line.Substring(line.IndexOf(":") + 1);

                if (line.StartsWith(".class"))
                {
                    string nodeText = "";

                    if (line.Contains("`"))
                        nodeText = line.Substring(line.LastIndexOf(" ", line.LastIndexOf('`')));
                    else
                        nodeText = line.Substring(line.LastIndexOf(" "));

                    TreeNode currentClassNode = new LineRefTreeNode(nodeText, i);
                    treeView1.Nodes.Add(currentClassNode);
                    treeView1.SelectedNode = currentClassNode;
                    continue;
                }
                else if (line.StartsWith(".method"))
                {
                    string methodName = "";

                    for (int lineAddIndex = i; ; lineAddIndex++)
                    {
                        methodName += lines[lineAddIndex];
                        if (lines[lineAddIndex].Contains(")"))
                            break;
                    }
                    int i1 = methodName.IndexOf("(");
                    int i2 = (methodName.LastIndexOf(' ', i1) + 1);
                    methodName = methodName.Substring(i2, i1 - i2).Trim();

                    TreeNode newChildNode = new LineRefTreeNode(methodName, i);

                    treeView1.SelectedNode.Nodes.Add(newChildNode);
                }
            }
            treeView1.Sort();
            treeReady = true;

            //TEMP: Find EntityTable::LoadTable
            ilCodeViewBox.Focus();
            ilCodeViewBox.SelectionStart = ilCodeViewBox.Text.IndexOf("// end of method EntityTable::loadTable");
            ilCodeViewBox.ScrollToCaret();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText(Application.StartupPath + "//" + openedFileName + ".il", ilCodeViewBox.Text);

            csCodeViewBox.Text = runExternalTool(Settings.Singleton().getSetting("IL_ASM_Path"), "/DLL \"" + Application.StartupPath + "//" + openedFileName + ".il" + "\"");
            File.Delete(Application.StartupPath + "//" + openedFileName + ".il");
            MessageBox.Show("File saved to " + Application.StartupPath + "\\" + openedFileName + ".dll" );
        }

        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            //Ctrl-F
            //Find
            //...Should probably be a setting
            if (e.Control && e.KeyCode == Keys.F)
            {
                FindReplace find = new FindReplace();
                find.ShowDialog();

                if (ilCodeViewBox.Text.Contains(find.findString))
                {
                    ilCodeViewBox.Select(ilCodeViewBox.Text.IndexOf(find.findString), find.findString.Length);
                }
            }
        }

        private bool treeReady = false;
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //If the tree is loaded and ready, then we can go searching for the code selected
            if (treeReady)
            {
                //No scrolling if we don't have focus
                ilCodeViewBox.Focus();
                //Select the line
                ilCodeViewBox.Select(ilCodeViewBox.GetFirstCharIndexFromLine(((LineRefTreeNode)treeView1.SelectedNode).refLineNumber), 0);
                //Now scroll
                ilCodeViewBox.ScrollToCaret();
            }
        }

        //Sort based on key entry
        private void treeView1_KeyUp(object sender, KeyEventArgs e)
        {
            int keyValue = e.KeyValue;
            if ((keyValue >= 0x30 && keyValue <= 0x39) // numbers
             || (keyValue >= 0x41 && keyValue <= 0x5A) // letters
             || (keyValue >= 0x60 && keyValue <= 0x69)) // numpad
            {
                //treeView1.Nodes
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsDialog dia = new SettingsDialog();
            dia.ShowDialog();
        }

        private void insertHookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string hookClassText = File.ReadAllText(Application.StartupPath + "//TDLHookCode//class.txt");
            string hookMethodText = File.ReadAllText(Application.StartupPath + "//TDLHookCode//examplemethod.txt");

            string classMarker = "// =============================================================";

            //int methodTopIndex = ilCodeViewBox.GetFirstCharIndexFromLine(ilCodeViewBox.GetLineFromCharIndex(ilCodeViewBox.Text.IndexOf(@"loadTable() cil managed")) - 1);
            //int methodBottomIndex = ilCodeViewBox.Text.IndexOf(@"// end of method EntityTable::loadTable", methodTopIndex);

            ilCodeViewBox.Text = ilCodeViewBox.Text.Insert(ilCodeViewBox.Text.LastIndexOf(classMarker) - 1, hookClassText);

            //ilCodeViewBox.Text = ilCodeViewBox.Text.Remove(methodTopIndex, methodBottomIndex - methodTopIndex);
            //ilCodeViewBox.Text = ilCodeViewBox.Text.Insert(methodTopIndex, hookMethodText);
        }

        private void verifyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            csCodeViewBox.Text = runExternalTool(Settings.Singleton().getSetting("IL_PEVerify_Path"), "");
        }

        private string runExternalTool(string filename, string parameters)
        {
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = filename;
            p.StartInfo.Arguments = parameters;
            p.Start();
            // Read the output stream first and then wait for exit.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return output;
        }

        private void replaceInstructionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int argStartIndex = ilCodeViewBox.Text.IndexOf("(", ilCodeViewBox.Text.LastIndexOf(".method", ilCodeViewBox.SelectionStart));
            int methodNameStartIndex = ilCodeViewBox.Text.LastIndexOf(" ", argStartIndex) + 1;
            int methodNameLength = ilCodeViewBox.Text.IndexOf(")", methodNameStartIndex) - methodNameStartIndex;

            string methodName = ilCodeViewBox.Text.Substring(methodNameStartIndex, methodNameLength + 1);

            int instructionStartIndex = ilCodeViewBox.Text.LastIndexOf('\n', ilCodeViewBox.SelectionStart) + 1;
            int selectionIndexDiff = ilCodeViewBox.SelectionStart - instructionStartIndex;
            int instructionLength = ilCodeViewBox.Text.IndexOf('\n', instructionStartIndex + ilCodeViewBox.SelectionLength);
            instructionLength -= instructionStartIndex;

            string[] instructions = ilCodeViewBox.Text.Substring(instructionStartIndex, instructionLength).Split('\n');

            if (MessageBox.Show(String.Format("Are you sure that you want to replace instructions\n{0}\nof the method {1}?", String.Join("\n", instructions), methodName), "Confirm instruction replacement", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                int methodStartIndex = ilCodeViewBox.Text.LastIndexOf(".method", ilCodeViewBox.SelectionStart);
                int methodLength = ilCodeViewBox.Text.IndexOf("\n", ilCodeViewBox.Text.IndexOf("} // end of method", ilCodeViewBox.SelectionStart)) - methodStartIndex;
                
                string methodContent = ilCodeViewBox.Text.Substring(methodStartIndex, methodLength);
                //Pass in the entire method, to rewrite
                string replaced = rewriteMethod(instructions, methodContent);
                ilCodeViewBox.Text = ilCodeViewBox.Text.Replace(methodContent, replaced);

                csCodeViewBox.Text = replaced;
                ilCodeViewBox.SelectionStart = methodStartIndex;
            }
        }

        private string rewriteMethod(String[] instructions, String methodContent)
        {
            //Lines 0 and 2 are comments, don't read from them
            string[] hookMethodLines = File.ReadAllLines(Application.StartupPath + "//TDLHookCode//methodContent.txt");
            const int varLine = 1;
            const int methodLineStart = 3;

            //Remove the .maxstack header - I think this should still work regardless... (It is just for verfication)
            methodContent = methodContent.Insert(methodContent.IndexOf(".maxstack"), "//");
            
            //Count locals, and append our own
            //WARNING - if this doesn't exist, this will throw. Need to catch, and create it !!!
            int localSIndex = methodContent.IndexOf("(", methodContent.IndexOf(".locals"));
            int localEIndex = methodContent.IndexOf(")", methodContent.IndexOf(".locals"));

            int orginalLocalsCount = methodContent.Substring(localSIndex, localEIndex).Split(',').Length - 1;
            int localsCount = orginalLocalsCount;

            //Loop through all locals, add our own
            int i = 0;
            string localsAdded = "";
            foreach (string var in hookMethodLines[varLine].Split('/'))
            {
                localsAdded += (var + localsCount);
                localsCount++;
            }

            //Add our locals to the end of the list
            methodContent = methodContent.Insert(localEIndex, localsAdded);

            //Find where the requested instruction is
            int workingOffset = Int32.Parse(instructions[0].Substring(instructions[0].IndexOf("IL_") + 3, 4), System.Globalization.NumberStyles.HexNumber);
            int currentOffset = 0;
            int removalOffset = 1;

            //Insert each instruction
            string newContent = "";
            for (i = methodLineStart; i < hookMethodLines.Length; i++ )
            {
                //New instruction label
                newContent += "    IL_" + (workingOffset + currentOffset).ToString("X4").ToLower() + ":  ";
                //Rewrite the instruction
                newContent += hookMethodLines[i].Substring(hookMethodLines[i].IndexOf(':') + 1) + "\n";
                //Replace var references
                for (int localReplaceIndex = 0; localReplaceIndex < localsCount - orginalLocalsCount; localReplaceIndex++)
                    newContent = newContent.Replace(("__" + localReplaceIndex + "__"), (orginalLocalsCount + localReplaceIndex).ToString());

                //Don't increase the offset too much, so references still work
                //if(i + 1 != hookMethodLines.Length)
                  currentOffset += Int32.Parse(hookMethodLines[i].Substring(0, hookMethodLines[i].IndexOf(':')));
            }

            foreach(string instruct in instructions)
            {
                int addOffset = Int32.Parse(instruct.Substring(instruct.IndexOf("IL_") + 3, instruct.IndexOf(":") - (instruct.IndexOf("IL_") + 3)), System.Globalization.NumberStyles.HexNumber);
                removalOffset += (addOffset - workingOffset);
            }

            //Replacement positions
            int oldStartIndex = methodContent.IndexOf(String.Join("\n", instructions));
            int oldLength = String.Join("\n", instructions).Length;

            //Update all instruction references (IL_****)
            //Line labels
            Regex labelRegex = new Regex("IL_....:");
            foreach(Match m in labelRegex.Matches(methodContent))
            {
                int index = m.Index;
                //Get the hex value
                string match = m.Value.Substring(3, m.Value.Length - 4);
                int hexValue = Int32.Parse(match, System.Globalization.NumberStyles.HexNumber);

                if (hexValue > workingOffset)
                {
                    string newVal = "IL_" + (hexValue + currentOffset - removalOffset).ToString("X4").ToLower() + ":";
                    methodContent = methodContent.Replace(m.Value, newVal);
                }

            }

            //Replace the old instruction
            methodContent = methodContent.Remove(oldStartIndex, oldLength);
            methodContent = methodContent.Insert(oldStartIndex, newContent);
            
            //methodContent = methodContent.Replace(String.Join("\n", instructions), newContent);

            //Links
            Regex linkRegex = new Regex("IL_....(\\s)");
            foreach (Match m in linkRegex.Matches(methodContent))
            {
                int index = m.Index;
                //Get the hex value
                string match = m.Value.Substring(3, m.Value.Length - 3).Replace("\n", "").Replace(" ", "");
                int hexValue = Int32.Parse(match, System.Globalization.NumberStyles.HexNumber);

                if (hexValue >= workingOffset)
                {
                    string newVal = "IL_" + (hexValue + currentOffset).ToString("X4").ToLower();
                    methodContent = methodContent.Replace(m.Value, newVal);
                }

            }

            return methodContent;
        }
    }
}
