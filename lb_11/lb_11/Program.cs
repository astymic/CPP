using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using lb_11.Classes;
using lb_11.Interfaces;


namespace lb_11;

enum ContainerType
{
    None,
    Array,
    LinkedList
}

class Program
{
    static Container<IName>? containerArray = null;
    static ContainerLinkedList<IName>? containerList = null;
    static ContainerType activeContainerType = ContainerType.None;
    static Random random = new Random();

    static void Main()
    {
        while (true)
        {
            PrintMenu();
            string choice = Console.ReadLine()?.ToLower() ?? "";

            try
            {
                switch (choice)
                {
                    case "1": HandleContainerSelectionAndAction(HandleAutomaticGeneration); break;
                    case "2": HandleContainerSelectionAndAction(HandleManualInput); break;
                    case "3": HandleShowContainer(); break;
                    case "4": HandleGetElementByInsertionId(); break;
                    case "5": HandleGetElementByName(); break;
                    case "6": HandleChangeItemByInsertionId(); break;
                    case "7": HandleChangeItemByName(); break;
                    case "8": HandleSortContainer(); break;
                    case "9": HandleRemoveElementByIndex(); break;
                    case "10": HandleReverseGenerator(); break;
                    case "11": HandleSublineGenerator(); break;
                    case "12": HandleSortedPriceGenerator(); break;
                    case "13": HandleSortedNameGenerator(); break;
                    case "14": HandleSerializeContainer(); break; 
                    case "15": HandleDeserializeContainer(); break; 
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
            catch (KeyNotFoundException ex)
            {
                PrintErrorMessage($"Error: Key (e.g., Insertion ID) not found. {ex.Message}");
            }
            catch (FileNotFoundException ex)
            {
                PrintErrorMessage($"File Error: {ex.Message}");
            }
            catch (IOException ex)
            {
                PrintErrorMessage($"File IO Error: {ex.Message}");
            }
            catch (InvalidOperationException ex) // Catch errors from serialization/deserialization etc.
            {
                PrintErrorMessage($"Operation Error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                PrintErrorMessage($"Argument Error: {ex.Message}");
            }
            catch (TargetInvocationException ex)
            {
                Exception inner = ex.InnerException ?? ex;
                while (inner.InnerException != null) { inner = inner.InnerException; }
                PrintErrorMessage($"Error during operation: {inner.GetType().Name} - {inner.Message}");
            }
            catch (Exception ex)
            {
                PrintErrorMessage($"An unexpected error occurred: {ex.GetType().Name} - {ex.Message}");
            }
            finally
            {
                // Add a small delay or prompt to continue after an error or action
                // Console.WriteLine("\nPress Enter to continue...");
                // Console.ReadLine();
                Console.ResetColor();
            }
        }
    }

    // --- Menu Printing ---
    static void PrintMenu()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n------ Menu ------");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"Active Container: {activeContainerType}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("1. Automatic Generation (Select/Switch Container)");
        Console.WriteLine("2. Manual Input (Select/Switch Container)");
        Console.WriteLine("3. Show Active Container");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- Getters ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("4. Get Element by Insertion ID (1-based)");
        Console.WriteLine("5. Get Elements by Name");
        // Console.WriteLine("6. Get Elements by Price");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- Modifiers ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("6. Change Item by Insertion ID (1-based)");
        Console.WriteLine("7. Change Item by Name");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- Container Operations ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("8. Sort Active Container (In-Place)");
        Console.WriteLine("9. Remove Element by Current Index (0-based)");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- Generators (Non-Mutating Iterators) ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("10. Show Elements in Reverse Order");
        Console.WriteLine("11. Show Elements with Name Containing Substring");
        Console.WriteLine("12. Show Elements Sorted by Price (Generator)");
        Console.WriteLine("13. Show Elements Sorted by Name (Generator)");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- File Operations ---"); // New Section
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("14. Serialize Active Container to File"); // New Option
        Console.WriteLine("15. Deserialize Container from File (Replaces Active)"); // New Option
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- ### ### ### ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("q. Exit");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter your choice: ");
        Console.ResetColor();
    }

    static void PrintErrorMessage(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nERROR: {message}");
        Console.ResetColor();
    }

    // --- Container Selection Logic ---
    static void HandleContainerSelectionAndAction(Action actionToPerform)
    {
        ContainerType chosenType = AskContainerType();
        if (chosenType == ContainerType.None)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Operation cancelled.");
            Console.ResetColor();
            return;
        }

        if (activeContainerType != chosenType || (activeContainerType == ContainerType.None))
        {
            bool switchConfirmed = true;
            if (activeContainerType != ContainerType.None && activeContainerType != chosenType)
            {
                // Check if the current container has items before warning about clearing
                bool currentHasItems = (activeContainerType == ContainerType.Array && containerArray?.GetCount() > 0) ||
                                      (activeContainerType == ContainerType.LinkedList && containerList?.Count > 0);

                if (currentHasItems)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"Switching to {chosenType} container will clear the current {activeContainerType} container. Continue? (y/n): ");
                    Console.ResetColor();
                    switchConfirmed = (Console.ReadLine()?.Trim().ToLower() == "y");
                }
            }

            if (switchConfirmed)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"\nInitializing {chosenType} container...");
                Console.ResetColor();
                containerArray = null; // Explicitly null out inactive container
                containerList = null;
                activeContainerType = chosenType;
                if (activeContainerType == ContainerType.Array)
                {
                    containerArray = new Container<IName>();
                }
                else // LinkedList
                {
                    containerList = new ContainerLinkedList<IName>();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Switch cancelled. Keeping the current container.");
                Console.ResetColor();
                return; // Don't perform the action if switch is cancelled
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nContinuing with the active {activeContainerType} container.");
            Console.ResetColor();
        }

