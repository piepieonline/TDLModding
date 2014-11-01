using System;
using System.Collections.Generic;
using System.Text;

using System.Reflection;
using System.Runtime.Remoting;

using UnityEngine;


namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            demoMethod();

            Console.ReadLine();
        }

        void a()
        {
            _paneState s = _paneState.MULTIPLAYER;
            switch (s)
            {
                case _paneState.SPLASH:
                    break;
                case _paneState.NEWS:
                    break;
                case _paneState.SOLO:
                    break;
                case _paneState.MULTIPLAYER:
                    break;
                case _paneState.SETTINGS:
                    break;
                case _paneState.MODS:
                    break;
                default:
                    return;
            }
        }


        static GameObject prefab;
        public static void demoMethod() {
            prefab = LoadPrefab("candle_unlit_prefab");
        }


        public static void LoadEntityTable()
        {
            LoadTDLHook(new object[] { "loadEntityTable" });
        }


        private static object hookMethod = null;
        private static MethodInfo method = null;

        public static object LoadTDLHook(object[] args)
        {
            if (hookMethod == null)
            {
                string str = Assembly.GetExecutingAssembly().CodeBase.Substring(8);
                str = str.Substring(0, str.LastIndexOf("/"));
                string str1 = string.Concat((object)str, (object)"/TDLHookLib.dll");
                string str2 = "TDLHookLib.TDLPlugin";
                string str3 = "HookTDL";
                Type type = Assembly.LoadFile(str1).GetType(str2);
                method = type.GetMethod(str3);
                method.GetParameters();
                hookMethod = Activator.CreateInstance(type, null);
            }
            return method.Invoke(hookMethod, new object[] { args });
        }

        public static IBodyMenu ShowModsMenu()
        {
            return (IBodyMenu)LoadTDLHook(new object[] { "ShowModMenu" });
        }

        public static GameObject LoadPrefab(String preName)
        {
            return (GameObject)LoadTDLHook(new object[] { "LoadPrefab", preName });
        }
    }
}


public class IBodyMenu : MonoBehaviour
{ }

public enum _paneState
{
    SPLASH,
    NEWS,
    SOLO,
    MULTIPLAYER,
    SETTINGS,
    MODS
}