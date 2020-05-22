using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Serialize
{
    class Program
    {
        static void Main(string[] args)
        {
            string a = "mamba";
            object obj = a;
            byte[] bytes = SerializeObj(obj);

            object desObj = DeserializeObj(bytes);
            Console.WriteLine(desObj.ToString());
        }

        //serialize
        public static byte[] SerializeObj(object obj)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, obj);

                byte[] bytes = stream.ToArray();
                stream.Flush();

                return bytes;
            }
        }

        //deserialize
        public static object DeserializeObj(byte[] binaryObj)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(binaryObj))
            {
                stream.Position = 0;
                object desObj = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(stream);
                return desObj;
            }
        }
    }
}