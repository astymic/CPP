using lb_11.Interfaces;

namespace lb_11.Classes
{
    class RealEstateInvestment : Product, IName<RealEstateInvestment>, ICustomSerializable
    {
        public string Location { get; set; }
        public decimal MarketValue { get; set; }
        public string InvestmentType { get; set; }

        public RealEstateInvestment()
        {
            Location = string.Empty;
            MarketValue = 0;
            InvestmentType = string.Empty;
        }

        public RealEstateInvestment(string location, decimal marketValue)
        {
            if (marketValue <= 0) throw new ValueLessThanZero("Market value");
            Location = location;
            MarketValue = marketValue;
            InvestmentType = string.Empty;
        }

        public RealEstateInvestment(string location, decimal marketValue, string investmentType)
        {
            if (marketValue <= 0) throw new ValueLessThanZero("Market value");
            Location = location;
            MarketValue = marketValue;
            InvestmentType = investmentType;
        }

        public RealEstateInvestment(string name, decimal price, string location, decimal marketValue, string investmentType)
            : base(name, price)
        {
            if (marketValue <= 0) throw new ValueLessThanZero("Market value");
            Location = location;
            MarketValue = marketValue;
            InvestmentType = investmentType;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Location: {Location}, Market Value: {MarketValue}, Invenstment Type: {InvestmentType}";
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Price);
            writer.Write(Location);
            writer.Write(MarketValue);
            writer.Write(InvestmentType);
        }

        public static RealEstateInvestment Deserialize(BinaryReader reader)
        {
            return new RealEstateInvestment
            {
                Name = reader.ReadString(),
                Price = reader.ReadDecimal(),
                Location = reader.ReadString(),
                MarketValue = reader.ReadDecimal(),
                InvestmentType = reader.ReadString()
            };
        }
    }
}
