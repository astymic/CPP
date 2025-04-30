using lb_11.Interfaces;

namespace lb_11.Classes
{
    class LandPlot : RealEstateInvestment, IName<LandPlot>, ICustomSerializable
    {
        public string SoilType { get; set; }
        public bool InfrastructureAccess { get; set; }

        public LandPlot()
        {
            SoilType = string.Empty;
            InfrastructureAccess = true;
        }

        public LandPlot(string soilType, bool infrastructureAccess)
        {
            SoilType = soilType;
            InfrastructureAccess = infrastructureAccess;
        }

        public LandPlot(string name, decimal price, string location, decimal marketValue, string investmentType, string soilType, bool infrastructureAccess)
            : base(name, price, location, marketValue, investmentType)
        {
            SoilType = soilType;
            InfrastructureAccess = infrastructureAccess;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Soil Type: {SoilType}, {(InfrastructureAccess ? "Have" : "No")} Access to Infrastructure";
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Price);
            writer.Write(Location);
            writer.Write(MarketValue);
            writer.Write(InvestmentType);
            writer.Write(SoilType);
            writer.Write(InfrastructureAccess);
        }
    }
}