        // Perform the action only if the container is ready (selected or switched)
        actionToPerform();
    }

    static ContainerType AskContainerType()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nSelect Container Type:");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("1. Array-based Container");
        Console.WriteLine("2. Linked List Container");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter choice (1 or 2, or any other key to cancel): ");
        Console.ResetColor();
        string choice = Console.ReadLine() ?? "";
        return choice switch
        {
            "1" => ContainerType.Array,
            "2" => ContainerType.LinkedList,
            _ => ContainerType.None,
        };
    }


    // --- Action Handlers ---

    static void HandleAutomaticGeneration()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- Automatic Generation ---");
        Console.ResetColor();
        // Ensure the active container is not null before proceeding
        if ((activeContainerType == ContainerType.Array && containerArray == null) ||
            (activeContainerType == ContainerType.LinkedList && containerList == null))
        {
            PrintErrorMessage("Active container is not initialized. This should not happen here.");
            return;
        }

        Console.Write("Enter number of elements to generate: ");
        if (int.TryParse(Console.ReadLine(), out int count) && count > 0)
        {
            if (activeContainerType == ContainerType.Array)
            {
                AutomaticGenerationArray(containerArray!, random, count); // Null-forgiving operator used after check
                DemonstrateIndexersArray(containerArray!, random);
            }
            else // LinkedList
            {
                AutomaticGenerationList(containerList!, random, count); // Null-forgiving operator used after check
                DemonstrateIndexersList(containerList!, random);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nAutomatic generation of {count} elements complete for {activeContainerType} container.");
            Console.ResetColor();
        }
        else
        {
            PrintErrorMessage("Invalid input for count (must be a positive integer). Generation cancelled.");
        }
    }

    static void HandleManualInput()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Manual Input for {activeContainerType} Container ---");
        Console.ResetColor();
        // Ensure the active container is not null before proceeding
        if ((activeContainerType == ContainerType.Array && containerArray == null) ||
            (activeContainerType == ContainerType.LinkedList && containerList == null))
        {
            PrintErrorMessage("Active container is not initialized. This should not happen here.");
            return;
        }

        if (activeContainerType == ContainerType.Array)
        {
            ManualInputArray(containerArray!); // Null-forgiving operator used after check
        }
        else // LinkedList
        {
            ManualInputList(containerList!); // Null-forgiving operator used after check
        }
    }

    static void HandleShowContainer()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Show {activeContainerType} Container ---");
        Console.ResetColor();
        if (IsContainerEmpty(out int currentCount))
        {
            // Message printed by IsContainerEmpty
            return;
        }

        if (activeContainerType == ContainerType.Array)
        {
            if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
            ShowContainerArray(containerArray, currentCount);
        }
        else // LinkedList
        {
            if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
            ShowContainerList(containerList, currentCount);
        }
    }

    // Still gets by Insertion ID, displays current index
    static void HandleGetElementByInsertionId()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Get Element by Insertion ID from {activeContainerType} Container ---");
        Console.ResetColor();
        if (IsContainerEmpty(out _)) return;

        int maxId = GetNextInsertionId(); // Max ID is exclusive upper bound (next available ID)
        if (maxId == 0)
        {
            PrintErrorMessage("Container is empty, no IDs to get.");
            return;
        }
        Console.ForegroundColor = ConsoleColor.Yellow;
        // User enters 1-based ID, convert to 0-based internally
        Console.Write($"Enter insertion ID (1 to {maxId}): ");
        Console.ResetColor();

        if (int.TryParse(Console.ReadLine(), out int inputId) && inputId >= 1 && inputId <= maxId)
        {
            int insertionId = inputId - 1; // Convert to 0-based ID for internal use
            IName? item = null;
            try
            {
                if (activeContainerType == ContainerType.Array)
                {
                    if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
                    item = containerArray[insertionId]; // Use 0-based ID with indexer
                }
                else // LinkedList
                {
                    if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
                    item = containerList[insertionId]; // Use 0-based ID with indexer
                }
            }
            catch (IndexOutOfRangeException ex) // Catch specific exception from indexer
            {
                // Provide the ID the user entered (1-based) in the error message
                PrintErrorMessage($"Item with insertion ID {inputId} not found or invalid. {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                PrintErrorMessage($"Error accessing item with insertion ID {inputId}: {ex.Message}");
                return;
            }


            if (item == null)
            {
                // If indexer returns null (could happen if ID was valid but item removed/not found by GetInstanceByInsertionId logic)
                PrintErrorMessage($"Item with insertion ID {inputId} not found (possibly removed or ID never used/valid).");
                return;
            }

            // Find the current 0-based index for display purposes
            int currentIndex = FindIndexByReference(item);
            if (currentIndex == -1)
            {
                // This implies the item was found by ID but couldn't be located by reference in the current structure
                PrintErrorMessage($"Found item by insertion ID {inputId}, but could not determine its current index for display. Displaying raw data:");
                Console.WriteLine(item.ToString()); // Show raw data if index mapping fails
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            // Display 1-based ID and 0-based index (or adjust index display if desired)
            Console.WriteLine($"\nItem Details (Insertion ID: {inputId}, Current Index: {currentIndex}):");
            Console.ResetColor();
            DisplayItemTable(currentIndex + 1, item); // Display using 1-based index for user readability
        }
        else
        {
            PrintErrorMessage($"Invalid input. Please enter a valid integer ID between 1 and {maxId}.");
        }
    }

    static void HandleGetElementByName()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Get Elements by Name from {activeContainerType} Container ---");
        Console.ResetColor();
        if (IsContainerEmpty(out _)) return;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter the Name to search for: ");
        Console.ResetColor();
        string name = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            PrintErrorMessage("Invalid input. Name cannot be empty.");
            return;
        }

        List<IName> itemsFoundList = new List<IName>();

        try
        {
            if (activeContainerType == ContainerType.Array)
            {
                if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
                IName[]? itemsFoundArray = containerArray[name]; // Use name indexer
                if (itemsFoundArray != null)
                {
                    // The indexer should ideally return only non-null items matching the name
                    itemsFoundList.AddRange(itemsFoundArray);
                }
            }
            else // LinkedList
            {
                if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
                List<IName>? itemsFoundLinkedList = containerList[name]; // Use name indexer
                if (itemsFoundLinkedList != null)
                {
                    itemsFoundList.AddRange(itemsFoundLinkedList);
                }
            }
        }
        catch (Exception ex)
        {
            PrintErrorMessage($"Error searching by name '{name}': {ex.Message}");
            return;
        }


        if (itemsFoundList.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nFound {itemsFoundList.Count} element(s) with Name '{name}':");
            Console.ResetColor();

            int tableWidth = CalculateTableWidth();
            PrintTableHeader(tableWidth);

            foreach (var foundItem in itemsFoundList)
            {
                // Find current index for display
                int currentIndex = FindIndexByReference(foundItem);
                if (currentIndex != -1)
                {
                    WriteDataRowByDisplayId(currentIndex + 1, foundItem, tableWidth); // Display 1-based index
                }
                else
                {
                    // Item was found by name indexer but couldn't be located by reference
                    // This might happen if the container structure changed unexpectedly
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    string itemStr = foundItem.ToString() ?? "N/A";
                    string truncatedItemStr = itemStr.Length > tableWidth - 40 ? itemStr.Substring(0, tableWidth - 40) + "..." : itemStr;
                    Console.WriteLine($"|{PadAndCenter($"Warning: Could not map index for item: {truncatedItemStr}", tableWidth - 2)}|");
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

    // Still changes by Insertion ID, displays current id
    static void HandleChangeItemByInsertionId()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Change Item by Insertion ID in {activeContainerType} Container ---");
        Console.ResetColor();
        if (IsContainerEmpty(out _)) return;

        int maxId = GetNextInsertionId(); // Exclusive upper bound
        if (maxId == 0)
        {
            PrintErrorMessage("Container is empty, no IDs to modify.");
            return;
        }
        Console.ForegroundColor = ConsoleColor.Yellow;
        // Prompt for 1-based ID
        Console.Write($"Enter item insertion ID to modify (1 to {maxId}): ");
        Console.ResetColor();

        if (int.TryParse(Console.ReadLine(), out int inputId) && inputId >= 1 && inputId <= maxId)
        {
            int insertionId = inputId - 1; // Convert to 0-based ID for internal use
            IName? itemToModify = null;

            try
            {
                // First, retrieve the item using the 0-based insertion ID to ensure it exists
                if (activeContainerType == ContainerType.Array)
                {
                    if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
                    itemToModify = containerArray[insertionId];
                }
                else // LinkedList
                {
                    if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
                    itemToModify = containerList[insertionId];
                }
            }
            catch (IndexOutOfRangeException ex) // Catch specific exception from indexer
            {
                // Use 1-based ID in message
                PrintErrorMessage($"Item with insertion ID {inputId} not found or invalid for modification. {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                PrintErrorMessage($"Error accessing item with insertion ID {inputId} for modification: {ex.Message}");
                return;
            }

            // If indexer returned null even after passing the ID check (e.g., if GetInstance logic allows null return)
            if (itemToModify == null)
            {
                PrintErrorMessage($"Item with insertion ID {inputId} not found (possibly removed or ID never used/valid). Modification cancelled.");
                return;
            }

            // Find current 0-based index for display
            int currentIndex = FindIndexByReference(itemToModify);
            if (currentIndex == -1)
            {
                PrintErrorMessage($"Found item by insertion ID {inputId}, but could not determine its current index for display. Cannot proceed with modification.");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nCurrent item details:");
            Console.ResetColor();
            // Display using 1-based index for user
            DisplayItemTable(currentIndex + 1, itemToModify);

            // Modify the properties of the retrieved item.
            // The indexer logic for 'set' is not needed here, as we already have the object reference.
            // We pass the 0-based insertionId for potential use within ModifyProperty if needed.
            ModifyProperty(itemToModify, insertionId);
        }
        else
        {
            PrintErrorMessage($"Invalid input. Please enter a valid integer ID between 1 and {maxId}.");
        }
    }

    static void HandleChangeItemByName()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Change Item by Name in {activeContainerType} Container ---");
        Console.ResetColor();
        if (IsContainerEmpty(out _)) return;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter the Name of the item(s) to modify: ");
        Console.ResetColor();
        string name = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            PrintErrorMessage("Invalid input. Name cannot be empty.");
            return;
        }

        List<IName> validItems = new List<IName>();
        try
        {
            if (activeContainerType == ContainerType.Array)
            {
                if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
                IName[]? itemsFoundArray = containerArray[name];
                if (itemsFoundArray != null) validItems.AddRange(itemsFoundArray);
            }
            else // LinkedList
            {
                if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
                List<IName>? itemsFoundList = containerList[name];
                if (itemsFoundList != null) validItems.AddRange(itemsFoundList);
            }
        }
        catch (Exception ex)
        {
            PrintErrorMessage($"Error searching by name '{name}': {ex.Message}");
            return;
        }


        if (validItems.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"No valid elements found matching Name '{name}'.");
            Console.ResetColor();
            return;
        }

        IName itemToModify;
        int itemInsertionId = -1; // 0-based internal ID
        int currentDisplayIndex = -1; // 0-based current index

        if (validItems.Count == 1)
        {
            itemToModify = validItems[0];
            // Find the insertion ID (0-based) and current index (0-based)
            itemInsertionId = GetInsertionIdForItem(itemToModify);
            currentDisplayIndex = FindIndexByReference(itemToModify);

            if (itemInsertionId == -1 || currentDisplayIndex == -1) { PrintErrorMessage("Could not find ID or index for the item."); return; }

            Console.ForegroundColor = ConsoleColor.Green;
            // Display 1-based index and 1-based insertion ID to user
            Console.WriteLine($"\nFound one item (Current Index: {currentDisplayIndex + 1}, Insertion ID: {itemInsertionId + 1}):");
            Console.ResetColor();
        }
        else // Multiple items found
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nFound {validItems.Count} items with Name '{name}'. Please choose which one to modify:");
            Console.ResetColor();

            // Map user choice (1-based) to the 0-based current index of the item
            Dictionary<int, int> choiceToCurrentIndexMap = new Dictionary<int, int>();

            for (int i = 0; i < validItems.Count; i++)
            {
                // Get the current 0-based index of the item in the list/array
                int currentItemIndex = FindIndexByReference(validItems[i]);
                if (currentItemIndex != -1)
                {
                    string itemInfo = validItems[i].ToString() ?? "N/A";
                    // Show user choice (1-based) and current index (1-based)
                    Console.WriteLine($"{i + 1}. (Index: {currentItemIndex + 1}) {itemInfo}");
                    choiceToCurrentIndexMap[i + 1] = currentItemIndex; // Store 0-based index
                }
                else
                {
                    // If index mapping fails, inform the user but still list the item
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    string itemStr = validItems[i].ToString() ?? "N/A";
                    // Show user choice (1-based)
                    Console.WriteLine($"{i + 1}. (Index: ???) Could not map item - {itemStr}");
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"Enter choice (1 to {validItems.Count}): ");
            Console.ResetColor();

            if (int.TryParse(Console.ReadLine(), out int choice)
                && choice >= 1 && choice <= validItems.Count
                && choiceToCurrentIndexMap.TryGetValue(choice, out int chosenCurrentIndex)) // chosenCurrentIndex is 0-based
            {
                // Retrieve the item using its current 0-based index
                itemToModify = GetItemByCurrentIndex(chosenCurrentIndex);
                if (itemToModify == null) { PrintErrorMessage("Failed to re-acquire selected item by current index."); return; }

                // Get the 0-based insertion ID for the chosen item
                itemInsertionId = GetInsertionIdForItem(itemToModify);
                currentDisplayIndex = chosenCurrentIndex; // Keep the 0-based index

                if (itemInsertionId == -1) { PrintErrorMessage("Could not determine insertion ID for the chosen item."); return; }
            }
            else
            {
                PrintErrorMessage("Invalid choice or item mapping failed.");
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            // Display 1-based index and 1-based insertion ID to user
            Console.WriteLine($"\nSelected item (Current Index: {currentDisplayIndex + 1}, Insertion ID: {itemInsertionId + 1}):");
            Console.ResetColor();
        }

        // Modify the selected item (itemToModify is the object reference)
        if (currentDisplayIndex != -1 && itemToModify != null)
        {
            DisplayItemTable(currentDisplayIndex + 1, itemToModify); // Display using 1-based index
                                                                     // Pass the 0-based insertion ID to ModifyProperty
            ModifyProperty(itemToModify, itemInsertionId);
        }
        else
        {
            // Should not happen if logic above is correct, but safeguard
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nCould not reliably identify the selected item or its index. Modification cancelled.");
            Console.ResetColor();
        }
    }

    static void HandleSortContainer()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Sorting {activeContainerType} Container (In-Place) ---");
        Console.ResetColor();
        if (IsContainerEmpty(out int currentCount)) return;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Select sort parameter:");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("1. Sort by Name");
        Console.WriteLine("2. Sort by Price");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter choice (1 or 2): ");
        Console.ResetColor();

        string? sortChoice = Console.ReadLine()?.Trim();
        string sortParameter;
        Action? sortAction = null;

        switch (sortChoice)
        {
            case "1":
                sortParameter = "Name";
                Console.WriteLine("\nSorting by Name...");
                if (activeContainerType == ContainerType.Array)
                {
                    if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
                    sortAction = containerArray.SortByName;
                }
                else // LinkedList
                {
                    if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
                    // LinkedList sort modifies internal structure and InsertionOrder mapping
                    sortAction = () => containerList.Sort("Name");
                }
                break;

            case "2":
                sortParameter = "Price";
                Console.WriteLine("\nSorting by Price...");
                if (activeContainerType == ContainerType.Array)
                {
                    if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
                    sortAction = containerArray.SortByPrice;
                }
                else // LinkedList
                {
                    if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
                    // LinkedList sort modifies internal structure and InsertionOrder mapping
                    sortAction = () => containerList.Sort("Price");
                }
                break;

            default:
                PrintErrorMessage("Invalid sort choice.");
                return;
        }

        if (sortAction != null)
        {
            try
            {
                sortAction.Invoke();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Container sorted by {sortParameter}.");
                Console.WriteLine("Note: Sorting may change the current indices of items. Insertion IDs remain associated with the items.");
                Console.ResetColor();
                HandleShowContainer(); // Show the container after sorting
            }
            catch (Exception ex)
            {
                // Catch potential comparison errors etc.
                PrintErrorMessage($"Error during sorting by {sortParameter}: {ex.GetType().Name} - {ex.Message}");
            }
        }
        else
        {
            // Should not happen if logic above is correct, but as a safeguard
            PrintErrorMessage("Sort action could not be determined.");
        }
    }

    static void HandleRemoveElementByIndex()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Remove Element by Current Index from {activeContainerType} Container ---");
        Console.ResetColor();
        if (IsContainerEmpty(out int currentCount)) return;

        Console.ForegroundColor = ConsoleColor.Yellow;
        // Prompt for 0-based index as per function name
        Console.Write($"Enter element index to remove (0 to {currentCount - 1}): ");
        Console.ResetColor();

        if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < currentCount)
        {
            IName? removedItem = null;
            int removedItemInsertionId = -1; // 0-based internal ID

            try
            {
                // Get the item at the specified index *before* removal to find its insertion ID
                IName? itemToRemove = GetItemByCurrentIndex(index);

                if (itemToRemove != null)
                {
                    removedItemInsertionId = GetInsertionIdForItem(itemToRemove); // Get 0-based ID
                }
                else
                {
                    // This could happen if GetItemByCurrentIndex fails, though unlikely if index is valid
                    PrintErrorMessage($"Could not retrieve item details at index {index} before removal.");
                    // Optionally, still attempt removal if the index is valid
                }

                // Perform the removal using the 0-based index
                if (activeContainerType == ContainerType.Array)
                {
                    if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
                    removedItem = containerArray!.RemoveById(index);
                }
                else // LinkedList
                {
                    if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
                    removedItem = containerList!.RemoveByIndex(index);
                }

                // Check if the removal function returned the item successfully
                if (removedItem != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    // Display 1-based insertion ID to the user if found, otherwise indicate unknown
                    string idString = removedItemInsertionId != -1 ? $"(original Insertion ID: {removedItemInsertionId + 1})" : "(original Insertion ID unknown)";
                    Console.WriteLine($"\nElement at index {index} {idString} was removed:");
                    Console.WriteLine(removedItem.ToString() ?? "Removed item details unavailable.");
                    Console.ResetColor();
                }
                else
                {
                    // This might happen if the Remove method fails internally or returns null unexpectedly
                    PrintErrorMessage($"Error: Failed to confirm removal of item at index {index}. The item might have been removed, but confirmation failed.");
                }
            }
            catch (ArgumentOutOfRangeException ex) // Catch index errors specifically from RemoveByIndex/RemoveById
            {
                PrintErrorMessage($"Error removing item at index {index}: Index was out of range. {ex.Message}");
            }
            catch (Exception ex) // Catch other potential errors during removal
            {
                PrintErrorMessage($"Error during removal at index {index}: {ex.GetType().Name} - {ex.Message}");
            }
        }
        else
        {
            PrintErrorMessage($"Invalid input. Please enter a valid index between 0 and {currentCount - 1}.");
        }
    }

    // --- Generator Demonstrations ---

    static void HandleReverseGenerator()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Generator: Items in Reverse Order ({activeContainerType} Container) ---");
        Console.ResetColor();
        if (IsContainerEmpty(out _)) return;

        IEnumerable<IName> reversedItems;
        try
        {
            if (activeContainerType == ContainerType.Array)
            {
                if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
                reversedItems = containerArray.GetReverseArray();
            }
            else // LinkedList
            {
                if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
                reversedItems = containerList.GetReverseArray();
            }
        }
        catch (Exception ex)
        {
            PrintErrorMessage($"Error preparing reverse generator: {ex.Message}");
            return;
        }


        int tableWidth = CalculateTableWidth();
        PrintTableHeader(tableWidth);
        int count = 0;
        try
        {
            // Use foreach which implicitly calls GetEnumerator() and MoveNext()
            foreach (var item in reversedItems)
            {
                // Generators should yield non-null items if implemented correctly
                WriteDataRowByDisplayId(count + 1, item, tableWidth); // Display 1-based sequence number
                DrawHorizontalLine(tableWidth);
                count++;
            }
        }
        catch (Exception ex)
        {
            PrintErrorMessage($"Error during iteration with reverse generator: {ex.Message}");
        }

        if (count == 0 && !IsContainerEmpty(out _)) // Check if container wasn't empty but generator yielded nothing
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"|{PadAndCenter("(Generator yielded no items, check implementation)", tableWidth - 2)}|");
            DrawHorizontalLine(tableWidth);
            Console.ResetColor();
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Reverse generator yielded {count} items.");
        Console.ResetColor();
    }

    static void HandleSublineGenerator()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Generator: Items with Name Containing Substring ({activeContainerType} Container) ---");
        Console.ResetColor();
        if (IsContainerEmpty(out _)) return;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter substring to search for in Name: ");
        Console.ResetColor();
        string subline = Console.ReadLine() ?? "";

        if (string.IsNullOrEmpty(subline))
        {
            PrintErrorMessage("Substring cannot be empty.");
            return;
        }

        IEnumerable<IName> itemsWithSubline;
        try
        {
            if (activeContainerType == ContainerType.Array)
            {
                if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
                itemsWithSubline = containerArray.GetArrayWithSublineInName(subline);
            }
            else // LinkedList
            {
                if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
                itemsWithSubline = containerList.GetArrayWithSublineInName(subline);
            }
        }
        catch (Exception ex)
        {
            PrintErrorMessage($"Error preparing subline generator: {ex.Message}");
            return;
        }


        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nResults for names containing '{subline}':");
        Console.ResetColor();
        int tableWidth = CalculateTableWidth();
        PrintTableHeader(tableWidth);
        int count = 0;
        try
        {
            foreach (var item in itemsWithSubline)
            {
                WriteDataRowByDisplayId(count + 1, item, tableWidth); // Display 1-based sequence number
                DrawHorizontalLine(tableWidth);
                count++;
            }
        }
        catch (Exception ex)
        {
            PrintErrorMessage($"Error during iteration with subline generator: {ex.Message}");
        }


        if (count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"|{PadAndCenter($"No items found with names containing '{subline}'", tableWidth - 2)}|");
            DrawHorizontalLine(tableWidth);
            Console.ResetColor();
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Substring generator yielded {count} items.");
        Console.ResetColor();
    }

    static void HandleSortedPriceGenerator()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Generator: Items Sorted by Price ({activeContainerType} Container) ---");
        Console.ResetColor();
        if (IsContainerEmpty(out _)) return;

        IEnumerable<IName> sortedItems;
        try
        {
            if (activeContainerType == ContainerType.Array)
            {
                if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
                sortedItems = containerArray.GetSortedByArrayPrice();
            }
            else // LinkedList
            {
                if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
                sortedItems = containerList.GetSortedByArrayPrice();
            }
        }
        catch (Exception ex)
        {
            PrintErrorMessage($"Error preparing sorted price generator: {ex.Message}");
            return;
        }


        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nItems sorted by Price (Generated Sequence, original container unchanged):");
        Console.ResetColor();
        int tableWidth = CalculateTableWidth();
        PrintTableHeader(tableWidth);
        int count = 0;
        try
        {
            foreach (var item in sortedItems)
            {
                // Generators should yield non-null items
                WriteDataRowByDisplayId(count + 1, item, tableWidth); // Display 1-based sequence number
                DrawHorizontalLine(tableWidth);
                count++;
            }
        }
        catch (Exception ex) // Catch potential comparison errors during generation
        {
            PrintErrorMessage($"Error during iteration with sorted price generator: {ex.GetType().Name} - {ex.Message}");
        }


        if (count == 0 && !IsContainerEmpty(out _))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"|{PadAndCenter("(Generator yielded no items, check implementation/comparer)", tableWidth - 2)}|");
            DrawHorizontalLine(tableWidth);
            Console.ResetColor();
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Sorted by Price generator yielded {count} items.");
        Console.ResetColor();
    }

    static void HandleSortedNameGenerator()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Generator: Items Sorted by Name ({activeContainerType} Container) ---");
        Console.ResetColor();
        if (IsContainerEmpty(out _)) return;

        IEnumerable<IName> sortedItems;
        try
        {
            if (activeContainerType == ContainerType.Array)
            {
                if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
                sortedItems = containerArray.GetSortedArrayByName();
            }
            else // LinkedList
            {
                if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
                sortedItems = containerList.GetSortedArrayByName();
            }
        }
        catch (Exception ex)
        {
            PrintErrorMessage($"Error preparing sorted name generator: {ex.Message}");
            return;
        }


        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nItems sorted by Name (Generated Sequence, original container unchanged):");
        Console.ResetColor();
        int tableWidth = CalculateTableWidth();
        PrintTableHeader(tableWidth);
        int count = 0;
        try
        {
            foreach (var item in sortedItems)
            {
                WriteDataRowByDisplayId(count + 1, item, tableWidth); // Display 1-based sequence number
                DrawHorizontalLine(tableWidth);
                count++;
            }
        }
        catch (Exception ex) // Catch potential comparison errors during generation
        {
            PrintErrorMessage($"Error during iteration with sorted name generator: {ex.GetType().Name} - {ex.Message}");
        }


        if (count == 0 && !IsContainerEmpty(out _))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"|{PadAndCenter("(Generator yielded no items, check implementation/comparer)", tableWidth - 2)}|");
            DrawHorizontalLine(tableWidth);
            Console.ResetColor();
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Sorted by Name generator yielded {count} items.");
        Console.ResetColor();
    }

    // --- Indexer Interaction Methods ---

    // Array Container Indexer Demonstration
    static void DemonstrateIndexersArray(Container<IName> container, Random random)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n--- Demonstrating Array Container Indexer Usage ---");
        Console.ResetColor();
        if (container.IsEmpty(false))
        {
            Console.WriteLine("Container is empty. Cannot demonstrate indexers.");
            return;
        }

        int currentCount = container.GetCount();
        int nextId = container.GetInsertionId(); // Next available ID (0-based)
        int[] currentOrder = container.GetInsertionOrder(); // Get current valid IDs

        // 1. Demonstrate Insertion ID Indexer (Get)
        int demoGetId = -1;
        if (currentOrder.Length > 0)
        {
            demoGetId = currentOrder[random.Next(currentOrder.Length)]; // Pick a random *existing* 0-based ID
        }

        if (demoGetId != -1)
        {
            Console.WriteLine($"1. Accessing item by existing insertion ID [{demoGetId + 1}]:"); // Display 1-based
            try
            {
                IName? itemById = container[demoGetId]; // Use 0-based ID with indexer
                if (itemById != null)
                {
                    int currentIndex = FindIndexByReference(itemById); // Find 0-based index
                    string indexStr = currentIndex != -1 ? $"(Current Index: {currentIndex + 1})" : "(Index Unknown)"; // Display 1-based index
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"   Found {indexStr}: {itemById.ToString() ?? "N/A"}");
                    Console.ResetColor();
                }
                else
                {
                    // This shouldn't happen if demoGetId was from currentOrder and item wasn't null, but safeguard
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"   Item with insertion ID {demoGetId + 1} was unexpectedly null.");
                    Console.ResetColor();
                }
            }
            catch (IndexOutOfRangeException) // Catch specific indexer exception
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   Item with insertion ID {demoGetId + 1} not found (likely removed after ID selection).");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                PrintErrorMessage($"   Error getting item by insertion ID {demoGetId + 1}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("1. No valid insertion IDs available to demonstrate 'get'.");
        }

        // 2. Demonstrate Name Indexer (Get)
        // Get a copy of items to find a name without modifying original container structure
        string? demoName = FindDemoName(container.GetItems(), container.GetCount(), random);
        Console.WriteLine($"\n2. Using string indexer container[\"{demoName ?? "N/A"}\"]:");
        if (!string.IsNullOrWhiteSpace(demoName))
        {
            try
            {
                IName[]? itemsByName = container[demoName]; // Use name indexer
                if (itemsByName != null && itemsByName.Length > 0) // Check if array is not null and has elements
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"   Found {itemsByName.Length} item(s):");
                    foreach (var item in itemsByName) // Iterate through potentially non-null items
                    {
                        if (item == null) continue; // Should not happen if indexer is correct
                        int currentIndex = FindIndexByReference(item); // Find 0-based index
                        string indexStr = currentIndex != -1 ? $"(Index: {currentIndex + 1})" : "(Index Unknown)"; // Display 1-based index
                        Console.WriteLine($"   - {indexStr} {item.ToString() ?? "N/A"}");
                    }
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
            Console.WriteLine("   Could not find an item with a non-empty name in the current items to demonstrate.");
            Console.ResetColor();
        }

        // 3. Demonstrate Insertion ID Indexer (Set)
        int validDemoSetId = -1;
        if (currentOrder.Length > 0)
        {
            validDemoSetId = currentOrder[random.Next(currentOrder.Length)]; // Pick a random *existing* 0-based ID
        }

        if (validDemoSetId != -1)
        {
            Console.WriteLine($"\n3. Attempting to replace item with insertion ID [{validDemoSetId + 1}] using indexer set:"); // Display 1-based
            try
            {
                IName? originalItem = container[validDemoSetId]; // Get original item first
                if (originalItem == null)
                {
                    Console.WriteLine($"   Cannot get original item with ID {validDemoSetId + 1} to show difference.");
                }
                else
                {
                    int originalIndex = FindIndexByReference(originalItem);
                    string indexStr = originalIndex != -1 ? $"(Current Index: {originalIndex + 1})" : "";
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"   Original Item {indexStr}: '{originalItem.ToString() ?? "N/A"}'");
                    Console.ResetColor();
                }

                // Create a new item to replace the old one
                Product newItem = new Product($"ReplacedItem-{validDemoSetId + 1}", 999.99m);
                Console.WriteLine($"   Attempting to set item with ID {validDemoSetId + 1} to: '{newItem}'");

                container[validDemoSetId] = newItem; // Use indexer SET with 0-based ID

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   Indexer set operation completed.");

                // Verify the change
                IName? changedItem = container[validDemoSetId];
                int changedIndex = (changedItem != null) ? FindIndexByReference(changedItem) : -1;
                string changedIndexStr = changedIndex != -1 ? $"(Current Index: {changedIndex + 1})" : "(Index Unknown)";

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"   Current value at ID {validDemoSetId + 1} {changedIndexStr}: {changedItem?.ToString() ?? "Not Found!"}");
                Console.ResetColor();

                if (changedItem != null && ReferenceEquals(changedItem, newItem))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("   Verification: Item reference matches the new item. Replacement successful.");
                    Console.ResetColor();
                }
                else if (changedItem != null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("   Verification: Item found but reference doesn't match the new item (unexpected).");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("   Verification: Item not found after set operation!");
                    Console.ResetColor();
                }
            }
            catch (IndexOutOfRangeException) // Catch specific indexer exception
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   Item with insertion ID {validDemoSetId + 1} not found for set operation (likely removed after ID selection).");
                Console.ResetColor();
            }
            catch (ArgumentNullException ex) { PrintErrorMessage($"   Error: Cannot set item to null. {ex.Message}"); }
            catch (Exception ex) { PrintErrorMessage($"   Error setting item by insertion ID {validDemoSetId + 1}: {ex.Message}"); }
        }
        else
        {
            Console.WriteLine("\n3. Cannot demonstrate modification: No suitable item found with a valid insertion ID.");
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--- End Array Indexer Demonstration ---");
        Console.ResetColor();
    }

    // LinkedList Container Indexer Demonstration
    static void DemonstrateIndexersList(ContainerLinkedList<IName> container, Random random)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n--- Demonstrating LinkedList Container Indexer Usage ---");
        Console.ResetColor();
        if (container.Count == 0)
        {
            Console.WriteLine("Container is empty. Cannot demonstrate indexers.");
            return;
        }

        List<int> currentInsertionOrder = container.GetInsertionOrder(); // Get copy of current IDs
        if (currentInsertionOrder.Count == 0)
        {
            Console.WriteLine("Container has items but insertion order list is empty (unexpected). Cannot demonstrate.");
            return;
        }

        // 1. Demonstrate Insertion ID Indexer (Get)
        int demoGetIdList = currentInsertionOrder[random.Next(currentInsertionOrder.Count)]; // Pick existing 0-based ID

        Console.WriteLine($"1. Accessing item by existing insertion ID [{demoGetIdList + 1}]:"); // Display 1-based
        try
        {
            IName? itemById = container[demoGetIdList]; // Use 0-based ID with indexer
            if (itemById != null)
            {
                int currentIndex = FindIndexByReference(itemById); // 0-based index
                string indexStr = currentIndex != -1 ? $"(Current Index: {currentIndex + 1})" : "(Index Unknown)"; // 1-based index display
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"   Found {indexStr}: {itemById.ToString() ?? "N/A"}");
                Console.ResetColor();
            }
            else
            {
                // Should not happen if ID exists and indexer works correctly
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   Item with insertion ID {demoGetIdList + 1} was unexpectedly null.");
                Console.ResetColor();
            }
        }
        catch (IndexOutOfRangeException) // Catch specific indexer exception
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"   Item with insertion ID {demoGetIdList + 1} not found (likely removed after ID selection).");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            PrintErrorMessage($"   Error getting item by insertion ID {demoGetIdList + 1}: {ex.Message}");
        }


        // 2. Demonstrate Name Indexer (Get)
        // Find name from current list items
        string? demoName = FindDemoName(container, random);
        Console.WriteLine($"\n2. Using string indexer container[\"{demoName ?? "N/A"}\"]:");
        if (!string.IsNullOrWhiteSpace(demoName))
        {
            try
            {
                List<IName>? itemsByName = container[demoName]; // Use name indexer
                if (itemsByName != null && itemsByName.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"   Found {itemsByName.Count} item(s):");
                    foreach (var item in itemsByName)
                    {
                        int currentIndex = FindIndexByReference(item); // 0-based index
                        string indexStr = currentIndex != -1 ? $"(Index: {currentIndex + 1})" : "(Index Unknown)"; // 1-based index display
                        Console.WriteLine($"   - {indexStr} {item.ToString() ?? "N/A"}");
                    }
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
            Console.WriteLine("   Could not find an item with a non-empty name in the current items to demonstrate.");
            Console.ResetColor();
        }

        // 3. Demonstrate Insertion ID Indexer (Set)
        int validDemoSetIdList = currentInsertionOrder[random.Next(currentInsertionOrder.Count)]; // Pick existing 0-based ID

        Console.WriteLine($"\n3. Attempting to replace item with insertion ID [{validDemoSetIdList + 1}] using indexer set:"); // Display 1-based
        try
        {
            IName? originalItem = container[validDemoSetIdList]; // Get original item first
            if (originalItem == null)
            {
                Console.WriteLine($"   Cannot get original item with ID {validDemoSetIdList + 1} to show difference.");
            }
            else
            {
                int originalIndex = FindIndexByReference(originalItem);
                string indexStr = originalIndex != -1 ? $"(Current Index: {originalIndex + 1})" : "";
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"   Original Item {indexStr}: '{originalItem.ToString() ?? "N/A"}'");
                Console.ResetColor();
            }

            // Create a new item to replace the old one
            Product newItem = new Product($"ReplacedLLItem-{validDemoSetIdList + 1}", 888.88m);
            Console.WriteLine($"   Attempting to set item with ID {validDemoSetIdList + 1} to: '{newItem}'");

            container[validDemoSetIdList] = newItem; // Use indexer SET with 0-based ID

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"   Indexer set operation completed.");

            // Verify the change
            IName? changedItem = container[validDemoSetIdList];
            int changedIndex = (changedItem != null) ? FindIndexByReference(changedItem) : -1;
            string changedIndexStr = changedIndex != -1 ? $"(Current Index: {changedIndex + 1})" : "(Index Unknown)";

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"   Current value at ID {validDemoSetIdList + 1} {changedIndexStr}: {changedItem?.ToString() ?? "Not Found!"}");
            Console.ResetColor();

            if (changedItem != null && ReferenceEquals(changedItem, newItem))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("   Verification: Item reference matches the new item. Replacement successful.");
                Console.ResetColor();
            }
            else if (changedItem != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("   Verification: Item found but reference doesn't match the new item (unexpected).");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("   Verification: Item not found after set operation!");
                Console.ResetColor();
            }
        }
        catch (IndexOutOfRangeException) // Catch specific indexer exception
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"   Item with insertion ID {validDemoSetIdList + 1} not found for set operation (likely removed after ID selection).");
            Console.ResetColor();
        }
        catch (ArgumentNullException ex) { PrintErrorMessage($"   Error: Cannot set item to null. {ex.Message}"); }
        catch (InvalidOperationException ex) { PrintErrorMessage($"   Operation Error during set: {ex.Message}"); } // Catch inconsistency error
        catch (Exception ex) { PrintErrorMessage($"   Error setting item by insertion ID {validDemoSetIdList + 1}: {ex.Message}"); }


        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--- End LinkedList Indexer Demonstration ---");
        Console.ResetColor();
    }

    // Helper to find a non-empty name from a sample of items in an array
    static string? FindDemoName(IName?[] items, int count, Random random)
    {
        string? demoName = null;
        if (count == 0) return null;
        int attempts = Math.Min(5, count); // Try up to 5 times or count times
        List<int> triedIndices = new List<int>();

        for (int i = 0; i < attempts; ++i)
        {
            int randomIndex;
            do
            {
                randomIndex = random.Next(count); // Generate index within 0 to count-1
            } while (triedIndices.Contains(randomIndex)); // Ensure we don't try the same index twice
            triedIndices.Add(randomIndex);

            IName? sourceItemForName = items[randomIndex];
            if (sourceItemForName != null)
            {
                demoName = GetPropertyValue<string>(sourceItemForName, "Name");
                if (!string.IsNullOrWhiteSpace(demoName)) break; // Found a name, exit loop
            }
        }
        return demoName;
    }

    // Helper to find a non-empty name from a sample of items in a linked list
    static string? FindDemoName(ContainerLinkedList<IName> listContainer, Random random)
    {
        string? demoName = null;
        int count = listContainer.Count;
        if (count == 0) return null;
        int attempts = Math.Min(5, count);
        List<int> triedIndices = new List<int>();


        for (int i = 0; i < attempts; ++i)
        {
            int randomIndex;
            do
            {
                randomIndex = random.Next(count);
            } while (triedIndices.Contains(randomIndex));
            triedIndices.Add(randomIndex);

            // Traverse to the random index
            var node = listContainer.First;
            int currentIndex = 0;
            while (node != null && currentIndex < randomIndex)
            {
                node = node.Next;
                currentIndex++;
            }

            if (node != null && node.Data != null) // Check if node and its data are valid
            {
                demoName = GetPropertyValue<string>(node.Data, "Name");
                if (!string.IsNullOrWhiteSpace(demoName)) break; // Found a name
            }
        }
        return demoName;
    }

    // Finds a valid, *existing* 0-based insertion ID in the Array container
    static int FindValidInsertionId(Container<IName> container)
    {
        int[] currentOrder = container.GetInsertionOrder(); // Get list of currently valid IDs
        if (currentOrder.Length == 0) return -1;
        // Return a random ID from the list of existing IDs
        return currentOrder[random.Next(currentOrder.Length)];
    }

    // Finds a valid, *existing* 0-based insertion ID in the LinkedList container
    static int FindValidInsertionId(ContainerLinkedList<IName> container)
    {
        List<int> order = container.GetInsertionOrder(); // Get copy of current IDs
        if (order.Count == 0) return -1;
        // Return a random ID from the list of existing IDs
        return order[random.Next(order.Count)];
    }

    // --- Property Modification Logic ---
    // Modifies property of the given object reference. Uses itemInsertionId (0-based) for messages.
    static void ModifyProperty(object itemToModify, int itemInsertionId)
    {
        ArgumentNullException.ThrowIfNull(itemToModify);

        var properties = itemToModify.GetType()
                                        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty) // Ensure SetProperty flag
                                        .Where(p => p.CanWrite && p.GetSetMethod(true) != null) // Check CanWrite and setter exists
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
            try { currentValue = properties[i].GetValue(itemToModify); } catch { /* Ignore read errors */ }
            Console.ForegroundColor = ConsoleColor.Cyan;
            // Display user choice (1-based)
            Console.WriteLine($"{i + 1}. {properties[i].Name} (Type: {properties[i].PropertyType.Name}, Current: '{currentValue ?? "null"}')");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Enter choice (1 to {properties.Count}): ");
        Console.ResetColor();
        // User enters 1-based choice, convert to 0-based index
        if (int.TryParse(Console.ReadLine(), out int propChoice) && propChoice >= 1 && propChoice <= properties.Count)
        {
            PropertyInfo selectedProperty = properties[propChoice - 1]; // Use 0-based index
            Type propertyType = selectedProperty.PropertyType;
            Type underlyingType = Nullable.GetUnderlyingType(propertyType);
            bool isNullable = underlyingType != null;
            Type targetType = underlyingType ?? propertyType;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"Enter new value for {selectedProperty.Name} (Type: {targetType.Name}{(isNullable ? ", or empty for null" : "")}): ");
            Console.ResetColor();
            string newValueString = Console.ReadLine() ?? "";

            object? convertedValue;

            if (!TryParseValue(newValueString, targetType, isNullable, out convertedValue))
            {
                // Error message printed by TryParseValue
                return;
            }

            try
            {
                // Set the property value on the existing object reference
                selectedProperty.SetValue(itemToModify, convertedValue, null);
                Console.ForegroundColor = ConsoleColor.Green;
                // Display 1-based insertion ID in message
                Console.WriteLine($"\nProperty '{selectedProperty.Name}' updated successfully for item (Insertion ID: {itemInsertionId + 1}).");
                Console.WriteLine("New item details:");
                Console.ResetColor();

                // Re-find the current index (0-based) as it might have changed due to sorting etc.
                int currentIndex = FindIndexByReference((IName)itemToModify);
                if (currentIndex != -1)
                {
                    DisplayItemTable(currentIndex + 1, (IName)itemToModify); // Display using 1-based index
                }
                else
                {
                    PrintErrorMessage("Could not determine current index after modification for display. Raw data:");
                    Console.WriteLine(itemToModify.ToString());
                }
            }
            catch (TargetInvocationException tie) // Catch validation errors from property setters
            {
                PrintErrorMessage($"Validation Error setting property '{selectedProperty.Name}': {tie.InnerException?.Message ?? tie.Message}");
            }
            catch (ArgumentException argEx) // Catch type mismatch errors
            {
                PrintErrorMessage($"Error setting property '{selectedProperty.Name}': Type mismatch or invalid argument. {argEx.Message}");
            }
            catch (Exception ex) // Catch other unexpected errors
            {
                PrintErrorMessage($"Unexpected error setting property '{selectedProperty.Name}': {ex.Message}");
            }
        }
        else
        {
            PrintErrorMessage("Invalid property choice.");
        }
    }

    static bool TryParseValue(string input, Type targetType, bool isNullable, out object? parsedValue)
    {
        parsedValue = null;
        if (isNullable && string.IsNullOrEmpty(input))
        {
            return true; // Null is valid for nullable types
        }

        try
        {
            if (targetType == typeof(bool))
            {
                string lowerVal = input.Trim().ToLowerInvariant();
                if (lowerVal == "true" || lowerVal == "1" || lowerVal == "yes" || lowerVal == "y")
                    parsedValue = true;
                else if (lowerVal == "false" || lowerVal == "0" || lowerVal == "no" || lowerVal == "n")
                    parsedValue = false;
                else
                    throw new FormatException($"Cannot convert '{input}' to Boolean.");
            }
            else if (targetType.IsEnum) // Handle enums
            {
                if (Enum.TryParse(targetType, input, true, out object? enumValue)) // Case-insensitive parse
                {
                    parsedValue = enumValue;
                }
                else
                {
                    throw new FormatException($"'{input}' is not a valid value for enum {targetType.Name}. Valid values are: {string.Join(", ", Enum.GetNames(targetType))}");
                }
            }
            else
            {
                // Use TypeConverter for robust conversion (handles culture, etc.)
                TypeConverter converter = TypeDescriptor.GetConverter(targetType);
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    // Use InvariantCulture for consistent parsing regardless of system locale
                    parsedValue = converter.ConvertFromString(null, CultureInfo.InvariantCulture, input);
                }
                else
                {
                    // Fallback to Convert.ChangeType (less flexible)
                    parsedValue = Convert.ChangeType(input, targetType, CultureInfo.InvariantCulture);
                }
            }
            return true; // Conversion successful
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is NotSupportedException || ex is ArgumentException)
        {
            // Provide more specific error feedback
            PrintErrorMessage($"Conversion Error: Could not convert '{input}' to type {targetType.Name}. Check format (e.g., use '.' for decimals). Details: {ex.Message}");
            return false; // Conversion failed
        }
        catch (Exception ex) // Catch unexpected conversion errors
        {
            PrintErrorMessage($"Unexpected Conversion Error for '{input}' to type {targetType.Name}: {ex.Message}");
            return false;
        }
    }


    // --- Automatic Generation ---
    static void AutomaticGenerationArray(Container<IName> container, Random random, int count)
    {
        Console.WriteLine("Generating elements for Array Container...");
        GenerateItems(count, random, item => container.Add(item));
        Console.WriteLine("\nArray Generation process finished.");
    }

    static void AutomaticGenerationList(ContainerLinkedList<IName> container, Random random, int count)
    {
        Console.WriteLine("Generating elements for LinkedList Container...");
        GenerateItems(count, random, item => container.AddLast(item)); // Add to end for list
        Console.WriteLine("\nLinkedList Generation process finished.");
    }

    static void GenerateItems(int count, Random random, Action<IName> addAction)
    {
        for (int i = 0; i < count; i++)
        {
            IName newItem;
            int typeChoice = random.Next(1, 9); // Generates 1 through 8
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
                    // Add a base RealEstate case to ensure all paths return
                    case 8: newItem = new RealEstate($"BaseRE-{i}", (decimal)(random.Next(5000, 20000) + random.NextDouble()), $"Loc{i}", random.Next(50, 200), "Base"); break;
                    default:
                        Console.Write("?"); // Should not happen with Next(1, 9)
                        continue; // Skip if typeChoice is unexpected
                }
                addAction(newItem);
                Console.Write("."); // Progress indicator
            }
            catch (ValueLessThanZero ex) // Catch specific validation errors during generation
            {
                Console.Write("V"); // Indicate validation error
                System.Diagnostics.Debug.WriteLine($"\nGeneration Validation Error (Type {typeChoice}): {ex.Message}");
            }
            catch (Exception ex) // Catch other unexpected errors during generation
            {
                Console.Write("X"); // Indicate general error
                System.Diagnostics.Debug.WriteLine($"\nGeneration Error (Type {typeChoice}): {ex.GetType().Name} - {ex.Message}");
                // Optionally re-throw or handle more gracefully depending on requirements
            }
        }
        Console.WriteLine(); // New line after progress indicators
    }

    // --- Manual Input ---
    static void ManualInputArray(Container<IName> container)
    {
        IName? newItem = CreateItemManually();
        if (newItem != null)
        {
            container.Add(newItem);
            Console.ForegroundColor = ConsoleColor.Green;
            // Display 1-based insertion ID (GetInsertionId() returns the *next* ID, so subtract 1)
            Console.WriteLine($"{newItem.GetType().Name} added successfully to Array Container (Insertion ID: {container.GetInsertionId()}).");
            Console.ResetColor();
        }
    }

    static void ManualInputList(ContainerLinkedList<IName> container)
    {
        IName? newItem = CreateItemManually();
        if (newItem != null)
        {
            container.AddLast(newItem);
            Console.ForegroundColor = ConsoleColor.Green;
            // Display 1-based insertion ID (GetNextInsertionId() returns the *next* ID, so subtract 1)
            Console.WriteLine($"{newItem.GetType().Name} added successfully to LinkedList Container (Insertion ID: {container.GetNextInsertionId()}).");
            Console.ResetColor();
        }
    }

    static IName? CreateItemManually()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Choose class to create:");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("1. Product");
        Console.WriteLine("2. RealEstate");
        Console.WriteLine("3. RealEstateInvestment");
        Console.WriteLine("4. Apartment");
        Console.WriteLine("5. House");
        Console.WriteLine("6. Hotel");
        Console.WriteLine("7. LandPlot");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter choice: ");
        Console.ResetColor();
        string classChoice = Console.ReadLine() ?? "";

        try
        {
            return classChoice switch
            {
                "1" => CreateManualProduct(),
                "2" => CreateManualRealEstate(),
                "3" => CreateManualRealEstateInvestment(),
                "4" => CreateManualApartment(),
                "5" => CreateManualHouse(),
                "6" => CreateManualHotel(),
                "7" => CreateManualLandPlot(),
                _ => throw new ArgumentException("Invalid class choice.") // Use ArgumentException for invalid input
            };
        }
        // Catch specific expected exceptions from input/creation
        catch (ValueLessThanZero ex) { PrintErrorMessage($"Creation Error: {ex.Message}"); return null; }
        catch (FormatException ex) { PrintErrorMessage($"Invalid input format during creation: {ex.Message}"); return null; }
        catch (ArgumentException ex) { PrintErrorMessage($"Invalid argument during creation: {ex.Message}"); return null; }
        catch (Exception ex) { PrintErrorMessage($"Unexpected error during creation: {ex.Message}"); return null; } // Catch other errors
    }


    // --- Container Display ---
    static void ShowContainerArray(Container<IName> container, int currentCount)
    {
        string title = $"Array Container Contents ({currentCount} items)";
        int tableWidth = CalculateTableWidth();

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(CenterString(title, tableWidth));
        Console.ResetColor();

        PrintTableHeader(tableWidth);

        int i = 0;
        // Use the container's enumerator, which iterates correctly over populated items
        foreach (var item in container)
        {
            // Display 1-based index
            WriteDataRowByDisplayId(i + 1, item, tableWidth);
            DrawHorizontalLine(tableWidth);
            i++;
        }
        // If loop finishes and i is still 0 (and count > 0), it means enumeration failed
        if (i == 0 && currentCount > 0)
        {
            Console.WriteLine($"|{PadAndCenter("Error: Failed to enumerate items.", tableWidth - 2)}|");
            DrawHorizontalLine(tableWidth);
        }
    }

    static void ShowContainerList(ContainerLinkedList<IName> container, int currentCount)
    {
        string title = $"LinkedList Container Contents ({currentCount} items)";
        int tableWidth = CalculateTableWidth();

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(CenterString(title, tableWidth));
        Console.ResetColor();

        PrintTableHeader(tableWidth);

        int i = 0;
        // Use the container's enumerator (foreach works because it implements IEnumerable<T>)
        foreach (var item in container)
        {
            // Display 1-based index
            WriteDataRowByDisplayId(i + 1, item, tableWidth);
            DrawHorizontalLine(tableWidth);
            i++;
        }
        // If loop finishes and i is still 0 (and count > 0), it means enumeration failed
        if (i == 0 && currentCount > 0)
        {
            Console.WriteLine($"|{PadAndCenter("Error: Failed to enumerate items.", tableWidth - 2)}|");
            DrawHorizontalLine(tableWidth);
        }
    }

    // Displays item using 1-based current index (passed as displayId)
    static void DisplayItemTable(int displayId, IName item)
    {
        if (item == null) return;
        int tableWidth = CalculateTableWidth();
        PrintTableHeader(tableWidth);
        // Pass the 1-based ID directly
        WriteDataRowByDisplayId(displayId, item, tableWidth);
        DrawHorizontalLine(tableWidth);
    }

    // --- Table Formatting Helpers ---
    // Adjusted widths slightly for better fit
    const int idWidth = 5;          // For display index/sequence number
    const int classWidth = 18;
    const int nameWidth = 20;
    const int priceWidth = 14;      // Assumes N2 format (e.g., 1,234,567.89)
    const int locationWidth = 18;
    const int sizeWidth = 10;       // Assumes N1 format (e.g., 123.4)
    const int typeWidth = 15;
    const int marketValueWidth = 16;// Assumes N2 format
    const int investmentTypeWidth = 16;
    const int floorWidth = 6;
    const int hoaWidth = 10;        // Assumes N2 format
    const int gardenWidth = 10;     // Assumes N1 format
    const int poolWidth = 5;
    const int roomsWidth = 6;
    const int starWidth = 5;
    const int soilWidth = 10;
    const int infraWidth = 6;
    const int padding = 1;          // Space inside cell before/after content
    const int numColumns = 17;      // Number of data columns (ID + 16 properties)

    static int CalculateTableWidth()
    {
        // Sum of column widths
        int totalDataWidth = idWidth + classWidth + nameWidth + priceWidth + locationWidth + sizeWidth + typeWidth + marketValueWidth + investmentTypeWidth + floorWidth + hoaWidth + gardenWidth + poolWidth + roomsWidth + starWidth + soilWidth + infraWidth;
        // Total width includes borders (|) and padding inside cells
        // There are (numColumns + 1) borders
        // Each column has 2*padding space (left and right) - BUT we use PadAndCenter which handles padding within width
        // So, width is sum of defined widths + (numColumns + 1) for the borders
        // Or, let PadAndCenter handle padding within the defined width. Total width = sum(widths) + numColumns + 1 (borders) ?
        // Let's recalculate based on PadAndCenter: total width = sum of widths + number of internal borders (numColumns - 1) + 2 outer borders = sum(widths) + numColumns + 1
        // Simpler: Each cell is "|"+Padding+Content+Padding. Border is part of the next cell start.
        // Total Width = Sum of (colWidth) + (numColumns + 1) for the borders.
        return totalDataWidth + (numColumns + 1); // Add 1 border for each column + 1 outer border
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
        // Use PadAndCenter for headers as well
        Console.Write($"|{PadAndCenter("Idx", idWidth)}"); // Changed header to "Idx"
        Console.Write($"|{PadAndCenter("Class", classWidth)}");
        Console.Write($"|{PadAndCenter("Name", nameWidth)}");
        Console.Write($"|{PadAndCenter("Price", priceWidth)}");
        Console.Write($"|{PadAndCenter("Location", locationWidth)}");
        Console.Write($"|{PadAndCenter("Size", sizeWidth)}");
        Console.Write($"|{PadAndCenter("Type", typeWidth)}");
        Console.Write($"|{PadAndCenter("Market Value", marketValueWidth)}");
        Console.Write($"|{PadAndCenter("Invest Type", investmentTypeWidth)}");
        Console.Write($"|{PadAndCenter("Floor", floorWidth)}");
        Console.Write($"|{PadAndCenter("HOA Fees", hoaWidth)}");
        Console.Write($"|{PadAndCenter("Garden(m2)", gardenWidth)}");
        Console.Write($"|{PadAndCenter("Pool", poolWidth)}");
        Console.Write($"|{PadAndCenter("Rooms", roomsWidth)}");
        Console.Write($"|{PadAndCenter("Stars", starWidth)}");
        Console.Write($"|{PadAndCenter("Soil", soilWidth)}");
        Console.Write($"|{PadAndCenter("Infra.", infraWidth)}");
        Console.WriteLine("|");
        Console.ResetColor();
    }

    // Writes data row using the provided displayId (1-based index or sequence number)
    static void WriteDataRowByDisplayId(int displayId, object item, int tableWidth)
    {
        // Formatting helpers
        string FormatDecimal(decimal? d) => d.HasValue ? d.Value.ToString("N2", CultureInfo.InvariantCulture) : "-";
        string FormatDouble(double? d) => d.HasValue ? d.Value.ToString("N1", CultureInfo.InvariantCulture) : "-";
        string FormatBool(bool? b) => b.HasValue ? (b.Value ? "Yes" : "No") : "-";
        string FormatInt(int? i) => i.HasValue ? i.Value.ToString() : "-";
        string FormatString(string? s) => string.IsNullOrWhiteSpace(s) ? "-" : s;

        Type itemType = item.GetType();

        // Use GetPropertyValue for safe access
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

        // Use PadAndCenter for data cells
        Console.Write($"|{PadAndCenter(displayId.ToString(), idWidth)}"); // Use the passed displayId
        Console.Write($"|{PadAndCenter(itemType.Name, classWidth)}");
        Console.Write($"|{PadAndCenter(name, nameWidth)}");
        Console.Write($"|{PadAndCenter(fPrice, priceWidth)}");
        Console.Write($"|{PadAndCenter(loc, locationWidth)}");
        Console.Write($"|{PadAndCenter(fSize, sizeWidth)}");
        Console.Write($"|{PadAndCenter(type, typeWidth)}");
        Console.Write($"|{PadAndCenter(fMktVal, marketValueWidth)}");
        Console.Write($"|{PadAndCenter(invType, investmentTypeWidth)}");
        Console.Write($"|{PadAndCenter(fFloor, floorWidth)}");
        Console.Write($"|{PadAndCenter(fHoa, hoaWidth)}");
        Console.Write($"|{PadAndCenter(fGarden, gardenWidth)}");
        Console.Write($"|{PadAndCenter(fPool, poolWidth)}");
        Console.Write($"|{PadAndCenter(fRooms, roomsWidth)}");
        Console.Write($"|{PadAndCenter(fStar, starWidth)}");
        Console.Write($"|{PadAndCenter(soil, soilWidth)}");
        Console.Write($"|{PadAndCenter(fInfra, infraWidth)}");
        Console.WriteLine("|");
    }


    static void DrawHorizontalLine(int tableWidth)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(new string('-', tableWidth));
        Console.ResetColor();
    }

    static string PadAndCenter(string? value, int totalWidth)
    {
        string val = value ?? "-"; // Use "-" for null values
        if (totalWidth <= 0) return "";

        // Truncate if value is longer than the allowed width minus padding/ellipsis
        const int ellipsisLength = 3;
        int maxContentWidth = totalWidth - (padding * 2); // Space available for content

        if (val.Length > maxContentWidth)
        {
            if (maxContentWidth >= ellipsisLength)
            {
                val = val.Substring(0, maxContentWidth - ellipsisLength) + "...";
            }
            else
            {
                // Not enough space even for ellipsis, just truncate
                val = val.Substring(0, Math.Max(0, maxContentWidth));
            }
        }
        else
        {
            // Value fits or is shorter
        }

        // Center the (potentially truncated) value within the total width
        int spaces = totalWidth - val.Length;
        int padLeft = spaces / 2;
        int padRight = spaces - padLeft;

        return new string(' ', padLeft) + val + new string(' ', padRight);
    }


    static string CenterString(string s, int width)
    {
        if (string.IsNullOrEmpty(s) || width <= 0) return new string(' ', Math.Max(0, width));
        // Truncate if necessary before centering
        if (s.Length > width)
        {
            s = s.Substring(0, width - 3) + "..."; // Simple truncation for title
        }
        int padding = Math.Max(0, (width - s.Length) / 2);
        return new string(' ', padding) + s + new string(' ', Math.Max(0, width - s.Length - padding));
    }


    // Keep original Truncate if needed elsewhere, but PadAndCenter now handles truncation.
    static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (maxLength <= 0) return "";
        if (value.Length <= maxLength) return value;
        int subLength = Math.Max(0, maxLength - 3); // Space for "..."
        if (subLength == 0) return "...".Substring(0, Math.Min(3, maxLength)); // Handle very small maxLength
        return value.Substring(0, subLength) + "...";
    }


    // --- Reflection Property Getter ---
    // Generic helper to safely get property value, handling potential type mismatches and nulls
    private static TValue? GetPropertyValue<TValue>(object? item, string propertyName)
    {
        if (item == null) return default;

        PropertyInfo? property = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property != null && property.CanRead)
        {
            try
            {
                object? value = property.GetValue(item);
                if (value == null)
                {
                    // If TValue is a value type that's not nullable, we can't return null.
                    // Return default(TValue) which is 0 for numbers, false for bool, etc.
                    // If TValue is nullable or a reference type, default is null, which is fine.
                    return default;
                }

                // Direct cast if types match exactly
                if (value is TValue correctlyTyped) return correctlyTyped;

                // Handle Nullable<T> case where TValue is the nullable type itself
                Type tValueType = typeof(TValue);
                Type? underlyingTValue = Nullable.GetUnderlyingType(tValueType);
                if (underlyingTValue != null && underlyingTValue == property.PropertyType)
                {
                    // The property value is non-null and matches the underlying type of TValue (e.g., int matches int? )
                    // We need to create a Nullable<TValue> instance.
                    try { return (TValue)Convert.ChangeType(value, underlyingTValue, CultureInfo.InvariantCulture); } catch { /* Conversion failed */ }
                }
                // Handle case where TValue is the underlying type but the property is Nullable<T>
                else if (tValueType == property.PropertyType && Nullable.GetUnderlyingType(property.PropertyType) != null)
                {
                    // This case is less common for 'get', usually needed for 'set'
                    // But if value is not null, try converting
                    try { return (TValue)Convert.ChangeType(value, tValueType, CultureInfo.InvariantCulture); } catch { /* Conversion failed */ }
                }


                // Attempt conversion if types don't match directly (e.g., int to decimal?)
                // Be cautious with implicit conversions, prefer explicit checks if possible.
                try
                {
                    // Use Convert.ChangeType for broader compatibility, using InvariantCulture
                    return (TValue)Convert.ChangeType(value, underlyingTValue ?? tValueType, CultureInfo.InvariantCulture);
                }
                catch (InvalidCastException) { /* Conversion not possible */ }
                catch (FormatException) { /* String conversion failed */ }
                catch (OverflowException) { /* Value too large/small */ }
                catch (Exception ex) // Catch unexpected conversion errors
                {
                    System.Diagnostics.Debug.WriteLine($"Reflection Conversion Error getting '{propertyName}' from {value.GetType()} to {typeof(TValue)}: {ex.Message}");
                }
            }
            catch (Exception ex) // Catch errors during GetValue itself
            {
                System.Diagnostics.Debug.WriteLine($"Reflection Error getting '{propertyName}': {ex.Message}");
            }
        }
        // Property not found, not readable, or conversion failed
        return default;
    }
    private static bool IsNumericType(Type type)
    {
        if (type == null) return false;
        // Check for nullable numeric types
        Type underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
        {
            type = underlyingType; // Check the underlying type
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.SByte:
            case TypeCode.Single:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return true;
            default:
                return false;
        }
    }


    // --- Container State Helpers ---

    static bool IsContainerEmpty(out int count)
    {
        count = 0;
        bool isEmpty = true;

        if (activeContainerType == ContainerType.Array && containerArray != null)
        {
            // Use the container's method to check count
            count = containerArray.GetCount();
            isEmpty = (count == 0);
        }
        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
        {
            count = containerList.Count; // Use property
            isEmpty = (count == 0);
        }
        else // No active container or it's null
        {
            isEmpty = true;
        }

        // Print message only if a container type is active but empty
        if (isEmpty && activeContainerType != ContainerType.None)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The active container is empty.");
            Console.ResetColor();
        }
        // Print message if no container is selected at all
        else if (activeContainerType == ContainerType.None)
        {
            PrintErrorMessage("No container selected. Please use option 1 or 2 first.");
            isEmpty = true; // Treat as empty for operational purposes
        }

        return isEmpty;
    }

    // Not currently used, but could be helpful
    static int GetActiveContainerCount()
    {
        if (activeContainerType == ContainerType.Array && containerArray != null)
        {
            return containerArray.GetCount();
        }
        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
        {
            return containerList.Count;
        }
        return 0;
    }

    // Gets the next insertion ID (0-based) that *will be assigned*
    static int GetNextInsertionId()
    {
        if (activeContainerType == ContainerType.Array && containerArray != null)
        {
            return containerArray.GetInsertionId();
        }
        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
        {
            return containerList.GetNextInsertionId();
        }
        return 0; // No container active or initialized
    }

    // Finds the current 0-based index of an item by reference equality
    private static int FindIndexByReference(IName itemToFind)
    {
        if (itemToFind == null) return -1;

        if (activeContainerType == ContainerType.Array && containerArray != null)
        {
            // Get the current internal array (or a safe copy)
            IName?[] currentItems = containerArray.GetItems(); // GetItems now returns populated part
            for (int i = 0; i < currentItems.Length; i++) // Iterate through the returned array
            {
                // Use reference equality for exact match
                if (object.ReferenceEquals(currentItems[i], itemToFind))
                {
                    return i; // Return 0-based index
                }
            }
        }
        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
        {
            var node = containerList.First;
            int index = 0;
            while (node != null)
            {
                if (object.ReferenceEquals(node.Data, itemToFind))
                {
                    return index; // Return 0-based index
                }
                node = node.Next;
                index++;
            }
        }
        return -1; // Not found
    }

    // Gets the 0-based insertion ID associated with an item (by finding its current index first)
    private static int GetInsertionIdForItem(IName itemToFind)
    {
        if (itemToFind == null) return -1;

        // Find the current 0-based index of the item
        int index = FindIndexByReference(itemToFind);
        if (index == -1) return -1; // Item not found in current structure

        try
        {
            if (activeContainerType == ContainerType.Array && containerArray != null)
            {
                // GetInsertionOrder should return the order mapping for the current items
                int[] order = containerArray.GetInsertionOrder();
                if (index < order.Length) // Check if index is valid for the order array
                {
                    return order[index]; // Return the 0-based ID at that index
                }
                else { System.Diagnostics.Debug.WriteLine($"Warning: Index {index} out of bounds for insertion Order Array (Length: {order.Length})"); }
            }
            else if (activeContainerType == ContainerType.LinkedList && containerList != null)
            {
                // GetInsertionOrder returns a copy of the current ID list
                List<int> order = containerList.GetInsertionOrder();
                if (index < order.Count) // Check if index is valid for the order list
                {
                    return order[index]; // Return the 0-based ID at that index
                }
                else { System.Diagnostics.Debug.WriteLine($"Warning: Index {index} out of bounds for insertion Order List (Count: {order.Count})"); }
            }
        }
        catch (Exception ex)
        {
            // Log unexpected errors during ID retrieval
            System.Diagnostics.Debug.WriteLine($"Error in GetInsertionIdForItem for index {index}: {ex.Message}");
        }
        return -1; // ID not found or error occurred
    }

    // Gets an item by 0-based Insertion ID from the active container using the indexer
    private static IName? GetItemByInsertionId(int insertionId) // Accepts 0-based ID
    {
        try
        {
            if (activeContainerType == ContainerType.Array && containerArray != null)
            {
                return containerArray[insertionId]; // Use 0-based ID with indexer
            }
            else if (activeContainerType == ContainerType.LinkedList && containerList != null)
            {
                return containerList[insertionId]; // Use 0-based ID with indexer
            }
        }
        catch (IndexOutOfRangeException)
        {
            // ID not found by indexer, return null (or re-throw depending on desired behavior)
            return null;
        }
        catch (Exception ex)
        {
            PrintErrorMessage($"Unexpected error fetching item by internal insertion ID {insertionId}: {ex.Message}");
            return null;
        }
        return null; // No active container
    }

    // Gets an item by its current 0-based index in the container structure
    private static IName? GetItemByCurrentIndex(int index)
    {
        if (index < 0) return null;

        if (activeContainerType == ContainerType.Array && containerArray != null)
        {
            // Need direct access or a method that guarantees order if GetItems doesn't
            // Let's assume direct access for now, requires careful implementation
            // If GetItems returns a copy, this might not work reliably after modifications.
            // Modify Container<T> to have a GetByIndex method if needed.
            // For now, try using GetItems() copy:
            IName?[] items = containerArray.GetItems(); // Gets copy of current items
            int count = items.Length; // Use length of the returned array
            if (index < count)
            {
                return items[index]; // Return item from the copy at that index
            }
        }
        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
        {
            // Traverse the list to the specified index
            if (index < containerList.Count) // Check against current count
            {
                var node = containerList.First;
                int i = 0;
                while (node != null && i < index)
                {
                    node = node.Next;
                    i++;
                }
                return node?.Data; // Return data if node is found, null otherwise
            }
        }
        return null; // Index out of bounds or no active container
    }


    // --- Random Generators ---
    static Product GenerateRandomProduct(Random random)
    {
        string[] names = { "Table", "Chair", "Lamp", "Phone", "Book", "Laptop", "Mug" };
        decimal price = random.Next(10, 1000) + (decimal)random.NextDouble();
        // Ensure price is positive, handle potential zero from random
        return new Product(names[random.Next(names.Length)] + "-" + random.Next(100), Math.Max(0.01m, Math.Round(price, 2)));
    }

    static RealEstate GenerateRandomRealEstate(Random random)
    {
        string[] names = { "Cozy Apt", "Luxury Villa", "Small House", "Big Mansion", "Downtown Loft" };
        string[] locations = { "New York", "London", "Paris", "Tokyo", "Kyiv", "Berlin", "Sydney" };
        string[] types = { "Residential", "Commercial", "Industrial", "Mixed-Use" };
        decimal price = random.Next(100000, 1000000) + (decimal)random.NextDouble() * 1000;
        double size = random.Next(50, 500) + random.NextDouble() * 10;
        return new RealEstate(names[random.Next(names.Length)], Math.Max(0.01m, Math.Round(price, 2)), locations[random.Next(locations.Length)], Math.Max(1.0, Math.Round(size, 1)), types[random.Next(types.Length)]);
    }

    static RealEstateInvestment GenerateRandomRealEstateInvestment(Random random)
    {
        string[] names = { "Office Bldg", "Shopping Mall", "Warehouse", "Apt Complex", "Data Center" };
        string[] locations = { "Chicago", "Los Angeles", "Houston", "Phoenix", "Philadelphia", "Dallas" };
        string[] invTypes = { "REIT", "Direct Prop", "Mortgage Fund", "Syndication" };
        decimal price = random.Next(500000, 5000000) + (decimal)random.NextDouble() * 10000;
        decimal marketValue = price * (decimal)(0.8 + random.NextDouble() * 0.4); // Market value related to price
        // Ensure market value is positive
        return new RealEstateInvestment(names[random.Next(names.Length)], Math.Max(0.01m, Math.Round(price, 2)), locations[random.Next(locations.Length)], Math.Max(0.01m, Math.Round(marketValue, 2)), invTypes[random.Next(invTypes.Length)]);
    }

    static Apartment GenerateRandomApartment(Random random)
    {
        string[] names = { "Studio Apt", "1BR Apt", "2BR Apt", "Penthouse", "Garden Apt" };
        string[] locations = { "Miami", "San Francisco", "Seattle", "Boston", "Denver", "Austin" };
        string[] types = { "Condo", "Co-op", "Rental Unit", "Loft" };
        decimal price = random.Next(200000, 800000) + (decimal)random.NextDouble() * 500;
        double size = random.Next(40, 150) + random.NextDouble() * 5;
        int floor = random.Next(1, 31); // Ensure floor is at least 1
        decimal hoa = random.Next(50, 500) + (decimal)random.NextDouble() * 50;
        return new Apartment(names[random.Next(names.Length)], Math.Max(0.01m, Math.Round(price, 2)), locations[random.Next(locations.Length)], Math.Max(1.0, Math.Round(size, 1)), types[random.Next(types.Length)], floor, Math.Max(0m, Math.Round(hoa, 2))); // Ensure positive price/size, non-negative HOA
    }

    static House GenerateRandomHouse(Random random)
    {
        string[] names = { "Bungalow", "Townhouse", "Ranch", "Cottage", "Colonial" };
        string[] locations = { "Atlanta", "Dallas", "San Diego", "Orlando", "Las Vegas", "Nashville" };
        string[] types = { "Single-family", "Multi-family", "Duplex" };
        decimal price = random.Next(300000, 1200000) + (decimal)random.NextDouble() * 1000;
        double size = random.Next(100, 400) + random.NextDouble() * 15;
        double gardenSize = random.Next(0, 1000) + random.NextDouble() * 100; // Ensure non-negative
        bool pool = random.Next(3) == 0; // 1/3 chance of having a pool
        return new House(names[random.Next(names.Length)], Math.Max(0.01m, Math.Round(price, 2)), locations[random.Next(locations.Length)], Math.Max(1.0, Math.Round(size, 1)), types[random.Next(types.Length)], Math.Max(0.0, Math.Round(gardenSize, 1)), pool); // Ensure positive price/size, non-negative garden
    }

    static Hotel GenerateRandomHotel(Random random)
    {
        string[] names = { "Luxury Hotel", "Budget Inn", "Resort & Spa", "Boutique Hotel", "Airport Motel" };
        string[] locations = { "Hawaii", "Bali", "Maldives", "Fiji", "Santorini", "Las Vegas Strip" };
        string[] invTypes = { "Hospitality REIT", "Hotel Mgmt", "Timeshare", "Franchise" };
        decimal price = random.Next(1000000, 10000000) + (decimal)random.NextDouble() * 50000;
        decimal marketValue = price * (decimal)(0.9 + random.NextDouble() * 0.3); // Market value related
        int rooms = random.Next(20, 501); // Ensure at least 20 rooms
        int rating = random.Next(1, 6); // Rating 1-5
        return new Hotel(names[random.Next(names.Length)], Math.Max(0.01m, Math.Round(price, 2)), locations[random.Next(locations.Length)], Math.Max(1.0m, Math.Round(marketValue, 2)), invTypes[random.Next(invTypes.Length)], Math.Max(1, rooms), rating); // Ensure positive values, valid rooms/rating
    }

    static LandPlot GenerateRandomLandPlot(Random random)
    {
        string[] names = { "Farmland", "Forest", "Comm Land", "Resid Land", "Waterfront" };
        string[] locations = { "Rural Area", "Suburban Edge", "Urban Infill", "Coastal Zone", "Mountain Base" };
        string[] invTypes = { "Land Banking", "Development", "Agriculture", "Conservation" };
        string[] soilTypes = { "Loam", "Clay", "Sand", "Silt", "Peat", "Chalky", "Unknown" }; // Added Unknown
        decimal price = random.Next(50000, 500000) + (decimal)random.NextDouble() * 2000;
        decimal marketValue = price * (decimal)(0.7 + random.NextDouble() * 0.6); // Market value related
        bool infra = random.Next(2) == 0; // 50% chance of infra access
        return new LandPlot(names[random.Next(names.Length)], Math.Max(0.01m, Math.Round(price, 2)), locations[random.Next(locations.Length)], Math.Max(1.0m, Math.Round(marketValue, 2)), invTypes[random.Next(invTypes.Length)], soilTypes[random.Next(soilTypes.Length)], infra); // Ensure positive values
    }


    // --- Manual Creation Methods ---

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
        double size = ReadDouble("Enter Size (m^2 > 0): ", minValue: 0.01);
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
        double size = ReadDouble("Enter Size (m^2 > 0): ", minValue: 0.01);
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
        double size = ReadDouble("Enter Size (m^2 > 0): ", minValue: 0.01);
        string type = ReadString("Enter Type (e.g., Single-family): ");
        double gardenSize = ReadDouble("Enter Garden Size (m^2 >= 0): ", minValue: 0.0);
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
        string? input;
        do
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(prompt);
            Console.ResetColor();
            input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                PrintErrorMessage("Input cannot be empty or just whitespace.");
            }
        } while (string.IsNullOrWhiteSpace(input));
        return input;
    }

    static decimal ReadDecimal(string prompt, decimal? minValue = null, decimal? maxValue = null)
    {
        decimal value;
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(prompt);
            Console.ResetColor();
            string? input = Console.ReadLine();
            // Use InvariantCulture for consistent decimal separator parsing (.)
            if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                bool minOk = minValue == null || value >= minValue.Value;
                bool maxOk = maxValue == null || value <= maxValue.Value;

                if (minOk && maxOk)
                {
                    return value;
                }
                else
                {
                    // Build clearer error message
                    string rangeMsg = "";
                    if (minValue != null) rangeMsg += $" >= {minValue.Value.ToString("N2", CultureInfo.InvariantCulture)}";
                    if (minValue != null && maxValue != null) rangeMsg += " and";
                    if (maxValue != null) rangeMsg += $" <= {maxValue.Value.ToString("N2", CultureInfo.InvariantCulture)}";
                    PrintErrorMessage($"Value must be in the range [{rangeMsg.Trim()}]. You entered: {value.ToString("N2", CultureInfo.InvariantCulture)}");
                }
            }
            else
            {
                PrintErrorMessage($"Invalid decimal format. Please use '.' as the decimal separator (e.g., 123.45). Input was: '{input}'");
            }
        }
    }
    static double ReadDouble(string prompt, double? minValue = null, double? maxValue = null)
    {
        double value;
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(prompt);
            Console.ResetColor();
            string? input = Console.ReadLine();
            if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                bool minOk = minValue == null || value >= minValue.Value;
                bool maxOk = maxValue == null || value <= maxValue.Value;
                if (minOk && maxOk)
                {
                    return value;
                }
                else
                {
                    string rangeMsg = "";
                    if (minValue != null) rangeMsg += $" >= {minValue.Value.ToString("N1", CultureInfo.InvariantCulture)}";
                    if (minValue != null && maxValue != null) rangeMsg += " and";
                    if (maxValue != null) rangeMsg += $" <= {maxValue.Value.ToString("N1", CultureInfo.InvariantCulture)}";
                    PrintErrorMessage($"Value must be in the range [{rangeMsg.Trim()}]. You entered: {value.ToString("N1", CultureInfo.InvariantCulture)}");
                }
            }
            else
            {
                PrintErrorMessage($"Invalid number format. Please use '.' as the decimal separator (e.g., 12.3). Input was: '{input}'");
            }
        }
    }
    static int ReadInt(string prompt, int? minValue = null, int? maxValue = null)
    {
        int value;
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(prompt);
            Console.ResetColor();
            string? input = Console.ReadLine();
            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                bool minOk = minValue == null || value >= minValue.Value;
                bool maxOk = maxValue == null || value <= maxValue.Value;
                if (minOk && maxOk)
                {
                    return value;
                }
                else
                {
                    string minStr = minValue?.ToString(CultureInfo.InvariantCulture) ?? "-infinity";
                    string maxStr = maxValue?.ToString(CultureInfo.InvariantCulture) ?? "+infinity";
                    PrintErrorMessage($"Value must be between {minStr} and {maxStr}. You entered: {value}");
                }
            }
            else
            {
                PrintErrorMessage($"Invalid integer format. Input was: '{input}'");
            }
        }
    }
    static bool ReadBool(string prompt)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(prompt);
            Console.ResetColor();
            string input = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";
            if (input == "true" || input == "1" || input == "yes" || input == "y") return true;
            if (input == "false" || input == "0" || input == "no" || input == "n") return false;
            PrintErrorMessage("Invalid boolean input. Use true/false/yes/no/1/0.");
        }
    }

    // --- Serialization / Deserialization Handlers ---

    static void HandleSerializeContainer()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Serialize Active {activeContainerType} Container ---");
        Console.ResetColor();

        object? activeContainerObject = null;
        if (activeContainerType == ContainerType.Array && containerArray != null)
        {
            activeContainerObject = containerArray;
            if (containerArray.IsEmpty(true)) return; // Check if empty
        }
        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
        {
            activeContainerObject = containerList;
            if (containerList.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("The active container is empty.");
                Console.ResetColor();
                return;
            }
        }
        else
        {
            PrintErrorMessage("No active and non-empty container to serialize.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter filename to save (without extension, e.g., 'mydata'): ");
        Console.ResetColor();
        string? filename = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(filename))
        {
            PrintErrorMessage("Invalid filename.");
            return;
        }

        try
        {
            // Ensure filename doesn't contain invalid path characters (basic check)
            if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                PrintErrorMessage($"Filename '{filename}' contains invalid characters.");
                return;
            }

            string fullPath = ContainerSerializer.SerializeContainer(activeContainerObject, filename);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Container successfully serialized to: {fullPath}");
            Console.ResetColor();
        }
        catch (IOException ex)
        {
            PrintErrorMessage($"File IO error during serialization: {ex.Message}");
        }
        catch (InvalidOperationException ex) // Catch errors like non-serializable types
        {
            PrintErrorMessage($"Serialization Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            PrintErrorMessage($"An unexpected error occurred during serialization: {ex.GetType().Name} - {ex.Message}");
        }
    }

    static void HandleDeserializeContainer()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- Deserialize Container from File ---");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter filename to load (without extension, e.g., 'mydata'): ");
        Console.ResetColor();
        string? filename = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(filename))
        {
            PrintErrorMessage("Invalid filename.");
            return;
        }

        if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            PrintErrorMessage($"Filename '{filename}' contains invalid characters.");
            return;
        }

        object deserializedContainer;
        try
        {
            deserializedContainer = ContainerSerializer.DeserializeContainer(filename);
        }
        catch (FileNotFoundException) { return; }
        catch (IOException) { return; }
        catch (InvalidOperationException) { return; }
        catch (Exception ex) 
        {
            PrintErrorMessage($"An unexpected error occurred calling deserialization: {ex.GetType().Name} - {ex.Message}");
            return;
        }


        if (deserializedContainer == null)
        {
            Console.WriteLine("Deserialization failed.");
            return;
        }

        int loadedItemCount = 0;
        if (deserializedContainer is Container<IName> deserializedContainerArray)
        {
            loadedItemCount = deserializedContainerArray.GetCount();
        }
        else if (deserializedContainer is ContainerLinkedList<IName> deserializedContainerList)
        {
            loadedItemCount = deserializedContainerList.Count;
        }
        else
        {
            PrintErrorMessage($"Deserialization returned an unexpected object type: {deserializedContainer.GetType().Name}");
            return;
        }

        if (loadedItemCount == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Deserialized container is empty.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Successfully deserialized {loadedItemCount} items.");
            Console.ResetColor();
        }


        // Confirm switching the active container
        bool switchConfirmed = true;
        if (activeContainerType != ContainerType.None)
        {
            bool currentHasItems = (activeContainerType == ContainerType.Array && containerArray?.GetCount() > 0) ||
                                  (activeContainerType == ContainerType.LinkedList && containerList?.GetCount() > 0);
            if (currentHasItems)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"Replace the current {activeContainerType} container with the deserialized data? (y/n): ");
                Console.ResetColor();
                switchConfirmed = (Console.ReadLine()?.Trim().ToLower() == "y");
            }
        }

        if (switchConfirmed)
        {
            if (deserializedContainer is Container<IName> _containerArray)
            {
                containerArray = _containerArray;
                containerList = null;
                activeContainerType = ContainerType.Array;
            }
            if (deserializedContainer is ContainerLinkedList<IName> _containerList)
            {
                containerList = _containerList;
                containerArray = null;
                activeContainerType = ContainerType.Array;
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Active container switched to the deserialized {activeContainerType} container.");
            Console.ResetColor();
            HandleShowContainer(); 
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Switch cancelled. Keeping the current active container.");
            Console.ResetColor();
        }
    }

}

