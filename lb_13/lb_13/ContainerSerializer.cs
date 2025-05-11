using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using lb_13.Interfaces;

namespace lb_13
{
    public static class ContainerSerializer
    {
        private static JsonSerializerOptions GetJsonSerializerOptions(Type containerActualType)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = false, 
            };

            if (containerActualType.IsGenericType)
            {
                Type genericTypeDef = containerActualType.GetGenericTypeDefinition();
                Type genericArg = containerActualType.GetGenericArguments()[0];

                if (genericTypeDef == typeof(Container<>))
                {
                    Type converterType = typeof(ContainerJsonConverter<>).MakeGenericType(genericArg);
                    options.Converters.Add((JsonConverter)Activator.CreateInstance(converterType)!);
                }
                else if (genericTypeDef == typeof(ContainerLinkedList<>))
                {
                    Type converterType = typeof(ContainerLinkedListJsonConverter<>).MakeGenericType(genericArg);
                    options.Converters.Add((JsonConverter)Activator.CreateInstance(converterType)!);
                }
            }
            return options;
        }

        public static string SerializeContainer(object container, string name)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container), "Container to serialize cannot be null.");
            }

            string filePath = $"{name}.json";

            using FileStream stream = new FileStream(filePath, FileMode.Create);
            using BinaryWriter writer = new BinaryWriter(stream);

            string containerTypeFullName = container.GetType().AssemblyQualifiedName ??
                                           throw new InvalidOperationException("Could not get AssemblyQualifiedName for container type.");
            writer.Write(containerTypeFullName);
            writer.Flush();

            JsonSerializer.Serialize(stream, container, container.GetType(), GetJsonSerializerOptions(container.GetType()));

            stream.Close();
            return filePath;
        }

        public static IEnumerable<IName?>? DeserializeContainer(string name)
        {
            string filePath = $"{name}.json";
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File '{filePath}' doesn't exist.", filePath);
            }

            using FileStream stream = new FileStream(filePath, FileMode.Open);
            using BinaryReader reader = new BinaryReader(stream);

            string containerTypeFullName = reader.ReadString();
            Type? containerType = Type.GetType(containerTypeFullName);

            if (containerType == null)
            {
                throw new InvalidDataException($"Unknown or invalid container type name found in file: {containerTypeFullName}");
            }

            object? deserializedObject = JsonSerializer.Deserialize(stream, containerType, GetJsonSerializerOptions(containerType));


            if (deserializedObject is IEnumerable<IName> result)
            {
                return result;
            }

            if (deserializedObject == null && stream.Length > (stream.Position - reader.BaseStream.Position) + 10)
            {
                throw new InvalidOperationException($"Deserialization resulted in a null object, but stream data seemed present for type {containerType.Name}. Check JSON content and class definitions.");
            }
            return null;
        }
    }
}