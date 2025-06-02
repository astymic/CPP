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
        public int Compare(object? x, object? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

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

    class Container<T> : IEnumerable<T> where T : IName
    {
        private T?[] items;
        private int[] insertionOrder;
        private int count;
        private int size;
        private int nextInsertionId;

        public decimal TotalPrice
        {
            get
            {
                decimal sum = 0;
                for (int i = 0; i < count; i++)
                {
                    if (items[i] is IPrice priceItem) 
                    {
                        sum += priceItem.Price;
                    }
                }
                return sum;
            }
        }

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
                int newSize = size * 2;
                T?[] newItemsArray = new T?[newSize];
                int[] newInsertionOrderArray = new int[newSize];
                for (int i = 0; i < size; i++)
                {
                    newItemsArray[i] = items[i];
                    newInsertionOrderArray[i] = insertionOrder[i];
                }
                items = newItemsArray;
                insertionOrder = newInsertionOrderArray;
                size = newSize;
            }
            items[count] = _newObject;
            insertionOrder[count] = nextInsertionId;
            nextInsertionId++;
            count++;
        }

        public T? RemoveById(int _index)
        {
            if (_index < 0 || _index >= count)
                throw new IndexOutOfRangeException("Index is out of range for removal.");

            T? deletedObject = items[_index];

            for (int i = _index; i < count - 1; i++)
            {
                items[i] = items[i + 1];
                insertionOrder[i] = insertionOrder[i + 1];
            }
            items[count - 1] = default;
            insertionOrder[count - 1] = -1;
            count--;

            return deletedObject;
        }

        public void Sort(Comparison<T> comparison)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));
            if (count > 1)
            {
                InsertionSortWithTracking(items, insertionOrder, count, comparison);
            }
        }

        
        private static void InsertionSortWithTracking(T?[] array, int[] trackingArray, int currentItemCount, Comparison<T> comparison)
        {
            for (int i = 1; i < currentItemCount; i++)
            {
                T? keyItem = array[i];
                int keyTrack = trackingArray[i];
                int j = i - 1;
                
                while (j >= 0 && comparison(keyItem!, array[j]!) < 0)
                {
                    array[j + 1] = array[j];
                    trackingArray[j + 1] = trackingArray[j];
                    j = j - 1;
                }
                array[j + 1] = keyItem;
                trackingArray[j + 1] = keyTrack;
            }
        }

        private static T?[] CloneArraySegment(T?[] source, int startIndex, int length)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (startIndex < 0 || length < 0 || startIndex + length > source.Length)
                throw new ArgumentOutOfRangeException("Invalid clone segment parameters.");

            T?[] newArray = new T?[length];
            for (int i = 0; i < length; i++)
            {
                newArray[i] = source[startIndex + i];
            }
            return newArray;
        }

        private static void ReverseArraySegment(T?[] array, int startIndex, int length)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (startIndex < 0 || length < 0 || startIndex + length > array.Length)
                throw new ArgumentOutOfRangeException("Invalid reverse segment parameters.");

            int i = startIndex;
            int j = startIndex + length - 1;
            while (i < j)
            {
                (array[i], array[j]) = (array[j], array[i]);
                i++;
                j--;
            }
        }

        private static void InsertionSortArray(T?[] array, int currentItemCount, IComparer comparer)
        {
            for (int i = 1; i < currentItemCount; i++)
            {
                T? key = array[i];
                int j = i - 1;
                
                while (j >= 0 && comparer.Compare(key, array[j]) < 0)
                {
                    array[j + 1] = array[j];
                    j = j - 1;
                }
                array[j + 1] = key;
            }
        }

        private static void InsertionSortArrayForIName(T?[] array, int currentItemCount)
        {
            for (int i = 1; i < currentItemCount; i++)
            {
                T? key = array[i]; 
                int j = i - 1;

                
                while (j >= 0)
                {
                    bool shouldMoveCurrentJ;
                    T? currentJItem = array[j]; 

                    if (key == null) 
                    {
                        if (currentJItem != null) shouldMoveCurrentJ = true; 
                        else break; 
                    }
                    else 
                    {
                        if (currentJItem == null) break; 
                        else if (key.CompareTo(currentJItem) < 0) 
                        {
                            shouldMoveCurrentJ = true;
                        }
                        else break; 
                    }

                    if (shouldMoveCurrentJ)
                    {
                        array[j + 1] = currentJItem;
                        j--;
                    }
                    else
                    {
                        break; 
                    }
                }
                array[j + 1] = key;
            }
        }
        

        public override string ToString()
        {
            if (count == 0) return "Container is empty.";
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.AppendLine(items[i]!.ToString());
            }
            return sb.ToString();
        }

        public int GetCount() => count;
        public int GetInsertionId() => nextInsertionId;

        public int[] GetInsertionOrder()
        {
            int[] currentOrder = new int[count];
            for (int i = 0; i < count; i++)
            {
                currentOrder[i] = insertionOrder[i];
            }
            return currentOrder;
        }

        public T[] GetItemsByParameter<Y>(string param, Y val)
        {
            T[] foundItemsBuffer = new T[count];
            int foundCount = 0;
            for (int i = 0; i < count; i++)
            {
                if (items[i] != null)
                {
                    var propValue = Helper.GetPropertyValue<Y>(items[i]!, param);
                    if (propValue != null && propValue.Equals(val))
                    {
                        foundItemsBuffer[foundCount] = items[i]!;
                        foundCount++;
                    }
                }
            }
            if (foundCount == 0) return System.Array.Empty<T>();

            T[] result = new T[foundCount];
            for (int i = 0; i < foundCount; i++)
            {
                result[i] = foundItemsBuffer[i];
            }
            return result;
        }

        public T? GetInstanceByInsertionId(int id)
        {
            if (id < 0 || id >= nextInsertionId)
                throw new IndexOutOfRangeException($"Insertion ID {id} is out of the valid range of assigned IDs (0 to {nextInsertionId - 1}).");

            for (int i = 0; i < count; i++)
            {
                if (insertionOrder[i] == id)
                {
                    return items[i];
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
                {
                    return items[i];
                }
            }
            return default(T); 
        }

        public IEnumerable<T> FindAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            for (int i = 0; i < count; i++)
            {
                if (items[i] != null && match(items[i]!))
                {
                    yield return items[i]!;
                }
            }
        }

        public T? this[int id]
        {
            get => GetInstanceByInsertionId(id);
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value), "Cannot set item to null using indexer.");
                bool found = false;
                for (int i = 0; i < count; i++)
                {
                    if (insertionOrder[i] == id)
                    {
                        items[i] = value; 
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new IndexOutOfRangeException($"Cannot find element by insertion ID {id} to set.");
                }
            }
        }

        public T[] this[string name] 
        {
            get => GetItemsByParameter("Name", name);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                if (items[i] != null)
                {
                    yield return items[i]!;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable<T> GetReverseArray()
        {
            T?[] activeItems = CloneArraySegment(items, 0, count);
            ReverseArraySegment(activeItems, 0, count);
            for (int i = 0; i < activeItems.Length; i++) 
            {
                if (activeItems[i] != null) yield return activeItems[i]!;
            }
        }

        public IEnumerable<T> GetArrayWithSublineInName(string subline)
        {
            for (int i = 0; i < count; i++)
            {
                if (items[i] != null && items[i]!.Name.Contains(subline))
                    yield return items[i]!;
            }
        }

        public IEnumerable<T> GetSortedByArrayPrice()
        {
            T?[] activeItems = CloneArraySegment(items, 0, count);
            InsertionSortArray(activeItems, count, new PriceComparer());
            for (int i = 0; i < activeItems.Length; i++)
            {
                if (activeItems[i] != null) yield return activeItems[i]!;
            }
        }

        public IEnumerable<T> GetSortedArrayByName()
        {
            T?[] activeItems = CloneArraySegment(items, 0, count);
            InsertionSortArrayForIName(activeItems, count);
            for (int i = 0; i < activeItems.Length; i++)
            {
                if (activeItems[i] != null) yield return activeItems[i]!;
            }
        }
    }


    class ContainerLinkedList<T> : IEnumerable<T>, IEnumerator<T> where T : class, IName
    {
        public class Node
        {
            public T Data { get; set; }
            public Node? Next { get; set; }
            public Node? Previous { get; set; }

            public Node(T data)
            {
                Data = data;
                Next = null;
                Previous = null;
            }
        }

        public decimal TotalPrice
        {
            get
            {
                decimal sum = 0;
                Node? current = _head;
                while (current != null)
                {
                    if (current.Data is IPrice priceItem)
                    {
                        sum += priceItem.Price;
                    }
                    current = current.Next;
                }
                return sum;
            }
        }


        public ContainerLinkedList()
        {
            _head = null;
            _currentNode = null;
            Count = 0;
            InsertionOrderMap = new Dictionary<Node, int>();
            NextInsertionId = 0;
        }

        private Node? _head;
        private Node? _currentNode;

        public Node? First => _head;
        public Node? Last => GetLastNode();

        public int Count { get; private set; }

        private int NextInsertionId;
        private Dictionary<Node, int> InsertionOrderMap { get; set; }


        public void AddFirst(T data)
        {
            Node newNode = new Node(data);
            InsertionOrderMap[newNode] = NextInsertionId;

            if (_head != null)
            {
                newNode.Next = _head;
                _head.Previous = newNode;
            }
            _head = newNode;

            Count++;
            NextInsertionId++;
        }

        public void AddLast(T data)
        {
            Node newNode = new Node(data);
            InsertionOrderMap[newNode] = NextInsertionId;

            if (_head == null)
            {
                _head = newNode;
            }
            else
            {
                Node lastNode = GetLastNode()!;
                lastNode.Next = newNode;
                newNode.Previous = lastNode;
            }

            Count++;
            NextInsertionId++;
        }

        public void Sort(Comparison<T> comparison)
        {
            if (_head == null || _head.Next == null || comparison == null) return;

            T[] tempArray = ConvertToArray(); 
            Node[] nodeArray = new Node[Count];

            int idx = 0;
            Node? current = _head;
            while (current != null)
            {
                
                nodeArray[idx] = current;
                current = current.Next;
                idx++;
            }

            
            for (int i = 1; i < Count; i++)
            {
                T keyData = tempArray[i];
                Node keyNode = nodeArray[i]; 
                int j = i - 1;

                while (j >= 0 && comparison(keyData, tempArray[j]) < 0)
                {
                    tempArray[j + 1] = tempArray[j];
                    nodeArray[j + 1] = nodeArray[j]; 
                    j--;
                }
                tempArray[j + 1] = keyData;
                nodeArray[j + 1] = keyNode; 
            }

            _head = null;
            Node? prevNode = null;
            for (int i = 0; i < Count; i++)
            {
                Node sortedNode = nodeArray[i]; 
                sortedNode.Next = null;
                sortedNode.Previous = prevNode;

                if (prevNode != null)
                {
                    prevNode.Next = sortedNode;
                }
                else
                {
                    _head = sortedNode;
                }
                prevNode = sortedNode;
            }
        }
        
        private T[] ConvertToArray() 
        {
            T[] tempArray = new T[Count];
            int idx = 0;
            Node? current = _head;
            while (current != null)
            {
                tempArray[idx++] = current.Data; 
                current = current.Next;
            }
            return tempArray;
        }

        private static void ReverseArraySegmentGeneric<TItem>(TItem?[] array, int startIndex, int length)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (startIndex < 0 || length < 0 || startIndex + length > array.Length)
                throw new ArgumentOutOfRangeException("Invalid reverse segment parameters.");

            int i = startIndex;
            int j = startIndex + length - 1;
            while (i < j)
            {
                (array[i], array[j]) = (array[j], array[i]);
                i++;
                j--;
            }
        }

        private static void InsertionSortArrayGeneric<TItem>(TItem?[] array, int currentItemCount, IComparer comparer)
            where TItem : class 
        {
            for (int i = 1; i < currentItemCount; i++)
            {
                TItem? key = array[i];
                int j = i - 1;
                while (j >= 0 && comparer.Compare(key, array[j]) < 0)
                {
                    array[j + 1] = array[j];
                    j = j - 1;
                }
                array[j + 1] = key;
            }
        }

        private static void InsertionSortArrayForINameGeneric<TItem>(TItem?[] array, int currentItemCount)
             where TItem : class, IName 
        {
            for (int i = 1; i < currentItemCount; i++)
            {
                TItem? key = array[i];
                int j = i - 1;

                while (j >= 0)
                {
                    bool shouldMoveCurrentJ = false;
                    TItem? currentJItem = array[j];

                    if (key == null)
                    {
                        if (currentJItem != null) shouldMoveCurrentJ = true;
                        else break;
                    }
                    else
                    {
                        if (currentJItem == null) break;
                        else if (key.CompareTo(currentJItem) < 0)
                        {
                            shouldMoveCurrentJ = true;
                        }
                        else break;
                    }

                    if (shouldMoveCurrentJ)
                    {
                        array[j + 1] = currentJItem;
                        j--;
                    }
                    else
                    {
                        break;
                    }
                }
                array[j + 1] = key;
            }
        }
        

        private Node? GetLastNode()
        {
            if (_head == null) return null;
            Node node = _head;
            while (node.Next != null)
            {
                node = node.Next;
            }
            return node;
        }

        public List<int> GetInsertionOrder()
        {
            var order = new List<int>(Count); 
            var current = _head;
            while (current != null)
            {
                if (InsertionOrderMap.TryGetValue(current, out int insId))
                {
                    order.Add(insId);
                }
                else
                {
                    order.Add(-1);
                }
                current = current.Next;
            }
            return order;
        }

        public T? RemoveByIndex(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));

            Node? current = _head;
            for (int i = 0; i < index && current != null; i++)
            {
                current = current.Next;
            }

            if (current == null) throw new InvalidOperationException("Failed to find node at index during removal.");

            T deletedItem = current.Data; 
            InsertionOrderMap.Remove(current);

            if (current.Previous != null)
                current.Previous.Next = current.Next;
            else
                _head = current.Next;

            if (current.Next != null)
                current.Next.Previous = current.Previous;

            Count--;
            return deletedItem;
        }

        public int GetNextInsertionId() => NextInsertionId;
        public int GetCount() => Count;

        public override string ToString()
        {
            if (_head == null) return "Container is empty.";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var current = _head;
            while (current != null)
            {
                sb.AppendLine(current.Data.ToString());
                current = current.Next;
            }
            return sb.ToString();
        }

        private List<T> GetItemsByParameterList<Y>(string parameter, Y val) 
        {
            List<T> values = new List<T>();
            var current = _head;
            while (current != null)
            {
                var propValue = Helper.GetPropertyValue<Y>(current.Data, parameter);
                if (propValue != null && propValue.Equals(val))
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
            var current = _head;
            while (current != null)
            {
                if (match(current.Data)) yield return current.Data;
                current = current.Next;
            }
        }

        public T? this[int insertionIdToFind]
        {
            get
            {
                var current = _head;
                while (current != null)
                {
                    if (InsertionOrderMap.TryGetValue(current, out int currentId) && currentId == insertionIdToFind)
                    {
                        return current.Data;
                    }
                    current = current.Next;
                }
                return null;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value), "Cannot set item to null using indexer.");
                var current = _head;
                bool updated = false;
                while (current != null)
                {
                    if (InsertionOrderMap.TryGetValue(current, out int currentId) && currentId == insertionIdToFind)
                    {
                        current.Data = value;
                        updated = true;
                        break;
                    }
                    current = current.Next;
                }
                if (!updated) throw new IndexOutOfRangeException($"Cannot find element by insertion ID {insertionIdToFind} to set.");
            }
        }

        public List<T> this[string name] 
        {
            get => GetItemsByParameterList<string>("Name", name);
        }

        public T Current => _currentNode!.Data;

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

        public void Reset() => _currentNode = null;
        public void Dispose() { }

        public IEnumerator<T> GetEnumerator()
        {
            Reset();
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public IEnumerable<T> GetReverseArray()
        {
            T[] tempArray = ConvertToArray(); 
            ReverseArraySegmentGeneric(tempArray, 0, Count);
            for (int i = 0; i < tempArray.Length; i++)
            {
                yield return tempArray[i]; 
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
            T[] tempArray = ConvertToArray(); 
            InsertionSortArrayGeneric(tempArray, Count, new PriceComparer());
            for (int i = 0; i < tempArray.Length; i++)
            {
                yield return tempArray[i];
            }
        }

        public IEnumerable<T> GetSortedArrayByName()
        {
            if (_head == null) yield break;
            T[] tempArray = ConvertToArray(); 
            InsertionSortArrayForINameGeneric(tempArray, Count);
            for (int i = 0; i < tempArray.Length; i++)
            {
                yield return tempArray[i];
            }
        }
    }
}