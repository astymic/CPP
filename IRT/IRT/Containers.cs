using System.Collections.Generic;
using IRT.Interfaces;
using System.Reflection;
using System.Collections;
using System;
using System.Linq;

namespace IRT
{
    class Helper
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

    class PriceComparer : IComparer
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



    class Container<T> : IEnumerable<T> where T : class, IName
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
            if (_index < 0 || _index > count)
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
            foreach (var item in items)
            {
                if (item is null)
                    continue;
                res += item.ToString() + "\n";
            }
            return res;
        }


        public T?[] GetItems()
        {
            return items;
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
            return insertionOrder;
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
            var _items = new T[count];
            int index = 0;
            foreach (var item in items)
            {
                if (item != null)
                {
                    var value = Helper.GetPropertyValue<Y>(item, param);
                    if (value != null && value.Equals(i))
                    {
                        _items[index] = item;
                        index++;
                    }
                }
            }
            return index == 0 ? default : _items;
        }

        public T? GetInstanceByInsertionId(int id)
        {
            if (id < 0 | id > nextInsertionId) throw new IndexOutOfRangeException($"There is no entry number {id}");

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
            return items.FirstOrDefault(item => item != null && match(item));
        }

        public IEnumerable<T> FindAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            return items.Where(item => item != null && match(item)).Cast<T>();
        }


        
        public T? this[int id] 
        {
            get => GetInstanceByInsertionId(id);
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                T? _item = GetInstanceByInsertionId(id);
                if (_item != null)
                {
                    _item = value;
                }
                throw new IndexOutOfRangeException("Can not find element by this insertion index");
            }
        }

        
        public T[] this[string i] 
        {
            get => GetItemsByParameter("Name", i); 
        }

        
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in items)
            {
                if (item != null) yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        
        public IEnumerable<T> GetReverseArray()
        {
            var _items = (T[])items.Clone();
            Array.Reverse( _items );
            foreach (var item in _items)
            {
                if (item != null) yield return item;
            }
        }

        
        public IEnumerable<T> GetArrayWithSublineInName(string subline)
        {
            foreach (var item in items)
            {
                if (item != null && item.Name.Contains(subline)) 
                    yield return item;
            }
        }

        
        public IEnumerable<T> GetSortedByArrayPrice()
        {
            var _items = (T[])items.Clone();
            Array.Sort(_items, new PriceComparer());
            foreach (var item in _items)
            {
                yield return item;
            }
        }

        
        public IEnumerable<T> GetSortedArrayByName()
        {
            var _items = (T[])items.Clone();
            Array.Sort(_items);
            foreach (var item in _items)
            {
                yield return item;
            }
        }
    }


    class ContainerLinkedList<T> : IEnumerable<T>, IEnumerator<T> where T : class, IName
    {
        public class Node<V>
        {
            public V Data { get; set; }
            public Node<V> Next { get; set; }
            public Node<V> Previous { get; set; }

            public Node(V data)
            {
                Data = data;
                Next = null;
                Previous = null;
            }
        }

        public ContainerLinkedList()
        {
            _head = null;
            _currentNode = null;
            Count = 0;
            InsertionOrder = new List<int>(); 
            NextInsertionId = 0;
        }


        private Node<T> _head;
        private Node<T> _currentNode;

        public Node<T> First => _head;
        public Node<T> Last => GetLastNode();
        
        private int _count;
        public int Count
        {
            get
            {
                if (_count < 0)
                {
                    _count = 0;
                }
                return _count;
            }
            private set => _count = value;                
        }
        private int NextInsertionId;
        private List<int> InsertionOrder;
        private decimal totalPrice;
        public decimal TotalPrice => totalPrice;


        public void AddFirst(T data)
        {
            Node<T> newNode = new Node<T>(data);
            if (_head != null) 
            {
                newNode.Next = _head;
                _head.Previous = newNode;
            }
            _head = newNode;
            
            Count++;
            InsertionOrder.Add(NextInsertionId++);

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
                Node<T> lastNode = GetLastNode();
                lastNode.Next = newNode;
                newNode.Previous = lastNode;
            }
            
            Count++;
            InsertionOrder.Add(NextInsertionId++);

            if (data is IPrice price)
            {
                totalPrice += price.Price;
                price.PriceChanged += HandlePriceChange;
            }
        }

        private Node<T> GetLastNode()
        {
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
            int count = 0;
            var current = _head;
            while (current != null && count < index)
            {
                current = current.Next;
                count++;
            }

            if (current == null) throw new ArgumentOutOfRangeException(nameof(index));

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
                int currentInsertionValue = InsertionOrder[i];

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
            if (_head == null || comparison == null) return;

            List<T> list = NodeToList();
            list.Sort(comparison);

            var current = _head;
            foreach (var item in list)
            {
                current.Data = item;
                current = current.Next;
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
            throw new InvalidOperationException("Values are not comparable");
        }

        public int GetNextInsertionId()
        {
            return NextInsertionId;
        }
        public List<int> GetInsertionOrder()
        {
            return InsertionOrder;
        }
        public int GetCount()
        {
            return Count;
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
            return values.Count == 0 ? null : values;
        }

        public T? Find(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            return this.FirstOrDefault(item => item != null && match(item));
        }

        public IEnumerable<T> FindAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            return this.Where(item => item != null && match(item)).Cast<T>();
        }

        
        public T? this[int index]
        {
            get
            {
                var current = _head;
                int count = 0;
                while (current != null)
                {
                    if (InsertionOrder[count] == index)
                        return current.Data;
                    current = current.Next;
                    count++;
                }
                return null;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                var current = _head;
                int count = 0;
                while (current != null)
                {
                    if (InsertionOrder[count] == index)
                    { 
                        current.Data = value;
                        return;
                    }
                    current = current.Next;
                    count++;
                }
                throw new IndexOutOfRangeException("Can not find element by this insertion index");
            }
        }

        
        public List<T> this[string name]
        {
            get => GetItemsByParameter<string>("Name", name);
        }

        
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

        public void Dispose() {  }

        public IEnumerator<T> GetEnumerator()
        {
            Reset();
            return this;
        }      

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        
        public IEnumerable<T> GetReverseArray()
        {
            var current = GetLastNode();
            while (current != null)
            {
                yield return current.Data;
                current = current.Previous;
            }
        }

        
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
    }
}