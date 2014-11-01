/* TDLPlugin.cs by Piepieonline
 * 
 * This module gets called by the game, with HookTDL(System.Object[] args) it's entry point
 * It then determines what to do with each intercept, and sends it along
 * 
 * TODO: Potentially register handlers for calls from the game, rather than a hardcoded switch 
 */
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

                    XmlDocument modDoc = new XmlDocument();
                    XmlNodeList eleList = settingsDoc.SelectNodes("/modloader/loadorder/*");

                    List<Mod> modsTemp = new List<Mod>();

                    for (int i = 0; i < eleList.Count; i++)
                    {
                        if (!bool.Parse(eleList[i].SelectSingleNode("@enabled").Value))
                            continue;

                        //Is this mod allowed to use code?
                        bool canRunScripts = false;
                        try
                        {
                            canRunScripts = bool.Parse(eleList[i].SelectSingleNode("@scripts").Value);
                        }
                        catch
                        { }

                        string modPath = eleList[i].SelectSingleNode("text()").Value;
                        modDoc.Load(path + "\\" + modPath + "\\info.xml");
                        string modName = modDoc.SelectSingleNode("mod/name/text()").Value;
                        string modVersion = modDoc.SelectSingleNode("mod/version/text()").Value;
                        string modAuthor = modDoc.SelectSingleNode("mod/author/text()").Value;
                        modsTemp.Add(new Mod(modName, modVersion, modAuthor, modPath, canRunScripts));
                    }
                    mods = modsTemp.ToArray();


                    //Load the settings
                    eleList = settingsDoc.SelectNodes("/modloader/settings/setting");
                    for (int i = 0; i < eleList.Count; i++)
                    {
                        settings.Add(eleList[i].SelectSingleNode("@id").Value, eleList[i].SelectSingleNode("text()").Value);
                    }

                    //Create the reference
                    pl = new TDLPlugin();

                    //Add the ModsMenu menu
                    //TDLMenuCommon.Singleton.gameObject.AddComponent<ModsMenu>();

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

                        return "";
                    case "ShowModMenu":
                        if (!BodyMenuMods.singleton)
                            Component.FindObjectOfType<StartMenu>().gameObject.AddComponent<BodyMenuMods>();
                        return BodyMenuMods.singleton;
                    case "LoadPrefab":
                        DebugOutput("Prefab: '" + (string)args[1] + "'");

                        return LoadEntityTable.getPrefabObject((string)args[1]);
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

        //If the player doesn't have a mod directory, lets make one
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
                "<!-- Disable scripts on mods that you don't trust, as they have system wide access. However, this may break some mods -->",
	            "<loadorder>",
			    "\t\t<!--<modpath enabled='true'>ExampleMod</modpath>-->",
                "\t\t<!--<modpath enabled='true' scripts='true'>ExampleModWithScripts</modpath>-->",
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
            WorldPosition worldPos = WorldPosition.ClientToWorld(LocalPlayerManager.p.camera.transform.position + (LocalPlayerManager.p.camera.transform.forward * 3f));

            Moveable spawnedObject = new Moveable(WorldObject.getNextUid(), args[1], worldPos, RandomGenerator.Singleton.randomPlanarRotation());
            spawnedObject.EnsureInScenarioBlock();

            return "Spawned:" + args[1];
        }

        private object fire_callback(params string[] args)
        {
            string compList = "";
            foreach (GameObject c in LocalPlayerManager.p.cursorTarget.unityGameObject.GetComponent<WoodFireControl>().fireParticleObjects)
            //foreach (GameObject c in GameObject.FindGameObjectsWithTag("fireeffects"))
            {
                compList += c.GetType().ToString() + "\n";
                compList += c.name + "\n";
            }
            File.WriteAllText(@"D:\fire.txt", compList);
            return "done";
        }

        private object gettarget_callback(params string[] args)
        {
            if (LocalPlayerManager.p.cursorTarget != null && LocalPlayerManager.p.cursorTarget.unityGameObject != null)
                return LocalPlayerManager.p.cursorTarget.unityGameObject.name;
            return "";
        }

        private void probingCode()
        {
            //Temporary - give us a spawner command, to test with
            //DebugConsole.RegisterCommand("/spawn", new DebugConsole.DebugCommand(this.spawner_callback));

            DebugConsole.RegisterCommand("/fire", new DebugConsole.DebugCommand(this.fire_callback));

            DebugConsole.RegisterCommand("/target", new DebugConsole.DebugCommand(this.gettarget_callback));
            try
            {
                //Entity ent = Entity.GetEntityByName("candle_lit");
                //ent.prefab.AddComponent<HookFlickerComp>();
            }
            catch (Exception)
            { }

            DebugOutput(GameObject.FindObjectsOfType<Camera>().Length.ToString());

            foreach (Camera c in GameObject.FindObjectsOfType<Camera>())
            {
                DebugOutput(c.name);
                //c.renderingPath = RenderingPath.DeferredLighting;
            }


            //"mixed_category"

            foreach (Category sub in CategoryReader.categories.Values)
            //foreach(SubCategory sub in CategoryReader.categories["town_biome"].subcats.Values)
            {
                //DebugConsole.Log(sub.name);
            }

            //Camera cam = GameObject.FindObjectOfType<Camera>();
            //cam.renderingPath = RenderingPath.DeferredLighting;


            //Adding our own bikes and such
            //...Promising...
            string n = Entity.GetEntityByName("bicycle_mountainbike").prefab.GetComponent<TDLTwoWheelVehicleMotor>().frontWheelNode.GetComponent<Transform>().position.ToString();

            DebugConsole.Log(n);

            //Entity.GetEntityByName("bicycle_mountainbike").prefab.GetComponent<TDLTwoWheelVehicleMotor>().frontWheelNode.GetComponent<Transform>().position = new Vector3(0f, 1.0f, 0.6f);

            string compList = "";
            //foreach (Component c in Entity.GetEntityByName("bicycle_mountainbike").prefab.GetComponent<TDLTwoWheelVehicleMotor>().frontWheelNode.GetComponents<Component>())
            //foreach (GameObject c in Entity.GetEntityByName("item_wood_campfire_large").prefab.GetComponent<WoodFireControl>().fireParticleObjects)
            foreach (Component c in Entity.GetEntityByName("candle_unlit").prefab.GetComponents<Component>())
            {
                compList += c.GetType().ToString() + "\n";
            }
            File.WriteAllText(@"D:\TDLOut.txt", compList);

            //Dammit. They are using unity lod groups
            
            foreach (LODGroup c in Entity.GetEntityByName("house_suburban_1story_plain").prefab.GetComponents<LODGroup>())
            {
                DebugConsole.Log("LODs: " + c.lodCount.ToString());
            }
            
            
            /*
            Rigidbody rb = Entity.GetEntityByName("zombie_medium").prefab.AddComponent<Rigidbody>();
            CapsuleCollider bc = Entity.GetEntityByName("zombie_medium").prefab.AddComponent<CapsuleCollider>();
            rb.mass = 90.0f;
            rb.drag = 0f;
            rb.angularDrag = 0.5f;
            bc.center = new Vector3(0f, 0f, 0f);
            //bc.size = new Vector3(0.3f, 1.8f, 0.3f);
            bc.radius = 0.3f;
            bc.height = 1.8f;
             */

            //new LineRenderer().

            DebugOutput("Probe Finished");
        }
    }
}