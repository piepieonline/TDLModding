using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace TDLModdingTools
{
    public class LineRefTreeNode : TreeNode
    {
        public int refLineNumber { get; set; }

        public LineRefTreeNode(string text, int lineNo) : base(text)
        {
            refLineNumber = lineNo;
        }
    }
}
