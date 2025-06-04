using IRT.Interfaces;
using System;

namespace IRT.Classes
{
    class Product : IName, IName<Product>, IPrice, ICustomSerializable
    {
        public string Name { get; set; }
        string IName<Product>.Name 
        { 
            get => Name;
            set => Name = value;
        }
        private decimal price;
        public decimal Price
        {
            get => price;
            set
            {
                if (value <= 0) throw new ValueLessThanZero(nameof(Price)); 
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
            price = 0; 
        }

        public Product(string name, decimal price)
        {
            if (price <= 0) throw new ValueLessThanZero("Price");
            Name = name;
            this.price = price; 
                                
        }

        public override string ToString()
        {
            return $"{Name}, Price: {Price}";
        }

        
        public int CompareTo(object? obj)
        {
            if (obj == null) return 1;

            if (obj is not IName<Product> otherProduct)
            {
                throw new ArgumentException($"Object must be type {nameof(IName<Product>)}");
            }
            return StringComparer.OrdinalIgnoreCase.Compare(this.Name, otherProduct.Name);
        }

        
        public int CompareTo(Product? other)
        {
            if (other == null) return 1;
            
            int nameCompare = StringComparer.OrdinalIgnoreCase.Compare(this.Name, other.Name);
            if (nameCompare != 0)
            {
                return nameCompare;
            }
            return this.Price.CompareTo(other.Price);
        }


        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Price);
        }

        public static Product Deserialize(BinaryReader reader)
        {
            
            var product = new Product
            {
                Name = reader.ReadString()
            };
            
            product.price = reader.ReadDecimal();
            if (product.price <= 0) throw new ValueLessThanZero("Price", "read from file"); 
            return product;
        }

        public event EventHandler<decimal>? PriceChanged;
    }
}