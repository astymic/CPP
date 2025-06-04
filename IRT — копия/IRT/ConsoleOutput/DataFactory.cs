using IRT.Classes;
using IRT.Interfaces;
using System;

namespace IRT.ConsoleOutput
{
    public class DataFactory
    {
        private readonly Random _random = new Random();

        public void GenerateItemsAndAdd(Action<IName> addAction, int count)
        {
            for (int i = 0; i < count; i++)
            {
                IName newItem;
                int typeChoice = _random.Next(1, 9);
                try
                {
                    switch (typeChoice)
                    {
                        case 1: newItem = GenerateRandomProduct(); break;
                        case 2: newItem = GenerateRandomRealEstate(); break;
                        case 3: newItem = GenerateRandomRealEstateInvestment(); break;
                        case 4: newItem = GenerateRandomApartment(); break;
                        case 5: newItem = GenerateRandomHouse(); break;
                        case 6: newItem = GenerateRandomHotel(); break;
                        case 7: newItem = GenerateRandomLandPlot(); break;
                        case 8:
                            newItem = new RealEstate($"BaseRE{i}-{_random.Next(1000)}",
                                                     Math.Max(0.01m, Math.Round(_random.Next(5000, 20000) + (decimal)_random.NextDouble(), 2)),
                                                     $"Loc{i}",
                                                     Math.Max(1.0, Math.Round(_random.Next(50, 200) + _random.NextDouble(), 1)),
                                                     "Base");
                            break;
                        default: continue;
                    }
                    addAction(newItem);
                    Console.Write(".");
                }
                catch (ValueLessThanZero vlzEx)
                {
                    Console.Write("V");
                    System.Diagnostics.Debug.WriteLine($"\nGeneration Validation Error: {vlzEx.Message} for type {typeChoice}. Skipped.");
                }
                catch (Exception ex)
                {
                    Console.Write("X");
                    System.Diagnostics.Debug.WriteLine($"\nGeneration Critical Error: Failed to create item of type {typeChoice}. {ex.GetType().Name}: {ex.Message}");
                }
            }
            Console.WriteLine();
        }

        public IName? CreateItemManually()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Choose class to create:");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("1. Product");
            Console.WriteLine("2. RealEstate");
            Console.WriteLine("3. RealEstateInvestment");
            Console.WriteLine("4. Apartment");
            Console.WriteLine("5. House");
            Console.WriteLine("6. Hotel");
            Console.WriteLine("7. LandPlot");
            Console.ForegroundColor = ConsoleColor.Yellow;
            string classChoice = InputReader.ReadString("Enter choice: ");
            Console.ResetColor();

            try
            {
                return classChoice switch
                {
                    "1" => CreateManualProduct(),
                    "2" => CreateManualRealEstate(),
                    "3" => CreateManualRealEstateInvestment(),
                    "4" => CreateManualApartment(),
                    "5" => CreateManualHouse(),
                    "6" => CreateManualHotel(),
                    "7" => CreateManualLandPlot(),
                    _ => throw new ArgumentException("Invalid class choice.")
                };
            }
            catch (ValueLessThanZero ex) { ConsoleUI.PrintErrorMessage($"Creation Error: {ex.Message}"); return null; }
            catch (FormatException ex) { ConsoleUI.PrintErrorMessage($"Invalid input format during creation: {ex.Message}"); return null; }
            catch (ArgumentException ex) { ConsoleUI.PrintErrorMessage($"Invalid argument during creation: {ex.Message}"); return null; }
            catch (Exception ex) { ConsoleUI.PrintErrorMessage($"Unexpected error during creation: {ex.Message}"); return null; }
        }

        private Product GenerateRandomProduct()
        {
            string[] names = { "Table", "Chair", "Lamp", "Phone", "Book", "Laptop", "Mug", "Monitor", "Keyboard", "Mouse" };
            decimal price = _random.Next(10, 1000) + Math.Round((decimal)_random.NextDouble(), 2);
            return new Product(names[_random.Next(names.Length)] + "-" + _random.Next(100, 999), Math.Max(0.01m, price));
        }

        private RealEstate GenerateRandomRealEstate()
        {
            string[] names = { "Cozy Apt", "Luxury Villa", "Small House", "Big Mansion", "Downtown Loft", "Suburban Home" };
            string[] locations = { "New York", "London", "Paris", "Tokyo", "Kyiv", "Berlin", "Sydney", "Toronto", "Dubai" };
            string[] types = { "Residential", "Commercial", "Industrial", "Mixed-Use", "Land" };
            decimal price = _random.Next(100000, 1000000) + Math.Round((decimal)_random.NextDouble() * 1000, 2);
            double size = _random.Next(50, 500) + Math.Round(_random.NextDouble() * 10, 1);
            return new RealEstate(
                names[_random.Next(names.Length)] + "-" + _random.Next(10, 99),
                Math.Max(0.01m, price),
                locations[_random.Next(locations.Length)],
                Math.Max(1.0, size),
                types[_random.Next(types.Length)]);
        }

