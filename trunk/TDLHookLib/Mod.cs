using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDLHookLib
{
    public class Mod
    {
        public string Name { get; private set; }
        public string Path { get; private set; }
        
        public Mod(string _name, string _path)
        {
            Name = _name;
            Path = _path;
        }

    }
}
