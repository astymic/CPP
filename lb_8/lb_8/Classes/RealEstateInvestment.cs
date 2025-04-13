using lb_8.Interfaces;

namespace lb_8.Classes
{
    class RealEstateInvestment : Product, IName<RealEstateInvestment>
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
    }
}
