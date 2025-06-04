using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using IRT.Classes;
using IRT.ConsoleOutput;
using IRT.Interfaces;

namespace IRT
{
    public class OperationHandlers 
    {
        private readonly ContainerManager _containerManager;
        private readonly DataFactory _dataFactory;
        private readonly DisplayManager _displayManager;
        private readonly PropertyEditor _propertyEditor;

        public OperationHandlers(
            ContainerManager containerManager,
            DataFactory dataFactory,
            DisplayManager displayManager,
            PropertyEditor propertyEditor)
        {
            _containerManager = containerManager ?? throw new ArgumentNullException(nameof(containerManager));
            _dataFactory = dataFactory ?? throw new ArgumentNullException(nameof(dataFactory));
            _displayManager = displayManager ?? throw new ArgumentNullException(nameof(displayManager));
            _propertyEditor = propertyEditor ?? throw new ArgumentNullException(nameof(propertyEditor));
        }

        private void SelectAndInitializeContainer(bool isManualInput)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Select container type for generation/input:");
            Console.WriteLine("1. Array Container");
            Console.WriteLine("2. LinkedList Container");
            Console.ResetColor();
            string? typeChoiceStr = Console.ReadLine();

            ContainerType selectedType;
            switch (typeChoiceStr)
            {
                case "1":
                    selectedType = ContainerType.Array;
                    break;
                case "2":
                    selectedType = ContainerType.LinkedList;
                    break;
                default:
                    ConsoleUI.PrintErrorMessage("Invalid choice. Defaulting to Array container.");
                    selectedType = ContainerType.Array;
                    break;
            }

