using lb_12.Interfaces;

namespace lb_12.Classes
{
    class House : RealEstate, IName<House>, ICustomSerializable
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

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Price);
            writer.Write(Location);
            writer.Write(Size);
            writer.Write(Type);
            writer.Write(GardenSize);
            writer.Write(Pool);
        }

        public static House Deserialize(BinaryReader reader)
        {
            return new House
            {
                Name = reader.ReadString(),
                Price = reader.ReadDecimal(),
                Location = reader.ReadString(),
                Size = reader.ReadDouble(),
                Type = reader.ReadString(),
                GardenSize = reader.ReadDouble(),
                Pool = reader.ReadBoolean()
            };
        }
    }
}
