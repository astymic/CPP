namespace lb_4.Classes
{
    class RealEstate : Product
    {
        public string Location { get; set; }
        public double Size { get; set; }
        public string Type { get; set; }

        public RealEstate()
        {
            Location = string.Empty;
            Size = 0;
            Type = string.Empty;
        }

        public RealEstate(string location, double size)
        {
            if (size <= 0) throw new ValueLessThanZero("Size");
            Location = location;
            Size = size;
            Type = string.Empty;
        }

        public RealEstate(string location, double size, string type)

        {
            if (size <= 0) throw new ValueLessThanZero("Size");
            Location = location;
            Size = size;
            Type = type;
        }

        public RealEstate(string name, decimal price, string location, double size, string type)
            : base(name, price)
        {
            if (size <= 0) throw new ValueLessThanZero("Size");
            Location = location;
            Size = size;
            Type = type;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Location: {Location}, Size: {Size}, Type: {Type}";
        }
    }
}
