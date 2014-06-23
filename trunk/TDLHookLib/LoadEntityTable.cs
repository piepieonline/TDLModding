/* LoadEntityTable.cs by Piepieonline
 * We use this class to do the most common mods - entities
 * Both new and existing are handled here
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using System.IO;
using System.Xml;
using System.Timers;
using System.Reflection;

namespace TDLHookLib
{
    class LoadEntityTable
    {
        private static List<string> loadList = new List<string>();
        private static FileIO.LoadCallback objLoadedCallback = new FileIO.LoadCallback(objLoaded);

        private static List<ModelEntityMapping> objToEntity = new List<ModelEntityMapping>();
        //private static List<>

        //private static Dictionary<string, Type> scriptComponents = new Dictionary<string, Type>();

        private static Timer loadTimer;

        public LoadEntityTable()
        {
            //Last mod will overwrite the first, as per normal
            foreach (Mod loadingMod in TDLPlugin.mods)
            {
                TDLPlugin.DebugOutput("Loading mod: " + loadingMod.Name);
                //Determine the path to load the mod from
                string workingPath = TDLPlugin.path + "/" + loadingMod.Path + "/";

                //Load all of the components

                //Load the script dll file, if it exists
                Assembly workingAssembly = null;
                try
                {
                    //Security check
                    if(loadingMod.CanRunScripts)
                        workingAssembly = Assembly.LoadFile(workingPath + loadingMod.Name + ".dll");
                }
                catch
                { }

                //Load the entities
                loadModEntities(workingPath, loadingMod.Name, workingAssembly);

                //Load other text assets - such as spawn categories, or recipies
                if (Directory.Exists(workingPath + "textassets"))
                {
                    loadCategories(workingPath + "textassets");
                    loadRecipes(workingPath + "textassets");
                }
            }
            //Create the timer to load the next object
            //Some bug in the lib means that I can't call repeatedly
            loadTimer = new Timer(10);
            loadTimer.Elapsed += OnLoadTimerEvent;
            //Don't enable, the callback will do that for us
            if (loadList.Count > 0)
                loadNextObjFile();
        }

        #region EntityTable
        public void loadModEntities(string workingPath, string modName, Assembly workingAssembly)
        {
            //If we have entities to load, do so
            workingPath += "entities";
            if (Directory.Exists(workingPath))
            {
                //Load all entity files                  
                XmlDocument entityDoc = new XmlDocument();
                foreach (string fileName in Directory.GetFiles(workingPath, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    //Load the file
                    entityDoc.LoadXml(System.IO.File.ReadAllText(fileName));

                    //Store the name
                    string entityName = entityDoc.SelectSingleNode("/entity/@name").Value;

                    Entity newEnt = Entity.GetEntityByName(entityName);
                    GameObject newPrefab;

                    //If the object doesn't exist yet, make it
                    if (newEnt.entityName == "prop_unknown")
                    {
                        TDLPlugin.DebugOutput("Unknown entity, creating: " + entityName);
                        //Create the entity
                        XmlDocument xmlDocument = new XmlDocument();
                        //Load the Xml
                        xmlDocument.LoadXml("<entity class='" + entityDoc.SelectSingleNode("/entity/properties/class/text()").Value + "' name='" + entityName + "' near='yes' cache='3' ambient='yes' />");
                        newEnt = new Entity(xmlDocument.DocumentElement);
                        //Create the entity prefab
                        newPrefab = new GameObject(entityName + "_prefab");
                    }
                    else
                    {
                        newPrefab = newEnt.prefab;
                    }

                    //Entity properties
                    try
                    {
                        newEnt.carry = bool.Parse(entityDoc.SelectSingleNode("/entity/properties/canCarry/text()").Value);
                    }
                    catch
                    {}

                    try
                    {
                        newEnt.drag = bool.Parse(entityDoc.SelectSingleNode("/entity/properties/canDrag/text()").Value);
                    }
                    catch
                    { }

                    try
                    {
                        newEnt.nearFlag = bool.Parse(entityDoc.SelectSingleNode("/entity/properties/near/text()").Value);
                    }
                    catch
                    { }

                    try
                    {
                        newEnt.lootEntry = new LootEntry(entityDoc.SelectSingleNode("/entity/properties/lootEntry/text()").Value.Split(','));
                    }
                    catch
                    { }

                    //Mesh
                    try
                    {
                        string replacementMesh = entityDoc.SelectSingleNode("/entity/mesh/text()").Value;
                        if (replacementMesh != null)
                        {
                            //Disable the current model
                            try
                            {
                                newPrefab.GetComponent<LODGroup>().enabled = false;
                            }
                            catch
                            { }

                            //Load our model
                            loadList.Add(workingPath + "/" + entityDoc.SelectSingleNode("/entity/mesh/text()").Value);
                            string meshFileName = entityDoc.SelectSingleNode("/entity/mesh/text()").Value;
                            objToEntity.Add(new ModelEntityMapping(meshFileName.Substring(0, meshFileName.LastIndexOf('.')), entityName));
                        }
                    }
                    catch
                    {
                        //Nope, no mesh
                    }

                    //Physics
                    try
                    {
                        //Are we removing the orginal?
                        try
                        {
                            if (Boolean.Parse(entityDoc.SelectSingleNode("/entity/physics/@replace").Value))
                            {
                                foreach(Collider col in newPrefab.GetComponents<Collider>())
                                    Destroy.Destroy(col);
                            }
                        }
                        catch (System.Xml.XPath.XPathException)
                        {
                            //Not specified, so no
                        }

                        XmlNodeList eleList = entityDoc.SelectNodes("/entity/physics/*");

                        //Add all child physics elements
                        for (int i = 0; i < eleList.Count; i++)
                        {
                            string type = eleList[i].Name;

                            //TODO: Add all col types to this
                            Collider newCol = null;
                            switch (type)
                            {
                                case "rigidBody":
                                    Rigidbody rb = newPrefab.GetComponent<Rigidbody>();
                                    if (rb == null)
                                        rb = newPrefab.AddComponent<Rigidbody>();
                                    rb.mass = float.Parse(eleList[i].SelectSingleNode("mass/text()").Value);
                                    break;
                                case "boxCollider":

                                    string[] center = eleList[i].SelectSingleNode("center/text()").Value.Split('|');
                                    string[] size = eleList[i].SelectSingleNode("size/text()").Value.Split('|');

                                    newCol = newPrefab.AddComponent<BoxCollider>();
                                    ((BoxCollider)newCol).center = new Vector3(float.Parse(center[0]), float.Parse(center[1]), float.Parse(center[2]));
                                    ((BoxCollider)newCol).size = new Vector3(float.Parse(size[0]), float.Parse(size[1]), float.Parse(size[2]));
                                    //Temp, this should be loaded elsewhere, somehow
                                    newCol.material = (PhysicMaterial)Resources.Load("PhysicMaterials/Rubber");
                                    newCol.material.bounceCombine = PhysicMaterialCombine.Minimum;
                                    break;
                                case "sphereCollider":
                                    break;
                                case "meshCollider":
                                    Mesh colMesh = ObjImporter.ImportFile(workingPath + "/" + eleList[i].SelectSingleNode("mesh/text()").Value);
                                    newCol = newPrefab.AddComponent<MeshCollider>();
                                    ((MeshCollider)newCol).sharedMesh = colMesh;
                                    ((MeshCollider)newCol).convex = bool.Parse(eleList[i].SelectSingleNode("convex/text()").Value);
                                    break;

                                default:
                                    TDLPlugin.DebugOutput("Unknown physics type: " + type);
                                    break;
                            }

                            try
                            {
                                newCol.name = eleList[i].Attributes["id"].Value;
                                DebugConsole.Log("Setting name " + newCol.name + " on " + newPrefab.name);
                            }
                            catch
                            {}
                        }

                    }
                    catch
                    {
                        //Nope, no physics
                    }

                    //Script Components
                    try
                    {
                        XmlNodeList componentListXML = entityDoc.SelectNodes("/entity/scripts/*");
                        //Add all scripts to the entity
                        for (int i = 0; i < componentListXML.Count; i++)
                        {
                            newPrefab.AddComponent(workingAssembly.GetType("Mod." + modName + "." + componentListXML[i].SelectSingleNode("text()").Value));//, true));
                        }
                    }
                    catch
                    {
                        //No scripts to load
                    }

                    //Reassign the prefab
                    newEnt.prefab = newPrefab;
                }
            }
            //Assembly assemble = Assembly.LoadFile(@"D:\SteamLibrary\SteamApps\common\The Dead Linger\TDL_Data\Mods\RotateLock\RotateLock.dll");
            //TDLMenuCommon.Singleton.gameObject.AddComponent(assemble.GetType("Mod.RotateLock.RotateLockComponent", true));

        }

        public static void objLoaded(GameObject[] loaded)
        {
            try
            {
                //Sanity check
                if (objToEntity[0].model != loaded[0].name)
                    throw new Exception("Mod loading failed. Model doesn't map to entity. " + objToEntity[0].model + " != " + loaded[0].name);

                TDLPlugin.DebugOutput(objToEntity[0].entity + " Loaded");

                //Assign the loaded components
                Entity.GetEntityByName(objToEntity[0].entity).prefab.AddComponent<MeshFilter>().mesh = loaded[0].GetComponent<MeshFilter>().mesh;
                Entity.GetEntityByName(objToEntity[0].entity).prefab.AddComponent<MeshRenderer>().material = loaded[0].GetComponent<MeshRenderer>().material;

                //Destroy the created GameObject, and remove the head of the list
                Destroy.Destroy(loaded[0]);
                objToEntity.RemoveAt(0);

                //If we have more than 1 object to load, start loading again
                if (loadList.Count > 0)
                    loadTimer.Enabled = true;
                else
                    AllModelsLoadedHandler();
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(TDLPlugin.path + "/error_log.log", ex.ToString() + "\n\n");
            }
        }

        public static void loadNextObjFile()
        {
            //Load the next object, remove it from the list
            FileIO.Load3d(loadList[0], false, false, objLoadedCallback);
            loadList.RemoveAt(0);
        }

        private void OnLoadTimerEvent(System.Object source, ElapsedEventArgs args)
        {
            //Load the next object, disable the timer until it has loaded
            loadNextObjFile();
            loadTimer.Enabled = false;
        }

        private static void AllModelsLoadedHandler()
        {}

        private struct ModelEntityMapping
        {
            public string model;
            public string entity;

            public ModelEntityMapping(string _model, string _entity)
            {
                model = _model;
                entity = _entity;
            }
        }

        #endregion

        #region TextAssets
        private void loadCategories(string workingPath)
        {
            if (File.Exists(workingPath + "/categories.xml"))
            {
                //We are modifying catagories, lets go
                XmlDocument categoryDoc = new XmlDocument();
                //Load the file
                categoryDoc.LoadXml(System.IO.File.ReadAllText(workingPath += "/categories.xml"));

                XmlNodeList catList = categoryDoc.SelectNodes("/textasset/*");
                //Iterate through all categories
                for (int catCount = 0; catCount < catList.Count; catCount++)
                {
                    string category = catList[catCount].SelectSingleNode("@id").Value;
                    try
                    {
                        SubCategory newSub;
                        //Additions
                        XmlNodeList addList = catList[catCount].SelectNodes("add/*");
                        for (int addCount = 0; addCount < addList.Count; addCount++)
                        {
                            //TDLPlugin.DebugOutput(addList[addCount].SelectSingleNode("entity/@id").Value);
                            //Create the new entry
                            newSub = new SubCategory()
                            {
                                name = addList[addCount].SelectSingleNode("@id").Value,
                                freq = float.Parse(addList[addCount].SelectSingleNode("frequency/text()").Value),
                                subgroup = addList[addCount].SelectSingleNode("subgroup/text()").Value
                            };
                            //Add it
                            CategoryReader.categories[category].subcats.Add(addList[addCount].SelectSingleNode("@id").Value, newSub);
                        }
                        //Modifications
                        XmlNodeList modList = catList[catCount].SelectNodes("modify/*");
                        for (int modCount = 0; modCount < modList.Count; modCount++)
                        {
                            //Create the new entry
                            newSub = new SubCategory()
                            {
                                name = modList[modCount].SelectSingleNode("@id").Value,
                                freq = float.Parse(modList[modCount].SelectSingleNode("frequency/text()").Value),
                                subgroup = modList[modCount].SelectSingleNode("subgroup/text()").Value
                            };
                            //Change it
                            CategoryReader.categories[category].subcats[modList[modCount].SelectSingleNode("@id").Value] = newSub;
                        }
                        //Removals
                        XmlNodeList remList = catList[catCount].SelectNodes("remove/*");
                        for (int remCount = 0; remCount < remList.Count; remCount++)
                        {
                            //Delete it
                            CategoryReader.categories[category].subcats.Remove(remList[remCount].SelectSingleNode("@id").Value);
                        }
                    }
                    catch (System.Collections.Generic.KeyNotFoundException)
                    {
                        DebugConsole.LogError("Category not found: " + category);
                    }
                }
            }
        }

        private void loadRecipes(string workingPath)
        {
            if (File.Exists(workingPath + "/recipies.xml"))
            {
                //We are modifying catagories, lets go
                XmlDocument craftingDoc = new XmlDocument();
                //Load the file
                craftingDoc.LoadXml(System.IO.File.ReadAllText(workingPath += "/recipies.xml"));


                //...Not gunna lie, lifted from TDL IL Code
                foreach (XmlNode current in craftingDoc.ChildNodes)
                {
                    if (current.Name != "crafting")
                    {
                        continue;
                    }
                    foreach (XmlNode xmlNodes in current.ChildNodes)
                    {
                        if (xmlNodes.Name != "recipe")
                        {
                            continue;
                        }
                        CraftingRecipe craftingRecipe = new CraftingRecipe();
                        craftingRecipe.Load(xmlNodes);
                        if (!CraftingDefinitions.recipes.ContainsKey(craftingRecipe.name))
                        {
                            CraftingDefinitions.recipes[craftingRecipe.name] = craftingRecipe;
                        }
                        else
                        {
                            TDLLogging.LogError("reci_1", string.Concat("ERROR - Duplicate recipe name in recipies.xml: ", craftingRecipe.name));
                            CraftingDefinitions.recipes[string.Concat(craftingRecipe.name, "_error")] = craftingRecipe;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
