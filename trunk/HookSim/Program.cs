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
            injectCode();

            Console.ReadLine();
        }
        
        public static void injectCode()
        {
            Program.LoadTDLHook(new System.Object[] { "Resources" });
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

        public static void ShowModsMenu()
        {
            LoadTDLHook(new object[] { "ShowModMenu" });
        }
    }
}
