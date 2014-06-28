/*
 * Settings controller
 * Piepieonline
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Serialization;
/*
 * XML Serialization from:
 * http://web.archive.org/web/20100703052446/http://blogs.msdn.com/b/psheill/archive/2005/04/09/406823.aspx
*/

namespace TDLModdingTools
{
    class Settings
    {
        private static Settings set = null;

        private string path;

        private Dictionary<string, string> setValues = new Dictionary<string, string>();

        public Settings(string _startupPath)
        {
            if (set != null)
                throw new Exception("Only one settings object may exist");

            set = this;

            path = _startupPath + "/settings.xml";

            if (!File.Exists(path))
            {
                createDefaultSettings();
            }
            else
            {
                LoadSettings();
            }
        }

        public static Settings Singleton()
        {
            return set;
        }

        public bool isSetup()
        {
            if (setValues["TDL_Path"] == "")
                return false;
            if (setValues["DefaultStart"] == "MSIL" && setValues["IL_DASM_Path"] == "")
                return false;

            return true;
        }

        public string getSetting(string settingName)
        {
            return setValues[settingName];
        }

        public bool setSetting(string settingName, string value, bool save = true)
        {
            //We only want to be able to change the settings here, not in the rest of the program
            if (!setValues.ContainsKey(settingName))
                return false;

            setValues[settingName] = value;

            if (save)
                SaveSettings();

            return true;
        }

        private void createDefaultSettings()
        {
            setValues.Add("TDL_Path", @"");
            setValues.Add("IL_ASM_Path", @"C:\Windows\Microsoft.NET\Framework\v2.0.50727\ilasm.exe");
            setValues.Add("IL_DASM_Path", @"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\ildasm.exe");
            setValues.Add("IL_PEVerify_Path", @"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\PEVerify.exe");
            setValues.Add("DefaultStart", @"Mod Compiler");
            SaveSettings();
        }

        private void SaveSettings()
        {
            TextWriter writer = new StreamWriter(path);
            List<SettingsEntry> entries = new List<SettingsEntry>(setValues.Count);
            foreach(string key in setValues.Keys)
            {
                entries.Add(new SettingsEntry(key, setValues[key]));
            }
            XmlSerializer serial = new XmlSerializer(typeof(List<SettingsEntry>));
            serial.Serialize(writer, entries);
            writer.Close();
        }

        private void LoadSettings()
        {
            TextReader reader = new StreamReader(path);
            setValues.Clear();
            XmlSerializer serial = new XmlSerializer(typeof(List<SettingsEntry>));
            List<SettingsEntry> list = (List<SettingsEntry>)serial.Deserialize(reader);

            foreach(SettingsEntry entry in list)
            {
                setValues[entry.key] = entry.value;
            }
            reader.Close();
        }
    }

    //So we can serialise
    public class SettingsEntry
    {
        public string key;
        public string value;

        public SettingsEntry()
        { }

        public SettingsEntry(string _key, string _value)
        {
            key = _key;
            value = _value;
        }
    }
}