        private RealEstateInvestment GenerateRandomRealEstateInvestment()
        {
            string[] names = { "Office Bldg", "Shopping Mall", "Warehouse Complex", "Apt Complex Fund", "Data Center REIT" };
            string[] locations = { "Chicago", "Los Angeles", "Houston", "Phoenix", "Philadelphia", "Dallas", "Miami" };
            string[] invTypes = { "REIT", "Direct Property", "Mortgage Fund", "Syndication", "Crowdfunding" };
            decimal price = _random.Next(500000, 5000000) + Math.Round((decimal)_random.NextDouble() * 10000, 2);
            decimal marketValue = price * (decimal)(0.8 + _random.NextDouble() * 0.4);
            return new RealEstateInvestment(
                names[_random.Next(names.Length)],
                Math.Max(0.01m, price),
                locations[_random.Next(locations.Length)],
                Math.Max(1.0m, Math.Round(marketValue, 2)),
                invTypes[_random.Next(invTypes.Length)]);
        }

        private Apartment GenerateRandomApartment()
        {
            string[] names = { "Studio Apt", "1BR Condo", "2BR Luxury Apt", "Penthouse Suite", "Garden View Apt" };
            string[] locations = { "Miami Beach", "San Francisco Bay", "Seattle Downtown", "Boston Commons", "Denver LoDo", "Austin SoCo" };
            string[] types = { "Condominium", "Co-op Unit", "Rental Apartment", "Loft Style" };
            decimal price = _random.Next(200000, 800000) + Math.Round((decimal)_random.NextDouble() * 500, 2);
            double size = _random.Next(40, 150) + Math.Round(_random.NextDouble() * 5, 1);
            int floor = _random.Next(1, 30);
            decimal hoa = _random.Next(50, 500) + Math.Round((decimal)_random.NextDouble() * 50, 2);
            return new Apartment(
                names[_random.Next(names.Length)],
                Math.Max(0.01m, price),
                locations[_random.Next(locations.Length)],
                Math.Max(1.0, size),
                types[_random.Next(types.Length)],
                floor,
                Math.Max(0m, hoa));
        }

        private House GenerateRandomHouse()
        {
            string[] names = { "Bungalow Home", "Townhouse Unit", "Ranch Style House", "Cozy Cottage", "Colonial Estate" };
            string[] locations = { "Atlanta Suburbs", "Dallas North", "San Diego Hills", "Orlando Lakes", "Las Vegas Greens", "Nashville Scene" };
            string[] types = { "Single-family", "Multi-family Detached", "Duplex Attached" };
            decimal price = _random.Next(300000, 1200000) + Math.Round((decimal)_random.NextDouble() * 1000, 2);
            double size = _random.Next(100, 400) + Math.Round(_random.NextDouble() * 15, 1);
            double gardenSize = _random.Next(0, 1000) + Math.Round(_random.NextDouble() * 100, 1);
            bool pool = _random.Next(3) == 0;
            return new House(
                names[_random.Next(names.Length)],
                Math.Max(0.01m, price),
                locations[_random.Next(locations.Length)],
                Math.Max(1.0, size),
                types[_random.Next(types.Length)],
                gardenSize,
                pool);
        }

        private Hotel GenerateRandomHotel()
        {
            string[] names = { "Grand Luxury Hotel", "Budget Friendly Inn", "Seaside Resort & Spa", "Chic Boutique Hotel", "Airport Express Motel" };
            string[] locations = { "Hawaii Islands", "Bali Beaches", "Maldives Atolls", "Fiji Resorts", "Santorini Views", "Vegas Strip Central" };
            string[] invTypes = { "Hospitality REIT", "Hotel Management Co.", "Timeshare Property", "Franchise Brand" };
            decimal price = _random.Next(1000000, 10000000) + Math.Round((decimal)_random.NextDouble() * 50000, 2);
            decimal marketValue = price * (decimal)(0.9 + _random.NextDouble() * 0.3);
            int rooms = _random.Next(20, 500);
            int rating = _random.Next(1, 6);
            return new Hotel(
                names[_random.Next(names.Length)],
                Math.Max(0.01m, price),
                locations[_random.Next(locations.Length)],
                Math.Max(1.0m, Math.Round(marketValue, 2)),
                invTypes[_random.Next(invTypes.Length)],
                Math.Max(1, rooms),
                rating);
        }

        private LandPlot GenerateRandomLandPlot()
        {
            string[] names = { "Prime Farmland", "Dense Forest Tract", "Commercial Dev Land", "Residential Zoning Plot", "Waterfront Acreage" };
            string[] locations = { "Rural County Area", "Suburban Edge District", "Urban Infill Zone", "Coastal Region Strip", "Mountain Base Valley" };
            string[] invTypes = { "Land Banking", "Development Project", "Agricultural Use", "Conservation Trust" };
            string[] soilTypes = { "Loamy", "Clay Rich", "Sandy Loam", "Silty Clay", "Peaty Soil", "Chalky Ground" };
            decimal price = _random.Next(50000, 500000) + Math.Round((decimal)_random.NextDouble() * 2000, 2);
            decimal marketValue = price * (decimal)(0.7 + _random.NextDouble() * 0.6);
            bool infra = _random.Next(2) == 0;
            return new LandPlot(
                names[_random.Next(names.Length)],
                Math.Max(0.01m, price),
                locations[_random.Next(locations.Length)],
                Math.Max(1.0m, Math.Round(marketValue, 2)),
                invTypes[_random.Next(invTypes.Length)],
                soilTypes[_random.Next(soilTypes.Length)],
                infra);
        }

