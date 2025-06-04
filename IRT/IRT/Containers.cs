using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using IRT.Classes;
using IRT.Interfaces;

namespace IRT
{
    public class Container<T> : IEnumerable<T> where T : IName, IName<T>, IPrice, ICustomSerializable
    {
        private T[] items;
        private int[] insertionOrder;
        private int count;
        private int size;
        private int nextInsertionId;
        private decimal totalPrice;

        public decimal TotalPrice => totalPrice;
        public int Count => count;
        public int InsertionId => nextInsertionId;

        public Container()
        {
            items = new T[4];
            insertionOrder = new int[4];
            count = 0;
            size = 4;
            nextInsertionId = 0;
            totalPrice = 0;
        }

        public void Add(T item)
        {
            if (count == size)
            {
                int newSize = size * 2;
                T[] newItemsArray = new T[newSize];
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
            items[count] = item;
            insertionOrder[count] = nextInsertionId;
            nextInsertionId++;
            count++;
            totalPrice += item.Price;
            item.PriceChanged += OnItemPriceChanged;
        }

        public T RemoveById(int index)
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException("Index is out of range for removal.");

            T deletedObject = items[index];
            totalPrice -= deletedObject.Price;
            deletedObject.PriceChanged -= OnItemPriceChanged;

            for (int i = index; i < count - 1; i++)
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

            for (int i = 0; i < count - 1; i++)
            {
                for (int j = 0; j < count - 1 - i; j++)
                { 
                    if (comparison(items[j], items[j + 1]) > 0)
                    {
                        (items[j], items[j + 1]) = (items[j + 1], items[j]);
                        (insertionOrder[j], insertionOrder[j + 1]) = (insertionOrder[j + 1], insertionOrder[j]);
                    }
                }
            }
        }

        private void OnItemPriceChanged(object sender, decimal oldPrice)
        {
            decimal diff = ((IPrice)sender).Price - oldPrice;
            totalPrice += diff;
        }

        public override string ToString()
        {
            if (count == 0) return "Container is empty.";

            string res = "";
            foreach (var item in items)
            {
                if (item is null)
                    continue;
                res += item.ToString() + "\n";
            }
            return res;
        }

        public IEnumerable<T> GetReverseArray()
        {
            T[] activeItems = new T[count];
            for (int i = 0; i < count; i++) 
                activeItems[i] = items[i];

            int iRev = 0, jRev = count - 1;
            while (iRev < jRev)
            {
                (activeItems[iRev], activeItems[jRev]) = (activeItems[jRev], activeItems[iRev]);
                iRev++;
                jRev--;
            }

            for (int i = 0; i < activeItems.Length; i++)
                yield return activeItems[i];
        }

        public IEnumerable<T> GetArrayWithSublineInName(string subline)
        {
            for (int i = 0; i < count; i++)
            { 
                if (((IName<T>)items[i]).Name.Contains(subline))
                    yield return items[i];
            }
        }

        public IEnumerable<T> GetSortedByArrayPrice()
        {
            T[] tempArray = new T[count];
            for (int i = 0; i < count; i++) 
                tempArray[i] = items[i];

            for (int i = 0; i < tempArray.Length - 1; i++)
            {
                for (int j = 0; j < tempArray.Length - 1 - i; j++)
                { 
                    if (tempArray[j].Price > tempArray[j + 1].Price)
                    { 
                        (tempArray[j], tempArray[j + 1]) = (tempArray[j + 1], tempArray[j]);
                    }
                }
            }

            for (int i = 0; i < tempArray.Length; i++)
                yield return tempArray[i];
        }

        public IEnumerable<T> GetSortedArrayByName()
        {
            T[] tempArray = new T[count];
            for (int i = 0; i < count; i++) tempArray[i] = items[i];

            for (int i = 0; i < tempArray.Length - 1; i++)
            { 
                for (int j = 0; j < tempArray.Length - 1 - i; j++)
                { 
                    if (string.Compare(((IName<T>)tempArray[j]).Name, ((IName<T>)tempArray[j + 1]).Name, StringComparison.OrdinalIgnoreCase) > 0)
                    { 
                        (tempArray[j], tempArray[j + 1]) = (tempArray[j + 1], tempArray[j]);
                    }
                }
            }

            for (int i = 0; i < tempArray.Length; i++)
                yield return tempArray[i];
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(count);
            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                writer.Write(item.GetType().Name);
                item.Serialize(writer);
            }
        }


