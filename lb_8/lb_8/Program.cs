using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using lb_8.Classes;
using lb_8.Interfaces;

namespace lb_8;


class Program
{
    static void Main()
    {
        // Instantiate the generic container for types implementing IName
        Container<IName> container = new Container<IName>();
        Random random = new Random();

        while (true)
        {
            PrintMenu(); // Extracted menu printing logic
            string choice = Console.ReadLine()?.ToLower() ?? ""; // Handle null ReadLine

            try
            {
                switch (choice)
                {
                    case "1": HandleAutomaticGeneration(container, random); break;
                    case "2": HandleManualInput(container); break;
                    case "3": HandleShowContainer(container); break;
                    case "4": HandleGetElementByInsertionId(container); break;
                    case "5": HandleGetElementByName(container); break;
                    case "6": HandleChangeItemByInsertionId(container); break;
                    case "7": HandleChangeItemByName(container); break;
                    case "8": HandleSortContainer(container); break;
                    case "9": HandleRemoveElementByIndex(container); break;
                    case "q":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Exiting...");
                        Console.ResetColor();
                        return;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid choice. Please try again.");
                        Console.ResetColor();
                        break;
                }
            }
            // Specific exception first
            catch (ValueLessThanZero ex)
            {
                PrintErrorMessage($"Input/Validation Error: {ex.Message}");
            }
            catch (FormatException ex)
            {
                PrintErrorMessage($"Input Format Error: Invalid format entered. {ex.Message}");
            }
            catch (IndexOutOfRangeException ex)
            {
                PrintErrorMessage($"Error: Index out of range. {ex.Message}");
            }
            catch (KeyNotFoundException ex) // For indexer errors
            {
                PrintErrorMessage($"Error: Key (e.g., Insertion ID) not found. {ex.Message}");
            }
            catch (ArgumentException ex) // Catches ArgumentNullException, ArgumentOutOfRangeException etc.
            {
                PrintErrorMessage($"Argument Error: {ex.Message}");
            }
            catch (TargetInvocationException ex) // Catches exceptions from invoked methods/properties
            {
                PrintErrorMessage($"Error during operation: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex) // Generic catch-all
            {
                PrintErrorMessage($"An unexpected error occurred: {ex.GetType().Name} - {ex.Message}");
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }

    // --- Menu Printing ---
    static void PrintMenu()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n------ Menu ------");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("1. Automatic Generation");
        Console.WriteLine("2. Manual Input");
        Console.WriteLine("3. Show Container");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- ### ### ### ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("4. Get Element by Insertion ID (1-based)");
        Console.WriteLine("5. Get Element by Name");
        // Console.WriteLine("6. Get Elements by Price"); 
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- ### ### ### ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("6. Change Item by Insertion ID (1-based)"); 
        Console.WriteLine("7. Change Item by Name");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- ### ### ### ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("8. Sort Container by Price");
        Console.WriteLine("9. Remove Element by Current Index (0-based)"); 
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- ### ### ### ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("q. Exit");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter your choice: ");
        Console.ResetColor();
    }

    // --- Error Message Printing ---
    static void PrintErrorMessage(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n{message}");
        Console.ResetColor();
    }

    // --- Menu Handlers ---
    static void HandleAutomaticGeneration(Container<IName> container, Random random)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- Automatic Generation ---");
        Console.ResetColor();
        Console.Write("Enter number of elements to generate: ");
        if (int.TryParse(Console.ReadLine(), out int count) && count > 0)
        {
            AutomaticGeneration(container, random, count);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nAutomatic generation of {count} elements complete.");
            Console.ResetColor();
            DemonstrateIndexers(container, random);
        }
        else
        {
            PrintErrorMessage("Invalid input for count (must be a positive integer). Generation cancelled.");
        }
    }

    static void HandleManualInput(Container<IName> container)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- Manual Input ---");
        Console.ResetColor();
        ManualInput(container);
    }

    static void HandleShowContainer(Container<IName> container)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- Show Container ---");
        Console.ResetColor();
        ShowContainer(container);
    }

    static void HandleGetElementByInsertionId(Container<IName> container)
    {
        GetElementByInsertionId(container);
    }

    static void HandleGetElementByName(Container<IName> container)
    {
        GetElementByName(container);
    }

    static void HandleChangeItemByInsertionId(Container<IName> container)
    {
        ChangeItemByInsertionId(container);
    }

    static void HandleChangeItemByName(Container<IName> container)
    {
        ChangeItemByName(container);
    }

