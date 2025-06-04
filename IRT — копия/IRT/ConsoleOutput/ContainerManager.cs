using IRT.Classes;
using IRT.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IRT.ConsoleOutput
{
    public enum ContainerType
    {
        None,
        Array,
        LinkedList
    }

    public class ContainerManager
    {
        private Container<IName>? _containerArray = null;
        private ContainerLinkedList<IName>? _containerList = null;
        public ContainerType ActiveContainerType { get; private set; } = ContainerType.None;

        public bool SelectOrInitializeContainer(ContainerType chosenType, bool forceNew = false)
        {
            if (chosenType == ContainerType.None)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Operation cancelled by selecting no container type.");
                Console.ResetColor();
                return false;
            }

            if (ActiveContainerType != chosenType || ActiveContainerType == ContainerType.None || forceNew)
            {
                bool switchConfirmed = true;
                if (ActiveContainerType != ContainerType.None && ActiveContainerType != chosenType && !forceNew && GetActiveContainerCount() > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"Switching to {chosenType} container will clear the current {ActiveContainerType} container. Continue? (y/n): ");
                    Console.ResetColor();
                    switchConfirmed = Console.ReadLine()?.Trim().ToLower() == "y";
                }

                if (switchConfirmed)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"\nInitializing {chosenType} container...");
                    Console.ResetColor();
                    _containerArray = null;
                    _containerList = null;
                    ActiveContainerType = chosenType;
                    if (ActiveContainerType == ContainerType.Array)
                    {
                        _containerArray = new Container<IName>();
                    }
                    else
                    {
                        _containerList = new ContainerLinkedList<IName>();
                    }
                    return true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Switch cancelled. Keeping the current container.");
                    Console.ResetColor();
                    return false;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nContinuing with the active {ActiveContainerType} container.");
                Console.ResetColor();
                return true;
            }
        }

        public ContainerType AskContainerTypeSelection()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nSelect Container Type:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("1. Array-based Container");
            Console.WriteLine("2. Linked List Container");
            Console.ResetColor();
            string choice = InputReader.ReadString("Enter choice (1 or 2, or any other key to cancel): ", true);
            return choice switch
            {
                "1" => ContainerType.Array,
                "2" => ContainerType.LinkedList,
                _ => ContainerType.None,
            };
        }

        public int GetActiveContainerCount()
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null)
                return _containerArray.GetCount();
            if (ActiveContainerType == ContainerType.LinkedList && _containerList != null)
                return _containerList.GetCount();
            return 0;
        }

        public bool IsActiveContainerEmpty(bool printMessageIfEmpty = true)
        {
            if (ActiveContainerType == ContainerType.None)
            {
                if (printMessageIfEmpty) ConsoleUI.PrintErrorMessage("No container selected. Please use option 1 or 2 first.");
                return true;
            }

            bool isEmpty = GetActiveContainerCount() == 0;
            if (isEmpty && printMessageIfEmpty)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("The active container is empty.");
                Console.ResetColor();
            }
            return isEmpty;
        }

        public IEnumerable<IName> GetActiveItems()
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null)
                return _containerArray;
            if (ActiveContainerType == ContainerType.LinkedList && _containerList != null)
                return _containerList;
            return Enumerable.Empty<IName>();
        }

        public void AddItemToActive(IName item)
        {
            if (item == null) return;
            int nextId = -1;
            if (ActiveContainerType == ContainerType.Array && _containerArray != null)
            {
                _containerArray.Add(item);
                nextId = _containerArray.GetInsertionId();
            }
            else if (ActiveContainerType == ContainerType.LinkedList && _containerList != null)
            {
                _containerList.AddLast(item);
                nextId = _containerList.GetNextInsertionId();
            }
            else
            {
                ConsoleUI.PrintErrorMessage("Cannot add item: No active container or container not initialized.");
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;


            Console.WriteLine($"{item.GetType().Name} added successfully to {ActiveContainerType} Container (Insertion ID: {nextId}).");
            Console.ResetColor();
        }

        public int GetNextAvailableInsertionId()
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null)
                return _containerArray.GetInsertionId();
            if (ActiveContainerType == ContainerType.LinkedList && _containerList != null)
                return _containerList.GetNextInsertionId();
            return 0;
        }

        public IName? FindItemByZeroBasedInsertionId(int zeroBasedId)
        {
            if (zeroBasedId < 0) return null;
            if (ActiveContainerType == ContainerType.Array && _containerArray != null)
                return _containerArray[zeroBasedId];
            if (ActiveContainerType == ContainerType.LinkedList && _containerList != null)
                return _containerList[zeroBasedId];
            return null;
        }

        public IEnumerable<IName> FindItemsByNameInActive(string name)
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null)
            {
                IName[]? found = _containerArray[name];
                return found?.Where(i => i != null)! ?? Enumerable.Empty<IName>();
            }
            if (ActiveContainerType == ContainerType.LinkedList && _containerList != null)
            {
                List<IName>? found = _containerList[name];
                return found ?? Enumerable.Empty<IName>();
            }
            return Enumerable.Empty<IName>();
        }

        public void SortActiveContainer(Comparison<IName> comparison)
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null)
                _containerArray.Sort(comparison);
            else if (ActiveContainerType == ContainerType.LinkedList && _containerList != null)
                _containerList.Sort(comparison);
            else
                ConsoleUI.PrintErrorMessage("No active container to sort.");
        }

        public IName? RemoveFromActiveByZeroBasedIndex(int zeroBasedIndex)
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null)
                return _containerArray.RemoveById(zeroBasedIndex);
            if (ActiveContainerType == ContainerType.LinkedList && _containerList != null)
                return _containerList.RemoveByIndex(zeroBasedIndex);
            return null;
        }

        public IEnumerable<IName> GetReverseIteratorForActive()
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null) return _containerArray.GetReverseArray();
            if (ActiveContainerType == ContainerType.LinkedList && _containerList != null) return _containerList.GetReverseArray();
            return Enumerable.Empty<IName>();
        }

        public IEnumerable<IName> GetSublineIteratorForActive(string subline)
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null) return _containerArray.GetArrayWithSublineInName(subline);
            if (ActiveContainerType == ContainerType.LinkedList && _containerList != null) return _containerList.GetArrayWithSublineInName(subline);
            return Enumerable.Empty<IName>();
        }

        public IEnumerable<IName> GetSortedByPriceIteratorForActive()
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null) return _containerArray.GetSortedByArrayPrice();
            if (ActiveContainerType == ContainerType.LinkedList && _containerList != null) return _containerList.GetSortedByArrayPrice();
            return Enumerable.Empty<IName>();
        }

        public IEnumerable<IName> GetSortedByNameIteratorForActive()
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null) return _containerArray.GetSortedArrayByName();
            if (ActiveContainerType == ContainerType.LinkedList && _containerList != null) return _containerList.GetSortedArrayByName();
            return Enumerable.Empty<IName>();
        }

        public IName? FindFirstInActive(Predicate<IName> match)
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null) return _containerArray.Find(match);
            if (ActiveContainerType == ContainerType.LinkedList && _containerList != null) return _containerList.Find(match);
            return null;
        }

        public IEnumerable<IName> FindAllInActive(Predicate<IName> match)
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null) return _containerArray.FindAll(match);
            if (ActiveContainerType == ContainerType.LinkedList && _containerList != null) return _containerList.FindAll(match);
            return Enumerable.Empty<IName>();
        }

        public decimal GetTotalPriceOfActive()
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null) return _containerArray.TotalPrice;
            if (ActiveContainerType == ContainerType.LinkedList && _containerList != null) return _containerList.TotalPrice;
            return 0m;
        }

        public object? GetActiveContainerForSerialization()
        {
            if (ActiveContainerType == ContainerType.Array) return _containerArray;
            if (ActiveContainerType == ContainerType.LinkedList) return _containerList;
            return null;
        }

        public void ReplaceActiveContainerWithDeserialized(object deserializedContainer)
        {
            if (deserializedContainer is Container<IName> newArray)
            {
                _containerArray = newArray;
                _containerList = null;
                ActiveContainerType = ContainerType.Array;
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Active container switched to the deserialized {ActiveContainerType} container.");
            }
            else if (deserializedContainer is ContainerLinkedList<IName> newList)
            {
                _containerList = newList;
                _containerArray = null;
                ActiveContainerType = ContainerType.LinkedList;
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Active container switched to the deserialized {ActiveContainerType} container.");
            }
            else
            {
                ConsoleUI.PrintErrorMessage($"Deserialization returned an unexpected object type: {deserializedContainer.GetType().Name}");
                return;
            }
            Console.ResetColor();
        }


        public int FindIndexByReferenceInActive(IName itemToFind)
        {
            if (itemToFind == null) return -1;
            var items = GetActiveItems();
            int index = 0;
            foreach (var currentItem in items)
            {
                if (ReferenceEquals(currentItem, itemToFind))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }


        public int GetZeroBasedInsertionIdForItemInActive(IName itemToFind)
        {
            if (itemToFind == null) return -1;
            int currentIndex = FindIndexByReferenceInActive(itemToFind);
            if (currentIndex == -1) return -1;

            try
            {
                if (ActiveContainerType == ContainerType.Array && _containerArray != null)
                {
                    int[] order = _containerArray.GetInsertionOrder();
                    if (currentIndex < order.Length && currentIndex < _containerArray.GetCount())
                        return order[currentIndex];
                }
                else if (ActiveContainerType == ContainerType.LinkedList && _containerList != null)
                {
                    List<int> order = _containerList.GetInsertionOrder();
                    if (currentIndex < order.Count && currentIndex < _containerList.GetCount())
                        return order[currentIndex];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetZeroBasedInsertionIdForItemInActive for current index {currentIndex}: {ex.Message}");
            }
            return -1;
        }


        public IName? GetItemByCurrentZeroBasedIndexInActive(int zeroBasedIndex)
        {
            if (zeroBasedIndex < 0) return null;
            var items = GetActiveItems();
            int count = 0;
            foreach (var item in items)
            {
                if (count == zeroBasedIndex) return item;
                count++;
            }
            return null;
        }


        public void DemonstrateIndexers(DataFactory dataFactory)
        {
            Random random = new Random();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n--- Demonstrating {ActiveContainerType} Container Indexer Usage ---");
            Console.ResetColor();

            if (IsActiveContainerEmpty(false))
            {
                Console.WriteLine("Container is empty, cannot demonstrate indexers.");
                return;
            }

            int nextAvailableId = GetNextAvailableInsertionId();


            if (nextAvailableId > 0)
            {
                List<int> currentValidInsertionIds = new List<int>();
                if (ActiveContainerType == ContainerType.Array && _containerArray != null)
                    currentValidInsertionIds = _containerArray.GetInsertionOrder().Take(_containerArray.GetCount()).ToList();
                else if (ActiveContainerType == ContainerType.LinkedList && _containerList != null)
                    currentValidInsertionIds = _containerList.GetInsertionOrder().Take(_containerList.GetCount()).ToList();


                int demoInsertionId = -1;
                if (currentValidInsertionIds.Any())
                {
                    demoInsertionId = currentValidInsertionIds[random.Next(currentValidInsertionIds.Count)];
                }

                if (demoInsertionId != -1)
                {
                    Console.WriteLine($"1. Accessing item by existing insertion ID [{demoInsertionId + 1}]:");
                    try
                    {
                        IName? itemById = FindItemByZeroBasedInsertionId(demoInsertionId);
                        if (itemById != null)
                        {
                            int currentIndex = FindIndexByReferenceInActive(itemById);
                            string indexStr = currentIndex != -1 ? $"(Current Index: {currentIndex + 1})" : "";
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
                        ConsoleUI.PrintErrorMessage($"   Error getting item by insertion ID {demoInsertionId + 1}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("1. No valid insertion IDs found in current order to demonstrate get by ID.");
                }
            }
            else
            {
                Console.WriteLine("1. No items added yet, cannot demonstrate insertion ID indexer for get.");
            }



            string? demoName = null;
            var activeItemsList = GetActiveItems().ToList();
            if (activeItemsList.Any())
            {
                var namedItems = activeItemsList.Where(it => it != null && !string.IsNullOrWhiteSpace(it.Name)).ToList();
                if (namedItems.Any()) demoName = namedItems[random.Next(namedItems.Count)].Name;
            }

            Console.WriteLine($"\n2. Using string indexer for name \"{demoName ?? "N/A"}\":");
            if (!string.IsNullOrWhiteSpace(demoName))
            {
                try
                {
                    var itemsByName = FindItemsByNameInActive(demoName).ToList();
                    if (itemsByName.Any())
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"   Found {itemsByName.Count()} item(s):");
                        foreach (var item in itemsByName)
                        {
                            int currentIndex = FindIndexByReferenceInActive(item!);
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
                    ConsoleUI.PrintErrorMessage($"   Error getting item(s) by name '{demoName}': {ex.Message}");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("   Could not find an item with a non-empty name in the sample to demonstrate.");
                Console.ResetColor();
            }



            int validDemoIdToSet = -1;
            var allInsertionIds = Enumerable.Range(0, nextAvailableId).ToList();
            if (allInsertionIds.Any())
            {

                foreach (var idInOrder in GetActiveItems().Select(it => GetZeroBasedInsertionIdForItemInActive(it)).Where(id => id != -1).Distinct())
                {
                    if (FindItemByZeroBasedInsertionId(idInOrder) != null)
                    {
                        validDemoIdToSet = idInOrder;
                        break;
                    }
                }
                if (validDemoIdToSet == -1 && allInsertionIds.Any())
                {
                    validDemoIdToSet = allInsertionIds[random.Next(allInsertionIds.Count)];
                }
            }


            if (validDemoIdToSet != -1)
            {
                Console.WriteLine($"\n3. Attempting to change item with insertion ID [{validDemoIdToSet + 1}] using property modification:");
                IName? itemToModify = FindItemByZeroBasedInsertionId(validDemoIdToSet);
                if (itemToModify != null)
                {
                    int currentIndex = FindIndexByReferenceInActive(itemToModify);
                    string indexStr = currentIndex != -1 ? $"(Current Index: {currentIndex + 1})" : "";
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"   Original Item {indexStr}: '{itemToModify.ToString() ?? "N/A"}'");
                    Console.ResetColor();
                    try
                    {
                        string newName = $"ChangedViaIndexer-{validDemoIdToSet + 1}";
                        Console.WriteLine($"   Attempting to set Name to '{newName}'...");



                        itemToModify.GetType().GetProperty("Name")?.SetValue(itemToModify, newName);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"   Property 'Name' potentially updated.");

                        IName? changedItem = FindItemByZeroBasedInsertionId(validDemoIdToSet);
                        int changedIndex = changedItem != null ? FindIndexByReferenceInActive(changedItem) : -1;
                        string changedIndexStr = changedIndex != -1 ? $"(Current Index: {changedIndex + 1})" : "";

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"   Current value {changedIndexStr}: {changedItem?.ToString() ?? "Not Found!"}");
                        Console.ResetColor();
                    }
                    catch (TargetInvocationException tie) { ConsoleUI.PrintErrorMessage($"   Error modifying property: {tie.InnerException?.Message ?? tie.Message}"); }
                    catch (Exception ex) { ConsoleUI.PrintErrorMessage($"   Error modifying property: {ex.Message}"); }
                }
                else { Console.WriteLine($"   Could not retrieve item with insertion ID {validDemoIdToSet + 1} for modification demonstration."); }
            }
            else
            {
                Console.WriteLine("\n3. Cannot demonstrate modification: No suitable item found with a valid insertion ID.");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"--- End {ActiveContainerType} Indexer Demonstration ---");
            Console.ResetColor();
        }

    }
}