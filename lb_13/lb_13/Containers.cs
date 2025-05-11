using System.Collections.Generic;
using lb_13.Interfaces;
using System.Reflection;
using System.Collections;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace lb_13
{
    public class Helper
    {
        public static V? GetPropertyValue<V>(object item, string propertyName)
        {
            if (item == null) return default;

            PropertyInfo? property = item.GetType().GetProperty(propertyName);
            if (property != null && property.CanRead)
            {
                return (V?)property.GetValue(item);
            }
            return default;
        }
    }

    public class PriceComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            bool xHasPrice = x is IPrice;
            bool yHasPrice = y is IPrice;

            if (xHasPrice && yHasPrice)
            {
                return ((IPrice)x).Price.CompareTo(((IPrice)y).Price);
            }

            if (xHasPrice) return -1;
            if (yHasPrice) return 1;
            return 0;
        }
    }



    public class Container<T> : IEnumerable<T> where T : class, IName
    {
        private T?[] items;
        private int[] insertionOrder;
        private int count;
        private int size;
        private int nextInsertionId;
        private decimal totalPrice;
        public decimal TotalPrice => totalPrice;

        public Container()
        {
            items = new T?[1];
            insertionOrder = new int[1];
            count = 0;
            size = 1;
            nextInsertionId = 0;
            totalPrice = 0; 
        }

        public void Add(T _newObject)
        {
            if (count == size)
            {
                T?[] newArray = new T?[size * 2];
                int[] newInsertionOrder = new int[size * 2];
                for (int i = 0; i < size; i++)
                {
                    newArray[i] = items[i];
                    newInsertionOrder[i] = insertionOrder[i];
                }
                items = newArray;
                insertionOrder = newInsertionOrder;
                size *= 2;
            }
            items[count] = _newObject;
            insertionOrder[count] = nextInsertionId++;
            count++;

            if (_newObject is IPrice price)
            {
                totalPrice += price.Price;
                price.PriceChanged += HandlePriceChange;
            }
        }

        private void HandlePriceChange(object? sender, decimal oldPrice)
        {
            if (sender is IPrice price)
            {
                totalPrice += price.Price - oldPrice;
            }
        }

        public T? RemoveById(int _index)
        {
            if (_index < 0 || _index >= count) 
                throw new IndexOutOfRangeException();

            var deletedObject = items[_index]!;
            for (int i = _index; i < count - 1; i++)
            {
                items[i] = items[i + 1];
                insertionOrder[i] = insertionOrder[i + 1];
            }
            items[count - 1] = default;
            insertionOrder[count - 1] = 0; 
            count--;

            if (deletedObject is IPrice price)
            {
                totalPrice -= price.Price;
                price.PriceChanged -= HandlePriceChange;
            }

            return deletedObject;
        }

        public void Sort(Comparison<T> comparison)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));
            if (count > 1)
            {
                Array.Sort(items, 0, count, Comparer<T>.Create(comparison));
            }
        }

        public override string ToString()
        {
            string res = "";
            for (int i = 0; i < count; ++i) 
            {
                if (items[i] is null)
                    continue;
                res += items[i]!.ToString() + "\n";
            }
            return res;
        }


        public T?[] GetItems()
        {
            T?[] currentItems = new T?[count];
            Array.Copy(items, currentItems, count);
            return currentItems;
        }
        public int GetCount()
        {
            return count;
        }
        public int GetInsertionId()
        {
            return nextInsertionId;
        }
        public int[] GetInsertionOrder()
        {
            int[] currentOrder = new int[count];
            Array.Copy(insertionOrder, currentOrder, count);
            return currentOrder;
        }


        public bool IsEmpty(bool printMessage = true)
        {
            if (count == 0)
            {
                if (printMessage)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Container is empty");
                    Console.ResetColor();
                }
                return true;
            }
            return false;
        }

        public T[] GetItemsByParameter<Y>(string param, Y i)
        {
            var _items = new List<T>(); 
            for (int k = 0; k < count; ++k) 
            {
                if (items[k] != null)
                {
                    var value = Helper.GetPropertyValue<Y>(items[k], param);
                    if (value != null && value.Equals(i))
                    {
                        _items.Add(items[k]!);
                    }
                }
            }
            return _items.Count == 0 ? Array.Empty<T>() : _items.ToArray();
        }

        public T? GetInstanceByInsertionId(int id)
        {
            if (id < 0 || id >= nextInsertionId) 
                throw new IndexOutOfRangeException($"There is no entry number {id}");

            for (int j = 0; j < count; j++)
            {
                if (insertionOrder[j] == id)
                {
                    return items[j];
                }
            }
            return default;
        }

        public T? Find(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            for (int i = 0; i < count; i++)
            {
                if (items[i] != null && match(items[i]!))
                    return items[i];
            }
            return null;
        }

        public IEnumerable<T> FindAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            List<T> foundItems = new List<T>();
            for (int i = 0; i < count; i++) 
            {
                if (items[i] != null && match(items[i]!))
                    foundItems.Add(items[i]!);
            }
            return foundItems;
        }


        // Insertion order indexer
        public T? this[int id]
        {
            get => GetInstanceByInsertionId(id);
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                for (int j = 0; j < count; j++)
                {
                    if (insertionOrder[j] == id)
                    {
                        if (items[j] is IPrice oldPriceItem)
                        {
                            totalPrice -= oldPriceItem.Price;
                            oldPriceItem.PriceChanged -= HandlePriceChange;
                        }
                        items[j] = value;
                        if (value is IPrice newPriceItem)
                        {
                            totalPrice += newPriceItem.Price;
                            newPriceItem.PriceChanged += HandlePriceChange;
                        }
                        return;
                    }
                }
                throw new IndexOutOfRangeException("Can not find element by this insertion index");
            }
        }

        // Name indexer
        public T[] this[string i]
        {
            get => GetItemsByParameter("Name", i);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++) 
            {
                if (items[i] != null) yield return items[i]!;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        // Reverse Generator 
        public IEnumerable<T> GetReverseArray()
        {
            for (int i = count - 1; i >= 0; i--) 
            {
                if (items[i] != null) yield return items[i]!;
            }
        }

        // Substring Generator
        public IEnumerable<T> GetArrayWithSublineInName(string subline)
        {
            for (int i = 0; i < count; i++) 
            {
                if (items[i] != null && items[i]!.Name.Contains(subline))
                    yield return items[i]!;
            }
        }

        // Sorted by Price Generator
        public IEnumerable<T> GetSortedByArrayPrice()
        {
            if (count == 0) yield break;
            T?[] _itemsCopy = new T?[count];
            Array.Copy(items, _itemsCopy, count);
            Array.Sort(_itemsCopy, new PriceComparer());
            foreach (var item in _itemsCopy)
            {
                if (item != null) yield return item; 
            }
        }

        // Sorted by Name Generator
        public IEnumerable<T> GetSortedArrayByName()
        {
            if (count == 0) yield break;
            T?[] _itemsCopy = new T?[count];
            Array.Copy(items, _itemsCopy, count);
            Array.Sort(_itemsCopy); 
            foreach (var item in _itemsCopy)
            {
                if (item != null) yield return item; 
            }
        }

        public void PostDeserializeInitialize()
        {
            for (int i = 0; i < count; i++)
            {
                if (items[i] is IPrice priceItem)
                {
                    priceItem.PriceChanged -= HandlePriceChange; 
                    priceItem.PriceChanged += HandlePriceChange;
                }
            }
        }
    }


    public class ContainerLinkedList<T> : IEnumerable<T>, IEnumerator<T> where T : class, IName
    {
        public class Node<V>
        {
            public V Data { get; set; }
            public Node<V>? Next { get; set; } 
            public Node<V>? Previous { get; set; } 

            public Node() 
            {
                Data = default!; 
                Next = null;
                Previous = null;
            }

            public Node(V data)
            {
                Data = data;
                Next = null;
                Previous = null;
            }
        }

        private Node<T>? _head; 

        [JsonInclude]
        private Node<T>? _currentNode; 

        public Node<T>? First => _head;
        public Node<T>? Last => GetLastNode(); 

        private int _count;
        public int Count
        {
            get => _count;
            private set => _count = value;
        }

        private int NextInsertionId;
        private List<int> InsertionOrder;
        private decimal totalPrice;
        public decimal TotalPrice => totalPrice;


        public ContainerLinkedList()
        {
            _head = null;
            _currentNode = null;
            Count = 0;
            InsertionOrder = new List<int>();
            NextInsertionId = 0;
            totalPrice = 0; 
        }


        public void AddFirst(T data)
        {
            Node<T> newNode = new Node<T>(data);
            if (_head != null)
            {
                newNode.Next = _head;
                _head.Previous = newNode;
            }
            _head = newNode;

            InsertionOrder.Insert(0, NextInsertionId++);
            Count++;


            if (data is IPrice price)
            {
                totalPrice += price.Price;
                price.PriceChanged += HandlePriceChange;
            }
        }

        public void AddLast(T data)
        {
            Node<T> newNode = new Node<T>(data);
            if (_head == null)
            {
                _head = newNode;
            }
            else
            {
                Node<T> lastNode = GetLastNode()!; 
                lastNode.Next = newNode;
                newNode.Previous = lastNode;
            }

            InsertionOrder.Add(NextInsertionId++);
            Count++;

            if (data is IPrice price)
            {
                totalPrice += price.Price;
                price.PriceChanged += HandlePriceChange;
            }
        }

        private Node<T>? GetLastNode()
        {
            if (_head == null) return null;
            Node<T> node = _head;
            while (node.Next != null)
            {
                node = node.Next;
            }
            return node;
        }

        private void HandlePriceChange(object? sender, decimal oldPrice)
        {
            if (sender is IPrice price)
            {
                totalPrice += price.Price - oldPrice;
            }
        }

        public T? RemoveByIndex(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));

            int currentIdx = 0;
            var current = _head;
            while (current != null && currentIdx < index)
            {
                current = current.Next;
                currentIdx++;
            }

            if (current == null) throw new InvalidOperationException("Failed to find node at valid index.");

            T? deletedItem = current.Data;

            if (current.Previous != null)
                current.Previous.Next = current.Next;
            else
                _head = current.Next;

            if (current.Next != null)
                current.Next.Previous = current.Previous;

            Count--;
            InsertionOrder.RemoveAt(index); 

            if (deletedItem is IPrice price)
            {
                totalPrice -= price.Price;
                price.PriceChanged -= HandlePriceChange;
            }

            return deletedItem;
        }

        public List<T> NodeToList()
        {
            List<T> list = new List<T>();
            for (var node = _head; node != null; node = node.Next)
            {
                list.Add(node.Data);
            }
            return list;
        }

        private void BinaryInsertionSort(List<T> list, string propertyName, bool ChangeMainNode = true)
        {
            for (int i = 1; i < list.Count; i++)
            {
                int currentInsertionValue = ChangeMainNode ? InsertionOrder[i] : -1; 

                T currentItem = list[i];
                var currentValue = Helper.GetPropertyValue<object>(currentItem, propertyName);
                int ins = BinarySearch(list, currentValue, propertyName, 0, i);

                if (ins < i)
                {
                    list.RemoveAt(i);
                    list.Insert(ins, currentItem);

                    if (ChangeMainNode)
                    {
                        InsertionOrder.RemoveAt(i);
                        InsertionOrder.Insert(ins, currentInsertionValue);
                    }
                }
            }
        }

        private int BinarySearch(List<T> list, object? key, string propertyName, int low, int high)
        {
            while (low < high)
            {
                int mid = low + (high - low) / 2;
                var midValue = Helper.GetPropertyValue<object>(list[mid], propertyName);

                if (Compare(key, midValue) < 0)
                    high = mid;
                else
                    low = mid + 1;
            }
            return low;
        }

        public void Sort(Comparison<T> comparison)
        {
            if (_head == null || comparison == null || Count <= 1) return;

            List<T> list = NodeToList();
            List<int> originalInsertionIds = new List<int>(InsertionOrder); 

            var itemsWithIds = list.Zip(originalInsertionIds, (item, id) => new { Item = item, Id = id }).ToList();
            itemsWithIds.Sort((pair1, pair2) => comparison(pair1.Item, pair2.Item));

            _head = null;
            Node<T>? currentTail = null;
            InsertionOrder.Clear();

            foreach (var pair in itemsWithIds)
            {
                Node<T> newNode = new Node<T>(pair.Item);
                if (_head == null)
                {
                    _head = newNode;
                    currentTail = _head;
                }
                else
                {
                    currentTail!.Next = newNode;
                    newNode.Previous = currentTail;
                    currentTail = newNode;
                }
                InsertionOrder.Add(pair.Id); 
            }
        }


        private int Compare(object? a, object? b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            if (a is IComparable comparableA && b.GetType() == a.GetType())
            {
                return comparableA.CompareTo(b);
            }
            throw new InvalidOperationException($"Values are not comparable or of different types: {a.GetType()} vs {b.GetType()}");
        }

        public void Clear()
        {
            _head = null;
            _currentNode = null; 
            Count = 0;
            InsertionOrder.Clear();
            NextInsertionId = 0;
            totalPrice = 0;
        }

        public int GetNextInsertionId()
        {
            return NextInsertionId;
        }
        public List<int> GetInsertionOrder()
        {
            return new List<int>(InsertionOrder); 
        }
        public int GetCount()
        {
            return Count;
        }
        public void Add(T data)
        {
            AddLast(data);
        }

        public override string ToString()
        {
            if (_head is null) return "Container is empty.";

            string res = string.Empty;
            var current = _head;
            while (current != null)
            {
                res += current.Data?.ToString() + "\n";
                current = current.Next;
            }
            return res;
        }

        private List<T> GetItemsByParameter<Y>(string parameter, Y i)
        {
            List<T> values = new List<T>();
            var current = _head;
            while (current != null)
            {
                var propValue = Helper.GetPropertyValue<Y>(current.Data, parameter);
                if (propValue != null && propValue.Equals(i))
                {
                    values.Add(current.Data);
                }
                current = current.Next;
            }
            return values; 
        }

        public T? Find(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            var current = _head;
            while (current != null)
            {
                if (match(current.Data)) return current.Data;
                current = current.Next;
            }
            return null;
        }

        public IEnumerable<T> FindAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            List<T> foundItems = new List<T>();
            var current = _head;
            while (current != null)
            {
                if (match(current.Data)) foundItems.Add(current.Data);
                current = current.Next;
            }
            return foundItems;
        }

        // Insertion indexer 
        public T? this[int insertionIdValue]
        {
            get
            {
                var current = _head;
                for (int i = 0; i < Count; ++i) 
                {
                    if (InsertionOrder[i] == insertionIdValue)
                    {
                        // Need to get the i-th node
                        Node<T>? nodeToFind = _head;
                        for (int j = 0; j < i; ++j)
                        {
                            nodeToFind = nodeToFind?.Next;
                        }
                        return nodeToFind?.Data;
                    }
                }
                return null;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                var current = _head;
                for (int i = 0; i < Count; ++i)
                {
                    if (InsertionOrder[i] == insertionIdValue)
                    {
                        Node<T>? nodeToChange = _head;
                        for (int j = 0; j < i; ++j)
                        {
                            nodeToChange = nodeToChange?.Next;
                        }
                        if (nodeToChange != null)
                        {
                            if (nodeToChange.Data is IPrice oldPriceItem)
                            {
                                totalPrice -= oldPriceItem.Price;
                                oldPriceItem.PriceChanged -= HandlePriceChange;
                            }
                            nodeToChange.Data = value;
                            if (value is IPrice newPriceItem)
                            {
                                totalPrice += newPriceItem.Price;
                                newPriceItem.PriceChanged += HandlePriceChange;
                            }
                            return;
                        }
                    }
                }
                throw new IndexOutOfRangeException("Can not find element by this insertion ID value.");
            }
        }

        // Name indexer
        public List<T> this[string name]
        {
            get => GetItemsByParameter<string>("Name", name);
        }

        // Implemented foreach usage
        public T Current => _currentNode?.Data!;

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_currentNode == null)
            {
                _currentNode = _head;
            }
            else
            {
                _currentNode = _currentNode.Next;
            }
            return _currentNode != null;
        }

        public void Reset()
        {
            _currentNode = null;
        }

        public void Dispose() { } 

        public IEnumerator<T> GetEnumerator()
        {
            Reset();
            return this; 
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        // Reverse Generator 
        public IEnumerable<T> GetReverseArray()
        {
            var current = GetLastNode();
            while (current != null)
            {
                yield return current.Data;
                current = current.Previous;
            }
        }

        // Substring Generator
        public IEnumerable<T> GetArrayWithSublineInName(string subline)
        {
            var current = _head;
            while (current != null)
            {
                if (current.Data.Name.Contains(subline))
                {
                    yield return current.Data;
                }
                current = current.Next;
            }
        }


        // Sorted by Price Generator
        public IEnumerable<T> GetSortedByArrayPrice()
        {
            if (_head == null) yield break;
            List<T> list = NodeToList();
            BinaryInsertionSort(list, "Price", false);
            foreach (var item in list)
            {
                yield return item;
            }
        }

        // Sorted by Name Generator
        public IEnumerable<T> GetSortedArrayByName()
        {
            if (_head == null) yield break;
            List<T> list = NodeToList();
            BinaryInsertionSort(list, "Name", false);
            foreach (var item in list)
            {
                yield return item;
            }
        }


        public void PostDeserializeInitialize()
        {
            Node<T>? current = _head;
            while (current != null)
            {
                if (current.Data is IPrice priceItem)
                {
                    priceItem.PriceChanged -= HandlePriceChange;
                    priceItem.PriceChanged += HandlePriceChange;
                }
                current = current.Next;
            }
            _currentNode = null;
        }
    }
}