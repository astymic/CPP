using System.Collections.Generic;
using lb_8.Interfaces;
using System.Reflection;

namespace lb_8
{
    class Container<T> where T : class, IName
    {
        private T?[] items;
        private int[] insertionOrder;
        private int count;
        private int size;
        private int nextInsertionId;

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
        }

        public T? RemoveById(int _index)
        {
            if (_index < 0 || _index > count)
                throw new IndexOutOfRangeException();

            T? deletedObject = items[_index]!;
            for (int i = _index; i < count - 1; i++)
            {
                items[i] = items[i + 1];
                insertionOrder[i] = insertionOrder[i + 1];
            }
            items[count - 1] = default;
            insertionOrder[count - 1] = 0;
            count--;

            return deletedObject;
        }

        public void Sort()
        {
            try
            {
                for (int i = 0; i < count - 1; i++)
                {
                    for (int j = 0; j < count - i - 1; j++)
                    {
                        if (GetPropertyValue<decimal>(items[j], "Price") > GetPropertyValue<decimal>(items[j + 1], "Price"))
                        {
                            (items[j], items[j + 1]) = (items[j + 1], items[j]);
                            (insertionOrder[j], insertionOrder[j + 1]) = (insertionOrder[j + 1], insertionOrder[j]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }
        }

        private static V? GetPropertyValue<V>(object item, string propertyName)
        {
            if (item == null) return default;

            PropertyInfo? property = item.GetType().GetProperty(propertyName);
            if (property != null && property.CanRead)
            {
                return (V?)property.GetValue(item);
            }
            return default;
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
                    var value = GetPropertyValue<Y>(item, param);
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

        // Insertion order indexer
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

        // Name indexer
        public T[] this[string i] 
        {
            get => GetItemsByParameter("Name", i); 
        }

        //// Price indexer
        //public T[] this[decimal i] 
        //{
        //    get => GetItemsByParameter("Price", i);
        //}
    }


}