    static void HandleSortContainer(Container<IName> container)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- Sorting Container by Price ---");
        Console.ResetColor();
        if (container.GetCount() > 0)
        {
            container.Sort(); 
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Container sorted.");
            Console.ResetColor();
            ShowContainer(container); // Show result
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Container is empty. Nothing to sort.");
            Console.ResetColor();
        }
    }

    static void HandleRemoveElementByIndex(Container<IName> container)
    {
        RemoveElementByIndex(container);
    }


    // --- Indexer Interaction Methods ---

    static void GetElementByInsertionId(Container<IName> container)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- Get Element by Insertion ID ---");
        Console.ResetColor();
        if (container.IsEmpty()) return;

        int maxId = container.GetInsertionId(); // Get the upper bound for valid IDs
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Enter insertion ID (1 to {maxId}): ");
        Console.ResetColor();

        if (int.TryParse(Console.ReadLine(), out int id) && id - 1 >= 0 && id - 1 < maxId)
        {
            IName? item = container[id - 1]; // Use the insertion ID indexer

            if (item == null)
            {
                PrintErrorMessage($"Item with insertion ID {id} not found (possibly removed).");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nElement with insertion ID {id}:");
            Console.ResetColor();
            DisplayItemTable(id, item); // Use helper to display in table
        }
        else
        {
            PrintErrorMessage($"Invalid input. Please enter a valid integer ID between 1 and {maxId}.");
        }
    }

    static void GetElementByName(Container<IName> container)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- Get Elements by Name ---");
        Console.ResetColor();
        if (container.IsEmpty()) return;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter the Name to search for: ");
        Console.ResetColor();
        string name = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            PrintErrorMessage("Invalid input. Name cannot be empty.");
            return;
        }

        IName[]? itemsFound = container[name]; // Use name 

        if (itemsFound != null && itemsFound.Length > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nFound {itemsFound.Length} element(s) with Name '{name}':");
            Console.ResetColor();

            // Display results in a table
            int tableWidth = CalculateTableWidth();
            PrintTableHeader(tableWidth);

            IName[] allItems = container.GetItems(); // Cache current items
            int currentIds = GetIds(container); // Cache current IDs

            foreach (var foundItem in itemsFound)
            {
                int ID = FindId(foundItem, allItems, currentIds); // Find ID efficiently FindInsertionId
                if (ID != -1)
                {
                    WriteDataRowById(ID, foundItem, tableWidth); // Pass width for padding
                }
                else
                {
                    // This is unlikely if item came from container, but handle defensively
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"| Warning: Could not map found item '{foundItem.ToString()?.Substring(0, 20)}...'.".PadRight(tableWidth - 1) + "|");
                    Console.ResetColor();
                }
                DrawHorizontalLine(tableWidth);
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"No elements found with Name '{name}'.");
            Console.ResetColor();
        }
    }

    static void ChangeItemByInsertionId(Container<IName> container)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- Change Item by Insertion ID ---");
        Console.ResetColor();
        if (container.IsEmpty()) return;

        int maxId = container.GetInsertionId();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Enter item insertion ID to modify (0 to {maxId - 1}): ");
        Console.ResetColor();

        if (int.TryParse(Console.ReadLine(), out int id) && id >= 0 && id < maxId)
        {
            IName? itemToModify = container[id]; // Get item using indexer

            if (itemToModify == null)
            {
                PrintErrorMessage($"Item with insertion ID {id} not found (possibly removed).");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nCurrent item details:");
            Console.ResetColor();
            DisplayItemTable(id, itemToModify); // Show current state

            ModifyProperty(itemToModify, id); // Proceed to modify, passing ID for context
        }
        else
        {
            PrintErrorMessage($"Invalid input. Please enter a valid integer ID between 0 and {maxId - 1}.");
        }
    }

    static void ChangeItemByName(Container<IName> container)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- Change Item by Name ---");
        Console.ResetColor();
        if (container.IsEmpty()) return;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter the Name of the item(s) to modify: ");
        Console.ResetColor();
        string name = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            PrintErrorMessage("Invalid input. Name cannot be empty.");
            return;
        }

        IName[]? itemsFound = container[name]; // Use name indexer
        Console.WriteLine($"Items {itemsFound}");
        List<IName> validItems = itemsFound?.Where(item => item != null).ToList() ?? new List<IName>();

        if (validItems.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"No valid elements found matching Name '{name}'.");
            Console.ResetColor();
            return;
        }

        IName itemToModify;
        int itemId = -1;

        if (validItems.Count == 1)
        {
            itemToModify = validItems[0];
            itemId = FindId(container, itemToModify);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nFound one item (Insertion ID: {itemId}):");
            Console.ResetColor();
        }
        else // Multiple items found
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nFound {validItems.Count} items with Name '{name}'. Please choose which one to modify:");
            Console.ResetColor();

            IName[] allItems = container.GetItems(); // Cache items/IDs for lookup
            int currentIds = GetIds(container);
            Dictionary<int, int> choiceToInsertionIdMap = new Dictionary<int, int>();

            for (int i = 0; i < validItems.Count; i++)
            {
                int currentItemId = FindId(validItems[i], allItems, currentIds);
                // Display choice number, ID, and truncated item info
                string itemInfo = validItems[i].ToString() ?? "N/A";
                Console.WriteLine($"{i + 1}. (ID: {currentItemId}) {itemInfo.Substring(0, Math.Min(60, itemInfo.Length))}...");
                Console.ResetColor();
                if (currentItemId != -1)
                {
                    choiceToInsertionIdMap[i + 1] = currentItemId;
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"Enter choice (1 to {validItems.Count}): ");
            Console.ResetColor();
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= validItems.Count)
            {
                itemToModify = validItems[choice - 1];
                choiceToInsertionIdMap.TryGetValue(choice, out itemId); // Get ID from map
            }
            else
            {
                PrintErrorMessage("Invalid choice.");
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nSelected item (Insertion ID: {itemId}):");
            Console.ResetColor();
        }

        // Modify the selected item
        if (itemId != -1 && itemToModify != null)
        {
            DisplayItemTable(itemId, itemToModify); // Show current state
            ModifyProperty(itemToModify, itemId); // Modify, passing ID
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nCould not reliably identify the selected item or its ID. Modification cancelled.");
            Console.WriteLine(itemToModify?.ToString() ?? "N/A"); // Show basic info if possible
            Console.ResetColor();
        }
    }

    // --- Property Modification Logic ---
    static void ModifyProperty(object itemToModify, int itemInsertionId)
    {
        ArgumentNullException.ThrowIfNull(itemToModify);

        var properties = itemToModify.GetType()
                                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                        .Where(p => p.CanWrite && p.GetSetMethod(true) != null) // Check for accessible setter
                                        .ToList();

        if (properties.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("This object has no publicly writable properties.");
            Console.ResetColor();
            return;
        }

        // Display available properties
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nChoose property to modify:");
        Console.ResetColor();
        for (int i = 0; i < properties.Count; i++)
        {
            object? currentValue = "?";
            try { currentValue = properties[i].GetValue(itemToModify); } catch { /* Ignore read errors */ }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{i + 1}. {properties[i].Name} (Type: {properties[i].PropertyType.Name}, Current: '{currentValue ?? "null"}')");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Enter choice (1 to {properties.Count}): ");
        Console.ResetColor();
        if (int.TryParse(Console.ReadLine(), out int propChoice) && propChoice >= 1 && propChoice <= properties.Count)
        {
            PropertyInfo selectedProperty = properties[propChoice - 1];
            Type propertyType = selectedProperty.PropertyType;
            Type underlyingType = Nullable.GetUnderlyingType(propertyType);
            bool isNullable = underlyingType != null;
            Type targetType = underlyingType ?? propertyType;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"Enter new value for {selectedProperty.Name} (Type: {targetType.Name}{(isNullable ? ", or empty for null" : "")}): ");
            Console.ResetColor();
            string newValueString = Console.ReadLine() ?? "";

            object? convertedValue;

            // Try converting the input string to the target property type
            if (!TryParseValue(newValueString, targetType, isNullable, out convertedValue))
            {
                // Error message printed by TryParseValue
                return;
            }

            // Attempt to set the property value
            try
            {
                selectedProperty.SetValue(itemToModify, convertedValue, null); // Use reflection to set value
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nProperty '{selectedProperty.Name}' updated successfully.");
                Console.WriteLine("New item details:");
                Console.ResetColor();

                DisplayItemTable(itemInsertionId, (IName)itemToModify); // Display updated item
            }
            catch (TargetInvocationException tie)
            {
                // Rethrow the inner exception (likely a validation error from setter)
                throw tie.InnerException ?? tie;
            }
            catch (ArgumentException argEx)
            {
                PrintErrorMessage($"Error setting property '{selectedProperty.Name}': Type mismatch or invalid argument. {argEx.Message}");
            }
            catch (Exception ex) // Catch other reflection errors
            {
                PrintErrorMessage($"Unexpected error setting property '{selectedProperty.Name}': {ex.Message}");
            }
        }
        else
        {
            PrintErrorMessage("Invalid property choice.");
        }
    }

    // Helper for parsing value input in ModifyProperty
    static bool TryParseValue(string input, Type targetType, bool isNullable, out object? parsedValue)
    {
        parsedValue = null;
        if (isNullable && string.IsNullOrEmpty(input))
        {
            return true; // Null is valid for nullable types
        }

        try
        {
            if (targetType == typeof(bool)) // Special handling for bool
            {
                string lowerVal = input.Trim().ToLowerInvariant();
                if (lowerVal == "true" || lowerVal == "1" || lowerVal == "yes" || lowerVal == "y")
                    parsedValue = true;
                else if (lowerVal == "false" || lowerVal == "0" || lowerVal == "no" || lowerVal == "n")
                    parsedValue = false;
                else
                    throw new FormatException($"Cannot convert '{input}' to Boolean.");
            }
            else // Use TypeConverter for other types
            {
                TypeConverter converter = TypeDescriptor.GetConverter(targetType);
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    // Use InvariantCulture for consistent parsing (esp. decimals, dates)
                    parsedValue = converter.ConvertFromString(null, CultureInfo.InvariantCulture, input);
                }
                else
                {
                    // Fallback if TypeConverter fails
                    parsedValue = Convert.ChangeType(input, targetType, CultureInfo.InvariantCulture);
                }
            }
            return true; // Parsing succeeded
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is NotSupportedException || ex is ArgumentException)
        {
            PrintErrorMessage($"Conversion Error: Could not convert '{input}' to type {targetType.Name}. {ex.Message}");
            return false; // Parsing failed
        }
    }


    // --- Removal Logic ---
    static void RemoveElementByIndex(Container<IName> container)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- Remove Element by Current Index ---");
        Console.ResetColor();
        if (container.IsEmpty()) return;

        int currentCount = GetIds(container);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Enter element index to remove (0 to {currentCount - 1}): ");
        Console.ResetColor();

        if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < currentCount)
        {
            // Remove by index
            IName? removedItem = container.RemoveById(index); // Use RemoveByIndex now

            if (removedItem != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nElement at index {index} was removed:");
                Console.WriteLine(removedItem.ToString() ?? "Removed item details unavailable."); // Show details
                Console.ResetColor();
            }
            else
            {
                // Should not happen if index was valid, defensive check
                PrintErrorMessage($"Error: Failed to remove item at index {index}.");
            }
        }
        else
        {
            PrintErrorMessage($"Invalid input. Please enter a valid index between 0 and {currentCount - 1}.");
        }
    }

    // --- Automatic Generation & Demo ---
    static void AutomaticGeneration(Container<IName> container, Random random, int count)
    {
        Console.WriteLine("Generating elements...");
        for (int i = 0; i < count; i++)
        {
            IName newItem;
            int typeChoice = random.Next(1, 9); // 1 to 8 for specific types
            try
            {
                switch (typeChoice)
                {
                    case 1: newItem = GenerateRandomProduct(random); break;
                    case 2: newItem = GenerateRandomRealEstate(random); break;
                    case 3: newItem = GenerateRandomRealEstateInvestment(random); break;
                    case 4: newItem = GenerateRandomApartment(random); break;
                    case 5: newItem = GenerateRandomHouse(random); break;
                    case 6: newItem = GenerateRandomHotel(random); break;
                    case 7: newItem = GenerateRandomLandPlot(random); break;
                    case 8: newItem = new RealEstate($"BaseRE{i}", random.Next(5000, 20000), $"Loc{i}", random.Next(50, 200), "Base"); break;
                    default: continue; // Skip if somehow out of range
                }
                container.Add(newItem);
                Console.Write(".");
            }
            catch (Exception ex) 
            {
                Console.Write("X"); 
                System.Diagnostics.Debug.WriteLine($"\nGeneration Error: {ex.Message}");
            }
        }
        Console.WriteLine("\nGeneration process finished.");
    }

    static void DemonstrateIndexers(Container<IName> container, Random random)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n--- Demonstrating Indexer Usage ---");
        Console.ResetColor();
        if (container.IsEmpty(false)) return; 

        int currentCount = GetIds(container);
        int nextId = container.GetInsertionId();

        // 1. Demonstrate Insertion ID Indexer (Get)
        if (nextId > 0)
        {
            int demoInsertionId = random.Next(nextId); // Pick a random potential ID
            Console.WriteLine($"1. Accessing item by random insertion ID [{demoInsertionId}]:");
            try
            {
                IName? itemById = container[demoInsertionId]; // Use indexer
                if (itemById != null)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"   Found: {itemById.ToString() ?? "N/A"}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"   Item with insertion ID {demoInsertionId} not found (likely removed or ID never used).");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage($"   Error getting item by insertion ID {demoInsertionId}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("1. No items added yet, cannot demonstrate insertion ID indexer.");
        }

        // 2. Demonstrate Name Indexer (Get)
        string? demoName = null;
        IName?[] allItems = container.GetItems();
        if (allItems.Length > 0)
        {
            // Try a few times to find an item with a name
            for (int i = 0; i < 5 && string.IsNullOrWhiteSpace(demoName) && allItems.Length > 0; ++i)
            {
                IName? sourceItemForName = allItems[random.Next(allItems.Length)];
                demoName = GetPropertyValue<string>(sourceItemForName, "Name");
            }
        }

        Console.WriteLine($"\n2. Using string indexer container[\"{demoName ?? "N/A"}\"]:");
        if (!string.IsNullOrWhiteSpace(demoName))
        {
            try
            {
                IName[]? itemsByName = container[demoName]; // Use indexer
                if (itemsByName != null && itemsByName.Length > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"   Found {itemsByName.Length} item(s):");
                    foreach (var item in itemsByName) Console.WriteLine($"   - {item.ToString() ?? "N/A"}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"   No items found for name '{demoName}'.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage($"   Error getting item(s) by name '{demoName}': {ex.Message}");
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("   Could not find an item with a non-empty name in the sample to demonstrate.");
            Console.ResetColor();
        }

        // 3. Demonstrate Insertion ID Indexer (Set)
        // Find a valid ID first
        int validDemoId = -1;
        if (nextId > 0)
        {
            for (int id = nextId - 1; id >= 0; id--)
            { // Search backwards for likely existing ID
                if (container[id] != null)
                {
                    validDemoId = id;
                    break;
                }
            }
        }

        if (validDemoId != -1)
        {
            Console.WriteLine($"\n3. Attempting to change item with insertion ID [{validDemoId}] using indexer:");
            try
            {
                IName? originalItem = container[validDemoId]; // Get original
                Product replacement = new Product($"ChangedItem-{validDemoId}", 999.99m);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"   Replacing '{originalItem?.Name ?? "N/A"}' with '{replacement.Name}'...");
                container[validDemoId].Name = replacement.Name; // Use the setter indexer
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   Item at insertion ID {validDemoId} changed successfully.");
                IName? changedItem = container[validDemoId]; // Verify
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"   New value: {changedItem?.ToString() ?? "Not Found!"}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                PrintErrorMessage($"   Error setting item by insertion ID {validDemoId}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("\n3. Cannot demonstrate set indexer: No suitable item found.");
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--- End Indexer Demonstration ---");
        Console.ResetColor();
    }


    // --- Manual Input ---
    static void ManualInput(Container<IName> container)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Choose class to create:");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("1. Product");
        Console.WriteLine("2. RealEstate");
        Console.WriteLine("3. RealEstateInvestment");
        Console.WriteLine("4. Apartment");
        Console.WriteLine("5. House");
        Console.WriteLine("6. Hotel  ");
        Console.WriteLine("7. LandPlot");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter choice: ");
        Console.ResetColor();
        string classChoice = Console.ReadLine() ?? "";

        IName? newItem = null;
        try
        {
            // Use helper methods for creation, catching exceptions here
            switch (classChoice)
            {
                case "1": newItem = CreateManualProduct(); break;
                case "2": newItem = CreateManualRealEstate(); break;
                case "3": newItem = CreateManualRealEstateInvestment(); break;
                case "4": newItem = CreateManualApartment(); break;
                case "5": newItem = CreateManualHouse(); break;
                case "6": newItem = CreateManualHotel(); break;
                case "7": newItem = CreateManualLandPlot(); break;
                default: PrintErrorMessage("Invalid class choice."); return;
            }

            if (newItem != null)
            {
                container.Add(newItem);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{newItem.GetType().Name} added successfully (Insertion ID: {container.GetInsertionId() - 1}).");
                Console.ResetColor();
            }
        }
        // Catch exceptions specific to manual creation (parsing, validation)
        catch (ValueLessThanZero ex) { PrintErrorMessage($"Creation Error: {ex.Message}"); }
        catch (FormatException ex) { PrintErrorMessage($"Invalid input format during creation: {ex.Message}"); }
        catch (ArgumentException ex) { PrintErrorMessage($"Invalid argument during creation: {ex.Message}"); }
        catch (Exception ex) { PrintErrorMessage($"Unexpected error during creation: {ex.Message}"); }
    }

    // --- Container Display ---
    static void ShowContainer(Container<IName> container)
    {
        int currentCount = GetIds(container);
        string title = $"Container Contents ({currentCount} items)";
        int tableWidth = CalculateTableWidth();

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(CenterString(title, tableWidth + 2)); // Adjust for borders
        Console.ResetColor();

        if (container.IsEmpty(false))
        {
            Console.WriteLine("Container is empty.");
            return;
        }

        PrintTableHeader(tableWidth); // Print header row

        IName?[] items = container.GetItems(); // Get current items

        for (int i = 0; i < currentCount; i++)
        {
            IName? item = items[i];
            int id = i + 1;
            if (item == null) continue; // Should not happen

            WriteDataRowById(id, item, tableWidth); // Write the row data
            DrawHorizontalLine(tableWidth); // Draw separator line
        }
    }

    // Helper to display a single item in a table format
    static void DisplayItemTable(int insertionId, IName item)
    {
        if (item == null) return;
        int tableWidth = CalculateTableWidth();
        PrintTableHeader(tableWidth);
        WriteDataRowById(insertionId, item, tableWidth);
        DrawHorizontalLine(tableWidth);
    }

    // --- Table Formatting Helpers ---
    const int idWidth = 4;
    const int classWidth = 14;
    const int nameWidth = 18;
    const int priceWidth = 15;
    const int locationWidth = 20;
    const int sizeWidth = 8;
    const int typeWidth = 12;
    const int marketValueWidth = 15;
    const int investmentTypeWidth = 18;
    const int floorWidth = 7;
    const int hoaWidth = 7;
    const int gardenWidth = 9;
    const int poolWidth = 6;
    const int roomsWidth = 7;
    const int starWidth = 6;
    const int soilWidth = 10;
    const int infraWidth = 7;
    const int padding = 3; // Space between columns
    const int numColumns = 17;

    static int CalculateTableWidth()
    {
        // Sum of all column widths
        int totalWidth = idWidth + classWidth + nameWidth + priceWidth + locationWidth + sizeWidth + typeWidth + marketValueWidth + investmentTypeWidth + floorWidth + hoaWidth + gardenWidth + poolWidth + roomsWidth + starWidth + soilWidth + infraWidth;
        // Add padding between columns (numColumns - 1 spaces) and border separators (numColumns + 1 pipes)
        totalWidth += numColumns * padding;
        return totalWidth;
    }

    static void PrintTableHeader(int tableWidth)
    {
        DrawHorizontalLine(tableWidth);
        WriteHeaderRow();
        DrawHorizontalLine(tableWidth);
    }

    static void WriteHeaderRow()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        // Pad each header string to its respective width
        Console.Write($"| {"ID".PadRight(idWidth)} ");
        Console.Write($"| {"Class".PadRight(classWidth)} ");
        Console.Write($"| {"Name".PadRight(nameWidth)} ");
        Console.Write($"| {"Price".PadRight(priceWidth)} ");
        Console.Write($"| {"Location".PadRight(locationWidth)} ");
        Console.Write($"| {"Size".PadRight(sizeWidth)} ");
        Console.Write($"| {"Type".PadRight(typeWidth)} ");
        Console.Write($"| {"Mkt Value".PadRight(marketValueWidth)} ");
        Console.Write($"| {"Invest Type".PadRight(investmentTypeWidth)} ");
        Console.Write($"| {"Floor".PadRight(floorWidth)} ");
        Console.Write($"| {"HOA".PadRight(hoaWidth)} ");
        Console.Write($"| {"Garden".PadRight(gardenWidth)} ");
        Console.Write($"| {"Pool".PadRight(poolWidth)} ");
        Console.Write($"| {"Rooms".PadRight(roomsWidth)} ");
        Console.Write($"| {"Star".PadRight(starWidth)} ");
        Console.Write($"| {"Soil".PadRight(soilWidth)} ");
        Console.Write($"| {"Infra".PadRight(infraWidth)} ");
        Console.WriteLine("|");
        Console.ResetColor();
    }

    // Writes a single data row, using reflection to get properties
    static void WriteDataRowById(int id, object item, int tableWidth)
    {
        // Formatting helpers
        string FormatDecimal(decimal? d) => d?.ToString("N2", CultureInfo.InvariantCulture) ?? "-";
        string FormatDouble(double? d) => d?.ToString("N1", CultureInfo.InvariantCulture) ?? "-";
        string FormatBool(bool? b) => b.HasValue ? (b.Value ? "Yes" : "No") : "-";
        string FormatInt(int? i) => i?.ToString() ?? "-";
        string FormatString(string? s) => string.IsNullOrWhiteSpace(s) ? "-" : s;

        Type itemType = item.GetType();

        // Get property values safely using helper
        string name = FormatString(GetPropertyValue<string>(item, "Name"));
        string fPrice = FormatDecimal(GetPropertyValue<decimal?>(item, "Price"));
        string loc = FormatString(GetPropertyValue<string>(item, "Location"));
        string fSize = FormatDouble(GetPropertyValue<double?>(item, "Size"));
        string type = FormatString(GetPropertyValue<string>(item, "Type"));
        string fMktVal = FormatDecimal(GetPropertyValue<decimal?>(item, "MarketValue"));
        string invType = FormatString(GetPropertyValue<string>(item, "InvestmentType"));
        string fFloor = FormatInt(GetPropertyValue<int?>(item, "FloorNumber"));
        string fHoa = FormatDecimal(GetPropertyValue<decimal?>(item, "HOAFees"));
        string fGarden = FormatDouble(GetPropertyValue<double?>(item, "GardenSize"));
        string fPool = FormatBool(GetPropertyValue<bool?>(item, "Pool"));
        string fRooms = FormatInt(GetPropertyValue<int?>(item, "Rooms"));
        string fStar = FormatInt(GetPropertyValue<int?>(item, "StarRating"));
        string soil = FormatString(GetPropertyValue<string>(item, "SoilType"));
        string fInfra = FormatBool(GetPropertyValue<bool?>(item, "InfrastructureAccess"));

        // Write formatted and truncated values
        Console.Write($"| {id.ToString().PadRight(idWidth)} ");
        Console.Write($"| {Truncate(itemType.Name, classWidth).PadRight(classWidth)} ");
        Console.Write($"| {Truncate(name, nameWidth).PadRight(nameWidth)} ");
        Console.Write($"| {Truncate(fPrice, priceWidth).PadRight(priceWidth)} ");
        Console.Write($"| {Truncate(loc, locationWidth).PadRight(locationWidth)} ");
        Console.Write($"| {Truncate(fSize, sizeWidth).PadRight(sizeWidth)} ");
        Console.Write($"| {Truncate(type, typeWidth).PadRight(typeWidth)} ");
        Console.Write($"| {Truncate(fMktVal, marketValueWidth).PadRight(marketValueWidth)} ");
        Console.Write($"| {Truncate(invType, investmentTypeWidth).PadRight(investmentTypeWidth)} ");
        Console.Write($"| {Truncate(fFloor, floorWidth).PadRight(floorWidth)} ");
        Console.Write($"| {Truncate(fHoa, hoaWidth).PadRight(hoaWidth)} ");
        Console.Write($"| {Truncate(fGarden, gardenWidth).PadRight(gardenWidth)} ");
        Console.Write($"| {Truncate(fPool, poolWidth).PadRight(poolWidth)} ");
        Console.Write($"| {Truncate(fRooms, roomsWidth).PadRight(roomsWidth)} ");
        Console.Write($"| {Truncate(fStar, starWidth).PadRight(starWidth)} ");
        Console.Write($"| {Truncate(soil, soilWidth).PadRight(soilWidth)} ");
        Console.Write($"| {Truncate(fInfra, infraWidth).PadRight(infraWidth)} ");
        Console.WriteLine("|");
    }


    static void DrawHorizontalLine(int tableWidth)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('-', tableWidth));
        Console.ResetColor();
    }

    static string CenterString(string s, int width)
    {
        if (string.IsNullOrEmpty(s)) return new string(' ', width);
        if (s.Length >= width) return Truncate(s, width);
        int padding = (width - s.Length) / 2;
        return new string(' ', padding) + s + new string(' ', width - s.Length - padding);
    }

    // Truncate a value in table, if value width is greater than table column width
    static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (maxLength <= 0) return "";
        if (value.Length <= maxLength) return value;
        if (maxLength <= 3) return value.Substring(0, maxLength);
        return value.Substring(0, maxLength - 3) + "...";
    }

    // --- Reflection Property Getter ---
    private static TValue? GetPropertyValue<TValue>(object? item, string propertyName)
    {
        if (item == null) return default;
        PropertyInfo? property = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property != null && property.CanRead)
        {
            try
            {
                object? value = property.GetValue(item);
                if (value == null) return default;
                if (value is TValue correctlyTyped) return correctlyTyped;

                // Handle potential nullable/non-nullable mismatch if TValue is nullable
                Type? underlyingTValue = Nullable.GetUnderlyingType(typeof(TValue));
                if (underlyingTValue != null && underlyingTValue == property.PropertyType)
                {
                    // Try converting non-nullable value to nullable TValue
                    try { return (TValue)Convert.ChangeType(value, underlyingTValue, CultureInfo.InvariantCulture); } catch { /* Ignore */ }
                }
                // Final attempt with Convert.ChangeType (use cautiously)
                try { return (TValue)Convert.ChangeType(value, typeof(TValue), CultureInfo.InvariantCulture); } catch { /* Ignore */ }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Reflection Error getting '{propertyName}': {ex.Message}");
            }
        }
        return default; // Property not found or type mismatch
    }

    // --- ID Helpers ---
    private static int GetIds(Container<IName> container)
    {
        return container.GetCount();
    }

    // Finds the ID of a specific item instance
    private static int FindId(Container<IName> container, IName itemToFind)
    {
        IName[] currentItems = container.GetItems();
        int currentIds = GetIds(container); // Use helper
        return FindId(itemToFind, currentItems, currentIds); // Use overload
    }

    // Overload using pre-fetched arrays for efficiency inside loops
    private static int FindId(IName itemToFind, IName[] currentItems, int currentIds)
    {
        for (int i = 0; i < currentIds; i++)
        {
            if (object.ReferenceEquals(currentItems[i], itemToFind))
            {
                return i;
            }
        }
        return -1; // Not found
    }


    // --- Random Generators (Ensure generated values are valid) ---
    static Product GenerateRandomProduct(Random random)
    {
        string[] names = { "Table", "Chair", "Lamp", "Phone", "Book", "Laptop", "Mug" };
        decimal price = random.Next(10, 1000) + (decimal)random.NextDouble();
        return new Product(names[random.Next(names.Length)] + random.Next(100), Math.Round(price, 2));
    }

    static RealEstate GenerateRandomRealEstate(Random random)
    {
        string[] names = { "Cozy Apt", "Luxury Villa", "Small House", "Big Mansion", "Downtown Loft" };
        string[] locations = { "New York", "London", "Paris", "Tokyo", "Kyiv", "Berlin", "Sydney" };
        string[] types = { "Residential", "Commercial", "Industrial", "Mixed-Use" };
        decimal price = random.Next(100000, 1000000) + (decimal)random.NextDouble() * 1000;
        double size = random.Next(50, 500) + random.NextDouble() * 10;
        return new RealEstate(names[random.Next(names.Length)], Math.Round(price, 2), locations[random.Next(locations.Length)], Math.Max(1.0, Math.Round(size, 1)), types[random.Next(types.Length)]);
    }

    static RealEstateInvestment GenerateRandomRealEstateInvestment(Random random)
    {
        string[] names = { "Office Bldg", "Shopping Mall", "Warehouse", "Apt Complex", "Data Center" };
        string[] locations = { "Chicago", "Los Angeles", "Houston", "Phoenix", "Philadelphia", "Dallas" };
        string[] invTypes = { "REIT", "Direct Prop", "Mortgage Fund", "Syndication" };
        decimal price = random.Next(500000, 5000000) + (decimal)random.NextDouble() * 10000;
        decimal marketValue = price * (decimal)(0.8 + random.NextDouble() * 0.4);
        return new RealEstateInvestment(names[random.Next(names.Length)], Math.Round(price, 2), locations[random.Next(locations.Length)], Math.Max(1.0m, Math.Round(marketValue, 2)), invTypes[random.Next(invTypes.Length)]);
    }

    static Apartment GenerateRandomApartment(Random random)
    {
        string[] names = { "Studio Apt", "1BR Apt", "2BR Apt", "Penthouse", "Garden Apt" };
        string[] locations = { "Miami", "San Francisco", "Seattle", "Boston", "Denver", "Austin" };
        string[] types = { "Condo", "Co-op", "Rental Unit", "Loft" };
        decimal price = random.Next(200000, 800000) + (decimal)random.NextDouble() * 500;
        double size = random.Next(40, 150) + random.NextDouble() * 5;
        int floor = random.Next(1, 30);
        decimal hoa = random.Next(50, 500) + (decimal)random.NextDouble() * 50; // Ensure >= 0
        return new Apartment(names[random.Next(names.Length)], Math.Round(price, 2), locations[random.Next(locations.Length)], Math.Max(1.0, Math.Round(size, 1)), types[random.Next(types.Length)], floor, Math.Max(0m, Math.Round(hoa, 2)));
    }

    static House GenerateRandomHouse(Random random)
    {
        string[] names = { "Bungalow", "Townhouse", "Ranch", "Cottage", "Colonial" };
        string[] locations = { "Atlanta", "Dallas", "San Diego", "Orlando", "Las Vegas", "Nashville" };
        string[] types = { "Single-family", "Multi-family", "Duplex" };
        decimal price = random.Next(300000, 1200000) + (decimal)random.NextDouble() * 1000;
        double size = random.Next(100, 400) + random.NextDouble() * 15;
        double gardenSize = random.Next(-50, 1000) + random.NextDouble() * 100; // Allow 0
        bool pool = random.Next(3) == 0;
        return new House(names[random.Next(names.Length)], Math.Round(price, 2), locations[random.Next(locations.Length)], Math.Max(1.0, Math.Round(size, 1)), types[random.Next(types.Length)], Math.Max(0.0, Math.Round(gardenSize, 1)), pool);
    }

    static Hotel GenerateRandomHotel(Random random)
    {
        string[] names = { "Luxury Hotel", "Budget Inn", "Resort & Spa", "Boutique Hotel", "Airport Motel" };
        string[] locations = { "Hawaii", "Bali", "Maldives", "Fiji", "Santorini", "Las Vegas Strip" };
        string[] invTypes = { "Hospitality REIT", "Hotel Mgmt", "Timeshare", "Franchise" };
        decimal price = random.Next(1000000, 10000000) + (decimal)random.NextDouble() * 50000;
        decimal marketValue = price * (decimal)(0.9 + random.NextDouble() * 0.3);
        int rooms = random.Next(20, 500); // Ensure > 0
        int rating = random.Next(1, 6); // 1-5
        return new Hotel(names[random.Next(names.Length)], Math.Round(price, 2), locations[random.Next(locations.Length)], Math.Max(1.0m, Math.Round(marketValue, 2)), invTypes[random.Next(invTypes.Length)], Math.Max(1, rooms), rating);
    }

    static LandPlot GenerateRandomLandPlot(Random random)
    {
        string[] names = { "Farmland", "Forest", "Comm Land", "Resid Land", "Waterfront" };
        string[] locations = { "Rural Area", "Suburban Edge", "Urban Infill", "Coastal Zone", "Mountain Base" };
        string[] invTypes = { "Land Banking", "Development", "Agriculture", "Conservation" };
        string[] soilTypes = { "Loam", "Clay", "Sand", "Silt", "Peat", "Chalky" };
        decimal price = random.Next(50000, 500000) + (decimal)random.NextDouble() * 2000;
        decimal marketValue = price * (decimal)(0.7 + random.NextDouble() * 0.6);
        bool infra = random.Next(2) == 0;
        return new LandPlot(names[random.Next(names.Length)], Math.Round(price, 2), locations[random.Next(locations.Length)], Math.Max(1.0m, Math.Round(marketValue, 2)), invTypes[random.Next(invTypes.Length)], soilTypes[random.Next(soilTypes.Length)], infra);
    }


    // --- Manual Creation Methods (with robust parsing) ---

    static Product CreateManualProduct()
    {
        string name = ReadString("Enter Product Name: ");
        decimal price = ReadDecimal("Enter Product Price (> 0): ", minValue: 0.01m);
        return new Product(name, price);
    }

    static RealEstate CreateManualRealEstate()
    {
        string name = ReadString("Enter RealEstate Name: ");
        decimal price = ReadDecimal("Enter RealEstate Price (> 0): ", minValue: 0.01m);
        string location = ReadString("Enter Location: ");
        double size = ReadDouble("Enter Size (> 0): ", minValue: 0.01);
        string type = ReadString("Enter Type (e.g., Residential): ");
        return new RealEstate(name, price, location, size, type);
    }

    static RealEstateInvestment CreateManualRealEstateInvestment()
    {
        string name = ReadString("Enter Investment Name: ");
        decimal price = ReadDecimal("Enter Investment Price (> 0): ", minValue: 0.01m);
        string location = ReadString("Enter Location: ");
        decimal marketValue = ReadDecimal("Enter Market Value (> 0): ", minValue: 0.01m);
        string investmentType = ReadString("Enter Investment Type (e.g., REIT): ");
        return new RealEstateInvestment(name, price, location, marketValue, investmentType);
    }

    static Apartment CreateManualApartment()
    {
        string name = ReadString("Enter Apartment Name: ");
        decimal price = ReadDecimal("Enter Apartment Price (> 0): ", minValue: 0.01m);
        string location = ReadString("Enter Location: ");
        double size = ReadDouble("Enter Size (> 0): ", minValue: 0.01);
        string type = ReadString("Enter Type (e.g., Condo): ");
        int floorNumber = ReadInt("Enter Floor Number (> 0): ", minValue: 1);
        decimal hoaFees = ReadDecimal("Enter HOA Fees (>= 0): ", minValue: 0m);
        return new Apartment(name, price, location, size, type, floorNumber, hoaFees);
    }

    static House CreateManualHouse()
    {
        string name = ReadString("Enter House Name: ");
        decimal price = ReadDecimal("Enter House Price (> 0): ", minValue: 0.01m);
        string location = ReadString("Enter Location: ");
        double size = ReadDouble("Enter Size (> 0): ", minValue: 0.01);
        string type = ReadString("Enter Type (e.g., Single-family): ");
        double gardenSize = ReadDouble("Enter Garden Size (>= 0): ", minValue: 0.0);
        bool pool = ReadBool("Has Pool (true/false/yes/no/1/0): ");
        return new House(name, price, location, size, type, gardenSize, pool);
    }

    static Hotel CreateManualHotel()
    {
        string name = ReadString("Enter Hotel Name: ");
        decimal price = ReadDecimal("Enter Hotel Price (> 0): ", minValue: 0.01m);
        string location = ReadString("Enter Location: ");
        decimal marketValue = ReadDecimal("Enter Market Value (> 0): ", minValue: 0.01m);
        string investmentType = ReadString("Enter Investment Type: ");
        int rooms = ReadInt("Enter Number of Rooms (> 0): ", minValue: 1);
        int starRating = ReadInt("Enter Star Rating (1-5): ", minValue: 1, maxValue: 5);
        return new Hotel(name, price, location, marketValue, investmentType, rooms, starRating);
    }

    static LandPlot CreateManualLandPlot()
    {
        string name = ReadString("Enter LandPlot Name: ");
        decimal price = ReadDecimal("Enter LandPlot Price (> 0): ", minValue: 0.01m);
        string location = ReadString("Enter Location: ");
        decimal marketValue = ReadDecimal("Enter Market Value (> 0): ", minValue: 0.01m);
        string investmentType = ReadString("Enter Investment Type: ");
        string soilType = ReadString("Enter Soil Type (e.g., Loam): ");
        bool infrastructureAccess = ReadBool("Has Infrastructure Access (true/false/yes/no/1/0): ");
        return new LandPlot(name, price, location, marketValue, investmentType, soilType, infrastructureAccess);
    }

    // --- Robust Input Reading Helpers ---
    static string ReadString(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? "";
    }

    static decimal ReadDecimal(string prompt, decimal? minValue = null, decimal? maxValue = null)
    {
        decimal value;
        while (true)
        {
            Console.Write(prompt);
            if (decimal.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                if ((minValue == null || value >= minValue) && (maxValue == null || value <= maxValue))
                {
                    return value;
                }
                else
                {
                    PrintErrorMessage($"Value must be between {minValue?.ToString("N2") ?? "-inf"} and {maxValue?.ToString("N2") ?? "+inf"}.");
                }
            }
            else
            {
                PrintErrorMessage("Invalid decimal format.");
            }
        }
    }
    static double ReadDouble(string prompt, double? minValue = null, double? maxValue = null)
    {
        double value;
        while (true)
        {
            Console.Write(prompt);
            if (double.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                if ((minValue == null || value >= minValue) && (maxValue == null || value <= maxValue))
                {
                    return value;
                }
                else
                {
                    PrintErrorMessage($"Value must be between {minValue?.ToString("N1") ?? "-inf"} and {maxValue?.ToString("N1") ?? "+inf"}.");
                }
            }
            else
            {
                PrintErrorMessage("Invalid number format.");
            }
        }
    }
    static int ReadInt(string prompt, int? minValue = null, int? maxValue = null)
    {
        int value;
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out value))
            {
                if ((minValue == null || value >= minValue) && (maxValue == null || value <= maxValue))
                {
                    return value;
                }
                else
                {
                    PrintErrorMessage($"Value must be between {minValue ?? int.MinValue} and {maxValue ?? int.MaxValue}.");
                }
            }
            else
            {
                PrintErrorMessage("Invalid integer format.");
            }
        }
    }
    static bool ReadBool(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";
            if (input == "true" || input == "1" || input == "yes" || input == "y") return true;
            if (input == "false" || input == "0" || input == "no" || input == "n") return false;
            PrintErrorMessage("Invalid boolean input. Use true/false/yes/no/1/0.");
        }
    }

} 