//class Program
//{
//    static Container<IName>? containerArray = null;
//    static ContainerLinkedList<IName>? containerList = null;
//    static ContainerType activeContainerType = ContainerType.None;
//    static Random random = new Random();

//    static void Main()
//    {
//        while (true)
//        {
//            PrintMenu();
//            string choice = Console.ReadLine()?.ToLower() ?? "";

//            try
//            {
//                switch (choice)
//                {
//                    case "1": HandleContainerSelectionAndAction(HandleAutomaticGeneration); break;
//                    case "2": HandleContainerSelectionAndAction(HandleManualInput); break;
//                    case "3": HandleShowContainer(); break;
//                    case "4": HandleGetElementByInsertionId(); break;
//                    case "5": HandleGetElementByName(); break;
//                    case "6": HandleChangeItemByInsertionId(); break;
//                    case "7": HandleChangeItemByName(); break;
//                    case "8": HandleSortContainer(); break;
//                    case "9": HandleRemoveElementByIndex(); break;
//                    case "10": HandleReverseGenerator(); break;
//                    case "11": HandleSublineGenerator(); break;
//                    case "12": HandleSortedPriceGenerator(); break;
//                    case "13": HandleSortedNameGenerator(); break;
//                    case "q":
//                        Console.ForegroundColor = ConsoleColor.Yellow;
//                        Console.WriteLine("Exiting...");
//                        Console.ResetColor();
//                        return;
//                    default:
//                        Console.ForegroundColor = ConsoleColor.Red;
//                        Console.WriteLine("Invalid choice. Please try again.");
//                        Console.ResetColor();
//                        break;
//                }
//            }
//            catch (ValueLessThanZero ex)
//            {
//                PrintErrorMessage($"Input/Validation Error: {ex.Message}");
//            }
//            catch (FormatException ex)
//            {
//                PrintErrorMessage($"Input Format Error: Invalid format entered. {ex.Message}");
//            }
//            catch (IndexOutOfRangeException ex)
//            {
//                PrintErrorMessage($"Error: Index out of range. {ex.Message}");
//            }
//            catch (KeyNotFoundException ex)
//            {
//                PrintErrorMessage($"Error: Key (e.g., Insertion ID) not found. {ex.Message}");
//            }
//            catch (ArgumentException ex)
//            {
//                PrintErrorMessage($"Argument Error: {ex.Message}");
//            }
//            catch (TargetInvocationException ex)
//            {
//                Exception inner = ex.InnerException ?? ex;
//                while (inner.InnerException != null) { inner = inner.InnerException; }
//                PrintErrorMessage($"Error during operation: {inner.GetType().Name} - {inner.Message}");
//            }
//            catch (Exception ex)
//            {
//                PrintErrorMessage($"An unexpected error occurred: {ex.GetType().Name} - {ex.Message}");
//            }
//            finally
//            {
//                Console.ResetColor();
//            }
//        }
//    }

