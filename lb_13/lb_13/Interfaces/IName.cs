using System.Text.Json.Serialization; 
using lb_13.Classes;

namespace lb_13.Interfaces
{
    [JsonDerivedType(typeof(Product), "product")]
    [JsonDerivedType(typeof(RealEstate), "real_estate")]
    [JsonDerivedType(typeof(RealEstateInvestment), "real_estate_investment")]
    [JsonDerivedType(typeof(Apartment), "apartment")]
    [JsonDerivedType(typeof(House), "house")]
    [JsonDerivedType(typeof(Hotel), "hotel")]
    [JsonDerivedType(typeof(LandPlot), "land_plot")]
    public interface IName : IComparable
    {
        string Name { get; set; }
        decimal Price { get; set; } 

        int CompareTo(object? obj);
    }

    public interface IName<T>
    {
        //string Name { get; set; }
    }
}