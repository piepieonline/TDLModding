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
    public partial class FindReplace : Form
    {
        public FindReplace()
        {
            InitializeComponent();
        }

        public string findString { get { return textBox1.Text; } }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