//    // --- Menu Printing ---
//    static void PrintMenu()
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine("\n------ Menu ------");
//        Console.ForegroundColor = ConsoleColor.DarkGray;
//        Console.WriteLine($"Active Container: {activeContainerType}");
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.WriteLine("1. Automatic Generation (Select/Switch Container)");
//        Console.WriteLine("2. Manual Input (Select/Switch Container)");
//        Console.WriteLine("3. Show Active Container");
//        Console.ForegroundColor = ConsoleColor.White;
//        Console.WriteLine("#. --- Getters ---");
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.WriteLine("4. Get Element by Insertion ID (1-based)");
//        Console.WriteLine("5. Get Elements by Name");
//        // Console.WriteLine("6. Get Elements by Price");
//        Console.ForegroundColor = ConsoleColor.White;
//        Console.WriteLine("#. --- Modifiers ---");
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.WriteLine("6. Change Item by Insertion ID (1-based)");
//        Console.WriteLine("7. Change Item by Name");
//        Console.ForegroundColor = ConsoleColor.White;
//        Console.WriteLine("#. --- Container Operations ---");
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.WriteLine("8. Sort Active Container (In-Place)");
//        Console.WriteLine("9. Remove Element by Current Index (0-based)");
//        Console.ForegroundColor = ConsoleColor.White;
//        Console.WriteLine("#. --- Generators (Non-Mutating Iterators) ---");
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.WriteLine("10. Show Elements in Reverse Order");
//        Console.WriteLine("11. Show Elements with Name Containing Substring");
//        Console.WriteLine("12. Show Elements Sorted by Price (Generator)");
//        Console.WriteLine("13. Show Elements Sorted by Name (Generator)");
//        Console.ForegroundColor = ConsoleColor.White;
//        Console.WriteLine("#. --- ### ### ### ---");
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.WriteLine("q. Exit");
//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.Write("Enter your choice: ");
//        Console.ResetColor();
//    }

