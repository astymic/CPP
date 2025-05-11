using lb_13.Interfaces;
using System; 

namespace lb_13.Classes
{
    class Product : IName, IName<Product>, IPrice
    {
        public string Name { get; set; }
        private decimal price;
        public decimal Price
        {
            get => price;
            set
            {
                if (value <= 0) throw new ValueLessThanZero("Price"); 

                if (this.price != value) 
                {
                    var oldPrice = this.price;
                    this.price = value;
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

            if (obj is not IName otherProduct)
            {
                throw new ArgumentException($"Object must be type {nameof(IName)}");
            }
            return StringComparer.OrdinalIgnoreCase.Compare(this.Name, otherProduct.Name);
        }

        public event EventHandler<decimal>? PriceChanged;
    }
}