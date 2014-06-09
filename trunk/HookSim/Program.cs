using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Runtime.Remoting;

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
            Program.LoadTDLHook(new Object[] { "Resources" });
        }

        public static object LoadTDLHook(System.Object[] args)
        {
            String codebase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring(8);
            
            codebase = codebase.Substring( 0, codebase.LastIndexOf("/") );

            String assemblyName = codebase + "/TDLHookLib.dll";
            String typeName = "TDLHookLib.TDLPlugin";
            String methodName = "HookTDL";

            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(assemblyName);
            Type type = assembly.GetType(typeName);

            System.Reflection.MethodInfo methodInfo = type.GetMethod(methodName);

            System.Reflection.ParameterInfo[] parameters = methodInfo.GetParameters();
            object classInstance = Activator.CreateInstance(type, null);

            return methodInfo.Invoke(classInstance, new Object[] { args });
        }
    }
}
