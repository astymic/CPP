namespace lb_4.Classes
{
    class Apartment : RealEstate
    {
        public int FloorNumber { get; set; }
        public decimal HOAFees { get; set; }

        public Apartment()
        {
            FloorNumber = 0;
            HOAFees = 0;
        }

        public Apartment(int floorNumber, decimal fees)
        {
            if (floorNumber <= 0) throw new ValueLessThanZero("Floor number");
            if (fees < 0) throw new ValueLessThanZero("Fee");
            FloorNumber = floorNumber;
            HOAFees = fees;
        }
        public Apartment(string name, decimal price, string location, double size, string type, int floorNumber, decimal fees)
            : base(name, price, location, size, type)
        {
            if (floorNumber <= 0) throw new ValueLessThanZero("Floor number");
            if (fees < 0) throw new ValueLessThanZero("Fee");
            FloorNumber = floorNumber;
            HOAFees = fees;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, In {FloorNumber} Floor, Homeowners Association Fee: {HOAFees}";
        }
    }
}
