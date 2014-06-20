#define DEBUG_CONSOLE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using UnityEngine;

using System.IO;
using System.Xml;

namespace TDLHookLib
{
    public class TDLPlugin
    {
        static TDLPlugin pl;
        static LoadEntityTable let;
        public static String path = "";

        static XmlDocument settingsDoc = new XmlDocument();
        public static Mod[] mods;
        public static Dictionary<string, string> settings = new Dictionary<string, string>();

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

                    if (!Directory.Exists(path) || !File.Exists(path + "/modloader.xml"))
                        CreateModDirectory();

                    settingsDoc.LoadXml(System.IO.File.ReadAllText(path + "/modloader.xml"));

                    //Load the mods

                    //TODO: Added 'enable' as an option
                    XmlNodeList eleList = settingsDoc.SelectNodes("/modloader/loadorder/mod");
                    mods = new Mod[eleList.Count];
                    for (int i = 0; i < eleList.Count; i++)
                    {
                        mods[i] = new Mod(eleList[i].SelectSingleNode("@name").Value, eleList[i].SelectSingleNode("path/text()").Value);
                    }
                    
                    //Load the settings
                    eleList = settingsDoc.SelectNodes("/modloader/settings/setting");
                    for (int i = 0; i < eleList.Count; i++)
                    {
                        settings.Add(eleList[i].SelectSingleNode("@id").Value, eleList[i].SelectSingleNode("text()").Value);
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
                        let = new LoadEntityTable();

                        //TEMP
                        pl.probingCode();

                        return "";//System.IO.File.ReadAllText(path + "/mod/entity.xml");
                    default:
                        DebugOutput("No hook found for '" + command + "'");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(path + "/error_log.log", ex.ToString() + "\n\n");
            }

            return null;
        }

        private static void CreateModDirectory()
        {
            Directory.CreateDirectory(path);
            File.WriteAllLines(path + "/modloader.xml", new string[]{
                "<?xml version='1.0'?>",
                "<modloader>",
	            "\t<settings>",
		        "\t\t<setting id='dumpTextAssets'>false</setting>",
	            "\t</settings>",
                "",
                "<!-- Loads from the top down. So the last mod will overwrite any conflicts in the first -->",
                "<!-- Note that the directory doesn't need any slashes - For the example mod, it will look in TDL_Data/Mods/ExampleMod/* -->",
	            "<loadorder>",
		        "\t<!--<mod name='ExampleMod'>",
			    "\t\t\t<path>ExampleMod</path>",
		        "\t\t</mod>-->",
	            "\t</loadorder>",
                "</modloader>"
            });
            Directory.CreateDirectory(path + "/Default");
            Directory.CreateDirectory(path + "/Default/TextAssets");
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
            if (Boolean.Parse(settings["dumpTextAssets"]) == true)
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

        //Used for /spawn
        private object spawner_callback(params string[] args)
        {
            if (args.Length != 2)
                return "Must have an object (E.G. 'bicycle_mountainbike') as a parameter.";
            WorldPosition world = WorldPosition.ClientToWorld(LocalPlayerManager.p.localCamera.transform.position + (LocalPlayerManager.p.localCamera.transform.forward * 3f));
            Moveable spawnedObject = new Moveable(WorldObject.getNextUid(), args[1], world, RandomGenerator.Singleton.randomPlanarRotation(), eWorldObjectGroup.eWorldObjectGroup_NearDynamic);
            spawnedObject.ensureInScenarioBlock();
            return "Spawned:" + args[1];
        }

        private void probingCode()
        {
            //Temporary - give us a spawner command, to test with
            DebugConsole.RegisterCommand("/spawn", new DebugConsole.DebugCommand(this.spawner_callback));

            Entity candle = Entity.GetEntityByName("candle_lit");

            Light l = candle.prefab.AddComponent<Light>();
            l.color = new Color(255f, 81f, 25f);
            l.intensity = 0.02f;
            l.shadows = LightShadows.Soft;
            l.shadowStrength = 0.3f;
            candle.prefab.AddComponent<FlickerComp>();

            candle.lootEntry = new LootEntry(new string[]
                {
                    "candle_unlit",
                    "Candle (Unlit)",
                    "1x1",
                    "misc",
                    "0",
                    "weight(0)",
                    "Kumbaya"
                });


            DebugOutput("Probe Finished");
        }
    }

 

    //Candle test
    public class FlickerComp : MonoBehaviour
    {
        float maxInt = 0.02f;

        void Update()
        {
            if (light.intensity > maxInt)
                light.intensity -= 0.001f;
            else
                light.intensity += 0.001f;
        }
    }
}