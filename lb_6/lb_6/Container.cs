
using System.Reflection;

namespace lb_6
{
    class Container
    {
        private Object[] items;
        private int[] insertionOrder;
        private int count;
        private int size;
        private int nextInsertionId;

        public Container()
        {
            items = new Object[1];
            insertionOrder = new int[1];
            count = 0;
            size = 1;
            nextInsertionId = 0;
        }

        public void Add(object _newObject)
        {
            if (count == size)
            {
                Object[] newArray = new Object[size * 2];
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

        public object RemoveById(int _index)
        {
            if (_index < 0 || _index > count)
                throw new IndexOutOfRangeException();

            object deletedObject = items[_index];
            for (int i = _index; i < count - 1; i++)
            {
                items[i] = items[i + 1];
                insertionOrder[i] = insertionOrder[i + 1];
            }
            items[count - 1] = null;
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

        private static T GetPropertyValue<T>(object item, string propertyName)
        {
            if (item == null) return default;

            PropertyInfo property = item.GetType().GetProperty(propertyName);
            if (property != null && property.PropertyType == typeof(T))
            {
                return (T)property.GetValue(item);
            }
            return default;
        }

        public string ToString()
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


        public Object[] GetItems()
        {
            return items;
        }
        public int GetCount()
        {
            return count;
        }

        public Object[] GetItemsByParameter<T>(string param, T i)
        {
            Object[] _items = new Object[count];
            int index = 0;
            foreach (var item in items)
            {
                if (item != null)
                {
                    var value = GetPropertyValue<T>(item, param);
                    if (value != null && value.Equals(i))
                    {
                        _items[index] = item;
                        index++;
                    }
                }
            }
            return index == 0 ? default : _items;
        }

        public Object this[int i]
        {
            get
            {
                if (i < 0) throw new IndexOutOfRangeException();
                if (i > nextInsertionId) throw new IndexOutOfRangeException($"There is no entry number {i}");

                for (int j = 0; j < count; j++)
                    if (insertionOrder[j] == i)
                        return items[j];

                return null;
            }
        }

        public Object[] this[string i] => GetItemsByParameter("Name", i);
        public Object[] this[decimal i] => GetItemsByParameter("Price", i);

    }
}
