using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using lb_13.Interfaces; 

namespace lb_13
{
    public class ContainerJsonConverter<T> : JsonConverter<Container<T>> where T : class, IName
    {
        public override Container<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token");
            }

            var container = new Container<T>(); 

            var itemsField = typeof(Container<T>).GetField("items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var insertionOrderField = typeof(Container<T>).GetField("insertionOrder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var countField = typeof(Container<T>).GetField("count", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sizeField = typeof(Container<T>).GetField("size", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nextInsertionIdField = typeof(Container<T>).GetField("nextInsertionId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var totalPriceField = typeof(Container<T>).GetField("totalPrice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (itemsField == null || insertionOrderField == null || countField == null || sizeField == null || nextInsertionIdField == null || totalPriceField == null)
            {
                throw new InvalidOperationException("Could not find one or more required private fields in Container<T> via reflection.");
            }

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    container.PostDeserializeInitialize();
                    return container;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString() ?? throw new JsonException("Expected property name string");
                    reader.Read(); 

                    switch (propertyName)
                    {
                        case "items": 
                            var items = JsonSerializer.Deserialize<T?[]>(ref reader, options);
                            itemsField.SetValue(container, items);
                            break;
                        case "insertionOrder":
                            var insertionOrder = JsonSerializer.Deserialize<int[]>(ref reader, options);
                            insertionOrderField.SetValue(container, insertionOrder);
                            break;
                        case "count":
                            countField.SetValue(container, reader.GetInt32());
                            break;
                        case "size":
                            sizeField.SetValue(container, reader.GetInt32());
                            break;
                        case "nextInsertionId":
                            nextInsertionIdField.SetValue(container, reader.GetInt32());
                            break;
                        case "totalPrice":
                            totalPriceField.SetValue(container, reader.GetDecimal());
                            break;
                        default:
                            break;
                    }
                }
            }
            throw new JsonException("Expected EndObject token");
        }

        public override void Write(Utf8JsonWriter writer, Container<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            var items = (T?[])typeof(Container<T>).GetField("items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(value)!;
            var insertionOrder = (int[])typeof(Container<T>).GetField("insertionOrder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(value)!;
            int count = (int)typeof(Container<T>).GetField("count", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(value)!;
            int size = (int)typeof(Container<T>).GetField("size", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(value)!;
            int nextInsertionId = (int)typeof(Container<T>).GetField("nextInsertionId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(value)!;
            decimal totalPrice = (decimal)typeof(Container<T>).GetField("totalPrice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(value)!;

            T?[] actualItems = new T?[count];
            Array.Copy(items, actualItems, count);
            int[] actualInsertionOrder = new int[count];
            Array.Copy(insertionOrder, actualInsertionOrder, count);

            writer.WritePropertyName("items");
            JsonSerializer.Serialize(writer, actualItems, options);

            writer.WritePropertyName("insertionOrder");
            JsonSerializer.Serialize(writer, actualInsertionOrder, options);

            writer.WriteNumber("count", count);
            writer.WriteNumber("size", size);
            writer.WriteNumber("nextInsertionId", nextInsertionId);
            writer.WriteNumber("totalPrice", totalPrice);

            writer.WriteEndObject();
        }
    }
}