using IRT.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IRT.ConsoleOutput
{
    public class PropertyEditor
    {
        private readonly DisplayManager _displayManager;
        
        

        public PropertyEditor(DisplayManager displayManager, ContainerManager containerManager) 
        {
            _displayManager = displayManager;
            
        }

        public void ModifyProperty(IName itemToModify, int? itemZeroBasedInsertionId = null) 
        {
            ArgumentNullException.ThrowIfNull(itemToModify);

            var properties = itemToModify.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetSetMethod(true) != null && p.Name != "Item" /* Indexer property */)
                .ToList();

            if (properties.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("This object has no publicly writable properties.");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nChoose property to modify:");
            Console.ResetColor();
            for (int i = 0; i < properties.Count; i++)
            {
                object? currentValue = "?";
                try { currentValue = properties[i].GetValue(itemToModify); } catch { /* Ignore */ }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{i + 1}. {properties[i].Name} (Type: {properties[i].PropertyType.Name}, Current: '{currentValue ?? "null"}')");
                Console.ResetColor();
            }

            try
            {
                int propChoice = InputReader.ReadInt($"Enter choice (1 to {properties.Count}): ", 1, properties.Count);
                PropertyInfo selectedProperty = properties[propChoice - 1];
                Type propertyType = selectedProperty.PropertyType;
                Type underlyingType = Nullable.GetUnderlyingType(propertyType);
                string targetTypeName = (underlyingType ?? propertyType).Name;
                bool isNullable = underlyingType != null;

                string prompt = $"Enter new value for {selectedProperty.Name} (Type: {targetTypeName}{(isNullable ? ", or empty for null" : "")}): ";

                
                
                string newValueString = InputReader.ReadString(prompt, allowNullOrEmpty: isNullable);


                if (ReflectionHelper.TryParseAndSetValue(itemToModify, selectedProperty, newValueString))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    string idMsg = itemZeroBasedInsertionId.HasValue ? $"(Insertion ID: {itemZeroBasedInsertionId.Value + 1})" : "(ID not specified)";
                    Console.WriteLine($"\nProperty '{selectedProperty.Name}' updated successfully for item {idMsg}.");
                    Console.WriteLine("New item details:");
                    Console.ResetColor();

                    
                    _displayManager.DisplayItemTable(itemZeroBasedInsertionId.HasValue ? itemZeroBasedInsertionId.Value + 1 : 1, itemToModify);
                }
            }
            catch (FormatException) { /* Error already printed by InputReader or ReflectionHelper */ }
            catch (ValueLessThanZero) { /* Error already printed by InputReader or setter */ }
            
        }
    }
}