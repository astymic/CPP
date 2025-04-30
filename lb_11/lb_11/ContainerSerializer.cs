using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lb_11.Interfaces;
using lb_11.Classes;
using System.Reflection.PortableExecutable;

namespace lb_11;

public static class ContainerSerializer
{
    public static string SerializeContainer(object container)
    {
        using FileStream stream = new FileStream("data.bin", FileMode.Create);
        using BinaryWriter writer = new BinaryWriter(stream);

        int count;
        IEnumerable<IName> _container;
        
        switch (container)
        {
            case Container<IName> array:
                count = array.GetCount();
                _container = array;
                break;
            case ContainerLinkedList<IName> linkedList:
                count = linkedList.GetCount();
                _container = linkedList;
                break;
            default:
                throw new ArgumentException("Container is None. Please select a container.");
        }

        writer.Write(count);
        
        foreach (var item in _container)
        {
            Type type = item.GetType();
            
            switch (type.Name)
            {
                case "Product":
                    var product = (Product)item;
                    writer.Write(product.Name);
                    writer.Write(product.Price);
                    break;

                case "RealEstate":
                    var realEstate = (RealEstate)item;
                    writer.Write(realEstate.Name);
                    writer.Write(realEstate.Price);
                    writer.Write(realEstate.Location);
                    writer.Write(realEstate.Size);
                    writer.Write(realEstate.Type);
                    break;

                case "RealEstateInvestment":
                    var realEstateInvestment = (RealEstateInvestment)item;
                    writer.Write(realEstateInvestment.Name);
                    writer.Write(realEstateInvestment.Price);
                    writer.Write(realEstateInvestment.Location);
                    writer.Write(realEstateInvestment.MarketValue);
                    writer.Write(realEstateInvestment.InvestmentType);
                    break;

                case "Apartment":
                    var apartment = (Apartment)item;
                    writer.Write(apartment.Name);
                    writer.Write(apartment.Price);
                    writer.Write(apartment.Location);
                    writer.Write(apartment.Size);
                    writer.Write(apartment.Type);
                    writer.Write(apartment.FloorNumber);
                    writer.Write(apartment.HOAFees);
                    break;

                case "House":
                    var house = (House)item;
                    writer.Write(house.Name);
                    writer.Write(house.Price);
                    writer.Write(house.Location);
                    writer.Write(house.Size);
                    writer.Write(house.Type);
                    writer.Write(house.GardenSize);
                    writer.Write(house.Pool);
                    break;

                case "Hotel":
                    var hotel = (Hotel)item;
                    writer.Write(hotel.Name);
                    writer.Write(hotel.Price);
                    writer.Write(hotel.Location);
                    writer.Write(hotel.MarketValue);
                    writer.Write(hotel.InvestmentType);
                    writer.Write(hotel.Rooms);
                    writer.Write(hotel.StarRating);
                    break;

                case "LandPlot":
                    var landPlot = (LandPlot)item;
                    writer.Write(landPlot.Name);
                    writer.Write(landPlot.Price);
                    writer.Write(landPlot.Location);
                    writer.Write(landPlot.MarketValue);
                    writer.Write(landPlot.InvestmentType);
                    writer.Write(landPlot.SoilType);
                    writer.Write(landPlot.InfrastructureAccess);
                    break;

                default:
                    break;
            }
        }

        stream.Close();
        return stream.Name;
    }

    public static IEnumerable<IName?> DeserializeContainer(byte[] data)
    {
        using FileStream stream = new FileStream("data.bin", FileMode.Open);
        using BinaryReader reader = new BinaryReader(stream);

        int count = reader.ReadInt32();

        var container = new Container<IName>();

        for (int i = 0; i < count; i++)
        {
            string type = reader.ReadString();
            
            switch( type )
            {
                case "Product":
                    var product = new Product();
                    product.Name = reader.ReadString();
                    product.Price = reader.ReadDecimal();
                    container.Add(product);
                    break;
                    
                case "RealEstate":
                    var realEstate = new RealEstate();
                    realEstate.Name = reader.ReadString();
                    realEstate.Price = reader.ReadDecimal();
                    realEstate.Location = reader.ReadString();
                    realEstate.Size = reader.ReadDouble();
                    realEstate.Type = reader.ReadString();
                    container.Add(realEstate);
                    break;
                    
                case "RealEstateInvestment":
                    var realEstateInvestment = new RealEstateInvestment();
                    realEstateInvestment.Name = reader.ReadString();
                    realEstateInvestment.Price = reader.ReadDecimal();
                    realEstateInvestment.Location = reader.ReadString();
                    realEstateInvestment.MarketValue = reader.ReadDecimal();
                    realEstateInvestment.InvestmentType = reader.ReadString();
                    container.Add(realEstateInvestment);
                    break;
                    
                case "Apartment":
                    var apartment = new Apartment();
                    apartment.Name = reader.ReadString();
                    apartment.Price = reader.ReadDecimal();
                    apartment.Location = reader.ReadString();
                    apartment.Size = reader.ReadDouble();
                    apartment.Type = reader.ReadString();
                    apartment.FloorNumber = reader.ReadInt32();
                    apartment.HOAFees = reader.ReadDecimal();
                    container.Add(apartment);
                    break;

                case "House":
                    var house = new House();
                    house.Name = reader.ReadString();
                    house.Price = reader.ReadDecimal();
                    house.Location = reader.ReadString();
                    house.Size = reader.ReadDouble();
                    house.Type = reader.ReadString();
                    house.GardenSize = reader.ReadDouble();
                    house.Pool = reader.ReadBoolean();
                    container.Add(house);
                    break;

                case "Hotel":
                    var hotel = new Hotel();
                    hotel.Name = reader.ReadString();
                    hotel.Price = reader.ReadDecimal();
                    hotel.Location = reader.ReadString();
                    hotel.MarketValue = reader.ReadDecimal();
                    hotel.InvestmentType = reader.ReadString();
                    hotel.Rooms = reader.ReadInt16();
                    hotel.StarRating = reader.ReadInt16();
                    container.Add(hotel);
                    break;

                case "LandPlot":
                    var landPlot = new LandPlot();
                    landPlot.Name = reader.ReadString();
                    landPlot.Price = reader.ReadDecimal();
                    landPlot.Location = reader.ReadString();
                    landPlot.MarketValue = reader.ReadDecimal();
                    landPlot.InvestmentType = reader.ReadString();
                    landPlot.SoilType = reader.ReadString();
                    landPlot.InfrastructureAccess = reader.ReadBoolean();
                    container.Add(landPlot);
                    break;

                default:
                    container.Add(new Product());
                    break;
            }
        }

        return container;
    }
}
