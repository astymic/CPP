using lb_11.Interfaces;

namespace lb_11.Classes
{
    class Product : IName, IName<Product>, IPrice, ICustomSerializable
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

            if (obj is not IName otherProduct)
            {
                throw new ArgumentException($"Object must be type {nameof(IName)}");
            }
            return StringComparer.OrdinalIgnoreCase.Compare(this.Name, otherProduct.Name);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Price);
        }
    }
}
