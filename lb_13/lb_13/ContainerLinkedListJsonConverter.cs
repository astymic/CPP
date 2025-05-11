using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using lb_13.Interfaces; 

namespace lb_13
{
    public class ContainerLinkedListJsonConverter<T> : JsonConverter<ContainerLinkedList<T>> where T : class, IName
    {
        private static void SetPrivateField(object obj, string fieldName, object? value)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null) throw new MissingFieldException(obj.GetType().Name, fieldName);
            field.SetValue(obj, value);
        }

        private static object? GetPrivateField(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null) throw new MissingFieldException(obj.GetType().Name, fieldName);
            return field.GetValue(obj);
        }

        public override ContainerLinkedList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token for ContainerLinkedList.");
            }

            var container = new ContainerLinkedList<T>();

            List<T>? deserializedItemsList = null; 
            List<int>? insertionOrderList = null;  
            int deserializedCount = 0;             
            int deserializedNextInsertionId = 0;   
            decimal deserializedTotalPrice = 0;    

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (deserializedItemsList != null && insertionOrderList != null)
                    {
                        SetPrivateField(container, "_count", deserializedCount);
                        SetPrivateField(container, "NextInsertionId", deserializedNextInsertionId);
                        SetPrivateField(container, "totalPrice", deserializedTotalPrice);
                        SetPrivateField(container, "InsertionOrder", insertionOrderList); 

                        ContainerLinkedList<T>.Node<T>? headNode = null; 
                        ContainerLinkedList<T>.Node<T>? currentTail = null; 

                        foreach (var itemData in deserializedItemsList)
                        {
                            var newNode = new ContainerLinkedList<T>.Node<T>(itemData);
                            if (headNode == null)
                            {
                                headNode = newNode;
                                currentTail = headNode;
                            }
                            else
                            {
                                currentTail!.Next = newNode;
                                newNode.Previous = currentTail;
                                currentTail = newNode;
                            }
                        }
                        SetPrivateField(container, "_head", headNode);
                    }

                    container.PostDeserializeInitialize();
                    return container;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString()!;
                    reader.Read(); 

                    switch (propertyName)
                    {
                        case "ItemsData":
                            deserializedItemsList = JsonSerializer.Deserialize<List<T>>(ref reader, options);
                            break;
                        case "InsertionOrder":
                            insertionOrderList = JsonSerializer.Deserialize<List<int>>(ref reader, options);
                            break;
                        case "Count":
                            deserializedCount = reader.GetInt32();
                            break;
                        case "NextInsertionId":
                            deserializedNextInsertionId = reader.GetInt32();
                            break;
                        case "TotalPrice":
                            deserializedTotalPrice = reader.GetDecimal();
                            break;
                        default:
                            reader.Skip(); 
                            break;
                    }
                }
            }
            throw new JsonException("Expected EndObject token for ContainerLinkedList.");
        }

        public override void Write(Utf8JsonWriter writer, ContainerLinkedList<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("ItemsData");
            JsonSerializer.Serialize(writer, value.NodeToList(), options);

            writer.WritePropertyName("InsertionOrder");
            var insertionOrder = (List<int>?)GetPrivateField(value, "InsertionOrder");
            JsonSerializer.Serialize(writer, insertionOrder ?? new List<int>(), options);


            writer.WriteNumber("Count", value.Count); 
            var nextId = GetPrivateField(value, "NextInsertionId");
            if (nextId is int intNextId) writer.WriteNumber("NextInsertionId", intNextId); else writer.WriteNull("NextInsertionId");

            writer.WriteNumber("TotalPrice", value.TotalPrice); 

            writer.WriteEndObject();
        }
    }
}