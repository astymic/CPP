using lb_11.Interfaces;

namespace lb_11.Classes
{
    class Apartment : RealEstate, IName<Apartment>, ICustomSerializable
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

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Price);
            writer.Write(Location);
            writer.Write(Size);
            writer.Write(Type);
            writer.Write(FloorNumber);
            writer.Write(HOAFees);
        }

        public static Apartment Deserialize(BinaryReader reader)
        {
            return new Apartment
            {
                Name = reader.ReadString(),
                Price = reader.ReadDecimal(),
                Location = reader.ReadString(),
                Size = reader.ReadDouble(),
                Type = reader.ReadString(),
                FloorNumber = reader.ReadInt32(),
                HOAFees = reader.ReadDecimal()
            };
        }
    }
}
