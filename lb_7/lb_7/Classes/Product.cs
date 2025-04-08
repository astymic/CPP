using lb_7.Interfaces;

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

        public int CompareTo(object that) { return this.Price.CompareTo(that); } // Compare Product.Price with recivied value (expected - decimal Price)
    }
}
