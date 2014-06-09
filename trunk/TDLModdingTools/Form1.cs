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
            String fileToDasm = @"D:\SteamLibrary\SteamApps\common\The Dead Linger\TDL_Data\Managed\Assembly-CSharp.dll";

            openedFileName = "Assembly-CSharp";

            // Start the child process.
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = Settings.Singleton().getSetting("IL_DASM_Path");
            p.StartInfo.Arguments = " \"" + fileToDasm + "\"" + @" /TEXT";
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            ilCodeViewBox.Text = output;
            codeToTreeView();
        }

        private void codeToTreeView()
        {
            string[] lines = ilCodeViewBox.Lines;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimStart(new char[] { '\t', ' ' });

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
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText(Application.StartupPath + "//" + openedFileName + ".il", ilCodeViewBox.Text);

            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = Settings.Singleton().getSetting("IL_ASM_Path");
            p.StartInfo.Arguments = "/DLL \"" + Application.StartupPath + "//" + openedFileName + ".il" + "\"";
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            //richTextBox1.Text = output;
            File.Delete(Application.StartupPath + "//" + openedFileName + ".il");
            MessageBox.Show("File saved to " + Application.StartupPath + "//" + openedFileName + ".dll");
        }

        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
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
            if (treeReady)
            {
                ilCodeViewBox.Focus();
                ilCodeViewBox.Select(ilCodeViewBox.GetFirstCharIndexFromLine(((LineRefTreeNode)treeView1.SelectedNode).refLineNumber), 0);
                ilCodeViewBox.ScrollToCaret();
                //MessageBox.Show(ilCodeViewBox.Lines[((LineRefTreeNode)treeView1.SelectedNode).refLineNumber + 1]);
            }
        }

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

        public class LineRefTreeNode : TreeNode
        {
            public int refLineNumber { get; set; }

            public LineRefTreeNode(string text, int lineNo)
                : base(text)
            {
                refLineNumber = lineNo;
            }
        }
    }
}
