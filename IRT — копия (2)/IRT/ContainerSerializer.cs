using System;
using System.IO;
using System.Linq;
using IRT.Interfaces;

namespace IRT
{
    public static class ContainerSerializer
    {
        public static void SerializeContainer<T>(object container, string filename)
            where T : IName, IName<T>, IPrice, ICustomSerializable
        {
            using var fs = new FileStream(filename, FileMode.Create);
            using var writer = new BinaryWriter(fs);

            if (container is Container<T> arrayContainer)
            {
                writer.Write("Array"); 
                arrayContainer.Serialize(writer);
            }
            else if (container is ContainerLinkedList<T> linkedListContainer)
            {
                writer.Write("LinkedList"); 
                linkedListContainer.Serialize(writer);
            }
            else
            {
                throw new ArgumentException("Unknown container type!");
            }
        }

        public static object DeserializeContainer<T>(string filename)
            where T : IName, IName<T>, IPrice, ICustomSerializable
        {
            using var fs = new FileStream(filename, FileMode.Open);
            using var reader = new BinaryReader(fs);

            string containerType = reader.ReadString();
            if (containerType == "Array")
                return Container<T>.Deserialize(reader);
            else if (containerType == "LinkedList")
                return ContainerLinkedList<T>.Deserialize(reader);
            else
                throw new InvalidOperationException("Unknown container type in file");
        }
    }
}
