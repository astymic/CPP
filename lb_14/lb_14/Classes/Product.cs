using lb_14.Interfaces;

namespace lb_14.Classes
{
    class Product : IName, IName<Product>, IPrice, ICustomSerializable
    {
        public string Name { get; set; }
        private decimal price;
        public decimal Price
        {
            get => price;
            set
            {
                if (price != value)
                {
                    var oldPrice = price;
                    price = value;
                    PriceChanged?.Invoke(this, oldPrice);
                }
            }
        }

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

        public static Product Deserialize(BinaryReader reader)
        {
            return new Product
            {
                Name = reader.ReadString(),
                Price = reader.ReadDecimal()
            };
        }

        public event EventHandler<decimal>? PriceChanged;
    }
}