        public static Container<T> Deserialize(BinaryReader reader)
        {
            var container = new Container<T>();
            int c = reader.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                string typeName = reader.ReadString();
                var type = typeof(T).Assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == typeName && typeof(T).IsAssignableFrom(t));
                if (type == null)
                    throw new InvalidOperationException("Unknown type: " + typeName);

                var method = type.GetMethod("Deserialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method == null)
                    throw new InvalidOperationException($"No Deserialize method in {typeName}");

                var obj = (T)method.Invoke(null, new object[] { reader });
                container.Add(obj);
            }
            return container;
        }

        public T this[int id]
        {
            get
            {
                for (int i = 0; i < count; i++)
                {
                    if (insertionOrder[i] == id)
                        return items[i];
                }
                return default;
            }
            set
            {
                for (int i = 0; i < count; i++)
                {
                    if (insertionOrder[i] == id)
                    {
                        totalPrice -= items[i].Price;
                        items[i].PriceChanged -= OnItemPriceChanged;
                        items[i] = value;
                        totalPrice += value.Price;
                        value.PriceChanged += OnItemPriceChanged;
                        break;
                    }
                }
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
                yield return items[i];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public T? Find(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            for (int i = 0; i < count; i++)
            { 
                if (match(items[i]))
                    return items[i];
            }
            return default;
        }

        public IEnumerable<T> FindAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            for (int i = 0; i < count; i++)
            { 
                if (match(items[i]))
                    yield return items[i];
            }
        }

        public IEnumerable<T> GetByNamePrefix(string prefix)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            for (int i = 0; i < count; i++)
            {
                if (((IName)items[i]).Name != null && ((IName)items[i]).Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    yield return items[i];
            }
        }
    }

    public class ContainerLinkedList<T> : IEnumerable<T> where T : IName, IName<T>, IPrice, ICustomSerializable
    {
        public class Node
        {
            public T Data { get; set; }
            public Node Next { get; set; }
            public Node Previous { get; set; }
            public Node(T data) { Data = data; }
        }

        private Node head;
        private Node tail;
        private int count;
        private int nextInsertionId;
        private decimal totalPrice;
        private readonly Dictionary<Node, int> insertionOrderMap = new();

        public decimal TotalPrice => totalPrice;
        public int Count => count;
        public int InsertionId => nextInsertionId;

        public ContainerLinkedList()
        {
            head = null;
            tail = null;
            count = 0;
            nextInsertionId = 0;
        }

        public void AddLast(T data)
        {
            Node node = new Node(data);
            insertionOrderMap[node] = nextInsertionId++;
            if (head == null)
            {
                head = tail = node;
            }
            else
            {
                tail.Next = node;
                node.Previous = tail;
                tail = node;
            }
            count++;
            totalPrice += data.Price;
            data.PriceChanged += OnItemPriceChanged;
        }

        public T RemoveByIndex(int index)
        {
            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException();

            Node current = head;
            for (int i = 0; i < index; i++)
                current = current.Next;

            T data = current.Data;
            totalPrice -= data.Price;
            data.PriceChanged -= OnItemPriceChanged;
            insertionOrderMap.Remove(current);

            if (current.Previous != null)
                current.Previous.Next = current.Next;
            else
                head = current.Next;
            if (current.Next != null)
                current.Next.Previous = current.Previous;
            else
                tail = current.Previous;

            count--;
            return data;
        }

        public void Sort(Comparison<T> comparison)
        {
            if (count < 2) return;
            T[] arr = new T[count];
            Node cur = head;

            for (int i = 0; i < count; i++) 
            { 
                arr[i] = cur.Data; 
                cur = cur.Next; 
            }

            for (int i = 0; i < arr.Length - 1; i++)
            { 
                for (int j = 0; j < arr.Length - 1 - i; j++)
                { 
                    if (comparison(arr[j], arr[j + 1]) > 0)
                    { 
                        (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                    }
                }
            }

            cur = head;
            int idx = 0;
            while (cur != null)
            {
                cur.Data.PriceChanged -= OnItemPriceChanged;
                cur.Data = arr[idx++];
                cur.Data.PriceChanged += OnItemPriceChanged;
                cur = cur.Next;
            }
        }

        private void OnItemPriceChanged(object sender, decimal oldPrice)
        {
            decimal diff = ((IPrice)sender).Price - oldPrice;
            totalPrice += diff;
        }

        public override string ToString()
        {
            if (count == 0) return "ContainerLinkedList is empty.";

            string res = string.Empty;
            var current = head;
            while (current != null)
            {
                res += current.Data?.ToString() + "\n";
                current = current.Next;
            }
            return res;
        }

        public IEnumerable<T> GetReverseArray()
        {
            T[] arr = new T[count];
            Node cur = head;
            for (int i = 0; i < count; i++) 
            { 
                arr[i] = cur.Data; 
                cur = cur.Next; 
            }

            int iRev = 0, jRev = count - 1;
            while (iRev < jRev)
            {
                (arr[iRev], arr[jRev]) = (arr[jRev], arr[iRev]);
                iRev++;
                jRev--;
            }

            for (int i = 0; i < arr.Length; i++)
                yield return arr[i];
        }

        public IEnumerable<T> GetArrayWithSublineInName(string subline)
        {
            Node cur = head;
            while (cur != null)
            {
                if (((IName<T>)cur.Data).Name.Contains(subline))
                    yield return cur.Data;
                cur = cur.Next;
            }
        }

        public IEnumerable<T> GetSortedByArrayPrice()
        {
            T[] arr = new T[count];
            Node cur = head;
            for (int i = 0; i < count; i++) 
            { 
                arr[i] = cur.Data; 
                cur = cur.Next; 
            }

            for (int i = 0; i < arr.Length - 1; i++)
            { 
                for (int j = 0; j < arr.Length - 1 - i; j++)
                { 
                    if (arr[j].Price > arr[j + 1].Price)
                    { 
                        (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                    }
                }
            }

            for (int i = 0; i < arr.Length; i++)
                yield return arr[i];
        }

        public IEnumerable<T> GetSortedArrayByName()
        {
            T[] arr = new T[count];
            Node cur = head;
            for (int i = 0; i < count; i++) 
            { 
                arr[i] = cur.Data; 
                cur = cur.Next; 
            }

            for (int i = 0; i < arr.Length - 1; i++)
            { 
                for (int j = 0; j < arr.Length - 1 - i; j++)
                { 
                    if (string.Compare(((IName<T>)arr[j]).Name, ((IName<T>)arr[j + 1]).Name, StringComparison.OrdinalIgnoreCase) > 0)
                    { 
                        (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                    }
                }
            }

            for (int i = 0; i < arr.Length; i++)
                yield return arr[i];
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(count);
            Node current = head;
            while (current != null)
            {
                var item = current.Data;
                writer.Write(item.GetType().Name); 
                item.Serialize(writer);
                current = current.Next;
            }
        }

        public static ContainerLinkedList<T> Deserialize(BinaryReader reader)
        {
            var container = new ContainerLinkedList<T>();
            int c = reader.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                string typeName = reader.ReadString();
                var type = typeof(T).Assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == typeName && typeof(T).IsAssignableFrom(t));
                if (type == null)
                    throw new InvalidOperationException("Unknown type: " + typeName);

                var method = type.GetMethod("Deserialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method == null)
                    throw new InvalidOperationException($"No static Deserialize method in {typeName}");

                var obj = (T)method.Invoke(null, new object[] { reader });
                container.AddLast(obj); 
            }
            return container;
        }


        public T this[int id]
        {
            get
            {
                foreach (var pair in insertionOrderMap)
                    if (pair.Value == id)
                        return pair.Key.Data;
                return default;
            }
            set
            {
                foreach (var pair in insertionOrderMap)
                {
                    if (pair.Value == id)
                    {
                        totalPrice -= pair.Key.Data.Price;
                        pair.Key.Data.PriceChanged -= OnItemPriceChanged;
                        pair.Key.Data = value;
                        totalPrice += value.Price;
                        value.PriceChanged += OnItemPriceChanged;
                        break;
                    }
                }
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            Node current = head;
            while (current != null)
            {
                yield return current.Data;
                current = current.Next;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public T? Find(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            var node = head;
            while (node != null)
            {
                if (match(node.Data))
                    return node.Data;
                node = node.Next;
            }
            return default;
        }

        public IEnumerable<T> FindAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            var node = head;
            while (node != null)
            {
                if (match(node.Data))
                    yield return node.Data;
                node = node.Next;
            }
        }

        public IEnumerable<T> GetByNamePrefix(string prefix)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            var node = head;
            while (node != null)
            {
                if (((IName)node.Data).Name != null && ((IName)node.Data).Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    yield return node.Data;
                node = node.Next;
            }
        }
    }
}