using IRT.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace IRT.ConsoleOutput
{
    public class DisplayManager
    {

        private const int idWidth = 6;
        private const int classWidth = 20;
        private const int nameWidth = 20;
        private const int priceWidth = 16;
        private const int locationWidth = 20;
        private const int sizeWidth = 10;
        private const int typeWidth = 18;
        private const int marketValueWidth = 18;
        private const int investmentTypeWidth = 20;
        private const int floorWidth = 7;
        private const int hoaWidth = 10;
        private const int gardenWidth = 12;
        private const int poolWidth = 6;
        private const int roomsWidth = 7;
        private const int starWidth = 6;
        private const int soilWidth = 12;
        private const int infraWidth = 7;
        private const int numColumns = 17;


        private const int avgPriceCategoryColWidth = 25;
        private const int avgPriceValueColWidth = 20;


        private readonly int generalTableWidth;
        private readonly int averagePriceTableWidth;


        public DisplayManager()
        {
            generalTableWidth = CalculateGeneralTableWidth();
            averagePriceTableWidth = avgPriceCategoryColWidth + avgPriceValueColWidth + 3;
        }

        private int CalculateGeneralTableWidth()
        {
            int totalDataWidth = idWidth + classWidth + nameWidth + priceWidth + locationWidth +
                                 sizeWidth + typeWidth + marketValueWidth + investmentTypeWidth +
                                 floorWidth + hoaWidth + gardenWidth + poolWidth + roomsWidth +
                                 starWidth + soilWidth + infraWidth;
            return totalDataWidth + numColumns + 1;
        }

        public void PrintGeneralTableHeader()
        {
            DrawHorizontalLine(generalTableWidth);
            WriteGeneralHeaderRow();
            DrawHorizontalLine(generalTableWidth);
        }

        private void WriteGeneralHeaderRow()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"|{PadAndCenter("ID", idWidth)}");
            Console.Write($"|{PadAndCenter("Class", classWidth)}");
            Console.Write($"|{PadAndCenter("Name", nameWidth)}");
            Console.Write($"|{PadAndCenter("Price", priceWidth)}");
            Console.Write($"|{PadAndCenter("Location", locationWidth)}");
            Console.Write($"|{PadAndCenter("Size (sq.unit)", sizeWidth)}");
            Console.Write($"|{PadAndCenter("RE Type", typeWidth)}");
            Console.Write($"|{PadAndCenter("Mkt Value", marketValueWidth)}");
            Console.Write($"|{PadAndCenter("Invest Type", investmentTypeWidth)}");
            Console.Write($"|{PadAndCenter("Floor", floorWidth)}");
            Console.Write($"|{PadAndCenter("HOA Fee", hoaWidth)}");
            Console.Write($"|{PadAndCenter("Garden (sq.u)", gardenWidth)}");
            Console.Write($"|{PadAndCenter("Pool", poolWidth)}");
            Console.Write($"|{PadAndCenter("Rooms", roomsWidth)}");
            Console.Write($"|{PadAndCenter("Stars", starWidth)}");
            Console.Write($"|{PadAndCenter("Soil Type", soilWidth)}");
            Console.Write($"|{PadAndCenter("Infra.", infraWidth)}");
            Console.WriteLine("|");
            Console.ResetColor();
        }

        public void WriteGeneralDataRow(int displayId, object item)
        {
            if (item == null)
            {
                Console.WriteLine($"|{PadAndCenter(displayId.ToString(), idWidth)}|{PadAndCenter("(Null Item)", generalTableWidth - idWidth - 3)}|");
                return;
            }

            string FormatDecimal(decimal? d) => d?.ToString("N2", CultureInfo.InvariantCulture) ?? "-";
            string FormatDouble(double? d) => d?.ToString("N1", CultureInfo.InvariantCulture) ?? "-";
            string FormatBool(bool? b) => b.HasValue ? b.Value ? "Yes" : "No" : "-";
            string FormatInt(int? i) => i?.ToString() ?? "-";
            string FormatString(string? s) => string.IsNullOrWhiteSpace(s) ? "-" : s;

            Type itemType = item.GetType();

            string name = FormatString(ReflectionHelper.GetPropertyValue<string>(item, "Name"));
            string fPrice = FormatDecimal(ReflectionHelper.GetPropertyValue<decimal?>(item, "Price"));
            string loc = FormatString(ReflectionHelper.GetPropertyValue<string>(item, "Location"));
            string fSize = FormatDouble(ReflectionHelper.GetPropertyValue<double?>(item, "Size"));
            string reType = FormatString(ReflectionHelper.GetPropertyValue<string>(item, "Type"));
            string fMktVal = FormatDecimal(ReflectionHelper.GetPropertyValue<decimal?>(item, "MarketValue"));
            string invType = FormatString(ReflectionHelper.GetPropertyValue<string>(item, "InvestmentType"));
            string fFloor = FormatInt(ReflectionHelper.GetPropertyValue<int?>(item, "FloorNumber"));
            string fHoa = FormatDecimal(ReflectionHelper.GetPropertyValue<decimal?>(item, "HOAFees"));
            string fGarden = FormatDouble(ReflectionHelper.GetPropertyValue<double?>(item, "GardenSize"));
            string fPool = FormatBool(ReflectionHelper.GetPropertyValue<bool?>(item, "Pool"));
            string fRooms = FormatInt(ReflectionHelper.GetPropertyValue<int?>(item, "Rooms"));
            string fStar = FormatInt(ReflectionHelper.GetPropertyValue<int?>(item, "StarRating"));
            string soil = FormatString(ReflectionHelper.GetPropertyValue<string>(item, "SoilType"));
            string fInfra = FormatBool(ReflectionHelper.GetPropertyValue<bool?>(item, "InfrastructureAccess"));

            Console.Write($"|{PadAndCenter(displayId.ToString(), idWidth)}");
            Console.Write($"|{PadAndCenter(itemType.Name, classWidth)}");
            Console.Write($"|{PadAndCenter(name, nameWidth)}");
            Console.Write($"|{PadAndCenter(fPrice, priceWidth)}");
            Console.Write($"|{PadAndCenter(loc, locationWidth)}");
            Console.Write($"|{PadAndCenter(fSize, sizeWidth)}");
            Console.Write($"|{PadAndCenter(reType, typeWidth)}");
            Console.Write($"|{PadAndCenter(fMktVal, marketValueWidth)}");
            Console.Write($"|{PadAndCenter(invType, investmentTypeWidth)}");
            Console.Write($"|{PadAndCenter(fFloor, floorWidth)}");
            Console.Write($"|{PadAndCenter(fHoa, hoaWidth)}");
            Console.Write($"|{PadAndCenter(fGarden, gardenWidth)}");
            Console.Write($"|{PadAndCenter(fPool, poolWidth)}");
            Console.Write($"|{PadAndCenter(fRooms, roomsWidth)}");
            Console.Write($"|{PadAndCenter(fStar, starWidth)}");
            Console.Write($"|{PadAndCenter(soil, soilWidth)}");
            Console.Write($"|{PadAndCenter(fInfra, infraWidth)}");
            Console.WriteLine("|");
        }

        public void DisplayItemTable(int displayId, IName item)
        {
            if (item == null)
            {
                ConsoleUI.PrintErrorMessage("Item to display is null.");
                return;
            }
            PrintGeneralTableHeader();
            WriteGeneralDataRow(displayId, item);
            DrawHorizontalLine(generalTableWidth);
        }

        public void DisplayCollectionInTable(IEnumerable<IName> items, string title)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(CenterString(title, generalTableWidth));
            Console.ResetColor();

            PrintGeneralTableHeader();
            int displayId = 1;
            int itemCount = 0;
            foreach (var item in items)
            {
                WriteGeneralDataRow(displayId++, item);
                DrawHorizontalLine(generalTableWidth);
                itemCount++;
            }
            if (itemCount == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"|{PadAndCenter("(No items to display)", generalTableWidth - 2)}|");
                DrawHorizontalLine(generalTableWidth);
                Console.ResetColor();
            }
        }

        public void PrintAveragePriceTableHeader()
        {
            DrawHorizontalLine(averagePriceTableWidth);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"|{PadAndCenter("Category", avgPriceCategoryColWidth)}");
            Console.Write($"|{PadAndCenter("Average Price", avgPriceValueColWidth)}");
            Console.WriteLine("|");
            Console.ResetColor();
            DrawHorizontalLine(averagePriceTableWidth);
        }

        public void WriteAveragePriceDataRow(string category, decimal avgPrice)
        {
            Console.Write($"|{PadAndCenter(category, avgPriceCategoryColWidth)}");
            Console.Write($"|{PadAndCenter(avgPrice.ToString("N2", CultureInfo.InvariantCulture), avgPriceValueColWidth)}");
            Console.WriteLine("|");
        }

        public void DisplayAveragePrices(IEnumerable<dynamic> averagePrices, string title)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(CenterString(title, averagePriceTableWidth));
            Console.ResetColor();
            PrintAveragePriceTableHeader();

            bool hasData = false;
            if (averagePrices != null)
            {
                foreach (var item in averagePrices)
                {
                    WriteAveragePriceDataRow(item.Category, item.AveragePrice);
                    DrawHorizontalLine(averagePriceTableWidth);
                    hasData = true;
                }
            }

            if (!hasData)
            {
                Console.WriteLine($"|{PadAndCenter("(No categories found)", averagePriceTableWidth - 2)}|");
                DrawHorizontalLine(averagePriceTableWidth);
            }
        }


        public void DrawHorizontalLine(int tableWidth)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string('-', tableWidth));
            Console.ResetColor();
        }

        private string PadAndCenter(string? value, int totalWidth)
        {
            string val = value ?? "";
            if (totalWidth <= 0) return "";
            val = Truncate(val, totalWidth);
            int spaces = totalWidth - val.Length;
            int padLeft = spaces / 2 + val.Length;
            return val.PadLeft(padLeft).PadRight(totalWidth);
        }

        public string CenterString(string s, int width)
        {
            if (string.IsNullOrEmpty(s) || width <= 0) return new string(' ', Math.Max(0, width));
            s = Truncate(s, width);
            int padding = Math.Max(0, (width - s.Length) / 2);
            return new string(' ', padding) + s + new string(' ', Math.Max(0, width - s.Length - padding));
        }

        private string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (maxLength <= 0) return "";
            if (value.Length <= maxLength) return value;
            if (maxLength < 3) return new string('.', maxLength);
            return value.Substring(0, maxLength - 3) + "...";
        }
    }
}