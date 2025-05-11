using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lb_12.Interfaces;
using lb_12.Classes;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.ComponentModel;

namespace lb_12;

public static class ContainerSerializer
{
    public static string SerializeContainer(object container, string name)
    {
        using FileStream stream = new FileStream($"{name}.bin", FileMode.Create);
        using BinaryWriter writer = new BinaryWriter(stream);

        int count;
        string containerType;
        IEnumerable<IName> _container;
        
        switch (container)
        {
            case Container<IName> array:
                count = array.GetCount();
                _container = array;
                containerType = typeof(Container<IName>).Name;
                break;
            case ContainerLinkedList<IName> linkedList:
                count = linkedList.GetCount();
                _container = linkedList;
                containerType = typeof(ContainerLinkedList<IName>).Name;
                break;
            default:
                throw new ArgumentException("Container is None. Please select a container.");
        }

        writer.Write(containerType.Split('`')[0]);
        writer.Write(count);

        foreach (var item in _container)
        {
            if (item is ICustomSerializable serializable)
            {
                writer.Write(item.GetType().Name);
                serializable.Serialize(writer);
            }
            else
            {
                throw new InvalidOperationException($"Type {item.GetType().Name} isn't serializable");
            }
        }

        stream.Close();
        return stream.Name;
    }

    static readonly Dictionary<string, Func<BinaryReader, IName>> Deserializers = new()
    {
        { "Product", reader => Product.Deserialize(reader) },
        { "RealEstate", reader => RealEstate.Deserialize(reader) },
        { "RealEstateInvestment", reader => RealEstateInvestment.Deserialize(reader) },
        { "Apartment", reader => Apartment.Deserialize(reader) },
        { "House", reader => House.Deserialize(reader) },
        { "Hotel", reader => Hotel.Deserialize(reader) },
        { "LandPlot", reader => LandPlot.Deserialize(reader) }
    };

    public static IEnumerable<IName?> DeserializeContainer(string name)
    {
        string filePath = $"{name}.bin";
        if (!File.Exists(filePath)) throw new FileNotFoundException($"File '{filePath}' doesn't exist");

        using FileStream stream = new FileStream(filePath, FileMode.Open);
        using BinaryReader reader = new BinaryReader(stream);

        string containerType = reader.ReadString();
        int count = reader.ReadInt32();

        var containerFactories = new Dictionary<string, Func<dynamic>>
        {
            { "Container", () => new Container<IName>() },
            { "ContainerLinkedList", () => new ContainerLinkedList<IName>() },
        };

        if (!containerFactories.TryGetValue(containerType, out var containerFactory))
            throw new InvalidDataException($"Unknown container type {containerType}");

        dynamic container = containerFactory();

        for (int i = 0; i < count; i++)
        {
            string typeName = reader.ReadString();

            if (Deserializers.TryGetValue(typeName, out var deserializer))
            {
                var item = deserializer(reader);
                container.Add(item);
            }
        }

        stream.Close();
        return container;

    }
}