//    static void PrintErrorMessage(string message)
//    {
//        Console.ForegroundColor = ConsoleColor.Red;
//        Console.WriteLine($"\nERROR: {message}");
//        Console.ResetColor();
//    }

//    // --- Container Selection Logic ---
//    static void HandleContainerSelectionAndAction(Action actionToPerform)
//    {
//        ContainerType chosenType = AskContainerType();
//        if (chosenType == ContainerType.None)
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine("Operation cancelled.");
//            Console.ResetColor();
//            return;
//        }

//        if (activeContainerType != chosenType || (activeContainerType == ContainerType.None))
//        {
//            bool switchConfirmed = true;
//            if (activeContainerType != ContainerType.None && activeContainerType != chosenType)
//            {
//                Console.ForegroundColor = ConsoleColor.Yellow;
//                Console.Write($"Switching to {chosenType} container will clear the current {activeContainerType} container. Continue? (y/n): ");
//                Console.ResetColor();
//                switchConfirmed = (Console.ReadLine()?.Trim().ToLower() == "y");
//            }

//            if (switchConfirmed)
//            {
//                Console.ForegroundColor = ConsoleColor.Magenta;
//                Console.WriteLine($"\nInitializing {chosenType} container...");
//                Console.ResetColor();
//                containerArray = null;
//                containerList = null;
//                activeContainerType = chosenType;
//                if (activeContainerType == ContainerType.Array)
//                {
//                    containerArray = new Container<IName>();
//                }
//                else // LinkedList
//                {
//                    containerList = new ContainerLinkedList<IName>();
//                }
//            }
//            else
//            {
//                Console.ForegroundColor = ConsoleColor.Yellow;
//                Console.WriteLine("Switch cancelled. Keeping the current container.");
//                Console.ResetColor();
//                return;
//            }
//        }
//        else
//        {
//            Console.ForegroundColor = ConsoleColor.Green;
//            Console.WriteLine($"\nContinuing with the active {activeContainerType} container.");
//            Console.ResetColor();
//        }

//        actionToPerform();
//    }

//    static ContainerType AskContainerType()
//    {
//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.WriteLine("\nSelect Container Type:");
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.WriteLine("1. Array-based Container");
//        Console.WriteLine("2. Linked List Container");
//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.Write("Enter choice (1 or 2, or any other key to cancel): ");
//        Console.ResetColor();
//        string choice = Console.ReadLine() ?? "";
//        return choice switch
//        {
//            "1" => ContainerType.Array,
//            "2" => ContainerType.LinkedList,
//            _ => ContainerType.None,
//        };
//    }


//    // --- Action Handlers ---

//    static void HandleAutomaticGeneration()
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine("\n--- Automatic Generation ---");
//        Console.ResetColor();
//        Console.Write("Enter number of elements to generate: ");
//        if (int.TryParse(Console.ReadLine(), out int count) && count > 0)
//        {
//            if (activeContainerType == ContainerType.Array)
//            {
//                AutomaticGenerationArray(containerArray!, random, count);
//                DemonstrateIndexersArray(containerArray!, random);
//            }
//            else // LinkedList
//            {
//                AutomaticGenerationList(containerList!, random, count);
//                DemonstrateIndexersList(containerList!, random);
//            }
//            Console.ForegroundColor = ConsoleColor.Green;
//            Console.WriteLine($"\nAutomatic generation of {count} elements complete for {activeContainerType} container.");
//            Console.ResetColor();
//        }
//        else
//        {
//            PrintErrorMessage("Invalid input for count (must be a positive integer). Generation cancelled.");
//        }
//    }

//    static void HandleManualInput()
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"\n--- Manual Input for {activeContainerType} Container ---");
//        Console.ResetColor();
//        if (activeContainerType == ContainerType.Array)
//        {
//            ManualInputArray(containerArray!);
//        }
//        else // LinkedList
//        {
//            ManualInputList(containerList!);
//        }
//    }

//    static void HandleShowContainer()
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"\n--- Show {activeContainerType} Container ---");
//        Console.ResetColor();
//        if (IsContainerEmpty(out int currentCount))
//        {
//            // Message printed by IsContainerEmpty
//            return;
//        }

//        if (activeContainerType == ContainerType.Array)
//        {
//            ShowContainerArray(containerArray!, currentCount);
//        }
//        else // LinkedList
//        {
//            ShowContainerList(containerList!, currentCount);
//        }
//    }

//    // Still gets by Insertion ID, displays current index
//    static void HandleGetElementByInsertionId()
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"\n--- Get Element by Insertion ID from {activeContainerType} Container ---");
//        Console.ResetColor();
//        if (IsContainerEmpty(out _)) return;

//        int maxId = GetNextInsertionId();
//        if (maxId == 0)
//        {
//            PrintErrorMessage("Container is empty, no IDs to get.");
//            return;
//        }
//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.Write($"Enter insertion ID (1 to {maxId}): ");
//        Console.ResetColor();

//        if (int.TryParse(Console.ReadLine(), out int inputId) && inputId >= 1 && inputId <= maxId)
//        {
//            int insertionId = inputId - 1;
//            IName? item = null;
//            try
//            {
//                if (activeContainerType == ContainerType.Array)
//                {
//                    item = containerArray![insertionId];
//                }
//                else // LinkedList
//                {
//                    item = containerList![insertionId];
//                }
//            }
//            catch (IndexOutOfRangeException)
//            {
//                PrintErrorMessage($"Item with insertion ID {inputId} not found or invalid for container structure.");
//                return;
//            }
//            catch (Exception ex)
//            {
//                PrintErrorMessage($"Error accessing item with insertion ID {inputId}: {ex.Message}");
//                return;
//            }


//            if (item == null)
//            {
//                PrintErrorMessage($"Item with insertion ID {inputId} not found (possibly removed or ID never used/valid).");
//                return;
//            }

//            int currentIndex = FindIndexByReference(item);
//            if (currentIndex == -1)
//            {
//                PrintErrorMessage($"Found item by insertion ID {inputId}, but could not determine its current index for display.");
//                return;
//            }

//            Console.ForegroundColor = ConsoleColor.Green;
//            Console.WriteLine($"\nItem Details (Insertion ID: {inputId}, Current Index: {currentIndex}):");
//            Console.ResetColor();
//            DisplayItemTable(currentIndex + 1, item);
//        }
//        else
//        {
//            PrintErrorMessage($"Invalid input. Please enter a valid integer ID between 1 and {maxId}.");
//        }
//    }

//    static void HandleGetElementByName()
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"\n--- Get Elements by Name from {activeContainerType} Container ---");
//        Console.ResetColor();
//        if (IsContainerEmpty(out _)) return;

//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.Write("Enter the Name to search for: ");
//        Console.ResetColor();
//        string name = Console.ReadLine() ?? "";

//        if (string.IsNullOrWhiteSpace(name))
//        {
//            PrintErrorMessage("Invalid input. Name cannot be empty.");
//            return;
//        }

//        List<IName> itemsFoundList = new List<IName>();

//        if (activeContainerType == ContainerType.Array)
//        {
//            IName[]? itemsFoundArray = containerArray![name];
//            if (itemsFoundArray != null)
//            {
//                itemsFoundList.AddRange(itemsFoundArray.Where(i => i != null)!);
//            }
//        }
//        else // LinkedList
//        {
//            List<IName>? itemsFoundLinkedList = containerList![name];
//            if (itemsFoundLinkedList != null)
//            {
//                itemsFoundList.AddRange(itemsFoundLinkedList);
//            }
//        }

//        if (itemsFoundList.Count > 0)
//        {
//            Console.ForegroundColor = ConsoleColor.Green;
//            Console.WriteLine($"\nFound {itemsFoundList.Count} element(s) with Name '{name}':");
//            Console.ResetColor();

//            int tableWidth = CalculateTableWidth();
//            PrintTableHeader(tableWidth);

//            foreach (var foundItem in itemsFoundList)
//            {
//                int currentIndex = FindIndexByReference(foundItem);
//                if (currentIndex != -1)
//                {
//                    WriteDataRowByDisplayId(currentIndex + 1, foundItem, tableWidth);
//                }
//                else
//                {
//                    // Item was found by name indexer but couldn't be located by reference
//                    Console.ForegroundColor = ConsoleColor.Yellow;
//                    string itemStr = foundItem.ToString() ?? "N/A";
//                    Console.WriteLine($"|{PadAndCenter($"Warning: Could not determine current index for item '{itemStr.Substring(0, Math.Min(20, itemStr.Length))}...'", tableWidth - 2)}|");
//                    Console.ResetColor();
//                }
//                DrawHorizontalLine(tableWidth);
//            }
//        }
//        else
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine($"No elements found with Name '{name}'.");
//            Console.ResetColor();
//        }
//    }

//    // Still changes by Insertion ID, displays current id
//    static void HandleChangeItemByInsertionId()
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"\n--- Change Item by Insertion ID in {activeContainerType} Container ---");
//        Console.ResetColor();
//        if (IsContainerEmpty(out _)) return;

//        int maxId = GetNextInsertionId();
//        if (maxId == 0)
//        {
//            PrintErrorMessage("Container is empty, no IDs to modify.");
//            return;
//        }
//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.Write($"Enter item insertion ID to modify (1 to {maxId}): ");
//        Console.ResetColor();

//        if (int.TryParse(Console.ReadLine(), out int inputId) && inputId >= 1 && inputId <= maxId)
//        {
//            int insertionId = inputId - 1;
//            IName? itemToModify = null;

//            try
//            {
//                if (activeContainerType == ContainerType.Array)
//                {
//                    itemToModify = containerArray![insertionId];
//                }
//                else // LinkedList
//                {
//                    itemToModify = containerList![insertionId];
//                }
//            }
//            catch (IndexOutOfRangeException)
//            {
//                PrintErrorMessage($"Item with insertion ID {inputId} not found or invalid for container structure.");
//                return;
//            }
//            catch (Exception ex)
//            {
//                PrintErrorMessage($"Error accessing item with insertion ID {inputId}: {ex.Message}");
//                return;
//            }

//            if (itemToModify == null)
//            {
//                PrintErrorMessage($"Item with insertion ID {inputId} not found (possibly removed or ID never used/valid).");
//                return;
//            }

//            int currentIndex = FindIndexByReference(itemToModify);
//            if (currentIndex == -1)
//            {
//                PrintErrorMessage($"Found item by insertion ID {inputId}, but could not determine its current index for display.");
//                return;
//            }

//            Console.ForegroundColor = ConsoleColor.Green;
//            Console.WriteLine("\nCurrent item details:");
//            Console.ResetColor();
//            DisplayItemTable(currentIndex + 1, itemToModify);

//            ModifyProperty(itemToModify, insertionId);
//        }
//        else
//        {
//            PrintErrorMessage($"Invalid input. Please enter a valid integer ID between 1 and {maxId}.");
//        }
//    }

//    static void HandleChangeItemByName()
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"\n--- Change Item by Name in {activeContainerType} Container ---");
//        Console.ResetColor();
//        if (IsContainerEmpty(out _)) return;

//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.Write("Enter the Name of the item(s) to modify: ");
//        Console.ResetColor();
//        string name = Console.ReadLine() ?? "";

//        if (string.IsNullOrWhiteSpace(name))
//        {
//            PrintErrorMessage("Invalid input. Name cannot be empty.");
//            return;
//        }

//        List<IName> validItems = new List<IName>();
//        if (activeContainerType == ContainerType.Array)
//        {
//            IName[]? itemsFoundArray = containerArray![name];
//            if (itemsFoundArray != null) validItems.AddRange(itemsFoundArray.Where(i => i != null)!);
//        }
//        else // LinkedList
//        {
//            List<IName>? itemsFoundList = containerList![name];
//            if (itemsFoundList != null) validItems.AddRange(itemsFoundList);
//        }


//        if (validItems.Count == 0)
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine($"No valid elements found matching Name '{name}'.");
//            Console.ResetColor();
//            return;
//        }

//        IName itemToModify;
//        int itemInsertionId = -1;
//        int currentDisplayIndex = -1;

//        if (validItems.Count == 1)
//        {
//            itemToModify = validItems[0];
//            itemInsertionId = GetInsertionIdForItem(itemToModify);
//            currentDisplayIndex = FindIndexByReference(itemToModify);

//            if (itemInsertionId == -1 || currentDisplayIndex == -1) { PrintErrorMessage("Could not find ID or index for the item."); return; }

//            Console.ForegroundColor = ConsoleColor.Green;
//            Console.WriteLine($"\nFound one item (Current Index: {currentDisplayIndex + 1}, Insertion ID: {itemInsertionId + 1}):");
//            Console.ResetColor();
//        }
//        else // Multiple items found
//        {
//            Console.ForegroundColor = ConsoleColor.Green;
//            Console.WriteLine($"\nFound {validItems.Count} items with Name '{name}'. Please choose which one to modify:");
//            Console.ResetColor();

//            Dictionary<int, int> choiceToCurrentIndexMap = new Dictionary<int, int>();