        private Product CreateManualProduct()
        {
            string name = InputReader.ReadString("Enter Product Name: ");
            decimal price = InputReader.ReadDecimal("Enter Product Price (> 0): ", minValue: 0.01m);
            return new Product(name, price);
        }

        private RealEstate CreateManualRealEstate()
        {
            string name = InputReader.ReadString("Enter RealEstate Name: ");
            decimal price = InputReader.ReadDecimal("Enter RealEstate Price (> 0): ", minValue: 0.01m);
            string location = InputReader.ReadString("Enter Location: ");
            double size = InputReader.ReadDouble("Enter Size (> 0 sq.units): ", minValue: 0.01);
            string type = InputReader.ReadString("Enter Type (e.g., Residential): ");
            return new RealEstate(name, price, location, size, type);
        }

        private RealEstateInvestment CreateManualRealEstateInvestment()
        {
            string name = InputReader.ReadString("Enter Investment Name: ");
            decimal price = InputReader.ReadDecimal("Enter Investment Price (> 0): ", minValue: 0.01m);
            string location = InputReader.ReadString("Enter Location: ");
            decimal marketValue = InputReader.ReadDecimal("Enter Market Value (> 0): ", minValue: 0.01m);
            string investmentType = InputReader.ReadString("Enter Investment Type (e.g., REIT): ");
            return new RealEstateInvestment(name, price, location, marketValue, investmentType);
        }

        private Apartment CreateManualApartment()
        {
            string name = InputReader.ReadString("Enter Apartment Name: ");
            decimal price = InputReader.ReadDecimal("Enter Apartment Price (> 0): ", minValue: 0.01m);
            string location = InputReader.ReadString("Enter Location: ");
            double size = InputReader.ReadDouble("Enter Size (> 0 sq.units): ", minValue: 0.01);
            string type = InputReader.ReadString("Enter Type (e.g., Condo): ");
            int floorNumber = InputReader.ReadInt("Enter Floor Number (> 0): ", minValue: 1);
            decimal hoaFees = InputReader.ReadDecimal("Enter HOA Fees (>= 0): ", minValue: 0m);
            return new Apartment(name, price, location, size, type, floorNumber, hoaFees);
        }

        private House CreateManualHouse()
        {
            string name = InputReader.ReadString("Enter House Name: ");
            decimal price = InputReader.ReadDecimal("Enter House Price (> 0): ", minValue: 0.01m);
            string location = InputReader.ReadString("Enter Location: ");
            double size = InputReader.ReadDouble("Enter Size (> 0 sq.units): ", minValue: 0.01);
            string type = InputReader.ReadString("Enter Type (e.g., Single-family): ");
            double gardenSize = InputReader.ReadDouble("Enter Garden Size (>= 0 sq.units): ", minValue: 0.0);
            bool pool = InputReader.ReadBool("Has Pool (true/false/yes/no/1/0): ");
            return new House(name, price, location, size, type, gardenSize, pool);
        }

        private Hotel CreateManualHotel()
        {
            string name = InputReader.ReadString("Enter Hotel Name: ");
            decimal price = InputReader.ReadDecimal("Enter Hotel Price (> 0): ", minValue: 0.01m);
            string location = InputReader.ReadString("Enter Location: ");
            decimal marketValue = InputReader.ReadDecimal("Enter Market Value (> 0): ", minValue: 0.01m);
            string investmentType = InputReader.ReadString("Enter Investment Type: ");
            int rooms = InputReader.ReadInt("Enter Number of Rooms (> 0): ", minValue: 1);
            int starRating = InputReader.ReadInt("Enter Star Rating (1-5): ", minValue: 1, maxValue: 5);
            return new Hotel(name, price, location, marketValue, investmentType, rooms, starRating);
        }

        private LandPlot CreateManualLandPlot()
        {
            string name = InputReader.ReadString("Enter LandPlot Name: ");
            decimal price = InputReader.ReadDecimal("Enter LandPlot Price (> 0): ", minValue: 0.01m);
            string location = InputReader.ReadString("Enter Location: ");
            decimal marketValue = InputReader.ReadDecimal("Enter Market Value (> 0): ", minValue: 0.01m);
            string investmentType = InputReader.ReadString("Enter Investment Type: ");
            string soilType = InputReader.ReadString("Enter Soil Type (e.g., Loam): ");
            bool infrastructureAccess = InputReader.ReadBool("Has Infrastructure Access (true/false/yes/no/1/0): ");
            return new LandPlot(name, price, location, marketValue, investmentType, soilType, infrastructureAccess);
        }
    }
}