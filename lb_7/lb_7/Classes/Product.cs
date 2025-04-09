using lb_7.Interfaces;
using System.Data;

namespace lb_7.Classes
{
    class Product : IName
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        public Product()
        {
            Name = string.Empty;
            Price = 0;
        }

        public Product(string name, decimal price)
        {
            if (price <= 0) throw new ValueLessThanZero("Price");
            Name = name;
            Price = price;
        }

        public override string ToString()

        {
            return $"{Name}, Price: {Price}";
        }

        public int CompareTo(object obj) 
        {
            if (obj == null) return 1;

            //if (obj is IName other)
            //{
            //    return this.T.CompareTo(other.T);
            //}
            throw new ArgumentException("");
        } 
    }
}
