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
                        {}

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
                    TDLMenuCommon.Singleton.gameObject.AddComponent<ModsMenu>();

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
                        //pl.probingCode();

                        return "";
                    case "ShowModMenu":
                        ModsMenu.activateGUI();
                        break;
                    case "InteractDown":
                        pl.interactDown();
                        break;
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
            WorldPosition world = WorldPosition.ClientToWorld(LocalPlayerManager.p.localCamera.transform.position + (LocalPlayerManager.p.localCamera.transform.forward * 3f));
            Moveable spawnedObject = new Moveable(WorldObject.getNextUid(), args[1], world, RandomGenerator.Singleton.randomPlanarRotation(), eWorldObjectGroup.eWorldObjectGroup_NearDynamic);
            spawnedObject.ensureInScenarioBlock();
            return "Spawned:" + args[1];
        }

        private void probingCode()
        {
            //Temporary - give us a spawner command, to test with
            DebugConsole.RegisterCommand("/spawn", new DebugConsole.DebugCommand(this.spawner_callback));

            try
            {
                Entity bin = Entity.GetEntityByName("prop_trash_wheelybin");

                bin.prefab.AddComponent<PhysHoldComp>();
            }
            catch (Exception)
            { }

            DebugOutput(GameObject.FindObjectsOfType<Camera>().Length.ToString());

            foreach(Camera c in GameObject.FindObjectsOfType<Camera>())
            {
                DebugOutput(c.name);
                //c.renderingPath = RenderingPath.DeferredLighting;
            }



            //Camera cam = GameObject.FindObjectOfType<Camera>();
            //cam.renderingPath = RenderingPath.DeferredLighting;


            //Adding our own bikes and such
            //...Promising...
            string n = Entity.GetEntityByName("bicycle_mountainbike").prefab.GetComponent<TDLTwoWheelVehicleMotor>().frontWheelNode.GetComponent<Transform>().position.ToString();

            DebugConsole.Log(n);

            Entity.GetEntityByName("bicycle_mountainbike").prefab.GetComponent<TDLTwoWheelVehicleMotor>().frontWheelNode.GetComponent<Transform>().position = new Vector3(0f, 1.0f, 0.6f);

            string compList = "";
            foreach (Component c in Entity.GetEntityByName("bicycle_mountainbike").prefab.GetComponent<TDLTwoWheelVehicleMotor>().frontWheelNode.GetComponents<Component>())
            {
                compList += c.GetType().ToString();
            }
            File.WriteAllText(@"D:\bike.txt", compList);

            DebugOutput("Probe Finished");
        }

        public void interactDown()
        {
            if (LocalPlayerManager.p.cursorTarget != null)
            {
                DebugOutput("Player grabbing at");
            }
        }
    }

 

    //Candle test
    /*
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
    */
    //Collision manager
    public class PhysHoldComp : MonoBehaviour
    {
        /*void OnCollisionEnter(Collision coll)
        {
            if (coll.gameObject.name.ToLower() == "terrain")
                return;
        }*/

        //List<Rigidbody> rbs = new List<Rigidbody>();
        //bool hasJoints = false;

        //Dictionary<string, PhysicMaterial> mats = new Dictionary<string, PhysicMaterial>();
        /*
        void OnTriggerEnter(Collider col)
        {
            mats[col.name] = col.material;

            col.material = (PhysicMaterial)Resources.Load("PhysicMaterials/Rubber");
            col.material.bounceCombine = PhysicMaterialCombine.Minimum;

            if (!rbs.Contains(col.rigidbody) && col.rigidbody.name.ToLower() != "terrain")
                rbs.Add(col.rigidbody);
        }

        void OnTriggerExit(Collider col)
        {
            col.material = mats[col.name];
            mats.Remove(col.name);
            
            if (rbs.Contains(col.rigidbody))
                rbs.Remove(col.rigidbody);
        }
        */

        bool pickedUp = false;
        Dictionary<int, Collider> bodies = new Dictionary<int, Collider>();
        Dictionary<int, PhysicMaterial> mats = new Dictionary<int, PhysicMaterial>();
        void Update()
        {
            if (!pickedUp && LocalPlayerManager.p.tdlPlayer.isDragging && WorldObject.findByGameObject(this.gameObject) == LocalPlayerManager.p.tdlPlayer.carryObject)
            {
                foreach(Collider col in Physics.OverlapSphere(this.gameObject.rigidbody.position, 1f))
                {
                    bodies[col.GetInstanceID()] = col;
                    mats[col.GetInstanceID()] = col.material;

                    col.material = (PhysicMaterial)Resources.Load("PhysicMaterials/Rubber");
                    col.material.bounceCombine = PhysicMaterialCombine.Minimum;
                }
                pickedUp = true;
            }
            else if (pickedUp && !LocalPlayerManager.p.tdlPlayer.isDragging)
            {
                foreach(int key in bodies.Keys)
                {
                    bodies[key].material = mats[key];
                }
            }
      
        }

        /*
                    if (LocalPlayerManager.p.tdlPlayer.isCarrying && WorldObject.findByGameObject(this.gameObject) == LocalPlayerManager.p.tdlPlayer.carryObject)
            {

                Bounds shrunkBounds = this.GetComponent<MeshCollider>().bounds;
                //shrunkBounds.Expand(-0.3f);

                DebugConsole.Log(this.GetComponent<MeshCollider>().bounds.size.ToString() + " > " + shrunkBounds.size.ToString());

                foreach (ContactPoint cp in coll.contacts)
                {
                    if (shrunkBounds.Contains(cp.point))
                    {
                        DebugConsole.Log("Joint Made");
                        FixedJoint joint = this.gameObject.AddComponent<FixedJoint>();
                        joint.connectedBody = coll.rigidbody;
                        joint.breakForce = Mathf.Infinity;
                        joint.breakTorque = Mathf.Infinity;
                        break;
                    }
                }
            }
            else
            {
                DebugConsole.Log("Joints Removed");
                FixedJoint[] joints = this.gameObject.GetComponents<FixedJoint>();
                for (int i = 0; i < joints.Length; i++)
                {
                    Destroy(joints[i]);
                }
            } 
        */
    }
}