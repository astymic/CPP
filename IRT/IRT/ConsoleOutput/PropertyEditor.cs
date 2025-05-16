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
        private readonly ContainerManager _containerManager;

        public PropertyEditor(DisplayManager displayManager, ContainerManager containerManager)
        {
            _displayManager = displayManager;
            _containerManager = containerManager;
        }

        public void ModifyProperty(IName itemToModify, int itemZeroBasedInsertionId)
        {
            ArgumentNullException.ThrowIfNull(itemToModify);

            var properties = itemToModify.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetSetMethod(true) != null && p.Name != "Item")
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
                    Console.WriteLine($"\nProperty '{selectedProperty.Name}' updated successfully for item (Insertion ID: {itemZeroBasedInsertionId + 1}).");
                    Console.WriteLine("New item details:");
                    Console.ResetColor();

                    int currentIndex = _containerManager.FindIndexByReferenceInActive(itemToModify);
                    if (currentIndex != -1)
                    {
                        _displayManager.DisplayItemTable(currentIndex + 1, itemToModify);
                    }
                    else
                    {
                        _displayManager.DisplayItemTable(itemZeroBasedInsertionId + 1, itemToModify);
                        ConsoleUI.PrintErrorMessage("Could not determine current index after modification for display. Displayed with Insertion ID.");
                    }
                }
            }
            catch (FormatException) { /* Error already printed by InputReader */ }
            catch (ValueLessThanZero) { /* Error already printed by InputReader */ }
        }
    }
}