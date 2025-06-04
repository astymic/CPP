using System;
using System.Globalization;

namespace IRT.ConsoleOutput
{
    public static class InputReader
    {
        public static string ReadString(string prompt, bool allowNullOrEmpty = false)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(prompt);
            Console.ResetColor();
            string? input = Console.ReadLine();
            if (!allowNullOrEmpty && string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input cannot be empty.");
            }
            return input ?? "";
        }

        public static decimal ReadDecimal(string prompt, decimal? minValue = null, decimal? maxValue = null)
        {
            decimal value;
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(prompt);
                Console.ResetColor();
                string? input = Console.ReadLine();
                if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                {
                    bool minOk = minValue == null || value >= minValue.Value;
                    bool maxOk = maxValue == null || value <= maxValue.Value;

                    if (minOk && maxOk) return value;

                    string errorMsg = "Value must be";
                    if (minValue != null) errorMsg += $" >= {minValue.Value.ToString("N2", CultureInfo.InvariantCulture)}";
                    if (minValue != null && maxValue != null) errorMsg += " and";
                    if (maxValue != null) errorMsg += $" <= {maxValue.Value.ToString("N2", CultureInfo.InvariantCulture)}";
                    errorMsg += ".";
                    ConsoleUI.PrintErrorMessage(errorMsg);
                    if (!minOk && value < minValue!.Value) throw new ValueLessThanZero("Input value", $" (must be >= {minValue.Value.ToString("N2", CultureInfo.InvariantCulture)})");
                }
                else
                {
                    ConsoleUI.PrintErrorMessage($"Invalid decimal format. Please use '.' as the decimal separator (e.g., 123.45). Input was: '{input}'");
                    throw new FormatException($"Invalid decimal input: {input}");
                }
            }
        }

        public static double ReadDouble(string prompt, double? minValue = null, double? maxValue = null)
        {
            double value;
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(prompt);
                Console.ResetColor();
                string? input = Console.ReadLine();
                if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                {
                    bool minOk = minValue == null || value >= minValue.Value;
                    bool maxOk = maxValue == null || value <= maxValue.Value;

                    if (minOk && maxOk) return value;

                    string errorMsg = "Value must be";
                    if (minValue != null) errorMsg += $" >= {minValue.Value.ToString("N1", CultureInfo.InvariantCulture)}";
                    if (minValue != null && maxValue != null) errorMsg += " and";
                    if (maxValue != null) errorMsg += $" <= {maxValue.Value.ToString("N1", CultureInfo.InvariantCulture)}";
                    errorMsg += ".";
                    ConsoleUI.PrintErrorMessage(errorMsg);
                    if (!minOk && value < minValue!.Value) throw new ValueLessThanZero("Input value", $" (must be >= {minValue.Value.ToString("N1", CultureInfo.InvariantCulture)})");
                }
                else
                {
                    ConsoleUI.PrintErrorMessage($"Invalid number format. Please use '.' as the decimal separator (e.g., 12.3). Input was: '{input}'");
                    throw new FormatException($"Invalid double input: {input}");
                }
            }
        }

        public static int ReadInt(string prompt, int? minValue = null, int? maxValue = null)
        {
            int value;
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(prompt);
                Console.ResetColor();
                string? input = Console.ReadLine();
                if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                {
                    bool minOk = minValue == null || value >= minValue.Value;
                    bool maxOk = maxValue == null || value <= maxValue.Value;

                    if (minOk && maxOk) return value;

                    string errorMsg = "Value must be";
                    if (minValue != null) errorMsg += $" >= {minValue.Value.ToString(CultureInfo.InvariantCulture)}";
                    if (minValue != null && maxValue != null) errorMsg += " and";
                    if (maxValue != null) errorMsg += $" <= {maxValue.Value.ToString(CultureInfo.InvariantCulture)}";
                    errorMsg += ".";
                    ConsoleUI.PrintErrorMessage(errorMsg);
                    if (!minOk && value < minValue!.Value) throw new ValueLessThanZero("Input value", $" (must be >= {minValue.Value.ToString(CultureInfo.InvariantCulture)})");
                }
                else
                {
                    ConsoleUI.PrintErrorMessage($"Invalid integer format. Input was: '{input}'");
                    throw new FormatException($"Invalid integer input: {input}");
                }
            }
        }

        public static bool ReadBool(string prompt)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(prompt);
                Console.ResetColor();
                string input = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";
                if (input == "true" || input == "1" || input == "yes" || input == "y") return true;
                if (input == "false" || input == "0" || input == "no" || input == "n") return false;
                ConsoleUI.PrintErrorMessage("Invalid boolean input. Use true/false/yes/no/1/0.");
            }
        }
    }
}