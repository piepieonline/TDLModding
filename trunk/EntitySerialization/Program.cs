using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Serialization;

namespace EntitySerialization
{
    class Program
    {
        static void Main(string[] args)
        {
            Entity e1 = new Entity("fred", 1);
            string n = "1";

            XmlSerializer ser = new XmlSerializer(e1.GetType());
            ser.Serialize(File.CreateText("D:\\streamed" + n + ".xml"), e1);
        }

        string noStore()
        {
            return "a";
        }

        string store()
        {
            string a = "a";
            return a;
        }
    }

    public class Entity
    {
        public string name;
        public int size;

        public Entity()
        {}

        public Entity( string n, int s )
        {
            name = n;
            size = s;
        }

        public string getName()
        {
            return name;
        }

        public void setName( string n )
        {
            name = n;
        }
    }
}
