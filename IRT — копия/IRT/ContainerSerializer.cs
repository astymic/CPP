using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRT.Interfaces;
using IRT.Classes;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.ComponentModel;

namespace IRT;

public static class ContainerSerializer
{
    public static string SerializeContainer(object container, string name)
    {
        using FileStream stream = new FileStream($"{name}.bin", FileMode.Create);
        using BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(container.GetType().Name.Split('`')[0]);

        (container as dynamic).Serialize(writer);

        stream.Close();
        return stream.Name;
    }

    public static object DeserializeContainer(string name)
    {
        string filePath = $"{name}.bin";
        using FileStream stream = new FileStream(filePath, FileMode.Open);
        using BinaryReader reader = new BinaryReader(stream);

        string containerType = reader.ReadString();

        if (containerType == "Container")
            return Container<Product>.Deserialize(reader);
        else if (containerType == "ContainerLinkedList")
            return ContainerLinkedList<Product>.Deserialize(reader);
        else
            throw new InvalidDataException($"Unknown container type {containerType}");
    }
}