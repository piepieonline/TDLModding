using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using UnityEngine;

namespace TDLHookLib
{
    public class TDLPlugin
    {
        static TDLPlugin pl;
        static String path = "";

        static Dictionary<String, bool> settings = new Dictionary<string, bool>();

        static bool debugging = false;

        //Blank constructor, do not remove or the hook will fail
        public TDLPlugin()
        { }

        //The game will call this function
        public static System.Object HookTDL(System.Object[] args)
        {
            DebugOutput("TDL Modding Hook Called...");
            try
            {
                //Set the startup path, so we can find the modding directory
                if (pl == null)
                {
                    path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring(8);
                    path = path.Substring(0, path.Substring(0, path.LastIndexOf("/")).LastIndexOf("/")) + "/Mods";

                    foreach (string line in System.IO.File.ReadAllLines(path + "/modloader_settings.cfg"))
                    {
                        string[] parts = line.Split(new char[] { '=' });
                        settings.Add(parts[0], Boolean.Parse(parts[1]));
                    }

                    //Create the reference
                    pl = new TDLPlugin();

                    //Check settings, such as TextAsset dumps
                    if (!debugging)
                    {
                        pl.WriteOutTextAssets();
                    }
                }

                //We are passing in the entrypoint as a string here, so a cast should be fine
                string command = "";
                try
                {
                    command = (string)args[0];
                }
                catch (Exception ex)
                {
                    //If it fails, give some feedback as to why
                    if (args != null)
                        DebugOutput("Invalid parameter (0): " + args[0]);
                    else
                        DebugOutput("Invalid parameter (0): null");
                    System.IO.File.WriteAllText(path + "/error_log.log", "Hook Argument Exception\r\n" + ex.ToString());
                }
                //Switch for all of the known hooks
                switch (command)
                {
                    case "loadEntityTable":
                        DebugOutput("loadTable executing");
                        return System.IO.File.ReadAllText(path + "/mod/entity.xml");
                    default:
                        DebugOutput("No hook found for '" + command + "'");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText(path + "/error_log.log", ex.ToString());
            }

            return null;
        }

        //If we are not running under the game, Console.WriteLine instead of DebugConsole.Log
        public static void DebugOutput(string debugOut)
        {
            if (debugging)
                Console.WriteLine(debugOut);
            else
                DebugConsole.Log(debugOut);
        }

        private void WriteOutTextAssets()
        {
            if (settings["DUMP_TEXT_ASSETS"] == true)
            {
                //Write the TextAsset files out.
                foreach (TextAsset asset in Resources.FindObjectsOfTypeAll(typeof(TextAsset)) as TextAsset[])
                {
                    //...Seriously...
                    if (asset.name == "")
                        continue;
                    if (asset.text == "")
                        continue;

                    string tempDirs = "";
                    //We may not have a dir, if not, skip
                    if (asset.name.LastIndexOf('/') != -1)
                    {
                        //Otherwise, create those that are missing
                        string[] dirsNeeded = asset.name.Substring(0, asset.name.LastIndexOf('/')).Split(new char[] { '/' });
                        foreach (string dir in dirsNeeded)
                        {
                            tempDirs += "/" + dir;
                            if (!System.IO.Directory.Exists(path + "/Default/TextAssets" + tempDirs))
                                System.IO.Directory.CreateDirectory(path + "/Default/TextAssets" + tempDirs);
                        }
                    }
                    //Write the XML to where it needs to go
                    System.IO.File.WriteAllText(path + "/Default/TextAssets/" + asset.name, asset.text);
                }
            }
        }
    }
}