//            for (int i = 0; i < validItems.Count; i++)
//            {
//                int currentItemIndex = FindIndexByReference(validItems[i]);
//                if (currentItemIndex != -1)
//                {
//                    string itemInfo = validItems[i].ToString() ?? "N/A";
//                    Console.WriteLine($"{i + 1}. (Index: {currentItemIndex + 1}) {itemInfo}");
//                    choiceToCurrentIndexMap[i + 1] = currentItemIndex;
//                }
//                else
//                {
//                    Console.ForegroundColor = ConsoleColor.Yellow;
//                    string itemStr = validItems[i].ToString() ?? "N/A";
//                    Console.WriteLine($"{i + 1}. (Index: ???) Could not map item - {itemStr}");
//                    Console.ResetColor();
//                }
//            }

//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.Write($"Enter choice (1 to {validItems.Count}): ");
//            Console.ResetColor();

//            if (int.TryParse(Console.ReadLine(), out int choice)
//                && choice >= 1 && choice <= validItems.Count
//                && choiceToCurrentIndexMap.TryGetValue(choice, out int chosenCurrentIndex))
//            {
//                itemToModify = GetItemByCurrentIndex(chosenCurrentIndex);
//                if (itemToModify == null) { PrintErrorMessage("Failed to re-acquire selected item by current index."); return; }

//                itemInsertionId = GetInsertionIdForItem(itemToModify);
//                currentDisplayIndex = chosenCurrentIndex;

//                if (itemInsertionId == -1) { PrintErrorMessage("Could not determine insertion ID for the chosen item."); return; }
//            }
//            else
//            {
//                PrintErrorMessage("Invalid choice or item mapping failed.");
//                return;
//            }
//            Console.ForegroundColor = ConsoleColor.Green;
//            Console.WriteLine($"\nSelected item (Current Index: {currentDisplayIndex + 1}, Insertion ID: {itemInsertionId + 1}):");
//            Console.ResetColor();
//        }

//        // Modify the selected item
//        if (currentDisplayIndex != -1 && itemToModify != null)
//        {
//            DisplayItemTable(currentDisplayIndex + 1, itemToModify);
//            ModifyProperty(itemToModify, itemInsertionId);
//        }
//        else
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine("\nCould not reliably identify the selected item or its index. Modification cancelled.");
//            Console.WriteLine(itemToModify?.ToString() ?? "N/A");
//            Console.ResetColor();
//        }
//    }

//    static void HandleSortContainer()
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"\n--- Sorting {activeContainerType} Container (In-Place) ---");
//        Console.ResetColor();
//        if (IsContainerEmpty(out int currentCount)) return;

//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.WriteLine("Select sort parameter:");
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.WriteLine("1. Sort by Name");
//        Console.WriteLine("2. Sort by Price");
//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.Write("Enter choice (1 or 2): ");
//        Console.ResetColor();

//        string? sortChoice = Console.ReadLine()?.Trim();
//        string sortParameter;
//        Action? sortAction = null;

//        switch (sortChoice)
//        {
//            case "1":
//                sortParameter = "Name";
//                Console.WriteLine("\nSorting by Name...");
//                if (activeContainerType == ContainerType.Array)
//                {
//                    if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
//                    sortAction = containerArray.SortByName;
//                }
//                else // LinkedList
//                {
//                    if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
//                    sortAction = () => containerList.Sort("Name");
//                }
//                break;

//            case "2":
//                sortParameter = "Price";
//                Console.WriteLine("\nSorting by Price...");
//                if (activeContainerType == ContainerType.Array)
//                {
//                    if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
//                    sortAction = containerArray.SortByPrice;
//                }
//                else // LinkedList
//                {
//                    if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
//                    sortAction = () => containerList.Sort("Price");
//                }
//                break;

//            default:
//                PrintErrorMessage("Invalid sort choice.");
//                return;
//        }

//        if (sortAction != null)
//        {
//            try
//            {
//                sortAction.Invoke();
//                Console.ForegroundColor = ConsoleColor.Green;
//                Console.WriteLine($"Container sorted by {sortParameter}.");
//                Console.ResetColor();
//                HandleShowContainer();
//            }
//            catch (Exception ex)
//            {
//                PrintErrorMessage($"Error during sorting by {sortParameter}: {ex.Message}");
//            }
//        }
//        else
//        {
//            // Should not happen if logic above is correct, but as a safeguard
//            PrintErrorMessage("Sort action could not be determined.");
//        }
//    }

//    static void HandleRemoveElementByIndex()
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"\n--- Remove Element by Current Index from {activeContainerType} Container ---");
//        Console.ResetColor();
//        if (IsContainerEmpty(out int currentCount)) return;

//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.Write($"Enter element index to remove (0 to {currentCount - 1}): ");
//        Console.ResetColor();

//        if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < currentCount)
//        {
//            IName? removedItem = null;
//            int removedItemInsertionId = -1;

//            try
//            {
//                IName? itemToRemove = GetItemByCurrentIndex(index);

//                if (itemToRemove != null)
//                {
//                    removedItemInsertionId = GetInsertionIdForItem(itemToRemove);
//                }
//                else
//                {
//                    PrintErrorMessage($"Could not retrieve item at index {index} before removal.");
//                }


//                if (activeContainerType == ContainerType.Array)
//                {
//                    removedItem = containerArray!.RemoveById(index);
//                }
//                else // LinkedList
//                {
//                    removedItem = containerList!.RemoveByIndex(index);
//                }

//                if (removedItem != null)
//                {
//                    Console.ForegroundColor = ConsoleColor.DarkCyan;
//                    string idString = removedItemInsertionId != -1 ? $"(original Insertion ID: {removedItemInsertionId + 1})" : "(original Insertion ID unknown)";
//                    Console.WriteLine($"\nElement at index {index} {idString} was removed:");
//                    Console.WriteLine(removedItem.ToString() ?? "Removed item details unavailable.");
//                    Console.ResetColor();
//                }
//                else
//                {
//                    PrintErrorMessage($"Error: Failed to remove item at index {index}. Item might have been null unexpectedly or removal failed.");
//                }
//            }
//            catch (Exception ex)
//            {
//                PrintErrorMessage($"Error during removal at index {index}: {ex.Message}");
//            }
//        }
//        else
//        {
//            PrintErrorMessage($"Invalid input. Please enter a valid index between 0 and {currentCount - 1}.");
//        }
//    }

//    // --- Generator Demonstrations ---

//    static void HandleReverseGenerator()
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"\n--- Generator: Items in Reverse Order ({activeContainerType} Container) ---");
//        Console.ResetColor();
//        if (IsContainerEmpty(out _)) return;

//        IEnumerable<IName> reversedItems;
//        if (activeContainerType == ContainerType.Array)
//        {
//            reversedItems = containerArray!.GetReverseArray();
//        }
//        else // LinkedList
//        {
//            reversedItems = containerList!.GetReverseArray();
//        }

//        int tableWidth = CalculateTableWidth();
//        PrintTableHeader(tableWidth);
//        int count = 0;
//        foreach (var item in reversedItems)
//        {
//            WriteDataRowByDisplayId(count + 1, item, tableWidth);
//            DrawHorizontalLine(tableWidth);
//            count++;
//        }
//        if (count == 0)
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine($"|{PadAndCenter("(No items yielded by generator)", tableWidth - 2)}|");
//            DrawHorizontalLine(tableWidth);
//            Console.ResetColor();
//        }
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"Reverse generator yielded {count} items.");
//        Console.ResetColor();
//    }

//    static void HandleSublineGenerator()
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"\n--- Generator: Items with Name Containing Substring ({activeContainerType} Container) ---");
//        Console.ResetColor();
//        if (IsContainerEmpty(out _)) return;

//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.Write("Enter substring to search for in Name: ");
//        Console.ResetColor();
//        string subline = Console.ReadLine() ?? "";

//        if (string.IsNullOrEmpty(subline))
//        {
//            PrintErrorMessage("Substring cannot be empty.");
//            return;
//        }

//        IEnumerable<IName> itemsWithSubline;
//        if (activeContainerType == ContainerType.Array)
//        {
//            itemsWithSubline = containerArray!.GetArrayWithSublineInName(subline);
//        }
//        else // LinkedList
//        {
//            itemsWithSubline = containerList!.GetArrayWithSublineInName(subline);
//        }

//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"\nResults for names containing '{subline}':");
//        Console.ResetColor();
//        int tableWidth = CalculateTableWidth();
//        PrintTableHeader(tableWidth);
//        int count = 0;
//        foreach (var item in itemsWithSubline)
//        {
//            WriteDataRowByDisplayId(count + 1, item, tableWidth);
//            DrawHorizontalLine(tableWidth);
//            count++;
//        }
//        if (count == 0)
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine($"|{PadAndCenter($"No items found with names containing '{subline}'", tableWidth - 2)}|");
//            DrawHorizontalLine(tableWidth);
//            Console.ResetColor();
//        }
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"Substring generator yielded {count} items.");
//        Console.ResetColor();
//    }

//    static void HandleSortedPriceGenerator()
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"\n--- Generator: Items Sorted by Price ({activeContainerType} Container) ---");
//        Console.ResetColor();
//        if (IsContainerEmpty(out _)) return;

//        IEnumerable<IName> sortedItems;
//        if (activeContainerType == ContainerType.Array)
//        {
//            sortedItems = containerArray!.GetSortedByArrayPrice();
//        }
//        else // LinkedList
//        {
//            sortedItems = containerList!.GetSortedByArrayPrice();
//        }

//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine("\nItems sorted by Price (Generated Sequence):");
//        Console.ResetColor();
//        int tableWidth = CalculateTableWidth();
//        PrintTableHeader(tableWidth);
//        int count = 0;
//        foreach (var item in sortedItems)
//        {
//            if (item != null)
//            {
//                WriteDataRowByDisplayId(count + 1, item, tableWidth);
//                DrawHorizontalLine(tableWidth);
//                count++;
//            }
//        }
//        if (count == 0)
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine($"|{PadAndCenter("(No items yielded by generator)", tableWidth - 2)}|");
//            DrawHorizontalLine(tableWidth);
//            Console.ResetColor();
//        }
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"Sorted by Price generator yielded {count} items.");
//        Console.ResetColor();
//    }

//    static void HandleSortedNameGenerator()
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"\n--- Generator: Items Sorted by Name ({activeContainerType} Container) ---");
//        Console.ResetColor();
//        if (IsContainerEmpty(out _)) return;

//        IEnumerable<IName> sortedItems;
//        if (activeContainerType == ContainerType.Array)
//        {
//            sortedItems = containerArray!.GetSortedArrayByName();
//        }
//        else // LinkedList
//        {
//            sortedItems = containerList!.GetSortedArrayByName();
//        }

//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine("\nItems sorted by Name (Generated Sequence):");
//        Console.ResetColor();
//        int tableWidth = CalculateTableWidth();
//        PrintTableHeader(tableWidth);
//        int count = 0;
//        foreach (var item in sortedItems)
//        {
//            if (item != null)
//            {
//                WriteDataRowByDisplayId(count + 1, item, tableWidth);
//                DrawHorizontalLine(tableWidth);
//                count++;
//            }
//        }
//        if (count == 0)
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine($"|{PadAndCenter("(No items yielded by generator)", tableWidth - 2)}|");
//            DrawHorizontalLine(tableWidth);
//            Console.ResetColor();
//        }
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"Sorted by Name generator yielded {count} items.");
//        Console.ResetColor();
//    }

//    // --- Indexer Interaction Methods ---

//    // Array Container Indexer Demonstration
//    static void DemonstrateIndexersArray(Container<IName> container, Random random)
//    {
//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.WriteLine("\n--- Demonstrating Array Container Indexer Usage ---");
//        Console.ResetColor();
//        if (container.IsEmpty(false)) return;

//        int currentCount = container.GetCount();
//        int nextId = container.GetInsertionId();

//        // 1. Demonstrate Insertion ID Indexer (Get)
//        if (nextId > 0)
//        {
//            int demoInsertionId = random.Next(nextId);
//            Console.WriteLine($"1. Accessing item by random insertion ID [{demoInsertionId + 1}]:");
//            try
//            {
//                IName? itemById = container[demoInsertionId];
//                if (itemById != null)
//                {
//                    int currentIndex = FindIndexByReference(itemById);
//                    string indexStr = currentIndex != -1 ? $"(Current Index: {currentIndex + 1})" : "";
//                    Console.ForegroundColor = ConsoleColor.Cyan;
//                    Console.WriteLine($"   Found {indexStr}: {itemById.ToString() ?? "N/A"}");
//                    Console.ResetColor();
//                }
//                else
//                {
//                    Console.ForegroundColor = ConsoleColor.Yellow;
//                    Console.WriteLine($"   Item with insertion ID {demoInsertionId + 1} not found (likely removed or ID never used/valid).");
//                    Console.ResetColor();
//                }
//            }
//            catch (Exception ex)
//            {
//                PrintErrorMessage($"   Error getting item by insertion ID {demoInsertionId + 1}: {ex.Message}");
//            }
//        }
//        else
//        {
//            Console.WriteLine("1. No items added yet, cannot demonstrate insertion ID indexer.");
//        }

//        // 2. Demonstrate Name Indexer (Get)
//        string? demoName = FindDemoName(container.GetItems(), container.GetCount(), random);
//        Console.WriteLine($"\n2. Using string indexer container[\"{demoName ?? "N/A"}\"]:");
//        if (!string.IsNullOrWhiteSpace(demoName))
//        {
//            try
//            {
//                IName[]? itemsByName = container[demoName];
//                if (itemsByName != null && itemsByName.Any(i => i != null))
//                {
//                    Console.ForegroundColor = ConsoleColor.Cyan;
//                    Console.WriteLine($"   Found {itemsByName.Count(i => i != null)} item(s):");
//                    foreach (var item in itemsByName.Where(i => i != null))
//                    {
//                        int currentIndex = FindIndexByReference(item!);
//                        string indexStr = currentIndex != -1 ? $"(Index: {currentIndex + 1})" : "";
//                        Console.WriteLine($"   - {indexStr} {item!.ToString() ?? "N/A"}");
//                    }
//                    Console.ResetColor();
//                }
//                else
//                {
//                    Console.ForegroundColor = ConsoleColor.Yellow;
//                    Console.WriteLine($"   No items found for name '{demoName}'.");
//                    Console.ResetColor();
//                }
//            }
//            catch (Exception ex)
//            {
//                PrintErrorMessage($"   Error getting item(s) by name '{demoName}': {ex.Message}");
//            }
//        }
//        else
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine("   Could not find an item with a non-empty name in the sample to demonstrate.");
//            Console.ResetColor();
//        }

//        // 3. Demonstrate Insertion ID Indexer (Set)
//        int validDemoId = FindValidInsertionId(container);
//        if (validDemoId != -1)
//        {
//            Console.WriteLine($"\n3. Attempting to change item with insertion ID [{validDemoId + 1}] using property modification:");
//            IName? itemToModify = container[validDemoId];
//            if (itemToModify != null)
//            {
//                int currentIndex = FindIndexByReference(itemToModify);
//                string indexStr = currentIndex != -1 ? $"(Current Index: {currentIndex + 1})" : "";
//                Console.ForegroundColor = ConsoleColor.Magenta;
//                Console.WriteLine($"   Original Item {indexStr}: '{itemToModify.ToString() ?? "N/A"}'");
//                Console.ResetColor();
//                try
//                {
//                    string newName = $"ChangedItem-{validDemoId + 1}";
//                    Console.WriteLine($"   Attempting to set Name to '{newName}'...");
//                    itemToModify.GetType().GetProperty("Name")?.SetValue(itemToModify, newName);
//                    Console.ForegroundColor = ConsoleColor.Green;
//                    Console.WriteLine($"   Property 'Name' potentially updated (check via Show Container).");

//                    IName? changedItem = container[validDemoId];
//                    int changedIndex = changedItem != null ? FindIndexByReference(changedItem) : -1;
//                    string changedIndexStr = changedIndex != -1 ? $"(Current Index: {changedIndex + 1})" : "";

//                    Console.ForegroundColor = ConsoleColor.Cyan;
//                    Console.WriteLine($"   Current value {changedIndexStr}: {changedItem?.ToString() ?? "Not Found!"}");
//                    Console.ResetColor();
//                }
//                catch (TargetInvocationException tie) { PrintErrorMessage($"   Error modifying property: {tie.InnerException?.Message ?? tie.Message}"); }
//                catch (Exception ex) { PrintErrorMessage($"   Error modifying property: {ex.Message}"); }
//            }
//            else { Console.WriteLine($"   Could not retrieve item with insertion ID {validDemoId + 1} for modification demonstration."); }
//        }
//        else
//        {
//            Console.WriteLine("\n3. Cannot demonstrate modification: No suitable item found with a valid insertion ID.");
//        }

//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.WriteLine("--- End Array Indexer Demonstration ---");
//        Console.ResetColor();
//    }

//    // LinkedList Container Indexer Demonstration
//    static void DemonstrateIndexersList(ContainerLinkedList<IName> container, Random random)
//    {
//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.WriteLine("\n--- Demonstrating LinkedList Container Indexer Usage ---");
//        Console.ResetColor();
//        if (container.Count == 0) return;

//        List<int> currentInsertionOrder = container.GetInsertionOrder();
//        if (currentInsertionOrder.Count == 0)
//        {
//            Console.WriteLine("Container has items but insertion order list is empty (unexpected). Cannot demonstrate.");
//            return;
//        }

//        // 1. Demonstrate Insertion ID Indexer (Get)
//        int randomIndexList = random.Next(currentInsertionOrder.Count);
//        int demoInsertionIdList = currentInsertionOrder[randomIndexList];

//        Console.WriteLine($"1. Accessing item by existing insertion ID [{demoInsertionIdList + 1}]:");
//        try
//        {
//            IName? itemById = container[demoInsertionIdList];
//            if (itemById != null)
//            {
//                int currentIndex = FindIndexByReference(itemById);
//                string indexStr = currentIndex != -1 ? $"(Current Index: {currentIndex + 1})" : "";
//                Console.ForegroundColor = ConsoleColor.Cyan;
//                Console.WriteLine($"   Found {indexStr}: {itemById.ToString() ?? "N/A"}");
//                Console.ResetColor();
//            }
//            else
//            {
//                Console.ForegroundColor = ConsoleColor.Yellow;
//                Console.WriteLine($"   Item with insertion ID {demoInsertionIdList + 1} not found (unexpected).");
//                Console.ResetColor();
//            }
//        }
//        catch (Exception ex)
//        {
//            PrintErrorMessage($"   Error getting item by insertion ID {demoInsertionIdList + 1}: {ex.Message}");
//        }


//        // 2. Demonstrate Name Indexer (Get)
//        string? demoName = FindDemoName(container, random);
//        Console.WriteLine($"\n2. Using string indexer container[\"{demoName ?? "N/A"}\"]:");
//        if (!string.IsNullOrWhiteSpace(demoName))
//        {
//            try
//            {
//                List<IName>? itemsByName = container[demoName];
//                if (itemsByName != null && itemsByName.Count > 0)
//                {
//                    Console.ForegroundColor = ConsoleColor.Cyan;
//                    Console.WriteLine($"   Found {itemsByName.Count} item(s):");
//                    foreach (var item in itemsByName)
//                    {
//                        int currentIndex = FindIndexByReference(item);
//                        string indexStr = currentIndex != -1 ? $"(Index: {currentIndex + 1})" : "";
//                        Console.WriteLine($"   - {indexStr} {item.ToString() ?? "N/A"}");
//                    }
//                    Console.ResetColor();
//                }
//                else
//                {
//                    Console.ForegroundColor = ConsoleColor.Yellow;
//                    Console.WriteLine($"   No items found for name '{demoName}'.");
//                    Console.ResetColor();
//                }
//            }
//            catch (Exception ex)
//            {
//                PrintErrorMessage($"   Error getting item(s) by name '{demoName}': {ex.Message}");
//            }
//        }
//        else
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine("   Could not find an item with a non-empty name in the sample to demonstrate.");
//            Console.ResetColor();
//        }

//        // 3. Demonstrate Insertion ID Indexer (Set)
//        int validDemoIdList = FindValidInsertionId(container);
//        if (validDemoIdList != -1)
//        {
//            Console.WriteLine($"\n3. Attempting to change item with insertion ID [{validDemoIdList + 1}] using property modification:");
//            IName? itemToModify = container[validDemoIdList];
//            if (itemToModify != null)
//            {
//                int currentIndex = FindIndexByReference(itemToModify);
//                string indexStr = currentIndex != -1 ? $"(Current Index: {currentIndex + 1})" : "";
//                Console.ForegroundColor = ConsoleColor.Magenta;
//                Console.WriteLine($"   Original Item {indexStr}: '{itemToModify.ToString() ?? "N/A"}'");
//                Console.ResetColor();
//                try
//                {
//                    string newName = $"ChangedItem-{validDemoIdList + 1}";
//                    Console.WriteLine($"   Attempting to set Name to '{newName}'...");
//                    itemToModify.GetType().GetProperty("Name")?.SetValue(itemToModify, newName);
//                    Console.ForegroundColor = ConsoleColor.Green;
//                    Console.WriteLine($"   Property 'Name' potentially updated (check via Show Container).");

