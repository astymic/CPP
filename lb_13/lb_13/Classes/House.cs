using lb_13.Interfaces;

namespace lb_13.Classes
{
    class House : RealEstate, IName<House>
    {
        public double GardenSize { get; set; }
        public bool Pool { get; set; }

        public House() : base()
        {
            GardenSize = 0;
            Pool = false;
        }

        public House(double gardenSize, bool pool) : base()
        {
            if (gardenSize < 0) throw new ValueLessThanZero("Garden size");
            GardenSize = gardenSize;
            Pool = pool;
        }

        public House(string name, decimal price, string location, double size, string type, double gardenSize, bool pool)
            : base(name, price, location, size, type)
        {
            if (gardenSize < 0) throw new ValueLessThanZero("Garden size");
            GardenSize = gardenSize;
            Pool = pool;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Garden Size: {GardenSize}, {(Pool ? "There is" : "No")} Pool";
        }
    }
}