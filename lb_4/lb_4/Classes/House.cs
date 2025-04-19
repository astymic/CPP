using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lb_4.Classes
{
    class House : RealEstate
    {
        public double GardenSize { get; set; }
        public bool Pool { get; set; }

        public House()
        {
            GardenSize = 0;
            Pool = false;
        }

        public House(double gardenSize, bool pool)
        {
            if (GardenSize < 0) throw new ValueLessThanZero("Garden size");
            GardenSize = gardenSize;
            Pool = pool;
        }

        public House(string name, decimal price, string location, double size, string type, double gardenSize, bool pool)
            : base(name, price, location, size, type)
        {
            if (GardenSize < 0) throw new ValueLessThanZero("Garden size");
            GardenSize = gardenSize;
            Pool = pool;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Garden Size: {GardenSize}, {(Pool ? "There is" : "No")} Pool";
        }
    }
}
