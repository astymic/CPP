using IRT.Classes;
using IRT.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IRT.ConsoleOutput
{
    public class OperationHandlers
    {
        private readonly ContainerManager _containerManager;
        private readonly DataFactory _dataFactory;
        private readonly DisplayManager _displayManager;
        private readonly PropertyEditor _propertyEditor;

        public OperationHandlers(ContainerManager containerManager, DataFactory dataFactory, DisplayManager displayManager, PropertyEditor propertyEditor)
        {
            _containerManager = containerManager;
            _dataFactory = dataFactory;
            _displayManager = displayManager;
            _propertyEditor = propertyEditor;
        }

        public void HandleAutomaticGeneration()
        {
            ContainerType chosenType = _containerManager.AskContainerTypeSelection();
            if (!_containerManager.SelectOrInitializeContainer(chosenType, true))
                return;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n--- Automatic Generation ---");
            Console.ResetColor();
            try
            {
                int count = InputReader.ReadInt("Enter number of elements to generate: ", 1);
                Console.WriteLine($"Generating {count} elements for {_containerManager.ActiveContainerType} Container...");
                _dataFactory.GenerateItemsAndAdd(_containerManager.AddItemToActive, count);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nAutomatic generation of {count} elements complete for {_containerManager.ActiveContainerType} container.");
                Console.ResetColor();
                _containerManager.DemonstrateIndexers(_dataFactory);
            }
            catch (FormatException) { /* Handled by InputReader */ }
            catch (ValueLessThanZero) { /* Handled by InputReader */ }
        }

        public void HandleManualInput()
        {
            ContainerType chosenType = _containerManager.AskContainerTypeSelection();
            if (!_containerManager.SelectOrInitializeContainer(chosenType, GetCurrentItemCountForConfirmation() > 0))
                return;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- Manual Input for {_containerManager.ActiveContainerType} Container ---");
            Console.ResetColor();
            IName? newItem = _dataFactory.CreateItemManually();
            if (newItem != null)
            {
                _containerManager.AddItemToActive(newItem);
            }
        }

        private int GetCurrentItemCountForConfirmation()
        {
            return _containerManager.GetActiveContainerCount();
        }


        public void HandleShowContainer()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- Show {_containerManager.ActiveContainerType} Container ---");
            Console.ResetColor();
            if (_containerManager.IsActiveContainerEmpty()) return;

            _displayManager.DisplayCollectionInTable(
                _containerManager.GetActiveItems(),
                $"{_containerManager.ActiveContainerType} Container Contents ({_containerManager.GetActiveContainerCount()} items)"
            );
        }

        public void HandleGetElementByInsertionId()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- Get Element by Insertion ID from {_containerManager.ActiveContainerType} Container ---");
            Console.ResetColor();
            if (_containerManager.IsActiveContainerEmpty()) return;

            int maxId = _containerManager.GetNextAvailableInsertionId();
            if (maxId == 0)
            {
                ConsoleUI.PrintErrorMessage("Container is empty or no IDs assigned yet.");
                return;
            }

            try
            {
                int userInputId = InputReader.ReadInt($"Enter insertion ID (1 to {maxId}): ", 1, maxId);
                int zeroBasedId = userInputId - 1;

                IName? item = _containerManager.FindItemByZeroBasedInsertionId(zeroBasedId);

                if (item == null)
                {
                    ConsoleUI.PrintErrorMessage($"Item with insertion ID {userInputId} not found.");
                    return;
                }

                int currentIndex = _containerManager.FindIndexByReferenceInActive(item);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nItem Details (Insertion ID: {userInputId}, Current Index: {(currentIndex != -1 ? (currentIndex + 1).ToString() : "Unknown")}):");
                Console.ResetColor();
                _displayManager.DisplayItemTable(userInputId, item);
            }
            catch (FormatException) { /* Handled */ }
            catch (ValueLessThanZero) { /* Handled */ }
        }

        public void HandleGetElementByName()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- Get Elements by Name from {_containerManager.ActiveContainerType} Container ---");
            Console.ResetColor();
            if (_containerManager.IsActiveContainerEmpty()) return;

            string name = InputReader.ReadString("Enter the Name to search for: ");
            if (string.IsNullOrWhiteSpace(name))
            {
                ConsoleUI.PrintErrorMessage("Invalid input. Name cannot be empty.");
                return;
            }

            var itemsFound = _containerManager.FindItemsByNameInActive(name).ToList();

            if (itemsFound.Any())
            {
                _displayManager.DisplayCollectionInTable(itemsFound, $"Found {itemsFound.Count} element(s) with Name '{name}'");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"No elements found with Name '{name}'.");
                Console.ResetColor();
            }
        }

        public void HandleChangeItemByInsertionId()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- Change Item by Insertion ID in {_containerManager.ActiveContainerType} Container ---");
            Console.ResetColor();
            if (_containerManager.IsActiveContainerEmpty()) return;

            int maxId = _containerManager.GetNextAvailableInsertionId();
            if (maxId == 0)
            {
                ConsoleUI.PrintErrorMessage("Container is empty or no IDs assigned yet.");
                return;
            }

            try
            {
                int userInputId = InputReader.ReadInt($"Enter item insertion ID to modify (1 to {maxId}): ", 1, maxId);
                int zeroBasedId = userInputId - 1;

                IName? itemToModify = _containerManager.FindItemByZeroBasedInsertionId(zeroBasedId);

                if (itemToModify == null)
                {
                    ConsoleUI.PrintErrorMessage($"Item with insertion ID {userInputId} not found.");
                    return;
                }

                int currentIndex = _containerManager.FindIndexByReferenceInActive(itemToModify);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nCurrent item details (Insertion ID: {userInputId}, Current Index: {(currentIndex != -1 ? (currentIndex + 1).ToString() : "Unknown")}):");
                Console.ResetColor();
                _displayManager.DisplayItemTable(userInputId, itemToModify);

                _propertyEditor.ModifyProperty(itemToModify, zeroBasedId);
            }
            catch (FormatException) { /* Handled */ }
            catch (ValueLessThanZero) { /* Handled */ }
        }

        public void HandleChangeItemByName()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- Change Item by Name in {_containerManager.ActiveContainerType} Container ---");
            Console.ResetColor();
            if (_containerManager.IsActiveContainerEmpty()) return;

            string name = InputReader.ReadString("Enter the Name of the item(s) to modify: ");
            if (string.IsNullOrWhiteSpace(name))
            {
                ConsoleUI.PrintErrorMessage("Name cannot be empty.");
                return;
            }

            List<IName> itemsFound = _containerManager.FindItemsByNameInActive(name).ToList();

            if (!itemsFound.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"No valid elements found matching Name '{name}'.");
                Console.ResetColor();
                return;
            }

            IName itemToModify;
            int itemZeroBasedInsertionId;

            if (itemsFound.Count == 1)
            {
                itemToModify = itemsFound[0];
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nFound {itemsFound.Count} items with Name '{name}'. Please choose which one to modify:");
                Console.ResetColor();
                for (int i = 0; i < itemsFound.Count; i++)
                {
                    int currentIdx = _containerManager.FindIndexByReferenceInActive(itemsFound[i]);
                    Console.WriteLine($"{i + 1}. (Index: {currentIdx + 1}) {itemsFound[i].ToString() ?? "N/A"}");
                }
                try
                {
                    int choice = InputReader.ReadInt($"Enter choice (1 to {itemsFound.Count}): ", 1, itemsFound.Count);
                    itemToModify = itemsFound[choice - 1];
                }
                catch (FormatException) { return; }
                catch (ValueLessThanZero) { return; }
            }

            itemZeroBasedInsertionId = _containerManager.GetZeroBasedInsertionIdForItemInActive(itemToModify);
            if (itemZeroBasedInsertionId == -1)
            {
                ConsoleUI.PrintErrorMessage("Could not determine insertion ID for the selected item.");
                return;
            }
            int currentPhysicalIndex = _containerManager.FindIndexByReferenceInActive(itemToModify);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nSelected item (Current Index: {currentPhysicalIndex + 1}, Insertion ID: {itemZeroBasedInsertionId + 1}):");
            Console.ResetColor();
            _displayManager.DisplayItemTable(currentPhysicalIndex + 1, itemToModify);
            _propertyEditor.ModifyProperty(itemToModify, itemZeroBasedInsertionId);
        }


        public void HandleSortContainer()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- Sorting {_containerManager.ActiveContainerType} Container ---");
            Console.ResetColor();
            if (_containerManager.IsActiveContainerEmpty()) return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Select sort parameter:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("1. Sort by Name");
            Console.WriteLine("2. Sort by Price");
            Console.ResetColor();
            string? sortChoice = InputReader.ReadString("Enter choice (1 or 2): ", true);

            Comparison<IName>? comparison = null;
            string sortParameter = "";

            switch (sortChoice)
            {
                case "1":
                    comparison = AppComparisons.NameComparison;
                    sortParameter = "Name";
                    break;
                case "2":
                    comparison = AppComparisons.PriceComparison;
                    sortParameter = "Price";
                    break;
                default:
                    ConsoleUI.PrintErrorMessage("Invalid sort choice.");
                    return;
            }

            Console.WriteLine($"\nSorting by {sortParameter}...");
            _containerManager.SortActiveContainer(comparison);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Container sorted by {sortParameter}.");
            Console.ResetColor();
            HandleShowContainer();
        }

        public void HandleRemoveElementByIndex()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- Remove Element by Current Index from {_containerManager.ActiveContainerType} Container ---");
            Console.ResetColor();
            if (_containerManager.IsActiveContainerEmpty()) return;

            int currentCount = _containerManager.GetActiveContainerCount();
            try
            {
                int zeroBasedIndex = InputReader.ReadInt($"Enter element index to remove (0 to {currentCount - 1}): ", 0, currentCount - 1);

                IName? itemToRemove = _containerManager.GetItemByCurrentZeroBasedIndexInActive(zeroBasedIndex);
                int originalInsertionId = -1;
                if (itemToRemove != null)
                {
                    originalInsertionId = _containerManager.GetZeroBasedInsertionIdForItemInActive(itemToRemove);
                }

                IName? removedItem = _containerManager.RemoveFromActiveByZeroBasedIndex(zeroBasedIndex);

                if (removedItem != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    string idString = originalInsertionId != -1 ? $"(original Insertion ID: {originalInsertionId + 1})" : "(original Insertion ID unknown)";
                    Console.WriteLine($"\nElement at index {zeroBasedIndex} {idString} was removed:");
                    Console.WriteLine(removedItem.ToString() ?? "Removed item details unavailable.");
                    Console.ResetColor();
                }
                else
                {
                    ConsoleUI.PrintErrorMessage($"Error: Failed to remove item at index {zeroBasedIndex}.");
                }
            }
            catch (FormatException) { /* Handled */ }
            catch (ValueLessThanZero) { /* Handled */ }
        }

        private void DisplayGeneratedSequence(IEnumerable<IName> items, string title, string emptyMessage)
        {
            if (_containerManager.IsActiveContainerEmpty(false))
            {
                _containerManager.IsActiveContainerEmpty(true);
                return;
            }

            var itemList = items.ToList();
            _displayManager.DisplayCollectionInTable(itemList, title);
            if (!itemList.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(emptyMessage);
                Console.ResetColor();
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Generator yielded {itemList.Count} items.");
            Console.ResetColor();
        }

        public void HandleReverseGenerator()
        {
            DisplayGeneratedSequence(
                _containerManager.GetReverseIteratorForActive(),
                $"Generator: Items in Reverse Order ({_containerManager.ActiveContainerType})",
                "Reverse generator yielded no items."
            );
        }

        public void HandleSublineGenerator()
        {
            if (_containerManager.IsActiveContainerEmpty()) return;
            string subline = InputReader.ReadString("Enter substring to search for in Name: ");
            if (string.IsNullOrEmpty(subline))
            {
                ConsoleUI.PrintErrorMessage("Substring cannot be empty.");
                return;
            }
            DisplayGeneratedSequence(
                _containerManager.GetSublineIteratorForActive(subline),
                $"Generator: Items with Name Containing '{subline}' ({_containerManager.ActiveContainerType})",
                $"No items found with names containing '{subline}'."
            );
        }

        public void HandleSortedPriceGenerator()
        {
            DisplayGeneratedSequence(
                _containerManager.GetSortedByPriceIteratorForActive(),
                $"Generator: Items Sorted by Price ({_containerManager.ActiveContainerType})",
                "Sorted by Price generator yielded no items."
            );
        }

        public void HandleSortedNameGenerator()
        {
            DisplayGeneratedSequence(
                _containerManager.GetSortedByNameIteratorForActive(),
                $"Generator: Items Sorted by Name ({_containerManager.ActiveContainerType})",
                "Sorted by Name generator yielded no items."
            );
        }

        public void HandleFindFirstElement()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- Find First Element in {_containerManager.ActiveContainerType} Container ---");
            Console.ResetColor();
            if (_containerManager.IsActiveContainerEmpty()) return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Select search criterion:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("1. By Exact Name");
            Console.WriteLine("2. By Price");
            Console.WriteLine("3. By Class Type Name");
            Console.ResetColor();
            string? choice = InputReader.ReadString("Enter choice: ", true);

            Predicate<IName>? predicate = null;
            string criteriaDescription = "";

            try
            {
                switch (choice)
                {
                    case "1":
                        string searchName = InputReader.ReadString("Enter exact Name to find: ");
                        predicate = item => item.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase);
                        criteriaDescription = $"Exact Name = '{searchName}'";
                        break;
                    case "2":
                        decimal priceThreshold = InputReader.ReadDecimal("Enter Price to find: ", 0.00001m);
                        predicate = item => item.Price == priceThreshold;
                        criteriaDescription = $"Price = {priceThreshold:N2}";
                        break;
                    case "3":
                        string typeName = InputReader.ReadString("Enter Class Type Name (e.g., Apartment, Product): ");
                        predicate = item => item.GetType().Name.Equals(typeName, StringComparison.OrdinalIgnoreCase);
                        criteriaDescription = $"Class Type = '{typeName}'";
                        break;
                    default:
                        ConsoleUI.PrintErrorMessage("Invalid choice.");
                        return;
                }
            }
            catch (ArgumentException ex) { ConsoleUI.PrintErrorMessage(ex.Message); return; }
            catch (FormatException) { return; }
            catch (ValueLessThanZero) { return; }


            IName? foundItem = _containerManager.FindFirstInActive(predicate!);
            if (foundItem != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nFirst element found matching criterion ({criteriaDescription}):");
                Console.ResetColor();
                int currentIndex = _containerManager.FindIndexByReferenceInActive(foundItem);
                _displayManager.DisplayItemTable(currentIndex != -1 ? currentIndex + 1 : 1, foundItem);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nNo element found matching criterion ({criteriaDescription}).");
                Console.ResetColor();
            }
        }

        public void HandleFindAllElements()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- Find All Elements in {_containerManager.ActiveContainerType} Container ---");
            Console.ResetColor();
            if (_containerManager.IsActiveContainerEmpty()) return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Select search criterion:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("1. By Exact Name");
            Console.WriteLine("2. By Price");
            Console.WriteLine("3. By Class Type Name");
            Console.WriteLine("4. By Name Containing Substring");
            Console.ResetColor();
            string? choice = InputReader.ReadString("Enter choice: ", true);

            Predicate<IName>? predicate = null;
            string criteriaDescription = "";
            try
            {
                switch (choice)
                {
                    case "1":
                        string searchName = InputReader.ReadString("Enter exact Name to find: ");
                        predicate = item => item.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase);
                        criteriaDescription = $"Exact Name = '{searchName}'";
                        break;
                    case "2":
                        decimal priceThreshold = InputReader.ReadDecimal("Enter Price to find: ", 0.00001m);
                        predicate = item => item.Price == priceThreshold;
                        criteriaDescription = $"Price = {priceThreshold:N2}";
                        break;
                    case "3":
                        string typeName = InputReader.ReadString("Enter Class Type Name (e.g., Apartment, Product): ");
                        predicate = item => item.GetType().Name.Equals(typeName, StringComparison.OrdinalIgnoreCase);
                        criteriaDescription = $"Class Type = '{typeName}'";
                        break;
                    case "4":
                        string subName = InputReader.ReadString("Enter substring for Name: ");
                        predicate = item => item.Name.IndexOf(subName, StringComparison.OrdinalIgnoreCase) >= 0;
                        criteriaDescription = $"Name containing '{subName}'";
                        break;
                    default:
                        ConsoleUI.PrintErrorMessage("Invalid choice.");
                        return;
                }
            }
            catch (ArgumentException ex) { ConsoleUI.PrintErrorMessage(ex.Message); return; }
            catch (FormatException) { return; }
            catch (ValueLessThanZero) { return; }


            var foundItems = _containerManager.FindAllInActive(predicate!).ToList();
            if (foundItems.Any())
            {
                _displayManager.DisplayCollectionInTable(foundItems, $"Found {foundItems.Count} element(s) matching criterion ({criteriaDescription})");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nNo elements found matching criterion ({criteriaDescription}).");
                Console.ResetColor();
            }
        }

        public void HandleSerializeContainer()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- Serialize Active {_containerManager.ActiveContainerType} Container ---");
            Console.ResetColor();

            object? activeContainerObject = _containerManager.GetActiveContainerForSerialization();
            if (activeContainerObject == null || _containerManager.IsActiveContainerEmpty(false))
            {
                ConsoleUI.PrintErrorMessage("No active and non-empty container to serialize.");
                if (_containerManager.ActiveContainerType != ContainerType.None && _containerManager.IsActiveContainerEmpty(false))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("The active container is empty.");
                    Console.ResetColor();
                }
                return;
            }

            string? filename = InputReader.ReadString("Enter filename to save (without extension, e.g., 'mydata'): ", true)?.Trim();
            if (string.IsNullOrWhiteSpace(filename))
            {
                ConsoleUI.PrintErrorMessage("Invalid filename.");
                return;
            }
            if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                ConsoleUI.PrintErrorMessage($"Filename '{filename}' contains invalid characters.");
                return;
            }

            string fullPath = ContainerSerializer.SerializeContainer(activeContainerObject, filename);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Container successfully serialized to: {fullPath}");
            Console.ResetColor();
        }

        public void HandleDeserializeContainer()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n--- Deserialize Container from File ---");
            Console.ResetColor();
            string? filename = InputReader.ReadString("Enter filename to load (without extension, e.g., 'mydata'): ", true)?.Trim();
            if (string.IsNullOrWhiteSpace(filename))
            {
                ConsoleUI.PrintErrorMessage("Invalid filename.");
                return;
            }
            if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                ConsoleUI.PrintErrorMessage($"Filename '{filename}' contains invalid characters.");
                return;
            }

            object deserializedContainerObj = ContainerSerializer.DeserializeContainer(filename);

            if (deserializedContainerObj == null)
            {
                Console.WriteLine("Deserialization resulted in a null container.");
                return;
            }

            int loadedItemCount = 0;
            ContainerType deserializedType = ContainerType.None;

            if (deserializedContainerObj is Container<IName> da) { loadedItemCount = da.GetCount(); deserializedType = ContainerType.Array; }
            else if (deserializedContainerObj is ContainerLinkedList<IName> dl) { loadedItemCount = dl.GetCount(); deserializedType = ContainerType.LinkedList; }
            else { ConsoleUI.PrintErrorMessage($"Deserialization returned an unrecognized object type: {deserializedContainerObj.GetType().Name}"); return; }


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
            if (_containerManager.ActiveContainerType != ContainerType.None && _containerManager.GetActiveContainerCount() > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                switchConfirmed = InputReader.ReadBool($"Replace the current {_containerManager.ActiveContainerType} container with the deserialized data? (y/n): ");
            }

            if (switchConfirmed)
            {
                _containerManager.ReplaceActiveContainerWithDeserialized(deserializedContainerObj);
                HandleShowContainer();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Switch cancelled. Keeping the current active container.");
                Console.ResetColor();
            }
        }

        public void HandleShowTotalPrice()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- Total Price in {_containerManager.ActiveContainerType} Container ---");
            Console.ResetColor();
            if (_containerManager.ActiveContainerType == ContainerType.None)
            {
                ConsoleUI.PrintErrorMessage("No active container.");
                return;
            }
            if (_containerManager.IsActiveContainerEmpty()) return;

            decimal totalPrice = _containerManager.GetTotalPriceOfActive();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Total Price of all items: {totalPrice:N2}");
            Console.ResetColor();
        }

        public void HandleFindMinMaxProduct()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- Minimum and Maximum Price in {_containerManager.ActiveContainerType} Container ---");
            Console.ResetColor();
            if (_containerManager.ActiveContainerType == ContainerType.None)
            {
                ConsoleUI.PrintErrorMessage("No active container.");
                return;
            }
            if (_containerManager.IsActiveContainerEmpty()) return;

            var prices = from product in _containerManager.GetActiveItems()
                         where product != null
                         select product.Price;

            if (!prices.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No items with prices found in the container.");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("The Minimal Price is: ");
            Console.ResetColor();
            Console.WriteLine($"{prices.Min():N2}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("The Maximum Price is: ");
            Console.ResetColor();
            Console.WriteLine($"{prices.Max():N2}");
        }

        public void HandleFindAverageCategoriesPrice()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- Average Price for each Category in {_containerManager.ActiveContainerType} Container ---");
            Console.ResetColor();
            if (_containerManager.ActiveContainerType == ContainerType.None)
            {
                ConsoleUI.PrintErrorMessage("No active container.");
                return;
            }
            if (_containerManager.IsActiveContainerEmpty()) return;
            
            var averagePrices = from IName product in _containerManager.GetActiveItems()
                                where product != null
                                group product by product.GetType().Name into g
                                select new
                                {
                                    Category = g.Key,
                                    AveragePrice = g.Average(p => p.Price)
                                };

            _displayManager.DisplayAveragePrices(averagePrices, "Average Prices by Category");
        }
    }
}