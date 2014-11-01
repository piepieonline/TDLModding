using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.IO;

namespace TDLTestBed
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        public static void loadCategoryFile(TextAsset a)
        {
            XmlDocument xmlDocument = new XmlDocument();
            XMLHelpers.currentFileName = a.name;
            try
            {
                if (!File.Exists(string.Concat(FileOperations.getWorldStoragePathPrefix(), "/", a.name, ".xml")))
                {
                    xmlDocument.LoadXml(a.text);
                }
                else
                {
                    xmlDocument.Load(string.Concat(FileOperations.getWorldStoragePathPrefix(), "/", a.name, ".xml"));
                }
            }
            catch (XmlException xmlException1)
            {
                XmlException xmlException = xmlException1;
                PopupDialog.showDeferredBootError(string.Concat("Error parsing category file ", a.name, " : ", xmlException.Message));
                DebugConsole.LogError(string.Concat("Error parsing category file ", a.name, " : ", xmlException.Message));
            }
            System.Collections.IEnumerator enumerator = xmlDocument.ChildNodes.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    XmlNode current = (XmlNode)enumerator.Current;
                    if (current.Name != "categories")
                    {
                        continue;
                    }
                    System.Collections.IEnumerator enumerator1 = current.ChildNodes.GetEnumerator();
                    try
                    {
                        while (enumerator1.MoveNext())
                        {
                            XmlNode xmlNodes = (XmlNode)enumerator1.Current;
                            if (xmlNodes.Name != "category")
                            {
                                continue;
                            }
                            CategoryReader.loadCategory(a.name, xmlNodes);
                        }
                    }
                    finally
                    {
                        IDisposable disposable = enumerator1 as IDisposable;
                        if (disposable == null)
                        {
                        }
                        disposable.Dispose();
                    }
                }
            }
            finally
            {
                IDisposable disposable1 = enumerator as IDisposable;
                if (disposable1 == null)
                {
                }
                disposable1.Dispose();
            }
        }


        public void loadTable()
        {
            Entity.entities.Clear();
            XMLHelpers.currentFileName = this.entityFile.name;
            this.loadFromString((string)TDLHookClass.LoadTDLHook(new object[] { "loadEntityTable" }));
            XMLHelpers.currentFileName = this.entityAimFile.name;
            if (EntityTable.ensureAimXmlFileName())
            {
                try
                {
                    string str = File.ReadAllText(EntityTable.aimXmlFileFullName);
                    if (str.Length > 0)
                    {
                        TDLLogging.LogRuntimeInfo("entity_table_aim_1", string.Concat("Loaded aim.xml data from SVN project location. ", EntityTable.aimXmlFileFullName));
                        this.loadAimFromString(str);
                        return;
                    }
                }
                catch
                {
                }
            }
            TDLLogging.LogRuntimeInfo("entity_table_aim_2", "Loaded aim.xml data from normal TDL.exe bundle.");
            this.loadAimFromString(this.entityAimFile.text);
        }

        #region loadTable Helpers
        TextAsset entityFile;
        TextAsset entityAimFile;

        void loadAimFromString(string a)
        { }

        void loadFromString( string a )
        { }
        #endregion
    }

    class TextAsset
    {
        public string name { get; set; }
        public string text { get; set; }
    }
}
