namespace lb_6.Classes
{
    class Product
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
    }
}
