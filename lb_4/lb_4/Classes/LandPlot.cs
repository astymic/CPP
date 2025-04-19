namespace lb_4.Classes
{
    class LandPlot : RealEstateInvestment
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
    }
}
