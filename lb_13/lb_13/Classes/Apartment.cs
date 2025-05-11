using lb_13.Interfaces;
// Removed: using System.IO; for BinaryWriter/Reader

namespace lb_13.Classes
{
    // Removed ICustomSerializable
    class Apartment : RealEstate, IName<Apartment>
    {
        public int FloorNumber { get; set; }
        public decimal HOAFees { get; set; }

        public Apartment() : base() // Ensure base constructor is called
        {
            FloorNumber = 0; // Default values
            HOAFees = 0;
        }

        // Constructor for manual creation or testing, not strictly needed for JSON deserialization if properties have setters
        public Apartment(int floorNumber, decimal fees) : base()
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

        // Removed Serialize method
        // Removed static Deserialize method
    }
}