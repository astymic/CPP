using System.Buffers.Text;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics.Contracts;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Xml.Linq;
using System.Xml;
using lb_14.Classes;
using lb_14.Interfaces;
using Microsoft.VisualBasic;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Numerics;


namespace lb_14;

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

    static Comparison<IName> NameComparison = (x, y) =>
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    };

    static Comparison<IName> PriceComparison = (x, y) =>
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        return x.Price.CompareTo(y.Price);
    };

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
                    case "14": HandleFindFirstElement(); break;
                    case "15": HandleFindAllElements(); break;
                    case "16": HandleSerializeContainer(); break;
                    case "17": HandleDeserializeContainer(); break;
                    case "18": HandleShowTotalPrice(); break;
                    case "19": HandleFindMinMaxProduct(); break;
                    case "20": HandleFindAvarageCategoriesPrice(); break;
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
            catch (InvalidOperationException ex)
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
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- Modifiers ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("6. Change Item by Insertion ID (1-based)");
        Console.WriteLine("7. Change Item by Name");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- Container Operations ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("8. Sort Active Container (In-Place, using Comparison delegate)");
        Console.WriteLine("9. Remove Element by Current Index (0-based)");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- Generators (Non-Mutating Iterators) ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("10. Show Elements in Reverse Order");
        Console.WriteLine("11. Show Elements with Name Containing Substring");
        Console.WriteLine("12. Show Elements Sorted by Price (Generator)");
        Console.WriteLine("13. Show Elements Sorted by Name (Generator)");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- Find Operations (using Predicate) ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("14. Find First Element by Criteria");
        Console.WriteLine("15. Find All Elements by Criteria");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- File Operations ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("16. Serialize Active Container to File");
        Console.WriteLine("17. Deserialize Container from File (Replaces Active)");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- Summary Information ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("18. Show Total Price of Items in Active Container");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("#. --- ### ### ### ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("19. Show Product with Minimum and Maximux Price");
        Console.WriteLine("20. Show Avarage Price for each Category");
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
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"Switching to {chosenType} container will clear the current {activeContainerType} container. Continue? (y/n): ");
                Console.ResetColor();
                switchConfirmed = (Console.ReadLine()?.Trim().ToLower() == "y");
            }

            if (switchConfirmed)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"\nInitializing {chosenType} container...");
                Console.ResetColor();
                containerArray = null;
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
                return;
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nContinuing with the active {activeContainerType} container.");
            Console.ResetColor();
        }

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
        Console.Write("Enter number of elements to generate: ");
        if (int.TryParse(Console.ReadLine(), out int count) && count > 0)
        {
            if (activeContainerType == ContainerType.Array)
            {
                AutomaticGenerationArray(containerArray!, random, count);
                DemonstrateIndexersArray(containerArray!, random);
            }
            else // LinkedList
            {
                AutomaticGenerationList(containerList!, random, count);
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
        if (activeContainerType == ContainerType.Array)
        {
            ManualInputArray(containerArray!);
        }
        else // LinkedList
        {
            ManualInputList(containerList!);
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
            ShowContainerArray(containerArray!, currentCount);
        }
        else // LinkedList
        {
            ShowContainerList(containerList!, currentCount);
        }
    }

    // Still gets by Insertion ID, displays current index
    static void HandleGetElementByInsertionId()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Get Element by Insertion ID from {activeContainerType} Container ---");
        Console.ResetColor();
        if (IsContainerEmpty(out _)) return;

        int maxId = GetNextInsertionId();
        if (maxId == 0)
        {
            PrintErrorMessage("Container is empty, no IDs to get.");
            return;
        }
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Enter insertion ID (1 to {maxId}): "); // User sees 1-based
        Console.ResetColor();

        if (int.TryParse(Console.ReadLine(), out int inputId) && inputId >= 1 && inputId <= maxId)
        {
            int insertionId = inputId - 1; // Internally 0-based
            IName? item = null;
            try
            {
                if (activeContainerType == ContainerType.Array)
                {
                    item = containerArray![insertionId];
                }
                else // LinkedList
                {
                    item = containerList![insertionId];
                }
            }
            catch (IndexOutOfRangeException)
            {
                PrintErrorMessage($"Item with insertion ID {inputId} not found or invalid for container structure.");
                return;
            }
            catch (Exception ex)
            {
                PrintErrorMessage($"Error accessing item with insertion ID {inputId}: {ex.Message}");
                return;
            }


            if (item == null)
            {
                PrintErrorMessage($"Item with insertion ID {inputId} not found (possibly removed or ID never used/valid).");
                return;
            }

            int currentIndex = FindIndexByReference(item);
            if (currentIndex == -1)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nItem Details (Insertion ID: {inputId}, Current Index: Unknown):");
                Console.ResetColor();
                DisplayItemTable(inputId, item); // Use inputId as a fallback display ID
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nItem Details (Insertion ID: {inputId}, Current Index: {currentIndex}):");
                Console.ResetColor();
                DisplayItemTable(currentIndex + 1, item); // Display with 1-based current index
            }
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

        if (activeContainerType == ContainerType.Array)
        {
            IName[]? itemsFoundArray = containerArray![name]; // Uses string indexer
            if (itemsFoundArray != null)
            {
                itemsFoundList.AddRange(itemsFoundArray.Where(i => i != null)!);
            }
        }
        else // LinkedList
        {
            List<IName>? itemsFoundLinkedList = containerList![name]; // Uses string indexer
            if (itemsFoundLinkedList != null)
            {
                itemsFoundList.AddRange(itemsFoundLinkedList);
            }
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
                int currentIndex = FindIndexByReference(foundItem);
                if (currentIndex != -1)
                {
                    WriteDataRowByDisplayId(currentIndex + 1, foundItem, tableWidth);
                }
                else
                {
                    // Item was found by name indexer but couldn't be located by reference
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    string itemStr = foundItem.ToString() ?? "N/A";
                    Console.WriteLine($"|{PadAndCenter($"Warning: Could not determine current index for item '{itemStr.Substring(0, Math.Min(20, itemStr.Length))}...'", tableWidth - 2)}|");
                    Console.ResetColor();
                    // Fallback: display with a generic ID (e.g., its order in the found list)
                    WriteDataRowByDisplayId(itemsFoundList.IndexOf(foundItem) + 1, foundItem, tableWidth);
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

    static void HandleChangeItemByInsertionId()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Change Item by Insertion ID in {activeContainerType} Container ---");
        Console.ResetColor();
        if (IsContainerEmpty(out _)) return;

        int maxId = GetNextInsertionId();
        if (maxId == 0)
        {
            PrintErrorMessage("Container is empty, no IDs to modify.");
            return;
        }
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Enter item insertion ID to modify (1 to {maxId}): "); // User sees 1-based
        Console.ResetColor();

        if (int.TryParse(Console.ReadLine(), out int inputId) && inputId >= 1 && inputId <= maxId)
        {
            int insertionId = inputId - 1; // Internally 0-based
            IName? itemToModify = null;

            try
            {
                if (activeContainerType == ContainerType.Array)
                {
                    itemToModify = containerArray![insertionId];
                }
                else // LinkedList
                {
                    itemToModify = containerList![insertionId];
                }
            }
            catch (IndexOutOfRangeException)
            {
                PrintErrorMessage($"Item with insertion ID {inputId} not found or invalid for container structure.");
                return;
            }
            catch (Exception ex)
            {
                PrintErrorMessage($"Error accessing item with insertion ID {inputId}: {ex.Message}");
                return;
            }

            if (itemToModify == null)
            {
                PrintErrorMessage($"Item with insertion ID {inputId} not found (possibly removed or ID never used/valid).");
                return;
            }

            int currentIndex = FindIndexByReference(itemToModify);
            if (currentIndex == -1)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nItem Details (Insertion ID: {inputId}, Current Index: Unknown):");
                DisplayItemTable(inputId, itemToModify); // Fallback display
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nCurrent item details:");
                Console.ResetColor();
                DisplayItemTable(currentIndex + 1, itemToModify);
            }


            ModifyProperty(itemToModify, insertionId); // Pass 0-based insertionId
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
        if (activeContainerType == ContainerType.Array)
        {
            IName[]? itemsFoundArray = containerArray![name];
            if (itemsFoundArray != null) validItems.AddRange(itemsFoundArray.Where(i => i != null)!);
        }
        else // LinkedList
        {
            List<IName>? itemsFoundList = containerList![name];
            if (itemsFoundList != null) validItems.AddRange(itemsFoundList);
        }


        if (validItems.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"No valid elements found matching Name '{name}'.");
            Console.ResetColor();
            return;
        }

        IName itemToModify;
        int itemInsertionId = -1; // 0-based
        int currentDisplayIndex = -1; // 0-based

        if (validItems.Count == 1)
        {
            itemToModify = validItems[0];
            currentDisplayIndex = FindIndexByReference(itemToModify);
            itemInsertionId = GetInsertionIdForItem(itemToModify); // Get 0-based insertion ID

            if (itemInsertionId == -1 || currentDisplayIndex == -1) { PrintErrorMessage("Could not find ID or index for the item."); return; }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nFound one item (Current Index: {currentDisplayIndex + 1}, Insertion ID: {itemInsertionId + 1}):");
            Console.ResetColor();
        }
        else // Multiple items found
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nFound {validItems.Count} items with Name '{name}'. Please choose which one to modify:");
            Console.ResetColor();

            Dictionary<int, int> choiceToCurrentIndexMap = new Dictionary<int, int>();

            for (int i = 0; i < validItems.Count; i++)
            {
                int currentItemIndex = FindIndexByReference(validItems[i]); // 0-based
                if (currentItemIndex != -1)
                {
                    string itemInfo = validItems[i].ToString() ?? "N/A";
                    Console.WriteLine($"{i + 1}. (Index: {currentItemIndex + 1}) {itemInfo}"); // Display 1-based
                    choiceToCurrentIndexMap[i + 1] = currentItemIndex;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    string itemStr = validItems[i].ToString() ?? "N/A";
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
                itemToModify = GetItemByCurrentIndex(chosenCurrentIndex);
                if (itemToModify == null) { PrintErrorMessage("Failed to re-acquire selected item by current index."); return; }

                itemInsertionId = GetInsertionIdForItem(itemToModify); // 0-based
                currentDisplayIndex = chosenCurrentIndex; // 0-based

                if (itemInsertionId == -1) { PrintErrorMessage("Could not determine insertion ID for the chosen item."); return; }
            }
            else
            {
                PrintErrorMessage("Invalid choice or item mapping failed.");
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nSelected item (Current Index: {currentDisplayIndex + 1}, Insertion ID: {itemInsertionId + 1}):"); // Display 1-based
            Console.ResetColor();
        }

        // Modify the selected item
        if (currentDisplayIndex != -1 && itemToModify != null)
        {
            DisplayItemTable(currentDisplayIndex + 1, itemToModify); // Display with 1-based current index
            ModifyProperty(itemToModify, itemInsertionId); // Pass 0-based insertionId
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nCould not reliably identify the selected item or its index. Modification cancelled.");
            Console.WriteLine(itemToModify?.ToString() ?? "N/A");
            Console.ResetColor();
        }
    }

    static void HandleSortContainer()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Sorting {activeContainerType} Container (In-Place using Comparison Delegate) ---");
        Console.ResetColor();
        if (IsContainerEmpty(out _)) return;

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
                    sortAction = () => containerArray.Sort(NameComparison); // Use new Sort with Comparison
                }
                else // LinkedList
                {
                    if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
                    sortAction = () => containerList.Sort(NameComparison); // Use new Sort with Comparison
                }
                break;

            case "2":
                sortParameter = "Price";
                Console.WriteLine("\nSorting by Price...");
                if (activeContainerType == ContainerType.Array)
                {
                    if (containerArray == null) { PrintErrorMessage("Array container is null."); return; }
                    sortAction = () => containerArray.Sort(PriceComparison); // Use new Sort with Comparison
                }
                else // LinkedList
                {
                    if (containerList == null) { PrintErrorMessage("LinkedList container is null."); return; }
                    sortAction = () => containerList.Sort(PriceComparison); // Use new Sort with Comparison
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
                Console.ResetColor();
                HandleShowContainer(); // Show sorted container
            }
            catch (Exception ex)
            {
                PrintErrorMessage($"Error during sorting by {sortParameter}: {ex.Message}");
            }
        }
        else
        {
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
        Console.Write($"Enter element index to remove (0 to {currentCount - 1}): ");
        Console.ResetColor();

        if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < currentCount)
        {
            IName? removedItem = null;
            int removedItemInsertionId = -1; // 0-based

            try
            {
                IName? itemToRemove = GetItemByCurrentIndex(index); // Get item before removal to find its original insertion ID

                if (itemToRemove != null)
                {
                    removedItemInsertionId = GetInsertionIdForItem(itemToRemove); // Get 0-based insertion ID
                }
                else
                {
                    PrintErrorMessage($"Could not retrieve item at index {index} before removal.");
                    // Attempt removal anyway if GetItemByCurrentIndex failed but index is valid
                }


                if (activeContainerType == ContainerType.Array)
                {
                    removedItem = containerArray!.RemoveById(index);
                }
                else // LinkedList
                {
                    removedItem = containerList!.RemoveByIndex(index);
                }

                if (removedItem != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    // Display 1-based insertion ID if known
                    string idString = removedItemInsertionId != -1 ? $"(original Insertion ID: {removedItemInsertionId + 1})" : "(original Insertion ID unknown)";
                    Console.WriteLine($"\nElement at index {index} {idString} was removed:");
                    Console.WriteLine(removedItem.ToString() ?? "Removed item details unavailable.");
                    Console.ResetColor();
                }
                else
                {
                    PrintErrorMessage($"Error: Failed to remove item at index {index}. Item might have been null unexpectedly or removal failed.");
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage($"Error during removal at index {index}: {ex.Message}");
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
        if (activeContainerType == ContainerType.Array)
        {
            reversedItems = containerArray!.GetReverseArray();
        }
        else // LinkedList
        {
            reversedItems = containerList!.GetReverseArray();
        }

        int tableWidth = CalculateTableWidth();
        PrintTableHeader(tableWidth);
        int count = 0;
        foreach (var item in reversedItems)
        {
            WriteDataRowByDisplayId(count + 1, item, tableWidth); // Display ID is just its order in the generated sequence
            DrawHorizontalLine(tableWidth);
            count++;
        }
        if (count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"|{PadAndCenter("(No items yielded by generator)", tableWidth - 2)}|");
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
        if (activeContainerType == ContainerType.Array)
        {
            itemsWithSubline = containerArray!.GetArrayWithSublineInName(subline);
        }
        else // LinkedList
        {
            itemsWithSubline = containerList!.GetArrayWithSublineInName(subline);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nResults for names containing '{subline}':");
        Console.ResetColor();
        int tableWidth = CalculateTableWidth();
        PrintTableHeader(tableWidth);
        int count = 0;
        foreach (var item in itemsWithSubline)
        {
            int actualCurrentIndex = FindIndexByReference(item);
            WriteDataRowByDisplayId(actualCurrentIndex != -1 ? actualCurrentIndex + 1 : count + 1, item, tableWidth);
            DrawHorizontalLine(tableWidth);
            count++;
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
        if (activeContainerType == ContainerType.Array)
        {
            sortedItems = containerArray!.GetSortedByArrayPrice();
        }
        else // LinkedList
        {
            sortedItems = containerList!.GetSortedByArrayPrice();
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nItems sorted by Price (Generated Sequence):");
        Console.ResetColor();
        int tableWidth = CalculateTableWidth();
        PrintTableHeader(tableWidth);
        int count = 0;
        foreach (var item in sortedItems)
        {
            if (item != null)
            {
                WriteDataRowByDisplayId(count + 1, item, tableWidth);
                DrawHorizontalLine(tableWidth);
                count++;
            }
        }
        if (count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"|{PadAndCenter("(No items yielded by generator)", tableWidth - 2)}|");
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
        if (activeContainerType == ContainerType.Array)
        {
            sortedItems = containerArray!.GetSortedArrayByName();
        }
        else // LinkedList
        {
            sortedItems = containerList!.GetSortedArrayByName();
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nItems sorted by Name (Generated Sequence):");
        Console.ResetColor();
        int tableWidth = CalculateTableWidth();
        PrintTableHeader(tableWidth);
        int count = 0;
        foreach (var item in sortedItems)
        {
            if (item != null)
            {
                WriteDataRowByDisplayId(count + 1, item, tableWidth);
                DrawHorizontalLine(tableWidth);
                count++;
            }
        }
        if (count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"|{PadAndCenter("(No items yielded by generator)", tableWidth - 2)}|");
            DrawHorizontalLine(tableWidth);
            Console.ResetColor();
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Sorted by Name generator yielded {count} items.");
        Console.ResetColor();
    }

    // --- Find Operations (New Handlers) ---
    static void HandleFindFirstElement()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Find First Element in {activeContainerType} Container ---");
        Console.ResetColor();
        if (IsContainerEmpty(out _)) return;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Select search criterion:");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("1. By Exact Name");
        Console.WriteLine("2. By Price");
        Console.WriteLine("3. By Class Type Name (e.g., 'Apartment', 'Product')");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter choice: ");
        Console.ResetColor();
        string? choice = Console.ReadLine();

        Predicate<IName>? predicate = null;
        string criteriaDescription = "";

        try
        {
            switch (choice)
            {
                case "1":
                    string searchName = ReadString("Enter exact Name to find: ");
                    if (string.IsNullOrWhiteSpace(searchName)) { PrintErrorMessage("Name cannot be empty."); return; }
                    predicate = item => item.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase);
                    criteriaDescription = $"Exact Name = '{searchName}'";
                    break;
                case "2":
                    decimal priceThreshold = ReadDecimal("Enter Price to find: ", 0.00001m);
                    predicate = item => item.Price == priceThreshold;
                    criteriaDescription = $"Price = {priceThreshold:N2}";
                    break;
                case "3":
                    string typeName = ReadString("Enter Class Type Name (e.g., Apartment, Product): ");
                    if (string.IsNullOrWhiteSpace(typeName)) { PrintErrorMessage("Type name cannot be empty."); return; }
                    predicate = item => item.GetType().Name.Equals(typeName, StringComparison.OrdinalIgnoreCase);
                    criteriaDescription = $"Class Type = '{typeName}'";
                    break;
                default:
                    PrintErrorMessage("Invalid choice.");
                    return;
            }
        }
        catch (FormatException ex) { PrintErrorMessage($"Invalid input format: {ex.Message}"); return; }
        catch (ValueLessThanZero ex) { PrintErrorMessage($"Input error: {ex.Message}"); return; }


        IName? foundItem = null;
        if (activeContainerType == ContainerType.Array && containerArray != null)
        {
            foundItem = containerArray.Find(predicate);
        }
        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
        {
            foundItem = containerList.Find(predicate);
        }
        else
        {
            PrintErrorMessage("Active container is not initialized correctly."); return;
        }


        if (foundItem != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nFirst element found matching criterion ({criteriaDescription}):");
            Console.ResetColor();
            int currentIndex = FindIndexByReference(foundItem);
            DisplayItemTable(currentIndex != -1 ? currentIndex + 1 : 1, foundItem); // Display with current index or fallback
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nNo element found matching criterion ({criteriaDescription}).");
            Console.ResetColor();
        }
    }

    static void HandleFindAllElements()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Find All Elements in {activeContainerType} Container ---");
        Console.ResetColor();
        if (IsContainerEmpty(out _)) return;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Select search criterion:");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("1. By Exact Name");
        Console.WriteLine("2. By Price");
        Console.WriteLine("3. By Class Type Name (e.g., 'Apartment', 'Product')");
        Console.WriteLine("4. By Name Containing Substring");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter choice: ");
        Console.ResetColor();
        string? choice = Console.ReadLine();

        Predicate<IName>? predicate = null;
        string criteriaDescription = "";

        try
        {
            switch (choice)
            {
                case "1":
                    string searchName = ReadString("Enter exact Name to find: ");
                    if (string.IsNullOrWhiteSpace(searchName)) { PrintErrorMessage("Name cannot be empty."); return; }
                    predicate = item => item.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase);
                    criteriaDescription = $"Exact Name = '{searchName}'";
                    break;
                case "2":
                    decimal priceThreshold = ReadDecimal("Enter Price to find: ", 0.00001m);
                    predicate = item => item.Price == priceThreshold;
                    criteriaDescription = $"Price = {priceThreshold:N2}";
                    break;
                case "3":
                    string typeName = ReadString("Enter Class Type Name (e.g., Apartment, Product): ");
                    if (string.IsNullOrWhiteSpace(typeName)) { PrintErrorMessage("Type name cannot be empty."); return; }
                    predicate = item => item.GetType().Name.Equals(typeName, StringComparison.OrdinalIgnoreCase);
                    criteriaDescription = $"Class Type = '{typeName}'";
                    break;
                case "4":
                    string subName = ReadString("Enter substring for Name: ");
                    if (string.IsNullOrWhiteSpace(subName)) { PrintErrorMessage("Substring cannot be empty."); return; }
                    // Case-insensitive substring search
                    predicate = item => item.Name.IndexOf(subName, StringComparison.OrdinalIgnoreCase) >= 0;
                    criteriaDescription = $"Name containing '{subName}'";
                    break;
                default:
                    PrintErrorMessage("Invalid choice.");
                    return;
            }
        }
        catch (FormatException ex) { PrintErrorMessage($"Invalid input format: {ex.Message}"); return; }
        catch (ValueLessThanZero ex) { PrintErrorMessage($"Input error: {ex.Message}"); return; }


        IEnumerable<IName>? foundItemsEnumerable = null;
        if (activeContainerType == ContainerType.Array && containerArray != null)
        {
            foundItemsEnumerable = containerArray.FindAll(predicate);
        }
        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
        {
            foundItemsEnumerable = containerList.FindAll(predicate);
        }
        else
        {
            PrintErrorMessage("Active container is not initialized correctly."); return;
        }

        if (foundItemsEnumerable != null && foundItemsEnumerable.Any())
        {
            List<IName> itemsList = foundItemsEnumerable.ToList(); // To count and iterate easily
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nFound {itemsList.Count} element(s) matching criterion ({criteriaDescription}):");
            Console.ResetColor();

            int tableWidth = CalculateTableWidth();
            PrintTableHeader(tableWidth);
            int displayIdCounter = 1;
            foreach (var item in itemsList)
            {
                int actualCurrentIndex = FindIndexByReference(item);
                WriteDataRowByDisplayId(actualCurrentIndex != -1 ? actualCurrentIndex + 1 : displayIdCounter, item, tableWidth);
                DrawHorizontalLine(tableWidth);
                displayIdCounter++;
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nNo elements found matching criterion ({criteriaDescription}).");
            Console.ResetColor();
        }
    }


    // --- Indexer Interaction Methods ---

    // Array Container Indexer Demonstration
    static void DemonstrateIndexersArray(Container<IName> container, Random random)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n--- Demonstrating Array Container Indexer Usage ---");
        Console.ResetColor();
        if (container.IsEmpty(false)) return;

        int currentCount = container.GetCount();
        int nextId = container.GetInsertionId(); // This is the *next* ID to be assigned (0-based)

        // 1. Demonstrate Insertion ID Indexer (Get)
        if (nextId > 0) // If at least one ID has been assigned
        {
            // Try to get a valid, existing insertion ID
            int demoInsertionId = -1;
            IName? itemById = null;
            List<int> currentValidInsertionIds = container.GetInsertionOrder().Take(container.GetCount()).ToList();

            if (currentValidInsertionIds.Any())
            {
                demoInsertionId = currentValidInsertionIds[random.Next(currentValidInsertionIds.Count)];
            }


            if (demoInsertionId != -1)
            {
                Console.WriteLine($"1. Accessing item by existing insertion ID [{demoInsertionId + 1}]:"); // Display 1-based
                try
                {
                    itemById = container[demoInsertionId]; // Use 0-based ID
                    if (itemById != null)
                    {
                        int currentIndex = FindIndexByReference(itemById);
                        string indexStr = currentIndex != -1 ? $"(Current Index: {currentIndex + 1})" : ""; // Display 1-based
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"   Found {indexStr}: {itemById.ToString() ?? "N/A"}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"   Item with insertion ID {demoInsertionId + 1} not found (unexpectedly null).");
                        Console.ResetColor();
                    }
                }
                catch (Exception ex)
                {
                    PrintErrorMessage($"   Error getting item by insertion ID {demoInsertionId + 1}: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("1. No valid insertion IDs found in current order to demonstrate get by ID.");
            }
        }
        else
        {
            Console.WriteLine("1. No items added yet, cannot demonstrate insertion ID indexer.");
        }

        // 2. Demonstrate Name Indexer (Get)
        string? demoName = FindDemoName(container.GetItems(), container.GetCount(), random);
        Console.WriteLine($"\n2. Using string indexer container[\"{demoName ?? "N/A"}\"]:");
        if (!string.IsNullOrWhiteSpace(demoName))
        {
            try
            {
                IName[]? itemsByName = container[demoName];
                if (itemsByName != null && itemsByName.Any(i => i != null))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"   Found {itemsByName.Count(i => i != null)} item(s):");
                    foreach (var item in itemsByName.Where(i => i != null))
                    {
                        int currentIndex = FindIndexByReference(item!);
                        string indexStr = currentIndex != -1 ? $"(Index: {currentIndex + 1})" : "";
                        Console.WriteLine($"   - {indexStr} {item!.ToString() ?? "N/A"}");
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
            Console.WriteLine("   Could not find an item with a non-empty name in the sample to demonstrate.");
            Console.ResetColor();
        }

        // 3. Demonstrate Insertion ID Indexer (Set)
        int validDemoIdToSet = FindValidInsertionId(container); // Returns 0-based ID
        if (validDemoIdToSet != -1)
        {
            Console.WriteLine($"\n3. Attempting to change item with insertion ID [{validDemoIdToSet + 1}] using property modification (via indexer access):"); // Display 1-based
            IName? itemToModify = container[validDemoIdToSet]; // Get by 0-based ID
            if (itemToModify != null)
            {
                int currentIndex = FindIndexByReference(itemToModify);
                string indexStr = currentIndex != -1 ? $"(Current Index: {currentIndex + 1})" : "";
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"   Original Item {indexStr}: '{itemToModify.ToString() ?? "N/A"}'");
                Console.ResetColor();
                try
                {
                    string newName = $"ChangedViaIndexer-{validDemoIdToSet + 1}";
                    Console.WriteLine($"   Attempting to set Name to '{newName}'...");
                    itemToModify.GetType().GetProperty("Name")?.SetValue(itemToModify, newName); // Modify property of retrieved item

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"   Property 'Name' potentially updated (check via Show Container).");

                    IName? changedItem = container[validDemoIdToSet]; // Re-fetch by ID
                    int changedIndex = changedItem != null ? FindIndexByReference(changedItem) : -1;
                    string changedIndexStr = changedIndex != -1 ? $"(Current Index: {changedIndex + 1})" : "";

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"   Current value {changedIndexStr}: {changedItem?.ToString() ?? "Not Found!"}");
                    Console.ResetColor();
                }
                catch (TargetInvocationException tie) { PrintErrorMessage($"   Error modifying property: {tie.InnerException?.Message ?? tie.Message}"); }
                catch (Exception ex) { PrintErrorMessage($"   Error modifying property or using indexer: {ex.Message}"); }
            }
            else { Console.WriteLine($"   Could not retrieve item with insertion ID {validDemoIdToSet + 1} for modification demonstration."); }
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
            Console.WriteLine("Container is empty, cannot demonstrate indexers.");
            Console.ResetColor();
            return;
        }

        List<int> currentInsertionOrder = container.GetInsertionOrder();
        if (currentInsertionOrder.Count == 0)
        {
            Console.WriteLine("Container has items but insertion order list is empty (unexpected). Cannot demonstrate.");
            return;
        }

        // 1. Demonstrate Insertion ID Indexer (Get)
        int demoInsertionIdList = -1;
        if (currentInsertionOrder.Any())
        {
            demoInsertionIdList = currentInsertionOrder[random.Next(currentInsertionOrder.Count)]; // Get an existing 0-based ID
        }

        if (demoInsertionIdList != -1)
        {
            Console.WriteLine($"1. Accessing item by existing insertion ID [{demoInsertionIdList + 1}]:"); // Display 1-based
            try
            {
                IName? itemById = container[demoInsertionIdList]; // Use 0-based ID
                if (itemById != null)
                {
                    int currentIndex = FindIndexByReference(itemById);
                    string indexStr = currentIndex != -1 ? $"(Current Index: {currentIndex + 1})" : "";
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"   Found {indexStr}: {itemById.ToString() ?? "N/A"}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"   Item with insertion ID {demoInsertionIdList + 1} not found (unexpectedly null).");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage($"   Error getting item by insertion ID {demoInsertionIdList + 1}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("1. No valid insertion IDs found in current order to demonstrate get by ID.");
        }


        // 2. Demonstrate Name Indexer (Get)
        string? demoName = FindDemoName(container, random);
        Console.WriteLine($"\n2. Using string indexer container[\"{demoName ?? "N/A"}\"]:");
        if (!string.IsNullOrWhiteSpace(demoName))
        {
            try
            {
                List<IName>? itemsByName = container[demoName];
                if (itemsByName != null && itemsByName.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"   Found {itemsByName.Count} item(s):");
                    foreach (var item in itemsByName)
                    {
                        int currentIndex = FindIndexByReference(item);
                        string indexStr = currentIndex != -1 ? $"(Index: {currentIndex + 1})" : "";
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
            Console.WriteLine("   Could not find an item with a non-empty name in the sample to demonstrate.");
            Console.ResetColor();
        }

        // 3. Demonstrate Insertion ID Indexer (Set)
        int validDemoIdListToSet = FindValidInsertionId(container); // Returns 0-based ID
        if (validDemoIdListToSet != -1)
        {
            Console.WriteLine($"\n3. Attempting to change item with insertion ID [{validDemoIdListToSet + 1}] using property modification (via indexer access):"); // Display 1-based
            IName? itemToModify = container[validDemoIdListToSet]; // Get by 0-based ID
            if (itemToModify != null)
            {
                int currentIndex = FindIndexByReference(itemToModify);
                string indexStr = currentIndex != -1 ? $"(Current Index: {currentIndex + 1})" : "";
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"   Original Item {indexStr}: '{itemToModify.ToString() ?? "N/A"}'");
                Console.ResetColor();
                try
                {
                    string newName = $"ChangedViaIndexer-{validDemoIdListToSet + 1}";
                    Console.WriteLine($"   Attempting to set Name to '{newName}' using direct property modification...");
                    itemToModify.GetType().GetProperty("Name")?.SetValue(itemToModify, newName); // Modify property

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"   Property 'Name' potentially updated (check via Show Container).");

                    IName? changedItem = container[validDemoIdListToSet]; // Re-fetch
                    int changedIndex = changedItem != null ? FindIndexByReference(changedItem) : -1;
                    string changedIndexStr = changedIndex != -1 ? $"(Current Index: {changedIndex + 1})" : "";

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"   Current value {changedIndexStr}: {changedItem?.ToString() ?? "Not Found!"}");
                    Console.ResetColor();
                }
                catch (TargetInvocationException tie) { PrintErrorMessage($"   Error modifying property: {tie.InnerException?.Message ?? tie.Message}"); }
                catch (Exception ex) { PrintErrorMessage($"   Error modifying property or using indexer: {ex.Message}"); }
            }
            else { Console.WriteLine($"   Could not retrieve item with insertion ID {validDemoIdListToSet + 1} for modification demonstration."); }
        }
        else
        {
            Console.WriteLine("\n3. Cannot demonstrate modification: No suitable item found with a valid insertion ID.");
        }


        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--- End LinkedList Indexer Demonstration ---");
        Console.ResetColor();
    }

    static string? FindDemoName(IName?[] items, int count, Random random)
    {
        string? demoName = null;
        if (count > 0)
        {
            List<IName> validItems = items.Take(count).Where(it => it != null && !string.IsNullOrWhiteSpace(it.Name)).ToList()!;
            if (validItems.Any())
            {
                demoName = validItems[random.Next(validItems.Count)].Name;
            }
        }
        return demoName;
    }
    static string? FindDemoName(ContainerLinkedList<IName> listContainer, Random random)
    {
        string? demoName = null;
        if (listContainer.Count > 0)
        {
            List<IName> validItems = listContainer.Where(it => it != null && !string.IsNullOrWhiteSpace(it.Name)).ToList();
            if (validItems.Any())
            {
                demoName = validItems[random.Next(validItems.Count)].Name;
            }
        }
        return demoName;
    }

    static int FindValidInsertionId(Container<IName> container)
    {
        if (container.GetCount() == 0) return -1;
        var currentInsertionIds = container.GetInsertionOrder().Take(container.GetCount()).ToList();
        if (currentInsertionIds.Any())
        {
            for (int i = currentInsertionIds.Count - 1; i >= 0; i--)
            {
                int id = currentInsertionIds[i];
                if (container[id] != null) return id; // container[id] uses the 0-based insertion ID
            }
        }
        return -1;
    }

    static int FindValidInsertionId(ContainerLinkedList<IName> container)
    {
        if (container.Count == 0) return -1;
        List<int> order = container.GetInsertionOrder();
        if (order.Any())
        {
            for (int i = order.Count - 1; i >= 0; i--)
            {
                int id = order[i]; // This is a 0-based insertion ID
                if (container[id] != null) return id;
            }
        }
        return -1;
    }

    // --- Property Modification Logic ---
    static void ModifyProperty(object itemToModify, int itemInsertionId)
    {
        ArgumentNullException.ThrowIfNull(itemToModify);

        var properties = itemToModify.GetType()
                                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                        .Where(p => p.CanWrite && p.GetSetMethod(true) != null && p.Name != "Item") // Exclude indexers
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

            if (!TryParseValue(newValueString, targetType, isNullable, out convertedValue))
            {
                return; // Error message printed by TryParseValue
            }

            try
            {
                selectedProperty.SetValue(itemToModify, convertedValue, null);
                Console.ForegroundColor = ConsoleColor.Green;
                // Display 1-based insertion ID
                Console.WriteLine($"\nProperty '{selectedProperty.Name}' updated successfully for item (Insertion ID: {itemInsertionId + 1}).");
                Console.WriteLine("New item details:");
                Console.ResetColor();

                int currentIndex = FindIndexByReference((IName)itemToModify);
                if (currentIndex != -1)
                {
                    DisplayItemTable(currentIndex + 1, (IName)itemToModify); // Display with 1-based current index
                }
                else
                {
                    DisplayItemTable(itemInsertionId + 1, (IName)itemToModify);
                    PrintErrorMessage("Could not determine current index after modification for display. Displayed with Insertion ID.");
                }
            }
            catch (TargetInvocationException tie)
            {
                PrintErrorMessage($"Validation Error setting property '{selectedProperty.Name}': {tie.InnerException?.Message ?? tie.Message}");
            }
            catch (ArgumentException argEx)
            {
                PrintErrorMessage($"Error setting property '{selectedProperty.Name}': Type mismatch or invalid argument. {argEx.Message}");
            }
            catch (Exception ex)
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
            return true; // Successfully parsed to null
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
            else
            {
                TypeConverter converter = TypeDescriptor.GetConverter(targetType);
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    // Use InvariantCulture for consistent parsing of numbers, dates, etc.
                    parsedValue = converter.ConvertFromString(null, CultureInfo.InvariantCulture, input);
                }
                else
                {
                    // Fallback if TypeConverter is not available or cannot convert from string
                    parsedValue = Convert.ChangeType(input, targetType, CultureInfo.InvariantCulture);
                }
            }
            return true;
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is NotSupportedException || ex is ArgumentException)
        {
            PrintErrorMessage($"Conversion Error: Could not convert '{input}' to type {targetType.Name}. {ex.Message}");
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
        GenerateItems(count, random, item => container.AddLast(item));
        Console.WriteLine("\nLinkedList Generation process finished.");
    }

    static void GenerateItems(int count, Random random, Action<IName> addAction)
    {
        for (int i = 0; i < count; i++)
        {
            IName newItem;
            int typeChoice = random.Next(1, 9); // 1 to 8
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
                    case 8: // Base RealEstate for variety
                        newItem = new RealEstate($"BaseRE{i}-{random.Next(1000)}",
                                                 Math.Max(0.01m, Math.Round(random.Next(5000, 20000) + (decimal)random.NextDouble(), 2)),
                                                 $"Loc{i}",
                                                 Math.Max(1.0, Math.Round(random.Next(50, 200) + random.NextDouble(), 1)),
                                                 "Base");
                        break;
                    default: continue; // Should not happen with Next(1,9)
                }
                addAction(newItem);
                Console.Write(".");
            }
            catch (ValueLessThanZero vlzEx)
            {
                Console.Write("V"); // Value less than zero error
                System.Diagnostics.Debug.WriteLine($"\nGeneration Validation Error: {vlzEx.Message} for type {typeChoice}. Skipped.");
            }
            catch (Exception ex)
            {
                Console.Write("X"); // Other critical error
                System.Diagnostics.Debug.WriteLine($"\nGeneration Critical Error: Failed to create item of type {typeChoice}. {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    // --- Manual Input ---
    static void ManualInputArray(Container<IName> container)
    {
        IName? newItem = CreateItemManually();
        if (newItem != null)
        {
            container.Add(newItem);
            Console.ForegroundColor = ConsoleColor.Green;
            // GetInsertionId() for array returns the *next* ID (0-based), so subtract 1 for the one just added.
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
            // GetNextInsertionId() for list returns the *next* ID (0-based), so subtract 1 for current.
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
                _ => throw new ArgumentException("Invalid class choice.")
            };
        }
        catch (ValueLessThanZero ex) { PrintErrorMessage($"Creation Error: {ex.Message}"); return null; }
        catch (FormatException ex) { PrintErrorMessage($"Invalid input format during creation: {ex.Message}"); return null; }
        catch (ArgumentException ex) { PrintErrorMessage($"Invalid argument during creation: {ex.Message}"); return null; }
        catch (Exception ex) { PrintErrorMessage($"Unexpected error during creation: {ex.Message}"); return null; }
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

        int displayIndex = 0; // This is the current iteration index, 0-based for display
        foreach (var item in container) // Relies on GetEnumerator which iterates over current items
        {
            // The displayId for showing the whole container should be its current 0-based index + 1
            WriteDataRowByDisplayId(displayIndex + 1, item, tableWidth);
            DrawHorizontalLine(tableWidth);
            displayIndex++;
        }
        if (displayIndex == 0 && currentCount > 0)
        {
            Console.WriteLine($"|{PadAndCenter("(Container might have only null items or GetEnumerator issue)", tableWidth - 2)}|");
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

        int displayIndex = 0; // Current iteration index for display
        foreach (var item in container) // Relies on GetEnumerator
        {
            WriteDataRowByDisplayId(displayIndex + 1, item, tableWidth);
            DrawHorizontalLine(tableWidth);
            displayIndex++;
        }
    }

    // Displays item using 1-based displayId (can be current index+1, result order+1, or insertionId+1)
    static void DisplayItemTable(int displayId, IName item)
    {
        if (item == null)
        {
            PrintErrorMessage("Item to display is null.");
            return;
        }
        int tableWidth = CalculateTableWidth();
        PrintTableHeader(tableWidth);
        WriteDataRowByDisplayId(displayId, item, tableWidth);
        DrawHorizontalLine(tableWidth);
    }

    // --- Table Formatting Helpers ---
    const int idWidth = 6; // For display ID (current index, result order, etc.)
    const int classWidth = 20; // Increased for longer class names like RealEstateInvestment
    const int nameWidth = 20;
    const int priceWidth = 16;
    const int locationWidth = 20;
    const int sizeWidth = 10;
    const int typeWidth = 18; // Increased
    const int marketValueWidth = 18;
    const int investmentTypeWidth = 20; // Increased
    const int floorWidth = 7;
    const int hoaWidth = 10; // Increased for HOA Fees
    const int gardenWidth = 12; // Increased for Garden Size
    const int poolWidth = 6;
    const int roomsWidth = 7;
    const int starWidth = 6;
    const int soilWidth = 12; // Increased
    const int infraWidth = 7;
    const int padding = 1; // Per column for the '|' separator
    // Num columns: ID, Class, Name, Price, Loc, Size, Type, MktVal, InvType, Floor, HOA, Garden, Pool, Rooms, Star, Soil, Infra = 17
    const int numColumns = 17;

    static int CalculateTableWidth()
    {
        // Sum of all column widths
        int totalDataWidth = idWidth + classWidth + nameWidth + priceWidth + locationWidth +
                             sizeWidth + typeWidth + marketValueWidth + investmentTypeWidth +
                             floorWidth + hoaWidth + gardenWidth + poolWidth + roomsWidth +
                             starWidth + soilWidth + infraWidth;
        // Add one for each '|' separator (numColumns + 1 if separators are on both ends)
        // My current implementation uses numColumns separators effectively.
        // Each PadAndCenter adds its content. The "|" are printed outside.
        // So it's effectively totalDataWidth + numColumns (for left padding of each cell after first) + 1 (for the final |)
        // Simpler: each cell has its width. Total width is sum of cell widths + (numColumns + 1) for all '|'.
        // Let's use the sum of widths and add separators
        return totalDataWidth + (numColumns + 1); // Sum of widths + number of vertical bars
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
        Console.Write($"|{PadAndCenter("ID", idWidth)}");
        Console.Write($"|{PadAndCenter("Class", classWidth)}");
        Console.Write($"|{PadAndCenter("Name", nameWidth)}");
        Console.Write($"|{PadAndCenter("Price", priceWidth)}");
        Console.Write($"|{PadAndCenter("Location", locationWidth)}");
        Console.Write($"|{PadAndCenter("Size (sq.unit)", sizeWidth)}"); // Clarified Size
        Console.Write($"|{PadAndCenter("RE Type", typeWidth)}"); // Clarified Type
        Console.Write($"|{PadAndCenter("Mkt Value", marketValueWidth)}");
        Console.Write($"|{PadAndCenter("Invest Type", investmentTypeWidth)}");
        Console.Write($"|{PadAndCenter("Floor", floorWidth)}");
        Console.Write($"|{PadAndCenter("HOA Fee", hoaWidth)}");
        Console.Write($"|{PadAndCenter("Garden (sq.u)", gardenWidth)}");
        Console.Write($"|{PadAndCenter("Pool", poolWidth)}");
        Console.Write($"|{PadAndCenter("Rooms", roomsWidth)}");
        Console.Write($"|{PadAndCenter("Stars", starWidth)}");
        Console.Write($"|{PadAndCenter("Soil Type", soilWidth)}");
        Console.Write($"|{PadAndCenter("Infra.", infraWidth)}");
        Console.WriteLine("|");
        Console.ResetColor();
    }

    // displayId is 1-based for user display
    static void WriteDataRowByDisplayId(int displayId, object item, int tableWidth)
    {
        if (item == null)
        {
            Console.WriteLine($"|{PadAndCenter(displayId.ToString(), idWidth)}|{PadAndCenter("(Null Item)", tableWidth - idWidth - 3)}|");
            return;
        }

        string FormatDecimal(decimal? d) => d?.ToString("N2", CultureInfo.InvariantCulture) ?? "-";
        string FormatDouble(double? d) => d?.ToString("N1", CultureInfo.InvariantCulture) ?? "-";
        string FormatBool(bool? b) => b.HasValue ? (b.Value ? "Yes" : "No") : "-";
        string FormatInt(int? i) => i?.ToString() ?? "-";
        string FormatString(string? s) => string.IsNullOrWhiteSpace(s) ? "-" : s;

        Type itemType = item.GetType();

        // Get common properties from IName first
        string name = FormatString(GetPropertyValue<string>(item, "Name"));
        string fPrice = FormatDecimal(GetPropertyValue<decimal?>(item, "Price")); // From IName

        // Properties specific to RealEstate or its derivatives
        string loc = FormatString(GetPropertyValue<string>(item, "Location"));
        string fSize = FormatDouble(GetPropertyValue<double?>(item, "Size")); // RealEstate.Size
        string reType = FormatString(GetPropertyValue<string>(item, "Type")); // RealEstate.Type

        // Properties specific to RealEstateInvestment or its derivatives
        string fMktVal = FormatDecimal(GetPropertyValue<decimal?>(item, "MarketValue")); // REI.MarketValue
        string invType = FormatString(GetPropertyValue<string>(item, "InvestmentType")); // REI.InvestmentType

        // Apartment specific
        string fFloor = FormatInt(GetPropertyValue<int?>(item, "FloorNumber"));
        string fHoa = FormatDecimal(GetPropertyValue<decimal?>(item, "HOAFees"));

        // House specific
        string fGarden = FormatDouble(GetPropertyValue<double?>(item, "GardenSize"));
        string fPool = FormatBool(GetPropertyValue<bool?>(item, "Pool"));

        // Hotel specific
        string fRooms = FormatInt(GetPropertyValue<int?>(item, "Rooms"));
        string fStar = FormatInt(GetPropertyValue<int?>(item, "StarRating"));

        // LandPlot specific
        string soil = FormatString(GetPropertyValue<string>(item, "SoilType"));
        string fInfra = FormatBool(GetPropertyValue<bool?>(item, "InfrastructureAccess"));


        Console.Write($"|{PadAndCenter(displayId.ToString(), idWidth)}");
        Console.Write($"|{PadAndCenter(itemType.Name, classWidth)}");
        Console.Write($"|{PadAndCenter(name, nameWidth)}");
        Console.Write($"|{PadAndCenter(fPrice, priceWidth)}");
        Console.Write($"|{PadAndCenter(loc, locationWidth)}");
        Console.Write($"|{PadAndCenter(fSize, sizeWidth)}");
        Console.Write($"|{PadAndCenter(reType, typeWidth)}");
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
        string val = value ?? ""; // Use empty string for null value
        if (totalWidth <= 0) return ""; // Cannot pad to zero or negative width

        // Truncate if value is too long
        val = Truncate(val, totalWidth);

        int spaces = totalWidth - val.Length;
        int padLeft = spaces / 2 + val.Length; // Spaces for left padding + length of string

        return val.PadLeft(padLeft).PadRight(totalWidth);
    }


    static string CenterString(string s, int width)
    {
        if (string.IsNullOrEmpty(s) || width <= 0) return new string(' ', Math.Max(0, width));
        s = Truncate(s, width); // Ensure fits
        int padding = Math.Max(0, (width - s.Length) / 2);
        return new string(' ', padding) + s + new string(' ', Math.Max(0, width - s.Length - padding));
    }


    static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (maxLength <= 0) return ""; // Cannot truncate to zero or negative length
        if (value.Length <= maxLength) return value;

        // Ensure "..." fits
        if (maxLength < 3) return new string('.', maxLength);

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
                if (value == null) return default; // Property value is null

                if (value is TValue correctlyTypedValue) return correctlyTypedValue;

                // Handle Nullable<T> for TValue
                Type tValueType = typeof(TValue);
                Type? underlyingTValueType = Nullable.GetUnderlyingType(tValueType);

                if (underlyingTValueType != null) // TValue is Nullable<U>
                {
                    if (value.GetType() == underlyingTValueType) // Direct cast if underlying types match
                    {
                        return (TValue)Convert.ChangeType(value, underlyingTValueType, CultureInfo.InvariantCulture);
                    }
                }

                // Attempt conversion for common scenarios (e.g. string to TValue or numeric to TValue)
                // This is a simplified conversion logic. More robust would involve TypeConverter.
                try
                {
                    return (TValue)Convert.ChangeType(value, underlyingTValueType ?? tValueType, CultureInfo.InvariantCulture);
                }
                catch (InvalidCastException)
                {
                    // Try specific conversions if general ChangeType fails
                    if (tValueType == typeof(string))
                        return (TValue)(object)value.ToString(); // Fallback to ToString for string requests

                    System.Diagnostics.Debug.WriteLine($"Reflection: Could not cast/convert value of type {value.GetType().Name} to {tValueType.Name} for property '{propertyName}'.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Reflection Error getting '{propertyName}': {ex.GetType().Name} - {ex.Message}");
            }
        }
        return default; // Property not found, not readable, or conversion failed
    }

    // This IsNumericType is not currently used by GetPropertyValue but kept for potential future use.
    private static bool IsNumericType(Type type)
    {
        if (type == null) return false;
        Type actualType = Nullable.GetUnderlyingType(type) ?? type; // Handle nullable numeric types

        switch (Type.GetTypeCode(actualType))
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
            isEmpty = containerArray.IsEmpty(false); // Pass false to prevent double printing
            count = containerArray.GetCount();
        }
        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
        {
            count = containerList.Count;
            isEmpty = (count == 0);
        }
        // No else for ContainerType.None, as that's handled below

        if (activeContainerType == ContainerType.None)
        {
            PrintErrorMessage("No container selected. Please use option 1 or 2 first.");
            isEmpty = true; // Consider it effectively empty for operations
        }
        else if (isEmpty) // Only print if a container is active but empty
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The active container is empty.");
            Console.ResetColor();
        }
        return isEmpty;
    }

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

    // Gets the next insertion ID (0-based), which is also the count of IDs ever assigned.
    // For user display, this is typically shown as 1-based.
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
        return 0; // No active container or container not initialized
    }

    // Finds the current 0-based index of an item by reference.
    private static int FindIndexByReference(IName itemToFind)
    {
        if (itemToFind == null) return -1;

        if (activeContainerType == ContainerType.Array && containerArray != null)
        {
            // Iterate using GetEnumerator which should yield non-null items in current order
            int index = 0;
            foreach (var currentItem in containerArray)
            {
                if (object.ReferenceEquals(currentItem, itemToFind))
                {
                    return index;
                }
                index++;
            }
        }
        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
        {
            int index = 0;
            foreach (var currentItem in containerList) // Iterates through nodes' data
            {
                if (object.ReferenceEquals(currentItem, itemToFind))
                {
                    return index;
                }
                index++;
            }
        }
        return -1; // Not found
    }

    private static int GetInsertionIdForItem(IName itemToFind)
    {
        if (itemToFind == null) return -1;

        int currentIndex = FindIndexByReference(itemToFind);
        if (currentIndex == -1) return -1; // Item not in container by reference

        try
        {
            if (activeContainerType == ContainerType.Array && containerArray != null)
            {
                int[] order = containerArray.GetInsertionOrder(); // This should be the order corresponding to current items
                if (currentIndex < order.Length && currentIndex < containerArray.GetCount()) // Ensure index is valid for current items
                {
                    return order[currentIndex]; // This ID is 0-based
                }
                else { System.Diagnostics.Debug.WriteLine($"Warning: Current index {currentIndex} out of bounds for insertion Order Array (Length: {order.Length}, Count: {containerArray.GetCount()})"); }
            }
            else if (activeContainerType == ContainerType.LinkedList && containerList != null)
            {
                List<int> order = containerList.GetInsertionOrder(); // This should be the order corresponding to current items
                if (currentIndex < order.Count && currentIndex < containerList.GetCount()) // Ensure index is valid
                {
                    return order[currentIndex]; // This ID is 0-based
                }
                else { System.Diagnostics.Debug.WriteLine($"Warning: Current index {currentIndex} out of bounds for insertion Order List (Count: {order.Count}, Actual List Count: {containerList.GetCount()})"); }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetInsertionIdForItem for current index {currentIndex}: {ex.Message}");
        }
        return -1; // Could not determine
    }

    // Gets an item by its 0-based current index in the container.
    private static IName? GetItemByCurrentIndex(int index) // index is 0-based
    {
        if (index < 0) return null;

        if (activeContainerType == ContainerType.Array && containerArray != null)
        {
            if (index < containerArray.GetCount())
            {
                // Need to iterate to the Nth non-null item if GetEnumerator() is the source of truth for current order
                int count = 0;
                foreach (var item in containerArray)
                {
                    if (count == index) return item;
                    count++;
                }
            }
        }
        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
        {
            if (index < containerList.Count)
            {
                int count = 0;
                foreach (var item in containerList)
                {
                    if (count == index) return item;
                    count++;
                }
            }
        }
        return null; // Index out of bounds or item not found
    }

    // --- Serialization / Deserialization Handlers --- (Renumbered options 16 and 17)

    static void HandleSerializeContainer()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Serialize Active {activeContainerType} Container ---");
        Console.ResetColor();

        object? activeContainerObject = null;
        if (activeContainerType == ContainerType.Array && containerArray != null)
        {
            if (containerArray.IsEmpty(true)) return; // Checks if empty and prints message
            activeContainerObject = containerArray;
        }
        else if (activeContainerType == ContainerType.LinkedList && containerList != null)
        {
            if (containerList.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("The active container is empty.");
                Console.ResetColor();
                return;
            }
            activeContainerObject = containerList;
        }
        else
        {
            PrintErrorMessage("No active and non-empty container to serialize. Select/Create a container first.");
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
        catch (InvalidOperationException ex)
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

        object deserializedContainerObj; // Use object for the return type
        try
        {
            var tempContainer = ContainerSerializer.DeserializeContainer(filename);
            if (tempContainer is Container<IName> || tempContainer is ContainerLinkedList<IName>)
            {
                deserializedContainerObj = tempContainer;
            }
            else
            {
                PrintErrorMessage("Deserialization did not return a recognized container type.");
                return;
            }
        }
        catch (FileNotFoundException ex) { PrintErrorMessage(ex.Message); return; } // Message already good
        catch (IOException ex) { PrintErrorMessage($"File IO error during deserialization: {ex.Message}"); return; }
        catch (InvalidOperationException ex) { PrintErrorMessage($"Deserialization Operation Error: {ex.Message}"); return; }
        catch (InvalidDataException ex) { PrintErrorMessage($"Invalid data in file: {ex.Message}"); return; }
        catch (Exception ex)
        {
            PrintErrorMessage($"An unexpected error occurred during deserialization: {ex.GetType().Name} - {ex.Message}");
            return;
        }


        if (deserializedContainerObj == null)
        {
            // Error already printed by catch blocks or DeserializeContainer itself
            Console.WriteLine("Deserialization resulted in a null container.");
            return;
        }

        int loadedItemCount = 0;
        ContainerType deserializedType = ContainerType.None;

        if (deserializedContainerObj is Container<IName> deserializedArray)
        {
            loadedItemCount = deserializedArray.GetCount();
            deserializedType = ContainerType.Array;
        }
        else if (deserializedContainerObj is ContainerLinkedList<IName> deserializedList)
        {
            loadedItemCount = deserializedList.GetCount();
            deserializedType = ContainerType.LinkedList;
        }
        else
        {
            PrintErrorMessage($"Deserialization returned an unexpected object type: {deserializedContainerObj.GetType().Name}");
            return;
        }

        if (loadedItemCount == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Deserialized {deserializedType} container is empty.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Successfully deserialized {loadedItemCount} items into a {deserializedType} container.");
            Console.ResetColor();
        }

        bool switchConfirmed = true;
        if (activeContainerType != ContainerType.None)
        {
            bool currentHasItems = GetActiveContainerCount() > 0;
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
            if (deserializedContainerObj is Container<IName> newArrayContainer)
            {
                containerArray = newArrayContainer;
                containerList = null;
                activeContainerType = ContainerType.Array;
            }
            else if (deserializedContainerObj is ContainerLinkedList<IName> newListContainer)
            {
                containerList = newListContainer;
                containerArray = null;
                activeContainerType = ContainerType.LinkedList;
            }
            // No else needed due to earlier type check

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Active container switched to the deserialized {activeContainerType} container.");
            Console.ResetColor();
            HandleShowContainer(); // Show the newly loaded container
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Switch cancelled. Keeping the current active container.");
            Console.ResetColor();
        }
    }

    // New Handler for Total Price
    static void HandleShowTotalPrice()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Total Price in {activeContainerType} Container ---");
        Console.ResetColor();

        if (activeContainerType == ContainerType.None)
        {
            PrintErrorMessage("No active container. Please select or create a container first.");
            return;
        }

        decimal totalPrice = 0;
        bool isEmptyOrNotInitialized = true;

        if (activeContainerType == ContainerType.Array)
        {
            if (containerArray != null)
            {
                totalPrice = containerArray.TotalPrice;
                isEmptyOrNotInitialized = containerArray.IsEmpty(false);
            }
        }
        else // LinkedList
        {
            if (containerList != null)
            {
                totalPrice = containerList.TotalPrice;
                isEmptyOrNotInitialized = (containerList.Count == 0);
            }
        }

        if (isEmptyOrNotInitialized && (containerArray != null || containerList != null)) // Container exists but is empty
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The active container is empty.");
            Console.ResetColor();
            Console.WriteLine($"Total Price: {totalPrice:N2}"); // Should be 0.00
        }
        else if (containerArray != null || containerList != null) // Container exists and has items (or had items, and price is tracked)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Total Price of all items: {totalPrice:N2}");
            Console.ResetColor();
        }
        else // Should ideally be caught by the first check, but as a safeguard
        {
            PrintErrorMessage("Container object is not initialized, cannot get total price.");
        }
    }

    // New Handler for Min and Max Price
    static void HandleFindMinMaxProduct()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Minimum and Maximum Price in {activeContainerType} Container ---");
        Console.ResetColor();

        if (activeContainerType == ContainerType.None)
        {
            PrintErrorMessage("No active container. Please select or create a container first.");
            return;
        }

        IEnumerable<IName> prices = Enumerable.Empty<IName>();
        bool isEmptyOrNotInitialized = true;
        int itemCount = 0;

        if (activeContainerType == ContainerType.Array)
        {
            if (containerArray != null)
            {
                prices = from product in containerArray
                         where product != null
                         select product;

                itemCount = containerArray.GetCount();
                isEmptyOrNotInitialized = containerArray.IsEmpty(false);
            }
        }
        else // LinkedList 
        {
            if (containerList != null)
            {
                prices = from product in containerList
                         where product != null
                         select product;

                itemCount = containerList.Count;
                isEmptyOrNotInitialized = (itemCount == 0);
            }
        }

        if (isEmptyOrNotInitialized)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The active container is empty or contains no items with prices.");
            Console.ResetColor();
        }
        else if (containerArray != null || containerList != null)
        {
            var minPrice = prices.OrderBy(p => p.Price).First();
            var maxPrice = prices.OrderByDescending(p => p.Price).First();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The Product with Minimal Price is: ");
            Console.ResetColor();
            DisplayItemTable(0, minPrice);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The Product with Maximum Price is: ");
            Console.ResetColor();
            DisplayItemTable(0, maxPrice);
        }
        else
        {
            PrintErrorMessage("Container object is not initialized, cannot get prices.");
        }
    }

    const int avgPriceCategoryColWidth = 25;
    const int avgPriceValueColWidth = 20;

    static void PrintAveragePriceTableHeader(int tableWidth)
    {
        DrawHorizontalLine(tableWidth);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"|{PadAndCenter("Category", avgPriceCategoryColWidth)}");
        Console.Write($"|{PadAndCenter("Average Price", avgPriceValueColWidth)}");
        Console.WriteLine("|");
        Console.ResetColor();
        DrawHorizontalLine(tableWidth);
    }

    static void WriteAveragePriceDataRow(string category, decimal avgPrice, int tableWidth)
    {
        Console.Write($"|{PadAndCenter(category, avgPriceCategoryColWidth)}");
        Console.Write($"|{PadAndCenter(avgPrice.ToString("N2", CultureInfo.InvariantCulture), avgPriceValueColWidth)}");
        Console.WriteLine("|");
    }

    static void HandleFindAvarageCategoriesPrice()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Average Price for each Category in {activeContainerType} Container ---");
        Console.ResetColor();

        if (activeContainerType == ContainerType.None)
        {
            PrintErrorMessage("No active container. Please select or create a container first.");
            return;
        }

        IEnumerable<dynamic> averagePrices = Enumerable.Empty<dynamic>();
        bool isEmptyOrNotInitialized = true;

        if (activeContainerType == ContainerType.Array)
        {
            if (containerArray != null)
            {
                averagePrices = from IName product in containerArray
                                where product != null
                                group product by product.GetType().Name into g
                                select new
                                {
                                    Category = g.Key,
                                    AveragePrice = g.Average(p => p.Price)
                                };

                isEmptyOrNotInitialized = containerArray.IsEmpty(false);
            }
        }
        else // LinkedList
        {
            if (containerList != null)
            {
                averagePrices = from IName product in containerList
                                where product != null
                                group product by product.GetType().Name into g
                                select new
                                {
                                    Category = g.Key,
                                    AveragePrice = g.Average(p => p.Price)
                                };

                isEmptyOrNotInitialized = (containerList.Count == 0);
            }
        }


        if (isEmptyOrNotInitialized)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The active container is empty or no categories found to average.");
            Console.ResetColor();
        }
        else if (containerArray != null || containerList != null)
        {
            int tableWidth = avgPriceCategoryColWidth + avgPriceValueColWidth + 3; 
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(CenterString("Average Prices by Category", tableWidth));
            Console.ResetColor();
            PrintAveragePriceTableHeader(tableWidth);

            if (averagePrices.Any())
            {
                foreach (var item in averagePrices)
                {
                    WriteAveragePriceDataRow(item.Category, item.AveragePrice, tableWidth);
                    DrawHorizontalLine(tableWidth);
                }
            }
            else
            {
                Console.WriteLine($"|{PadAndCenter("(No categories found)", tableWidth - 2)}|");
                DrawHorizontalLine(tableWidth);
            }
        }
        else
        {
            PrintErrorMessage("Container object is not initialized, cannot get prices.");
        }
    }


    // --- Random Generators ---
    static Product GenerateRandomProduct(Random random)
    {
        string[] names = { "Table", "Chair", "Lamp", "Phone", "Book", "Laptop", "Mug", "Monitor", "Keyboard", "Mouse" };
        decimal price = random.Next(10, 1000) + Math.Round((decimal)random.NextDouble(), 2);
        return new Product(names[random.Next(names.Length)] + "-" + random.Next(100, 999), Math.Max(0.01m, price));
    }

    static RealEstate GenerateRandomRealEstate(Random random)
    {
        string[] names = { "Cozy Apt", "Luxury Villa", "Small House", "Big Mansion", "Downtown Loft", "Suburban Home" };
        string[] locations = { "New York", "London", "Paris", "Tokyo", "Kyiv", "Berlin", "Sydney", "Toronto", "Dubai" };
        string[] types = { "Residential", "Commercial", "Industrial", "Mixed-Use", "Land" };
        decimal price = random.Next(100000, 1000000) + Math.Round((decimal)random.NextDouble() * 1000, 2);
        double size = random.Next(50, 500) + Math.Round(random.NextDouble() * 10, 1);
        return new RealEstate(
            names[random.Next(names.Length)] + "-" + random.Next(10, 99),
            Math.Max(0.01m, price),
            locations[random.Next(locations.Length)],
            Math.Max(1.0, size),
            types[random.Next(types.Length)]);
    }

    static RealEstateInvestment GenerateRandomRealEstateInvestment(Random random)
    {
        string[] names = { "Office Bldg", "Shopping Mall", "Warehouse Complex", "Apt Complex Fund", "Data Center REIT" };
        string[] locations = { "Chicago", "Los Angeles", "Houston", "Phoenix", "Philadelphia", "Dallas", "Miami" };
        string[] invTypes = { "REIT", "Direct Property", "Mortgage Fund", "Syndication", "Crowdfunding" };
        decimal price = random.Next(500000, 5000000) + Math.Round((decimal)random.NextDouble() * 10000, 2);
        decimal marketValue = price * (decimal)(0.8 + random.NextDouble() * 0.4); // Market value related to price
        return new RealEstateInvestment(
            names[random.Next(names.Length)],
            Math.Max(0.01m, price),
            locations[random.Next(locations.Length)],
            Math.Max(1.0m, Math.Round(marketValue, 2)),
            invTypes[random.Next(invTypes.Length)]);
    }

    static Apartment GenerateRandomApartment(Random random)
    {
        string[] names = { "Studio Apt", "1BR Condo", "2BR Luxury Apt", "Penthouse Suite", "Garden View Apt" };
        string[] locations = { "Miami Beach", "San Francisco Bay", "Seattle Downtown", "Boston Commons", "Denver LoDo", "Austin SoCo" };
        string[] types = { "Condominium", "Co-op Unit", "Rental Apartment", "Loft Style" };
        decimal price = random.Next(200000, 800000) + Math.Round((decimal)random.NextDouble() * 500, 2);
        double size = random.Next(40, 150) + Math.Round(random.NextDouble() * 5, 1);
        int floor = random.Next(1, 30);
        decimal hoa = random.Next(50, 500) + Math.Round((decimal)random.NextDouble() * 50, 2);
        return new Apartment(
            names[random.Next(names.Length)],
            Math.Max(0.01m, price),
            locations[random.Next(locations.Length)],
            Math.Max(1.0, size),
            types[random.Next(types.Length)],
            floor,
            Math.Max(0m, hoa));
    }

    static House GenerateRandomHouse(Random random)
    {
        string[] names = { "Bungalow Home", "Townhouse Unit", "Ranch Style House", "Cozy Cottage", "Colonial Estate" };
        string[] locations = { "Atlanta Suburbs", "Dallas North", "San Diego Hills", "Orlando Lakes", "Las Vegas Greens", "Nashville Scene" };
        string[] types = { "Single-family", "Multi-family Detached", "Duplex Attached" };
        decimal price = random.Next(300000, 1200000) + Math.Round((decimal)random.NextDouble() * 1000, 2);
        double size = random.Next(100, 400) + Math.Round(random.NextDouble() * 15, 1);
        double gardenSize = random.Next(0, 1000) + Math.Round(random.NextDouble() * 100, 1);
        bool pool = random.Next(3) == 0;
        return new House(
            names[random.Next(names.Length)],
            Math.Max(0.01m, price),
            locations[random.Next(locations.Length)],
            Math.Max(1.0, size),
            types[random.Next(types.Length)],
            gardenSize, 
            pool);
    }

    static Hotel GenerateRandomHotel(Random random)
    {
        string[] names = { "Grand Luxury Hotel", "Budget Friendly Inn", "Seaside Resort & Spa", "Chic Boutique Hotel", "Airport Express Motel" };
        string[] locations = { "Hawaii Islands", "Bali Beaches", "Maldives Atolls", "Fiji Resorts", "Santorini Views", "Vegas Strip Central" };
        string[] invTypes = { "Hospitality REIT", "Hotel Management Co.", "Timeshare Property", "Franchise Brand" };
        decimal price = random.Next(1000000, 10000000) + Math.Round((decimal)random.NextDouble() * 50000, 2);
        decimal marketValue = price * (decimal)(0.9 + random.NextDouble() * 0.3);
        int rooms = random.Next(20, 500);
        int rating = random.Next(1, 6); // 1 to 5 stars
        return new Hotel(
            names[random.Next(names.Length)],
            Math.Max(0.01m, price),
            locations[random.Next(locations.Length)],
            Math.Max(1.0m, Math.Round(marketValue, 2)),
            invTypes[random.Next(invTypes.Length)],
            Math.Max(1, rooms), // Ensure rooms > 0
            rating);
    }

    static LandPlot GenerateRandomLandPlot(Random random)
    {
        string[] names = { "Prime Farmland", "Dense Forest Tract", "Commercial Dev Land", "Residential Zoning Plot", "Waterfront Acreage" };
        string[] locations = { "Rural County Area", "Suburban Edge District", "Urban Infill Zone", "Coastal Region Strip", "Mountain Base Valley" };
        string[] invTypes = { "Land Banking", "Development Project", "Agricultural Use", "Conservation Trust" };
        string[] soilTypes = { "Loamy", "Clay Rich", "Sandy Loam", "Silty Clay", "Peaty Soil", "Chalky Ground" };
        decimal price = random.Next(50000, 500000) + Math.Round((decimal)random.NextDouble() * 2000, 2);
        decimal marketValue = price * (decimal)(0.7 + random.NextDouble() * 0.6);
        bool infra = random.Next(2) == 0;
        return new LandPlot(
            names[random.Next(names.Length)],
            Math.Max(0.01m, price),
            locations[random.Next(locations.Length)],
            Math.Max(1.0m, Math.Round(marketValue, 2)),
            invTypes[random.Next(invTypes.Length)],
            soilTypes[random.Next(soilTypes.Length)],
            infra);
    }


    // --- Manual Creation Methods ---

    static Product CreateManualProduct()
    {
        string name = ReadString("Enter Product Name: ");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Product Name cannot be empty.");
        decimal price = ReadDecimal("Enter Product Price (> 0): ", minValue: 0.01m);
        return new Product(name, price);
    }

    static RealEstate CreateManualRealEstate()
    {
        string name = ReadString("Enter RealEstate Name: ");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("RealEstate Name cannot be empty.");
        decimal price = ReadDecimal("Enter RealEstate Price (> 0): ", minValue: 0.01m);
        string location = ReadString("Enter Location: ");
        if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("Location cannot be empty.");
        double size = ReadDouble("Enter Size (> 0 sq.units): ", minValue: 0.01);
        string type = ReadString("Enter Type (e.g., Residential): ");
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Type cannot be empty.");
        return new RealEstate(name, price, location, size, type);
    }

    static RealEstateInvestment CreateManualRealEstateInvestment()
    {
        string name = ReadString("Enter Investment Name: ");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Investment Name cannot be empty.");
        decimal price = ReadDecimal("Enter Investment Price (> 0): ", minValue: 0.01m);
        string location = ReadString("Enter Location: ");
        if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("Location cannot be empty.");
        decimal marketValue = ReadDecimal("Enter Market Value (> 0): ", minValue: 0.01m);
        string investmentType = ReadString("Enter Investment Type (e.g., REIT): ");
        if (string.IsNullOrWhiteSpace(investmentType)) throw new ArgumentException("Investment Type cannot be empty.");
        return new RealEstateInvestment(name, price, location, marketValue, investmentType);
    }

    static Apartment CreateManualApartment()
    {
        string name = ReadString("Enter Apartment Name: ");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Apartment Name cannot be empty.");
        decimal price = ReadDecimal("Enter Apartment Price (> 0): ", minValue: 0.01m);
        string location = ReadString("Enter Location: ");
        if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("Location cannot be empty.");
        double size = ReadDouble("Enter Size (> 0 sq.units): ", minValue: 0.01);
        string type = ReadString("Enter Type (e.g., Condo): ");
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Type cannot be empty.");
        int floorNumber = ReadInt("Enter Floor Number (> 0): ", minValue: 1);
        decimal hoaFees = ReadDecimal("Enter HOA Fees (>= 0): ", minValue: 0m);
        return new Apartment(name, price, location, size, type, floorNumber, hoaFees);
    }

    static House CreateManualHouse()
    {
        string name = ReadString("Enter House Name: ");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("House Name cannot be empty.");
        decimal price = ReadDecimal("Enter House Price (> 0): ", minValue: 0.01m);
        string location = ReadString("Enter Location: ");
        if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("Location cannot be empty.");
        double size = ReadDouble("Enter Size (> 0 sq.units): ", minValue: 0.01);
        string type = ReadString("Enter Type (e.g., Single-family): ");
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Type cannot be empty.");
        double gardenSize = ReadDouble("Enter Garden Size (>= 0 sq.units): ", minValue: 0.0);
        bool pool = ReadBool("Has Pool (true/false/yes/no/1/0): ");
        return new House(name, price, location, size, type, gardenSize, pool);
    }

    static Hotel CreateManualHotel()
    {
        string name = ReadString("Enter Hotel Name: ");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Hotel Name cannot be empty.");
        decimal price = ReadDecimal("Enter Hotel Price (> 0): ", minValue: 0.01m);
        string location = ReadString("Enter Location: ");
        if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("Location cannot be empty.");
        decimal marketValue = ReadDecimal("Enter Market Value (> 0): ", minValue: 0.01m);
        string investmentType = ReadString("Enter Investment Type: ");
        if (string.IsNullOrWhiteSpace(investmentType)) throw new ArgumentException("Investment Type cannot be empty.");
        int rooms = ReadInt("Enter Number of Rooms (> 0): ", minValue: 1);
        int starRating = ReadInt("Enter Star Rating (1-5): ", minValue: 1, maxValue: 5);
        return new Hotel(name, price, location, marketValue, investmentType, rooms, starRating);
    }

    static LandPlot CreateManualLandPlot()
    {
        string name = ReadString("Enter LandPlot Name: ");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("LandPlot Name cannot be empty.");
        decimal price = ReadDecimal("Enter LandPlot Price (> 0): ", minValue: 0.01m);
        string location = ReadString("Enter Location: ");
        if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("Location cannot be empty.");
        decimal marketValue = ReadDecimal("Enter Market Value (> 0): ", minValue: 0.01m);
        string investmentType = ReadString("Enter Investment Type: ");
        if (string.IsNullOrWhiteSpace(investmentType)) throw new ArgumentException("Investment Type cannot be empty.");
        string soilType = ReadString("Enter Soil Type (e.g., Loam): ");
        if (string.IsNullOrWhiteSpace(soilType)) throw new ArgumentException("Soil Type cannot be empty.");
        bool infrastructureAccess = ReadBool("Has Infrastructure Access (true/false/yes/no/1/0): ");
        return new LandPlot(name, price, location, marketValue, investmentType, soilType, infrastructureAccess);
    }

    // --- Robust Input Reading Helpers ---
    static string ReadString(string prompt)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(prompt);
        Console.ResetColor();
        return Console.ReadLine() ?? "";
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
            if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                bool minOk = minValue == null || value >= minValue.Value;
                bool maxOk = maxValue == null || value <= maxValue.Value;

                if (minOk && maxOk) return value;

                string errorMsg = "Value must be";
                if (minValue != null) errorMsg += $" >= {minValue.Value.ToString("N2", CultureInfo.InvariantCulture)}";
                if (minValue != null && maxValue != null) errorMsg += " and";
                if (maxValue != null) errorMsg += $" <= {maxValue.Value.ToString("N2", CultureInfo.InvariantCulture)}";
                errorMsg += ".";
                PrintErrorMessage(errorMsg);
                if (!minOk && value < minValue!.Value) throw new ValueLessThanZero("Input value", $" (must be >= {minValue.Value.ToString("N2", CultureInfo.InvariantCulture)})");

            }
            else
            {
                PrintErrorMessage($"Invalid decimal format. Please use '.' as the decimal separator (e.g., 123.45). Input was: '{input}'");
                throw new FormatException($"Invalid decimal input: {input}");
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

                if (minOk && maxOk) return value;

                string errorMsg = "Value must be";
                if (minValue != null) errorMsg += $" >= {minValue.Value.ToString("N1", CultureInfo.InvariantCulture)}";
                if (minValue != null && maxValue != null) errorMsg += " and";
                if (maxValue != null) errorMsg += $" <= {maxValue.Value.ToString("N1", CultureInfo.InvariantCulture)}";
                errorMsg += ".";
                PrintErrorMessage(errorMsg);
                if (!minOk && value < minValue!.Value) throw new ValueLessThanZero("Input value", $" (must be >= {minValue.Value.ToString("N1", CultureInfo.InvariantCulture)})");
            }
            else
            {
                PrintErrorMessage($"Invalid number format. Please use '.' as the decimal separator (e.g., 12.3). Input was: '{input}'");
                throw new FormatException($"Invalid double input: {input}");
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

                if (minOk && maxOk) return value;

                string errorMsg = "Value must be";
                if (minValue != null) errorMsg += $" >= {minValue.Value.ToString(CultureInfo.InvariantCulture)}";
                if (minValue != null && maxValue != null) errorMsg += " and";
                if (maxValue != null) errorMsg += $" <= {maxValue.Value.ToString(CultureInfo.InvariantCulture)}";
                errorMsg += ".";
                PrintErrorMessage(errorMsg);
                if (!minOk && value < minValue!.Value) throw new ValueLessThanZero("Input value", $" (must be >= {minValue.Value.ToString(CultureInfo.InvariantCulture)})");
            }
            else
            {
                PrintErrorMessage($"Invalid integer format. Input was: '{input}'");
                throw new FormatException($"Invalid integer input: {input}");
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
}