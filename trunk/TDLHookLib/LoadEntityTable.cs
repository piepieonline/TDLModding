using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using System.IO;
using System.Xml;
using System.Timers;

namespace TDLHookLib
{
    class LoadEntityTable
    {
        private static List<string> loadList = new List<string>();
        private static FileIO.LoadCallback objLoadedCallback = new FileIO.LoadCallback(objLoaded);
        private static Dictionary<string, string> objToEntity = new Dictionary<string, string>();

        private static Timer loadTimer;

        public LoadEntityTable()
        {
            //Last mod will overwrite the first, as per normal
            foreach (Mod loadingMod in TDLPlugin.mods)
            {
                TDLPlugin.DebugOutput("Loading mod: " + loadingMod.Name);
                //Determine the path to load the mod from
                string workingPath = TDLPlugin.path + "/" + loadingMod.Path + "/";

                loadModEntities(workingPath);
                loadCategories(workingPath);
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
        public void loadModEntities(string workingPath)
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
                        newEnt.drag = bool.Parse(entityDoc.SelectSingleNode("/entity/properties/canDrag/text()").Value);
                        newEnt.nearFlag = bool.Parse(entityDoc.SelectSingleNode("/entity/properties/near/text()").Value);
                    }
                    catch (System.Xml.XPath.XPathException)
                    {
                        //Nope, no properties
                    }

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
                            //Blank components, these will be written to later
                            newPrefab.AddComponent<MeshFilter>();
                            newPrefab.AddComponent<MeshRenderer>();

                            loadList.Add(workingPath + "/" + entityDoc.SelectSingleNode("/entity/mesh/text()").Value);
                            string meshFileName = entityDoc.SelectSingleNode("/entity/mesh/text()").Value;
                            objToEntity.Add(meshFileName.Substring(0, meshFileName.LastIndexOf('.')), entityName);
                        }
                    }
                    catch (System.Xml.XPath.XPathException)
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
                                Destroy.Destroy(newPrefab.collider);
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
                        }

                    }
                    catch (System.Xml.XPath.XPathException)
                    {
                        TDLPlugin.DebugOutput("phys xml error");
                        //Nope, no physics
                    }

                    //Reassign the prefab
                    newEnt.prefab = newPrefab;
                }
            }
        }

        public static void objLoaded(GameObject[] loaded)
        {
            try
            {
                TDLPlugin.DebugOutput(objToEntity[loaded[0].name] + " Loaded");
               
                //Assign the loaded components
                Entity.GetEntityByName(objToEntity[loaded[0].name]).prefab.GetComponent<MeshFilter>().mesh = loaded[0].GetComponent<MeshFilter>().mesh;
                Entity.GetEntityByName(objToEntity[loaded[0].name]).prefab.GetComponent<MeshRenderer>().material = loaded[0].GetComponent<MeshRenderer>().material;

                //Destroy the created GameObject
                Destroy.Destroy(loaded[0]);

                //If we have more than 1 object to load, start loading again
                if(loadList.Count > 0)
                    loadTimer.Enabled = true;
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

        #endregion

        #region Categories
            private void loadCategories(string workingPath)
            {
                workingPath += "textassets";

                if(Directory.Exists(workingPath))
                {
                    if(File.Exists(workingPath + "/categories.xml"))
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
                            SubCategory newSub;
                            //Additions
                            XmlNodeList addList = catList[catCount].SelectNodes("add/*");
                            for(int addCount = 0; addCount < addList.Count; addCount++)
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
                    }
                }
            }
        #endregion
    }
}