//                    IName? changedItem = container[validDemoIdList];
//                    int changedIndex = changedItem != null ? FindIndexByReference(changedItem) : -1;
//                    string changedIndexStr = changedIndex != -1 ? $"(Current Index: {changedIndex + 1})" : "";

//                    Console.ForegroundColor = ConsoleColor.Cyan;
//                    Console.WriteLine($"   Current value {changedIndexStr}: {changedItem?.ToString() ?? "Not Found!"}");
//                    Console.ResetColor();
//                }
//                catch (TargetInvocationException tie) { PrintErrorMessage($"   Error modifying property: {tie.InnerException?.Message ?? tie.Message}"); }
//                catch (Exception ex) { PrintErrorMessage($"   Error modifying property: {ex.Message}"); }
//            }
//            else { Console.WriteLine($"   Could not retrieve item with insertion ID {validDemoIdList + 1} for modification demonstration."); }
//        }
//        else
//        {
//            Console.WriteLine("\n3. Cannot demonstrate modification: No suitable item found with a valid insertion ID.");
//        }


//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.WriteLine("--- End LinkedList Indexer Demonstration ---");
//        Console.ResetColor();
//    }

//    static string? FindDemoName(IName?[] items, int count, Random random)
//    {
//        string? demoName = null;
//        if (count > 0)
//        {
//            for (int i = 0; i < 5; ++i)
//            {
//                int randomIndex = random.Next(count);
//                IName? sourceItemForName = items[randomIndex];
//                demoName = GetPropertyValue<string>(sourceItemForName, "Name");
//                if (!string.IsNullOrWhiteSpace(demoName)) break;
//            }
//        }
//        return demoName;
//    }
//    static string? FindDemoName(ContainerLinkedList<IName> listContainer, Random random)
//    {
//        string? demoName = null;
//        if (listContainer.Count > 0)
//        {
//            for (int i = 0; i < 5; ++i)
//            {
//                int randomIndex = random.Next(listContainer.Count);
//                var node = listContainer.First;
//                int currentIndex = 0;
//                while (node != null && currentIndex < randomIndex)
//                {
//                    node = node.Next;
//                    currentIndex++;
//                }
//                if (node != null)
//                {
//                    demoName = GetPropertyValue<string>(node.Data, "Name");
//                    if (!string.IsNullOrWhiteSpace(demoName)) break;
//                }
//            }
//        }
//        return demoName;
//    }

//    static int FindValidInsertionId(Container<IName> container)
//    {
//        int nextId = container.GetInsertionId();
//        if (nextId <= 0) return -1;
//        for (int id = nextId - 1; id >= 0; id--)
//        {
//            try
//            {
//                if (container[id] != null) return id;
//            }
//            catch (IndexOutOfRangeException) { /* Ignore */ }
//            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Debug: Error checking ID {id} in FindValidInsertionId (Array): {ex.Message}"); }
//        }
//        return -1;
//    }

//    static int FindValidInsertionId(ContainerLinkedList<IName> container)
//    {
//        List<int> order = container.GetInsertionOrder();
//        if (container.Count == 0 || order.Count == 0) return -1;
//        // Return the last added insertion ID (0-based)
//        return order[order.Count - 1];
//    }

//    // --- Property Modification Logic ---
//    static void ModifyProperty(object itemToModify, int itemInsertionId)
//    {
//        ArgumentNullException.ThrowIfNull(itemToModify);

//        var properties = itemToModify.GetType()
//                                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
//                                        .Where(p => p.CanWrite && p.GetSetMethod(true) != null)
//                                        .ToList();

//        if (properties.Count == 0)
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine("This object has no publicly writable properties.");
//            Console.ResetColor();
//            return;
//        }

//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine("\nChoose property to modify:");
//        Console.ResetColor();
//        for (int i = 0; i < properties.Count; i++)
//        {
//            object? currentValue = "?";
//            try { currentValue = properties[i].GetValue(itemToModify); } catch { /* Ignore */ }
//            Console.ForegroundColor = ConsoleColor.Cyan;
//            Console.WriteLine($"{i + 1}. {properties[i].Name} (Type: {properties[i].PropertyType.Name}, Current: '{currentValue ?? "null"}')");
//            Console.ResetColor();
//        }

//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.Write($"Enter choice (1 to {properties.Count}): ");
//        Console.ResetColor();
//        if (int.TryParse(Console.ReadLine(), out int propChoice) && propChoice >= 1 && propChoice <= properties.Count)
//        {
//            PropertyInfo selectedProperty = properties[propChoice - 1];
//            Type propertyType = selectedProperty.PropertyType;
//            Type underlyingType = Nullable.GetUnderlyingType(propertyType);
//            bool isNullable = underlyingType != null;
//            Type targetType = underlyingType ?? propertyType;

//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.Write($"Enter new value for {selectedProperty.Name} (Type: {targetType.Name}{(isNullable ? ", or empty for null" : "")}): ");
//            Console.ResetColor();
//            string newValueString = Console.ReadLine() ?? "";

//            object? convertedValue;

//            if (!TryParseValue(newValueString, targetType, isNullable, out convertedValue))
//            {
//                return;
//            }

//            try
//            {
//                selectedProperty.SetValue(itemToModify, convertedValue, null);
//                Console.ForegroundColor = ConsoleColor.Green;
//                Console.WriteLine($"\nProperty '{selectedProperty.Name}' updated successfully for item (Insertion ID: {itemInsertionId + 1}).");
//                Console.WriteLine("New item details:");
//                Console.ResetColor();

//                int currentIndex = FindIndexByReference((IName)itemToModify);
//                if (currentIndex != -1)
//                {
//                    DisplayItemTable(currentIndex + 1, (IName)itemToModify);
//                }
//                else
//                {
//                    PrintErrorMessage("Could not determine current index after modification for display.");
//                }
//            }
//            catch (TargetInvocationException tie)
//            {
//                PrintErrorMessage($"Validation Error setting property '{selectedProperty.Name}': {tie.InnerException?.Message ?? tie.Message}");
//            }
//            catch (ArgumentException argEx)
//            {
//                PrintErrorMessage($"Error setting property '{selectedProperty.Name}': Type mismatch or invalid argument. {argEx.Message}");
//            }
//            catch (Exception ex)
//            {
//                PrintErrorMessage($"Unexpected error setting property '{selectedProperty.Name}': {ex.Message}");
//            }
//        }
//        else
//        {
//            PrintErrorMessage("Invalid property choice.");
//        }
//    }

//    static bool TryParseValue(string input, Type targetType, bool isNullable, out object? parsedValue)
//    {
//        parsedValue = null;
//        if (isNullable && string.IsNullOrEmpty(input))
//        {
//            return true;
//        }

//        try
//        {
//            if (targetType == typeof(bool))
//            {
//                string lowerVal = input.Trim().ToLowerInvariant();
//                if (lowerVal == "true" || lowerVal == "1" || lowerVal == "yes" || lowerVal == "y")
//                    parsedValue = true;
//                else if (lowerVal == "false" || lowerVal == "0" || lowerVal == "no" || lowerVal == "n")
//                    parsedValue = false;
//                else
//                    throw new FormatException($"Cannot convert '{input}' to Boolean.");
//            }
//            else
//            {
//                TypeConverter converter = TypeDescriptor.GetConverter(targetType);
//                if (converter != null && converter.CanConvertFrom(typeof(string)))
//                {
//                    parsedValue = converter.ConvertFromString(null, CultureInfo.InvariantCulture, input);
//                }
//                else
//                {
//                    parsedValue = Convert.ChangeType(input, targetType, CultureInfo.InvariantCulture);
//                }
//            }
//            return true;
//        }
//        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is NotSupportedException || ex is ArgumentException)
//        {
//            PrintErrorMessage($"Conversion Error: Could not convert '{input}' to type {targetType.Name}. {ex.Message}");
//            return false;
//        }
//    }


//    // --- Automatic Generation ---
//    static void AutomaticGenerationArray(Container<IName> container, Random random, int count)
//    {
//        Console.WriteLine("Generating elements for Array Container...");
//        GenerateItems(count, random, item => container.Add(item));
//        Console.WriteLine("\nArray Generation process finished.");
//    }

//    static void AutomaticGenerationList(ContainerLinkedList<IName> container, Random random, int count)
//    {
//        Console.WriteLine("Generating elements for LinkedList Container...");
//        GenerateItems(count, random, item => container.AddLast(item));
//        Console.WriteLine("\nLinkedList Generation process finished.");
//    }

//    static void GenerateItems(int count, Random random, Action<IName> addAction)
//    {
//        for (int i = 0; i < count; i++)
//        {
//            IName newItem;
//            int typeChoice = random.Next(1, 9);
//            try
//            {
//                switch (typeChoice)
//                {
//                    case 1: newItem = GenerateRandomProduct(random); break;
//                    case 2: newItem = GenerateRandomRealEstate(random); break;
//                    case 3: newItem = GenerateRandomRealEstateInvestment(random); break;
//                    case 4: newItem = GenerateRandomApartment(random); break;
//                    case 5: newItem = GenerateRandomHouse(random); break;
//                    case 6: newItem = GenerateRandomHotel(random); break;
//                    case 7: newItem = GenerateRandomLandPlot(random); break;
//                    case 8: newItem = new RealEstate($"BaseRE{i}", random.Next(5000, 20000), $"Loc{i}", random.Next(50, 200), "Base"); break;
//                    default: continue;
//                }
//                addAction(newItem);
//                Console.Write(".");
//            }
//            catch (Exception ex)
//            {
//                Console.Write("X");
//                System.Diagnostics.Debug.WriteLine($"\nGeneration Error: Failed to create item of type {typeChoice}. {ex.GetType().Name}: {ex.Message}");
//            }
//        }
//    }

//    // --- Manual Input ---
//    static void ManualInputArray(Container<IName> container)
//    {
//        IName? newItem = CreateItemManually();
//        if (newItem != null)
//        {
//            container.Add(newItem);
//            Console.ForegroundColor = ConsoleColor.Green;
//            Console.WriteLine($"{newItem.GetType().Name} added successfully to Array Container (Insertion ID: {container.GetInsertionId()}).");
//            Console.ResetColor();
//        }
//    }

//    static void ManualInputList(ContainerLinkedList<IName> container)
//    {
//        IName? newItem = CreateItemManually();
//        if (newItem != null)
//        {
//            container.AddLast(newItem);
//            Console.ForegroundColor = ConsoleColor.Green;
//            Console.WriteLine($"{newItem.GetType().Name} added successfully to LinkedList Container (Insertion ID: {container.GetNextInsertionId()}).");
//            Console.ResetColor();
//        }
//    }

//    static IName? CreateItemManually()
//    {
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.WriteLine("Choose class to create:");
//        Console.ForegroundColor = ConsoleColor.DarkCyan;
//        Console.WriteLine("1. Product");
//        Console.WriteLine("2. RealEstate");
//        Console.WriteLine("3. RealEstateInvestment");
//        Console.WriteLine("4. Apartment");
//        Console.WriteLine("5. House");
//        Console.WriteLine("6. Hotel");
//        Console.WriteLine("7. LandPlot");
//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.Write("Enter choice: ");
//        Console.ResetColor();
//        string classChoice = Console.ReadLine() ?? "";

//        try
//        {
//            return classChoice switch
//            {
//                "1" => CreateManualProduct(),
//                "2" => CreateManualRealEstate(),
//                "3" => CreateManualRealEstateInvestment(),
//                "4" => CreateManualApartment(),
//                "5" => CreateManualHouse(),
//                "6" => CreateManualHotel(),
//                "7" => CreateManualLandPlot(),
//                _ => throw new ArgumentException("Invalid class choice.")
//            };
//        }
//        catch (ValueLessThanZero ex) { PrintErrorMessage($"Creation Error: {ex.Message}"); return null; }
//        catch (FormatException ex) { PrintErrorMessage($"Invalid input format during creation: {ex.Message}"); return null; }
//        catch (ArgumentException ex) { PrintErrorMessage($"Invalid argument during creation: {ex.Message}"); return null; }
//        catch (Exception ex) { PrintErrorMessage($"Unexpected error during creation: {ex.Message}"); return null; }
//    }


//    // --- Container Display ---
//    static void ShowContainerArray(Container<IName> container, int currentCount)
//    {
//        string title = $"Array Container Contents ({currentCount} items)";
//        int tableWidth = CalculateTableWidth();

//        Console.ForegroundColor = ConsoleColor.Magenta;
//        Console.WriteLine(CenterString(title, tableWidth));
//        Console.ResetColor();

//        PrintTableHeader(tableWidth);

//        int i = 0;
//        foreach (var item in container)
//        {
//            WriteDataRowByDisplayId(i + 1, item, tableWidth);
//            DrawHorizontalLine(tableWidth);
//            i++;
//        }
//    }

//    static void ShowContainerList(ContainerLinkedList<IName> container, int currentCount)
//    {
//        string title = $"LinkedList Container Contents ({currentCount} items)";
//        int tableWidth = CalculateTableWidth();

//        Console.ForegroundColor = ConsoleColor.Magenta;
//        Console.WriteLine(CenterString(title, tableWidth));
//        Console.ResetColor();

//        PrintTableHeader(tableWidth);

//        int i = 0;

//        foreach (var item in container)
//        {
//            WriteDataRowByDisplayId(i + 1, item, tableWidth);
//            DrawHorizontalLine(tableWidth);
//            i++;
//        }
//    }

//    // Displays item using 1-based current index
//    static void DisplayItemTable(int displayId, IName item)
//    {
//        if (item == null) return;
//        int tableWidth = CalculateTableWidth();
//        PrintTableHeader(tableWidth);
//        WriteDataRowByDisplayId(displayId, item, tableWidth);
//        DrawHorizontalLine(tableWidth);
//    }

//    // --- Table Formatting Helpers ---
//    const int idWidth = 6;
//    const int classWidth = 16;
//    const int nameWidth = 18;
//    const int priceWidth = 16;
//    const int locationWidth = 20;
//    const int sizeWidth = 10;
//    const int typeWidth = 14;
//    const int marketValueWidth = 18;
//    const int investmentTypeWidth = 18;
//    const int floorWidth = 7;
//    const int hoaWidth = 7;
//    const int gardenWidth = 9;
//    const int poolWidth = 6;
//    const int roomsWidth = 7;
//    const int starWidth = 6;
//    const int soilWidth = 10;
//    const int infraWidth = 7;
//    const int padding = 1;
//    const int numColumns = 17;

//    static int CalculateTableWidth()
//    {
//        int totalDataWidth = idWidth + classWidth + nameWidth + priceWidth + locationWidth + sizeWidth + typeWidth + marketValueWidth + investmentTypeWidth + floorWidth + hoaWidth + gardenWidth + poolWidth + roomsWidth + starWidth + soilWidth + infraWidth;
//        int totalPaddingWidth = numColumns * padding;

//        return totalDataWidth + totalPaddingWidth;
//    }


//    static void PrintTableHeader(int tableWidth)
//    {
//        DrawHorizontalLine(tableWidth);
//        WriteHeaderRow();
//        DrawHorizontalLine(tableWidth);
//    }

//    static void WriteHeaderRow()
//    {
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.Write($"|{PadAndCenter("ID", idWidth)}");
//        Console.Write($"|{PadAndCenter("Class", classWidth)}");
//        Console.Write($"|{PadAndCenter("Name", nameWidth)}");
//        Console.Write($"|{PadAndCenter("Price", priceWidth)}");
//        Console.Write($"|{PadAndCenter("Location", locationWidth)}");
//        Console.Write($"|{PadAndCenter("Size", sizeWidth)}");
//        Console.Write($"|{PadAndCenter("Type", typeWidth)}");
//        Console.Write($"|{PadAndCenter("Mkt Value", marketValueWidth)}");
//        Console.Write($"|{PadAndCenter("Invest Type", investmentTypeWidth)}");
//        Console.Write($"|{PadAndCenter("Floor", floorWidth)}");
//        Console.Write($"|{PadAndCenter("HOA", hoaWidth)}");
//        Console.Write($"|{PadAndCenter("Garden", gardenWidth)}");
//        Console.Write($"|{PadAndCenter("Pool", poolWidth)}");
//        Console.Write($"|{PadAndCenter("Rooms", roomsWidth)}");
//        Console.Write($"|{PadAndCenter("Star", starWidth)}");
//        Console.Write($"|{PadAndCenter("Soil", soilWidth)}");
//        Console.Write($"|{PadAndCenter("Infra", infraWidth)}");
//        Console.WriteLine("|");
//        Console.ResetColor();
//    }

//    static void WriteDataRowByDisplayId(int displayId, object item, int tableWidth)
//    {
//        string FormatDecimal(decimal? d) => d?.ToString("N2", CultureInfo.InvariantCulture) ?? "-";
//        string FormatDouble(double? d) => d?.ToString("N1", CultureInfo.InvariantCulture) ?? "-";
//        string FormatBool(bool? b) => b.HasValue ? (b.Value ? "Yes" : "No") : "-";
//        string FormatInt(int? i) => i?.ToString() ?? "-";
//        string FormatString(string? s) => string.IsNullOrWhiteSpace(s) ? "-" : s;

//        Type itemType = item.GetType();

//        string name = FormatString(GetPropertyValue<string>(item, "Name"));
//        string fPrice = FormatDecimal(GetPropertyValue<decimal?>(item, "Price"));
//        string loc = FormatString(GetPropertyValue<string>(item, "Location"));
//        string fSize = FormatDouble(GetPropertyValue<double?>(item, "Size"));
//        string type = FormatString(GetPropertyValue<string>(item, "Type"));
//        string fMktVal = FormatDecimal(GetPropertyValue<decimal?>(item, "MarketValue"));
//        string invType = FormatString(GetPropertyValue<string>(item, "InvestmentType"));
//        string fFloor = FormatInt(GetPropertyValue<int?>(item, "FloorNumber"));
//        string fHoa = FormatDecimal(GetPropertyValue<decimal?>(item, "HOAFees"));
//        string fGarden = FormatDouble(GetPropertyValue<double?>(item, "GardenSize"));
//        string fPool = FormatBool(GetPropertyValue<bool?>(item, "Pool"));
//        string fRooms = FormatInt(GetPropertyValue<int?>(item, "Rooms"));
//        string fStar = FormatInt(GetPropertyValue<int?>(item, "StarRating"));
//        string soil = FormatString(GetPropertyValue<string>(item, "SoilType"));
//        string fInfra = FormatBool(GetPropertyValue<bool?>(item, "InfrastructureAccess"));

//        Console.Write($"|{PadAndCenter(displayId.ToString(), idWidth)}");
//        Console.Write($"|{PadAndCenter(itemType.Name, classWidth)}");
//        Console.Write($"|{PadAndCenter(name, nameWidth)}");
//        Console.Write($"|{PadAndCenter(fPrice, priceWidth)}");
//        Console.Write($"|{PadAndCenter(loc, locationWidth)}");
//        Console.Write($"|{PadAndCenter(fSize, sizeWidth)}");
//        Console.Write($"|{PadAndCenter(type, typeWidth)}");
//        Console.Write($"|{PadAndCenter(fMktVal, marketValueWidth)}");
//        Console.Write($"|{PadAndCenter(invType, investmentTypeWidth)}");
//        Console.Write($"|{PadAndCenter(fFloor, floorWidth)}");
//        Console.Write($"|{PadAndCenter(fHoa, hoaWidth)}");
//        Console.Write($"|{PadAndCenter(fGarden, gardenWidth)}");
//        Console.Write($"|{PadAndCenter(fPool, poolWidth)}");
//        Console.Write($"|{PadAndCenter(fRooms, roomsWidth)}");
//        Console.Write($"|{PadAndCenter(fStar, starWidth)}");
//        Console.Write($"|{PadAndCenter(soil, soilWidth)}");
//        Console.Write($"|{PadAndCenter(fInfra, infraWidth)}");
//        Console.WriteLine("|");
//    }


//    static void DrawHorizontalLine(int tableWidth)
//    {
//        Console.ForegroundColor = ConsoleColor.DarkGray;
//        Console.WriteLine(new string('-', tableWidth));
//        Console.ResetColor();
//    }

//    static string PadAndCenter(string? value, int totalWidth)
//    {
//        string val = value ?? "";
//        if (totalWidth <= 0) return "";

//        val = Truncate(val, totalWidth);

//        int spaces = totalWidth - val.Length;
//        int padLeft = spaces / 2;

//        return val.PadLeft(padLeft + val.Length).PadRight(totalWidth);
//    }


//    static string CenterString(string s, int width)
//    {
//        if (string.IsNullOrEmpty(s) || width <= 0) return new string(' ', Math.Max(0, width));
//        s = Truncate(s, width); // Ensure fits
//        int padding = Math.Max(0, (width - s.Length) / 2);
//        return new string(' ', padding) + s + new string(' ', Math.Max(0, width - s.Length - padding));
//    }


//    static string Truncate(string? value, int maxLength)
//    {
//        if (string.IsNullOrEmpty(value)) return "";
//        if (maxLength <= 0) return "";
//        if (value.Length <= maxLength) return value;
//        int subLength = Math.Max(0, maxLength - 3);
//        if (subLength == 0) return "...".Substring(0, Math.Min(3, maxLength));
//        return value.Substring(0, subLength) + "...";
//    }