            bool forceNew = false;
            if (_containerManager.GetActiveContainerTypeEnum() == selectedType && _containerManager.GetCount() > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"Container type '{selectedType}' is already active and contains items. ");
                forceNew = InputReader.ReadBool("Do you want to clear it and start fresh? (yes/no): ");
                Console.ResetColor();
            }
            _containerManager.SelectOrInitializeContainer(selectedType, forceNew);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Switched to {selectedType} container. It is {(forceNew || _containerManager.GetCount() == 0 ? "empty" : "active with existing items")}.");
            Console.ResetColor();
        }

        public void HandleAutomaticGeneration()
        {
            SelectAndInitializeContainer(false);
            int count = InputReader.ReadInt("Enter number of items to generate: ", 1);
            Console.WriteLine($"Generating {count} items for {_containerManager.GetActiveContainerTypeName()} container...");

            
            _dataFactory.GenerateItemsAndAdd(
                (IName item) => 
                {
                    if (item is Product product) 
                    {
                        _containerManager.AddItemToActive(product); 
                    }
                    else
                    {
                        ConsoleUI.PrintErrorMessage($"Item '{item.Name}' of type '{item.GetType().Name}' is not a Product and cannot be added to the current container.");
                    }
                },
                count
            );

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Automatic generation complete.");
            Console.ResetColor();
        }

        public void HandleManualInput()
        {
            SelectAndInitializeContainer(true);
            IName? newItem = _dataFactory.CreateItemManually();
            if (newItem is Product product)
            {
                _containerManager.AddItemToActive(product);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Item added successfully.");
                
                
                _displayManager.DisplayItemTable(_containerManager.GetNextInsertionId() - 1 + 1, product); 
            }
            else if (newItem != null)
            {
                ConsoleUI.PrintErrorMessage($"Created item '{newItem.Name}' was not a Product and could not be added.");
            }
            Console.ResetColor();
        }

        public void HandleShowContainer()
        {
            var items = _containerManager.GetAll().ToList();
            if (!items.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Container is empty.");
                Console.ResetColor();
                return;
            }
            _displayManager.DisplayCollectionInTable(items, $"Contents of {_containerManager.GetActiveContainerTypeName()} Container");
        }

        public void HandleGetElementByInsertionId()
        {
            int oneBasedId = InputReader.ReadInt("Enter Insertion ID (1-based): ", 1);
            Product? item = _containerManager.GetByInsertionId(oneBasedId - 1); 
            if (item != null)
            {
                _displayManager.DisplayItemTable(oneBasedId, item);
            }
            else
            {
                ConsoleUI.PrintErrorMessage($"No item found with Insertion ID {oneBasedId}.");
            }
        }

        public void HandleGetElementByName()
        {
            string prefix = InputReader.ReadString("Enter name prefix to search for: ");
            var items = _containerManager.GetByNamePrefix(prefix).ToList();
            if (!items.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"No items found with name starting with '{prefix}'.");
                Console.ResetColor();
            }
            else
            {
                _displayManager.DisplayCollectionInTable(items, $"Items with name starting with '{prefix}'");
            }
        }

        private int? FindInsertionIdForItem(Product itemToFind)
        {
            
            
            
            
            
            int maxPossibleId = _containerManager.GetNextInsertionId();
            for (int i = 0; i < maxPossibleId; i++)
            {
                if (ReferenceEquals(_containerManager.GetByInsertionId(i), itemToFind))
                {
                    return i;
                }
            }
            return null; 
        }

        public void HandleChangeItemByInsertionId()
        {
            int oneBasedId = InputReader.ReadInt("Enter Insertion ID of item to change (1-based): ", 1);
            int zeroBasedId = oneBasedId - 1;
            Product? item = _containerManager.GetByInsertionId(zeroBasedId);
            if (item != null)
            {
                Console.WriteLine("Current item details:");
                _displayManager.DisplayItemTable(oneBasedId, item);
                _propertyEditor.ModifyProperty(item, zeroBasedId);
            }
            else
            {
                ConsoleUI.PrintErrorMessage($"No item found with Insertion ID {oneBasedId}.");
            }
        }

        public void HandleChangeItemByName()
        {
            string name = InputReader.ReadString("Enter exact name of item to change: ");
            var matchingItems = _containerManager.FindAll(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!matchingItems.Any())
            {
                ConsoleUI.PrintErrorMessage($"No item found with name '{name}'.");
                return;
            }

            Product? itemToChange;
            int? itemZeroBasedInsertionId = null;

            if (matchingItems.Count == 1)
            {
                itemToChange = matchingItems.First();
                itemZeroBasedInsertionId = FindInsertionIdForItem(itemToChange);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Multiple items found with name '{name}'. Please select which one to modify by its displayed table ID:");
                Console.ResetColor();

                
                _displayManager.DisplayCollectionInTable(matchingItems.Cast<IName>(), "Matching Items - Select One");

                int choice = InputReader.ReadInt($"Enter table ID (1 to {matchingItems.Count}) to modify: ", 1, matchingItems.Count);
                itemToChange = matchingItems[choice - 1];
                itemZeroBasedInsertionId = FindInsertionIdForItem(itemToChange);
            }

            if (itemToChange != null)
            {
                Console.WriteLine("\nCurrent item details selected for modification:");
                _displayManager.DisplayItemTable(itemZeroBasedInsertionId.HasValue ? itemZeroBasedInsertionId.Value + 1 : 0, itemToChange);
                _propertyEditor.ModifyProperty(itemToChange, itemZeroBasedInsertionId);
            }
            else
            {
                ConsoleUI.PrintErrorMessage($"Could not select an item with name '{name}' for modification.");
            }
        }


        public void HandleSortContainer()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Sort by:");
            Console.WriteLine("1. Name");
            Console.WriteLine("2. Price");
            Console.ResetColor();
            string? choice = Console.ReadLine();
            Comparison<Product>? comparison = null;

            switch (choice)
            {
                case "1":
                    comparison = (p1, p2) => {
                        if (p1 == null && p2 == null) return 0;
                        if (p1 == null) return -1;
                        if (p2 == null) return 1;
                        return string.Compare(p1.Name, p2.Name, StringComparison.OrdinalIgnoreCase);
                    };
                    break;
                case "2":
                    comparison = (p1, p2) => {
                        if (p1 == null && p2 == null) return 0;
                        if (p1 == null) return -1; 
                        if (p2 == null) return 1;
                        return p1.Price.CompareTo(p2.Price);
                    };
                    break;
                default:
                    ConsoleUI.PrintErrorMessage("Invalid sort choice.");
                    return;
            }

            if (comparison != null)
            {
                _containerManager.SortActiveContainer(comparison);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Container sorted successfully.");
                HandleShowContainer();
            }
        }

        public void HandleRemoveElementByIndex()
        {
            if (_containerManager.GetCount() == 0)
            {
                ConsoleUI.PrintErrorMessage("Container is empty. Nothing to remove.");
                return;
            }
            int zeroBasedIndex = InputReader.ReadInt($"Enter current item index to remove (0 to {_containerManager.GetCount() - 1}): ", 0, _containerManager.GetCount() - 1);
            if (_containerManager.RemoveByCurrentIndex(zeroBasedIndex))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Item at index {zeroBasedIndex} removed successfully.");
            }
            else
            {
                ConsoleUI.PrintErrorMessage($"Failed to remove item at index {zeroBasedIndex}. It might not exist or an error occurred.");
            }
        }

        public void HandleReverseGenerator()
        {
            var items = _containerManager.GetReverseEnumerable().ToList();
            if (!items.Any() && _containerManager.GetCount() == 0) { ConsoleUI.PrintErrorMessage("Container is empty."); return; }
            _displayManager.DisplayCollectionInTable(items, "Items in Reverse Order (Generator)");
        }

        public void HandleSublineGenerator()
        {
            string subline = InputReader.ReadString("Enter substring to search in names: ");
            var items = _containerManager.GetEnumerableWithSublineInName(subline).ToList();
            if (!items.Any() && _containerManager.GetCount() == 0 && !string.IsNullOrEmpty(subline)) { ConsoleUI.PrintErrorMessage($"No items found with '{subline}' in name, or container is empty."); return; }
            if (!items.Any() && _containerManager.GetCount() == 0) { ConsoleUI.PrintErrorMessage("Container is empty."); return; }
            _displayManager.DisplayCollectionInTable(items, $"Items with '{subline}' in Name (Generator)");
        }

        public void HandleSortedPriceGenerator()
        {
            var items = _containerManager.GetSortedByPriceEnumerable().ToList();
            if (!items.Any() && _containerManager.GetCount() == 0) { ConsoleUI.PrintErrorMessage("Container is empty."); return; }
            _displayManager.DisplayCollectionInTable(items, "Items Sorted by Price (Generator)");
        }

        public void HandleSortedNameGenerator()
        {
            var items = _containerManager.GetSortedArrayByNameEnumerable().ToList(); 
            if (!items.Any() && _containerManager.GetCount() == 0) { ConsoleUI.PrintErrorMessage("Container is empty."); return; }
            _displayManager.DisplayCollectionInTable(items, "Items Sorted by Name (Generator)");
        }

        public void HandleFindFirstElement()
        {
            decimal minPrice = InputReader.ReadDecimal("Enter minimum price for search: ", 0.01m);
            Predicate<Product> predicate = p => p.Price > minPrice;
            Product? item = _containerManager.FindFirst(predicate);

            if (item != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"First item found matching criteria (Price > {minPrice:C}):");
                var insertionId = FindInsertionIdForItem(item);
                _displayManager.DisplayItemTable(insertionId.HasValue ? insertionId.Value + 1 : 0, item);
            }
            else
            {
                ConsoleUI.PrintErrorMessage($"No item found with price > {minPrice:C}.");
            }
        }

        public void HandleFindAllElements()
        {
            decimal maxPrice = InputReader.ReadDecimal("Enter maximum price for search: ", 0.01m);
            Predicate<Product> predicate = p => p.Price < maxPrice;
            var items = _containerManager.FindAll(predicate).ToList();

            if (!items.Any())
            {
                ConsoleUI.PrintErrorMessage($"No items found with price < {maxPrice:C}.");
                return;
            }
            _displayManager.DisplayCollectionInTable(items, $"Items with Price < {maxPrice:C}");
        }

        public void HandleSerializeContainer()
        {
            object? container = _containerManager.GetActiveContainerForSerialization();
            if (container == null || _containerManager.GetCount() == 0)
            {
                ConsoleUI.PrintErrorMessage("Active container is empty or not initialized. Nothing to serialize.");
                return;
            }

            string filename = InputReader.ReadString("Enter file name to save (e.g., data.bin): ");
            try
            {
                ContainerSerializer.SerializeContainer<Product>(container, filename);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Container serialized to file '{filename}' successfully.");
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintErrorMessage($"Serialization error: {ex.Message}");
            }
            finally
            {
                Console.ResetColor();
            }
        }

        public void HandleDeserializeContainer()
        {
            string filename = InputReader.ReadString("Enter file name to load (e.g., data.bin): ");
            if (!File.Exists(filename))
            {
                ConsoleUI.PrintErrorMessage($"File '{filename}' does not exist.");
                return;
            }

            try
            {
                object deserialized = ContainerSerializer.DeserializeContainer<Product>(filename);
                _containerManager.ReplaceActiveContainerWithDeserialized(deserialized);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Container deserialized from file '{filename}' successfully.");
                HandleShowContainer();
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintErrorMessage($"Deserialization error: {ex.Message}");
            }
            finally
            {
                Console.ResetColor();
            }
        }

        public void HandleShowTotalPrice()
        {
            decimal totalPrice = _containerManager.GetTotalPrice();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Total price of all items in the active container: {totalPrice:C}");
            Console.ResetColor();
        }

        public void HandleFindMinMaxProduct()
        {
            var items = _containerManager.GetAll().ToList();
            if (!items.Any())
            {
                ConsoleUI.PrintErrorMessage("Container is empty. Cannot find min/max.");
                return;
            }

            Product? minPriceProduct = null;
            Product? maxPriceProduct = null;

            if (items.Any())
            { 
                minPriceProduct = items[0];
                maxPriceProduct = items[0];
                foreach (var item in items.Skip(1))
                {
                    if (item.Price < minPriceProduct.Price) minPriceProduct = item;
                    if (item.Price > maxPriceProduct.Price) maxPriceProduct = item;
                }
            }


            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Product with Minimum Price:");
            if (minPriceProduct != null)
            {
                var insertionId = FindInsertionIdForItem(minPriceProduct);
                _displayManager.DisplayItemTable(insertionId.HasValue ? insertionId.Value + 1 : 0, minPriceProduct);
            }
            else Console.WriteLine("N/A");

            Console.WriteLine("\nProduct with Maximum Price:");
            if (maxPriceProduct != null)
            {
                var insertionId = FindInsertionIdForItem(maxPriceProduct);
                _displayManager.DisplayItemTable(insertionId.HasValue ? insertionId.Value + 1 : 0, maxPriceProduct);
            }
            else Console.WriteLine("N/A");
            Console.ResetColor();
        }

        public void HandleFindAverageCategoriesPrice()
        {
            var items = _containerManager.GetAll().ToList();
            if (!items.Any())
            {
                ConsoleUI.PrintErrorMessage("Container is empty. Cannot calculate average prices.");
                return;
            }

            var averagePrices = items
                .GroupBy(item => item.GetType().Name) 
                .Select(group => new
                {
                    Category = group.Key,
                    AveragePrice = group.Average(item => item.Price)
                })
                .OrderBy(g => g.Category)
                .Cast<dynamic>() 
                .ToList();

            _displayManager.DisplayAveragePrices(averagePrices, "Average Price by Category");
        }
    }
}