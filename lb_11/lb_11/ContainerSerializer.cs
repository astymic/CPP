using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lb_11.Interfaces;
using lb_11.Classes;
using System.Reflection;
using System.Reflection.PortableExecutable;

namespace lb_11;

public static class ContainerSerializer
{
    public static string SerializeContainer(object container, string name)
    {
        using FileStream stream = new FileStream($"{name}.bin", FileMode.Create);
        using BinaryWriter writer = new BinaryWriter(stream);

        int count;
        IEnumerable<IName> _container;
        
        switch (container)
        {
            case Container<IName> array:
                count = array.GetCount();
                _container = array;
                break;
            case ContainerLinkedList<IName> linkedList:
                count = linkedList.GetCount();
                _container = linkedList;
                break;
            default:
                throw new ArgumentException("Container is None. Please select a container.");
        }

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
        using FileStream stream = new FileStream($"{name}.bin", FileMode.Open);
        using BinaryReader reader = new BinaryReader(stream);

        int count = reader.ReadInt32();

        var container = new Container<IName>();

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
