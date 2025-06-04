using System;
using System.Collections.Generic;
using System.Linq; 
using IRT.Classes; 
using IRT.Interfaces;
using IRT.ConsoleOutput; 

namespace IRT
{
    public enum ContainerType 
    {
        Array,
        LinkedList
    }

    public class ContainerManager 
    {
        private Container<Product>? _containerArray;
        private ContainerLinkedList<Product>? _containerList;

        public ContainerType ActiveContainerType { get; private set; } = ContainerType.Array;

        public ContainerManager()
        {
            _containerArray = new Container<Product>();
            _containerList = null; 
            ActiveContainerType = ContainerType.Array;
        }

        public void SelectOrInitializeContainer(ContainerType type, bool forceNew = false)
        {
            ActiveContainerType = type; 
            if (type == ContainerType.Array)
            {
                if (_containerArray == null || forceNew)
                    _containerArray = new Container<Product>();
                _containerList = null; 
            }
            else if (type == ContainerType.LinkedList)
            {
                if (_containerList == null || forceNew)
                    _containerList = new ContainerLinkedList<Product>();
                _containerArray = null; 
            }
        }

        public object? GetActiveContainerForSerialization()
        {
            return ActiveContainerType switch
            {
                ContainerType.Array => _containerArray,
                ContainerType.LinkedList => _containerList,
                _ => null
            };
        }

        public void ReplaceActiveContainerWithDeserialized(object deserializedContainer)
        {
            if (deserializedContainer is Container<Product> newArray)
            {
                _containerArray = newArray;
                _containerList = null;
                ActiveContainerType = ContainerType.Array;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Loaded Array container as active.");
            }
            else if (deserializedContainer is ContainerLinkedList<Product> newList)
            {
                _containerList = newList;
                _containerArray = null;
                ActiveContainerType = ContainerType.LinkedList;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Loaded LinkedList container as active.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                
                Console.WriteLine("Deserialized object is not a valid container type!");
            }
            Console.ResetColor();
        }

        public void AddItemToActive(Product item) 
        {
            if (ActiveContainerType == ContainerType.Array)
            {
                _containerArray ??= new Container<Product>(); 
                _containerArray.Add(item);
            }
            else
            {
                _containerList ??= new ContainerLinkedList<Product>(); 
                _containerList.AddLast(item);
            }
        }

        public bool RemoveByCurrentIndex(int zeroBasedIndex)
        {
            try
            {
                if (ActiveContainerType == ContainerType.Array && _containerArray != null)
                {
                    if (zeroBasedIndex < 0 || zeroBasedIndex >= _containerArray.Count) return false;
                    var removed = _containerArray.RemoveById(zeroBasedIndex);
                    return removed != null;
                }
                else if (ActiveContainerType == ContainerType.LinkedList && _containerList != null)
                {
                    if (zeroBasedIndex < 0 || zeroBasedIndex >= _containerList.Count) return false;
                    var removed = _containerList.RemoveByIndex(zeroBasedIndex);
                    return removed != null;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return false;
        }

        public Product? GetByInsertionId(int zeroBasedInsertionId) 
        {
            if (ActiveContainerType == ContainerType.Array)
                return _containerArray?[zeroBasedInsertionId];
            else
                return _containerList?[zeroBasedInsertionId];
        }

        public void SetByInsertionId(int zeroBasedInsertionId, Product value) 
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null)
            {
                _containerArray[zeroBasedInsertionId] = value;
            }
            else if (ActiveContainerType == ContainerType.LinkedList && _containerList != null)
            {
                _containerList[zeroBasedInsertionId] = value;
            }
            else
            {
                throw new InvalidOperationException("Active container is not initialized or item not found for update.");
            }
        }


        public int GetCount()
        {
            if (ActiveContainerType == ContainerType.Array)
                return _containerArray?.Count ?? 0;
            else
                return _containerList?.Count ?? 0;
        }

        public IEnumerable<Product> GetAll() 
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null)
                return _containerArray;
            else if (ActiveContainerType == ContainerType.LinkedList && _containerList != null)
                return _containerList;
            else
                return Enumerable.Empty<Product>();
        }

        public void Clear()
        {
            if (ActiveContainerType == ContainerType.Array)
                _containerArray = new Container<Product>();
            else
                _containerList = new ContainerLinkedList<Product>();
        }

        public ContainerType GetActiveContainerTypeEnum() => ActiveContainerType;

        public string GetActiveContainerTypeName() => ActiveContainerType.ToString();

        public void SortActiveContainer(Comparison<Product> comparison) 
        {
            if (ActiveContainerType == ContainerType.Array && _containerArray != null)
                _containerArray.Sort(comparison);
            else if (ActiveContainerType == ContainerType.LinkedList && _containerList != null)
                _containerList.Sort(comparison);
            else 
                Console.WriteLine("Container is empty or not initialized, nothing to sort.");
        }

        public IEnumerable<Product> GetReverseEnumerable()
        {
            if (ActiveContainerType == ContainerType.Array)
                return _containerArray?.GetReverseArray() ?? Enumerable.Empty<Product>();
            else
                return _containerList?.GetReverseArray() ?? Enumerable.Empty<Product>();
        }

        public IEnumerable<Product> GetEnumerableWithSublineInName(string subline)
        {
            if (ActiveContainerType == ContainerType.Array)
                return _containerArray?.GetArrayWithSublineInName(subline) ?? Enumerable.Empty<Product>();
            else
                return _containerList?.GetArrayWithSublineInName(subline) ?? Enumerable.Empty<Product>();
        }

        public IEnumerable<Product> GetSortedByPriceEnumerable()
        {
            if (ActiveContainerType == ContainerType.Array)
                return _containerArray?.GetSortedByArrayPrice() ?? Enumerable.Empty<Product>();
            else
                return _containerList?.GetSortedByArrayPrice() ?? Enumerable.Empty<Product>();
        }

        public IEnumerable<Product> GetSortedArrayByNameEnumerable() 
        {
            if (ActiveContainerType == ContainerType.Array)
                return _containerArray?.GetSortedArrayByName() ?? Enumerable.Empty<Product>();
            else
                return _containerList?.GetSortedArrayByName() ?? Enumerable.Empty<Product>();
        }

        public IEnumerable<Product> GetByNamePrefix(string prefix)
        {
            if (ActiveContainerType == ContainerType.Array)
                return _containerArray?.GetByNamePrefix(prefix) ?? Enumerable.Empty<Product>();
            else
                return _containerList?.GetByNamePrefix(prefix) ?? Enumerable.Empty<Product>();
        }

        public Product? FindFirst(Predicate<Product> match) 
        {
            if (ActiveContainerType == ContainerType.Array)
                return _containerArray?.Find(match);
            else
                return _containerList?.Find(match);
        }

        public IEnumerable<Product> FindAll(Predicate<Product> match) 
        {
            if (ActiveContainerType == ContainerType.Array)
                return _containerArray?.FindAll(match) ?? Enumerable.Empty<Product>();
            else
                return _containerList?.FindAll(match) ?? Enumerable.Empty<Product>();
        }

        public decimal GetTotalPrice()
        {
            if (ActiveContainerType == ContainerType.Array)
                return _containerArray?.TotalPrice ?? 0m;
            else
                return _containerList?.TotalPrice ?? 0m;
        }

        public int GetNextInsertionId()
        {
            if (ActiveContainerType == ContainerType.Array)
                return _containerArray?.InsertionId ?? 0;
            else
                return _containerList?.InsertionId ?? 0;
        }
    }
}