//    // --- Reflection Property Getter ---
//    private static TValue? GetPropertyValue<TValue>(object? item, string propertyName)
//    {
//        if (item == null) return default;
//        PropertyInfo? property = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
//        if (property != null && property.CanRead)
//        {
//            try
//            {
//                object? value = property.GetValue(item);
//                if (value == null) return default;
//                if (value is TValue correctlyTyped) return correctlyTyped;

//                Type? underlyingTValue = Nullable.GetUnderlyingType(typeof(TValue));
//                if (underlyingTValue != null && underlyingTValue == property.PropertyType)
//                {
//                    try { return (TValue)Convert.ChangeType(value, underlyingTValue, CultureInfo.InvariantCulture); } catch { /* Ignore */ }
//                }
//                if (typeof(TValue) == typeof(string))
//                {
//                    try { return (TValue)(object)Convert.ToString(value, CultureInfo.InvariantCulture)!; } catch { /* Ignore */ }
//                }
//                else if (typeof(TValue) == typeof(decimal) && IsNumericType(property.PropertyType))
//                {
//                    try { return (TValue)(object)Convert.ToDecimal(value, CultureInfo.InvariantCulture); } catch { /* Ignore */ }
//                }
//                else if (typeof(TValue) == typeof(double) && IsNumericType(property.PropertyType))
//                {
//                    try { return (TValue)(object)Convert.ToDouble(value, CultureInfo.InvariantCulture); } catch { /* Ignore */ }
//                }
//                else if (typeof(TValue) == typeof(int) && IsNumericType(property.PropertyType))
//                {
//                    try { return (TValue)(object)Convert.ToInt32(value, CultureInfo.InvariantCulture); } catch { /* Ignore */ }
//                }
//                else if (typeof(TValue) == typeof(bool))
//                {
//                    if (IsNumericType(property.PropertyType))
//                    {
//                        try { return (TValue)(object)(Convert.ToDouble(value, CultureInfo.InvariantCulture) != 0); } catch { /* Ignore */ }
//                    }
//                    else if (property.PropertyType == typeof(string))
//                    {
//                        if (bool.TryParse((string)value, out bool boolVal)) return (TValue)(object)boolVal;
//                    }
//                }

//                try { return (TValue)Convert.ChangeType(value, typeof(TValue), CultureInfo.InvariantCulture); } catch { /* Ignore */ }
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine($"Reflection Error getting '{propertyName}': {ex.Message}");
//            }
//        }
//        return default;
//    }
//    private static bool IsNumericType(Type type)
//    {
//        if (type == null) return false;
//        switch (Type.GetTypeCode(type))
//        {
//            case TypeCode.Byte:
//            case TypeCode.Decimal:
//            case TypeCode.Double:
//            case TypeCode.Int16:
//            case TypeCode.Int32:
//            case TypeCode.Int64:
//            case TypeCode.SByte:
//            case TypeCode.Single:
//            case TypeCode.UInt16:
//            case TypeCode.UInt32:
//            case TypeCode.UInt64:
//                return true;
//            default:
//                return false;
//        }
//    }


//    // --- Container State Helpers ---

//    static bool IsContainerEmpty(out int count)
//    {
//        count = 0;
//        bool isEmpty = true;

//        if (activeContainerType == ContainerType.Array && containerArray != null)
//        {
//            isEmpty = containerArray.IsEmpty(false);
//            count = containerArray.GetCount();
//        }
//        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
//        {
//            count = containerList.Count;
//            isEmpty = (count == 0);
//        }
//        else
//        {
//            isEmpty = true;
//        }

//        if (isEmpty && activeContainerType != ContainerType.None)
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine("The active container is empty.");
//            Console.ResetColor();
//        }
//        else if (activeContainerType == ContainerType.None)
//        {
//            PrintErrorMessage("No container selected. Please use option 1 or 2 first.");
//            isEmpty = true;
//        }

//        return isEmpty;
//    }

//    static int GetActiveContainerCount()
//    {
//        if (activeContainerType == ContainerType.Array && containerArray != null)
//        {
//            return containerArray.GetCount();
//        }
//        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
//        {
//            return containerList.Count;
//        }
//        return 0;
//    }

//    // Gets the next insertion ID (0-based)
//    static int GetNextInsertionId()
//    {
//        if (activeContainerType == ContainerType.Array && containerArray != null)
//        {
//            return containerArray.GetInsertionId();
//        }
//        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
//        {
//            return containerList.GetNextInsertionId();
//        }
//        return 0;
//    }

//    // Finds the current 0-based index of an item
//    private static int FindIndexByReference(IName itemToFind)
//    {
//        if (itemToFind == null) return -1;

//        if (activeContainerType == ContainerType.Array && containerArray != null)
//        {
//            IName?[] currentItems = containerArray.GetItems();
//            int currentCount = containerArray.GetCount();
//            for (int i = 0; i < currentCount; i++)
//            {
//                if (object.ReferenceEquals(currentItems[i], itemToFind))
//                {
//                    return i;
//                }
//            }
//        }
//        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
//        {
//            var node = containerList.First;
//            int index = 0;
//            while (node != null)
//            {
//                if (object.ReferenceEquals(node.Data, itemToFind))
//                {
//                    return index;
//                }
//                node = node.Next;
//                index++;
//            }
//        }
//        return -1;
//    }

//    private static int GetInsertionIdForItem(IName itemToFind)
//    {
//        if (itemToFind == null) return -1;

//        int index = FindIndexByReference(itemToFind);
//        if (index == -1) return -1;

//        try
//        {
//            if (activeContainerType == ContainerType.Array && containerArray != null)
//            {
//                int[] order = containerArray.GetInsertionOrder();
//                if (index < order.Length)
//                {
//                    return order[index];
//                }
//                else { System.Diagnostics.Debug.WriteLine($"Warning: Index {index} out of bounds for insertion Order Array (Length: {order.Length})"); }
//            }
//            else if (activeContainerType == ContainerType.LinkedList && containerList != null)
//            {
//                List<int> order = containerList.GetInsertionOrder();
//                if (index < order.Count)
//                {
//                    return order[index];
//                }
//                else { System.Diagnostics.Debug.WriteLine($"Warning: Index {index} out of bounds for insertion Order List (Count: {order.Count})"); }
//            }
//        }
//        catch (Exception ex)
//        {
//            System.Diagnostics.Debug.WriteLine($"Error in GetInsertionIdForItem for index {index}: {ex.Message}");
//        }
//        return -1;
//    }

//    // Gets an item by Insertion ID from the active container
//    private static IName? GetItemByInsertionId(int insertionId)
//    {
//        try
//        {
//            if (activeContainerType == ContainerType.Array && containerArray != null)
//            {
//                return containerArray[insertionId];
//            }
//            else if (activeContainerType == ContainerType.LinkedList && containerList != null)
//            {
//                return containerList[insertionId];
//            }
//        }
//        catch (IndexOutOfRangeException) { /* ID not found or invalid for container */ }
//        catch (Exception ex) { PrintErrorMessage($"Unexpected error fetching item by insertion ID {insertionId + 1}: {ex.Message}"); }
//        return null;
//    }

//    private static IName? GetItemByCurrentIndex(int index)
//    {
//        if (index < 0) return null;

//        if (activeContainerType == ContainerType.Array && containerArray != null)
//        {
//            IName?[] items = containerArray.GetItems();
//            int count = containerArray.GetCount();
//            if (index < count)
//            {
//                return items[index];
//            }
//        }
//        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
//        {
//            if (index < containerList.Count)
//            {
//                var node = containerList.First;
//                int i = 0;
//                while (node != null && i < index)
//                {
//                    node = node.Next;
//                    i++;
//                }
//                return node?.Data;
//            }
//        }
//        return null;
//    }


//    // --- Random Generators ---
//    static Product GenerateRandomProduct(Random random)
//    {
//        string[] names = { "Table", "Chair", "Lamp", "Phone", "Book", "Laptop", "Mug" };
//        decimal price = random.Next(10, 1000) + (decimal)random.NextDouble();
//        return new Product(names[random.Next(names.Length)] + random.Next(100), Math.Max(0.01m, Math.Round(price, 2)));
//    }

//    static RealEstate GenerateRandomRealEstate(Random random)
//    {
//        string[] names = { "Cozy Apt", "Luxury Villa", "Small House", "Big Mansion", "Downtown Loft" };
//        string[] locations = { "New York", "London", "Paris", "Tokyo", "Kyiv", "Berlin", "Sydney" };
//        string[] types = { "Residential", "Commercial", "Industrial", "Mixed-Use" };
//        decimal price = random.Next(100000, 1000000) + (decimal)random.NextDouble() * 1000;
//        double size = random.Next(50, 500) + random.NextDouble() * 10;
//        return new RealEstate(names[random.Next(names.Length)], Math.Max(0.01m, Math.Round(price, 2)), locations[random.Next(locations.Length)], Math.Max(1.0, Math.Round(size, 1)), types[random.Next(types.Length)]);
//    }

//    static RealEstateInvestment GenerateRandomRealEstateInvestment(Random random)
//    {
//        string[] names = { "Office Bldg", "Shopping Mall", "Warehouse", "Apt Complex", "Data Center" };
//        string[] locations = { "Chicago", "Los Angeles", "Houston", "Phoenix", "Philadelphia", "Dallas" };
//        string[] invTypes = { "REIT", "Direct Prop", "Mortgage Fund", "Syndication" };
//        decimal price = random.Next(500000, 5000000) + (decimal)random.NextDouble() * 10000;
//        decimal marketValue = price * (decimal)(0.8 + random.NextDouble() * 0.4);
//        return new RealEstateInvestment(names[random.Next(names.Length)], Math.Max(0.01m, Math.Round(price, 2)), locations[random.Next(locations.Length)], Math.Max(1.0m, Math.Round(marketValue, 2)), invTypes[random.Next(invTypes.Length)]);
//    }

//    static Apartment GenerateRandomApartment(Random random)
//    {
//        string[] names = { "Studio Apt", "1BR Apt", "2BR Apt", "Penthouse", "Garden Apt" };
//        string[] locations = { "Miami", "San Francisco", "Seattle", "Boston", "Denver", "Austin" };
//        string[] types = { "Condo", "Co-op", "Rental Unit", "Loft" };
//        decimal price = random.Next(200000, 800000) + (decimal)random.NextDouble() * 500;
//        double size = random.Next(40, 150) + random.NextDouble() * 5;
//        int floor = random.Next(1, 30);
//        decimal hoa = random.Next(50, 500) + (decimal)random.NextDouble() * 50;
//        return new Apartment(names[random.Next(names.Length)], Math.Max(0.01m, Math.Round(price, 2)), locations[random.Next(locations.Length)], Math.Max(1.0, Math.Round(size, 1)), types[random.Next(types.Length)], floor, Math.Max(0m, Math.Round(hoa, 2)));
//    }

//    static House GenerateRandomHouse(Random random)
//    {
//        string[] names = { "Bungalow", "Townhouse", "Ranch", "Cottage", "Colonial" };
//        string[] locations = { "Atlanta", "Dallas", "San Diego", "Orlando", "Las Vegas", "Nashville" };
//        string[] types = { "Single-family", "Multi-family", "Duplex" };
//        decimal price = random.Next(300000, 1200000) + (decimal)random.NextDouble() * 1000;
//        double size = random.Next(100, 400) + random.NextDouble() * 15;
//        double gardenSize = random.Next(-50, 1000) + random.NextDouble() * 100;
//        bool pool = random.Next(3) == 0;
//        return new House(names[random.Next(names.Length)], Math.Max(0.01m, Math.Round(price, 2)), locations[random.Next(locations.Length)], Math.Max(1.0, Math.Round(size, 1)), types[random.Next(types.Length)], Math.Max(0.0, Math.Round(gardenSize, 1)), pool);
//    }

//    static Hotel GenerateRandomHotel(Random random)
//    {
//        string[] names = { "Luxury Hotel", "Budget Inn", "Resort & Spa", "Boutique Hotel", "Airport Motel" };
//        string[] locations = { "Hawaii", "Bali", "Maldives", "Fiji", "Santorini", "Las Vegas Strip" };
//        string[] invTypes = { "Hospitality REIT", "Hotel Mgmt", "Timeshare", "Franchise" };
//        decimal price = random.Next(1000000, 10000000) + (decimal)random.NextDouble() * 50000;
//        decimal marketValue = price * (decimal)(0.9 + random.NextDouble() * 0.3);
//        int rooms = random.Next(20, 500);
//        int rating = random.Next(1, 6);
//        return new Hotel(names[random.Next(names.Length)], Math.Max(0.01m, Math.Round(price, 2)), locations[random.Next(locations.Length)], Math.Max(1.0m, Math.Round(marketValue, 2)), invTypes[random.Next(invTypes.Length)], Math.Max(1, rooms), rating);
//    }

//    static LandPlot GenerateRandomLandPlot(Random random)
//    {
//        string[] names = { "Farmland", "Forest", "Comm Land", "Resid Land", "Waterfront" };
//        string[] locations = { "Rural Area", "Suburban Edge", "Urban Infill", "Coastal Zone", "Mountain Base" };
//        string[] invTypes = { "Land Banking", "Development", "Agriculture", "Conservation" };
//        string[] soilTypes = { "Loam", "Clay", "Sand", "Silt", "Peat", "Chalky" };
//        decimal price = random.Next(50000, 500000) + (decimal)random.NextDouble() * 2000;
//        decimal marketValue = price * (decimal)(0.7 + random.NextDouble() * 0.6);
//        bool infra = random.Next(2) == 0;
//        return new LandPlot(names[random.Next(names.Length)], Math.Max(0.01m, Math.Round(price, 2)), locations[random.Next(locations.Length)], Math.Max(1.0m, Math.Round(marketValue, 2)), invTypes[random.Next(invTypes.Length)], soilTypes[random.Next(soilTypes.Length)], infra);
//    }


//    // --- Manual Creation Methods ---

//    static Product CreateManualProduct()
//    {
//        string name = ReadString("Enter Product Name: ");
//        decimal price = ReadDecimal("Enter Product Price (> 0): ", minValue: 0.01m);
//        return new Product(name, price);
//    }

//    static RealEstate CreateManualRealEstate()
//    {
//        string name = ReadString("Enter RealEstate Name: ");
//        decimal price = ReadDecimal("Enter RealEstate Price (> 0): ", minValue: 0.01m);
//        string location = ReadString("Enter Location: ");
//        double size = ReadDouble("Enter Size (> 0): ", minValue: 0.01);
//        string type = ReadString("Enter Type (e.g., Residential): ");
//        return new RealEstate(name, price, location, size, type);
//    }

//    static RealEstateInvestment CreateManualRealEstateInvestment()
//    {
//        string name = ReadString("Enter Investment Name: ");
//        decimal price = ReadDecimal("Enter Investment Price (> 0): ", minValue: 0.01m);
//        string location = ReadString("Enter Location: ");
//        decimal marketValue = ReadDecimal("Enter Market Value (> 0): ", minValue: 0.01m);
//        string investmentType = ReadString("Enter Investment Type (e.g., REIT): ");
//        return new RealEstateInvestment(name, price, location, marketValue, investmentType);
//    }

//    static Apartment CreateManualApartment()
//    {
//        string name = ReadString("Enter Apartment Name: ");
//        decimal price = ReadDecimal("Enter Apartment Price (> 0): ", minValue: 0.01m);
//        string location = ReadString("Enter Location: ");
//        double size = ReadDouble("Enter Size (> 0): ", minValue: 0.01);
//        string type = ReadString("Enter Type (e.g., Condo): ");
//        int floorNumber = ReadInt("Enter Floor Number (> 0): ", minValue: 1);
//        decimal hoaFees = ReadDecimal("Enter HOA Fees (>= 0): ", minValue: 0m);
//        return new Apartment(name, price, location, size, type, floorNumber, hoaFees);
//    }

//    static House CreateManualHouse()
//    {
//        string name = ReadString("Enter House Name: ");
//        decimal price = ReadDecimal("Enter House Price (> 0): ", minValue: 0.01m);
//        string location = ReadString("Enter Location: ");
//        double size = ReadDouble("Enter Size (> 0): ", minValue: 0.01);
//        string type = ReadString("Enter Type (e.g., Single-family): ");
//        double gardenSize = ReadDouble("Enter Garden Size (>= 0): ", minValue: 0.0);
//        bool pool = ReadBool("Has Pool (true/false/yes/no/1/0): ");
//        return new House(name, price, location, size, type, gardenSize, pool);
//    }

//    static Hotel CreateManualHotel()
//    {
//        string name = ReadString("Enter Hotel Name: ");
//        decimal price = ReadDecimal("Enter Hotel Price (> 0): ", minValue: 0.01m);
//        string location = ReadString("Enter Location: ");
//        decimal marketValue = ReadDecimal("Enter Market Value (> 0): ", minValue: 0.01m);
//        string investmentType = ReadString("Enter Investment Type: ");
//        int rooms = ReadInt("Enter Number of Rooms (> 0): ", minValue: 1);
//        int starRating = ReadInt("Enter Star Rating (1-5): ", minValue: 1, maxValue: 5);
//        return new Hotel(name, price, location, marketValue, investmentType, rooms, starRating);
//    }

//    static LandPlot CreateManualLandPlot()
//    {
//        string name = ReadString("Enter LandPlot Name: ");
//        decimal price = ReadDecimal("Enter LandPlot Price (> 0): ", minValue: 0.01m);
//        string location = ReadString("Enter Location: ");
//        decimal marketValue = ReadDecimal("Enter Market Value (> 0): ", minValue: 0.01m);
//        string investmentType = ReadString("Enter Investment Type: ");
//        string soilType = ReadString("Enter Soil Type (e.g., Loam): ");
//        bool infrastructureAccess = ReadBool("Has Infrastructure Access (true/false/yes/no/1/0): ");
//        return new LandPlot(name, price, location, marketValue, investmentType, soilType, infrastructureAccess);
//    }

//    // --- Robust Input Reading Helpers ---
//    static string ReadString(string prompt)
//    {
//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.Write(prompt);
//        Console.ResetColor();
//        return Console.ReadLine() ?? "";
//    }

//    static decimal ReadDecimal(string prompt, decimal? minValue = null, decimal? maxValue = null)
//    {
//        decimal value;
//        while (true)
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.Write(prompt);
//            Console.ResetColor();
//            string? input = Console.ReadLine();
//            if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
//            {
//                if ((minValue == null || value >= minValue) && (maxValue == null || value <= maxValue))
//                {
//                    return value;
//                }
//                else
//                {
//                    string minStr = minValue?.ToString("N2", CultureInfo.InvariantCulture) ?? "-infinity";
//                    string maxStr = maxValue?.ToString("N2", CultureInfo.InvariantCulture) ?? "+infinity";
//                    PrintErrorMessage($"Value must be{(minValue != null ? $" >= {minStr}" : "")}{(minValue != null && maxValue != null ? " and" : "")}{(maxValue != null ? $" <= {maxStr}" : "")}.");
//                }
//            }
//            else
//            {
//                PrintErrorMessage($"Invalid decimal format. Please use '.' as the decimal separator (e.g., 123.45). Input was: '{input}'");
//            }
//        }
//    }
//    static double ReadDouble(string prompt, double? minValue = null, double? maxValue = null)
//    {
//        double value;
//        while (true)
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.Write(prompt);
//            Console.ResetColor();
//            string? input = Console.ReadLine();
//            if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
//            {
//                if ((minValue == null || value >= minValue) && (maxValue == null || value <= maxValue))
//                {
//                    return value;
//                }
//                else
//                {
//                    string minStr = minValue?.ToString("N1", CultureInfo.InvariantCulture) ?? "-infinity";
//                    string maxStr = maxValue?.ToString("N1", CultureInfo.InvariantCulture) ?? "+infinity";
//                    PrintErrorMessage($"Value must be{(minValue != null ? $" >= {minStr}" : "")}{(minValue != null && maxValue != null ? " and" : "")}{(maxValue != null ? $" <= {maxStr}" : "")}.");
//                }
//            }
//            else
//            {
//                PrintErrorMessage($"Invalid number format. Please use '.' as the decimal separator (e.g., 12.3). Input was: '{input}'");
//            }
//        }
//    }
//    static int ReadInt(string prompt, int? minValue = null, int? maxValue = null)
//    {
//        int value;
//        while (true)
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.Write(prompt);
//            Console.ResetColor();
//            string? input = Console.ReadLine();
//            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
//            {
//                if ((minValue == null || value >= minValue) && (maxValue == null || value <= maxValue))
//                {
//                    return value;
//                }
//                else
//                {
//                    string minStr = minValue?.ToString(CultureInfo.InvariantCulture) ?? "any";
//                    string maxStr = maxValue?.ToString(CultureInfo.InvariantCulture) ?? "any";
//                    PrintErrorMessage($"Value must be between {minStr} and {maxStr}.");
//                }
//            }
//            else
//            {
//                PrintErrorMessage($"Invalid integer format. Input was: '{input}'");
//            }
//        }
//    }
//    static bool ReadBool(string prompt)
//    {
//        while (true)
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.Write(prompt);
//            Console.ResetColor();
//            string input = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";
//            if (input == "true" || input == "1" || input == "yes" || input == "y") return true;
//            if (input == "false" || input == "0" || input == "no" || input == "n") return false;
//            PrintErrorMessage("Invalid boolean input. Use true/false/yes/no/1/0.");
//        }
//    }

//}