using IRT.Interfaces;

namespace IRT.Classes
{
    class Hotel : RealEstateInvestment, IName<Hotel>, ICustomSerializable
    {
        public int Rooms { get; set; }
        public int StarRating { get; set; }

        public Hotel()
        {
            Rooms = 0;
            StarRating = 0;
        }

        public Hotel(int rooms, int starRating)
        {
            if (rooms <= 0) throw new ValueLessThanZero("Rooms");
            if (starRating <= 0 || starRating > 5) throw new ValueLessThanZero("Rating", "and not higher than 5");
            Rooms = rooms;
            StarRating = starRating;
        }

        public Hotel(string name, decimal price, string location, decimal marketValue, string investmentType, int rooms, int starRating)
            : base(name, price, location, marketValue, investmentType)
        {
            if (rooms <= 0) throw new ValueLessThanZero("Rooms");
            if (starRating <= 0 || starRating > 5) throw new ValueLessThanZero("Rating", "and not higher than 5");
            Rooms = rooms;
            StarRating = starRating;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, There are {Rooms} Rooms, Hotel Rating: {StarRating}";
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Price);
            writer.Write(Location);
            writer.Write(MarketValue);
            writer.Write(InvestmentType);
            writer.Write(Rooms);
            writer.Write(StarRating);
        }

        public static Hotel Deserialize(BinaryReader reader)
        {
            return new Hotel
            {
                Name = reader.ReadString(),
                Price = reader.ReadDecimal(),
                Location = reader.ReadString(),
                MarketValue = reader.ReadDecimal(),
                InvestmentType = reader.ReadString(),
                Rooms = reader.ReadInt32(),
                StarRating = reader.ReadInt32()
            };
        }
    }
}
