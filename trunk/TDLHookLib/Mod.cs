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
        public string Version { get; private set; }
        public string Author { get; private set; }
        public string Path { get; private set; }
        public bool CanRunScripts { get; private set; }
        
        public Mod(string _name, string _version, string _author, string _path, bool _canRunScripts)
        {
            Name = _name;
            Version = _version;
            Author = _author;
            Path = _path;
            CanRunScripts = _canRunScripts;
        }

        public string GetGUIListString()
        {
            return Name + " (v" + Version + ") by " + Author;
        }
    }
}
