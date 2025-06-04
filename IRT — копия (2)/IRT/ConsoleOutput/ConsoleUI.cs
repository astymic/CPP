using System;
using System.IO;
using System.Reflection;

namespace IRT.ConsoleOutput
{
    public class ConsoleUI
    {
        private readonly ContainerManager _containerManager;
        private readonly OperationHandlers _operationHandlers;
        private readonly DisplayManager _displayManager;

        public ConsoleUI(ContainerManager containerManager, OperationHandlers operationHandlers, DisplayManager displayManager)
        {
            _containerManager = containerManager;
            _operationHandlers = operationHandlers;
            _displayManager = displayManager;
        }

        public void Run()
        {
            while (true)
            {
                PrintMenu();
                string choice = InputReader.ReadString("Enter your choice: ", true).ToLowerInvariant();

                try
                {
                    switch (choice)
                    {
                        case "1": _operationHandlers.HandleAutomaticGeneration(); break;
                        case "2": _operationHandlers.HandleManualInput(); break;
                        case "3": _operationHandlers.HandleShowContainer(); break;
                        case "4": _operationHandlers.HandleGetElementByInsertionId(); break;
                        case "5": _operationHandlers.HandleGetElementByName(); break;
                        case "6": _operationHandlers.HandleChangeItemByInsertionId(); break;
                        case "7": _operationHandlers.HandleChangeItemByName(); break;
                        case "8": _operationHandlers.HandleSortContainer(); break;
                        case "9": _operationHandlers.HandleRemoveElementByIndex(); break;
                        case "10": _operationHandlers.HandleReverseGenerator(); break;
                        case "11": _operationHandlers.HandleSublineGenerator(); break;
                        case "12": _operationHandlers.HandleSortedPriceGenerator(); break;
                        case "13": _operationHandlers.HandleSortedNameGenerator(); break;
                        case "14": _operationHandlers.HandleFindFirstElement(); break;
                        case "15": _operationHandlers.HandleFindAllElements(); break;
                        case "16": _operationHandlers.HandleSerializeContainer(); break;
                        case "17": _operationHandlers.HandleDeserializeContainer(); break;
                        case "18": _operationHandlers.HandleShowTotalPrice(); break;
                        case "19": _operationHandlers.HandleFindMinMaxProduct(); break;
                        case "20": _operationHandlers.HandleFindAverageCategoriesPrice(); break;
                        case "q":
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Exiting...");
                            Console.ResetColor();
                            return;
                        default:
                            PrintErrorMessage("Invalid choice. Please try again.");
                            break;
                    }
                }
                catch (ValueLessThanZero ex) { PrintErrorMessage($"Input/Validation Error: {ex.Message}"); }
                catch (FormatException ex) { PrintErrorMessage($"Input Format Error: Invalid format entered. {ex.Message}"); }
                catch (IndexOutOfRangeException ex) { PrintErrorMessage($"Error: Index out of range. {ex.Message}"); }
                catch (KeyNotFoundException ex) { PrintErrorMessage($"Error: Key (e.g., Insertion ID) not found. {ex.Message}"); }
                catch (FileNotFoundException ex) { PrintErrorMessage($"File Error: {ex.Message}"); }
                catch (IOException ex) { PrintErrorMessage($"File IO Error: {ex.Message}"); }
                catch (InvalidOperationException ex) { PrintErrorMessage($"Operation Error: {ex.Message}"); }
                catch (ArgumentException ex) { PrintErrorMessage($"Argument Error: {ex.Message}"); }
                catch (TargetInvocationException ex)
                {
                    Exception inner = ex.InnerException ?? ex;
                    while (inner.InnerException != null) { inner = inner.InnerException; }
                    PrintErrorMessage($"Error during operation: {inner.GetType().Name} - {inner.Message}");
                }
                catch (Exception ex)
                {
                    PrintErrorMessage($"An unexpected error occurred: {ex.GetType().Name} - {ex.Message}");
                }
                finally
                {
                    Console.ResetColor();
                }
            }
        }

        private void PrintMenu()
        {
            string menuTitle = "------ Menu ------";
            int menuWidth = 60;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{_displayManager.CenterString(menuTitle, menuWidth)}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(_displayManager.CenterString($"Active Container: {_containerManager.ActiveContainerType}", menuWidth));

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("1. Automatic Generation (Select/Switch Container)");
            Console.WriteLine("2. Manual Input (Select/Switch Container)");
            Console.WriteLine("3. Show Active Container");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("#. --- Getters ---");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("4. Get Element by Insertion ID (1-based)");
            Console.WriteLine("5. Get Elements by Name");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("#. --- Modifiers ---");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("6. Change Item by Insertion ID (1-based)");
            Console.WriteLine("7. Change Item by Name");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("#. --- Container Operations ---");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("8. Sort Active Container (In-Place, using Comparison delegate)");
            Console.WriteLine("9. Remove Element by Current Index (0-based)");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("#. --- Generators (Non-Mutating Iterators) ---");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("10. Show Elements in Reverse Order");
            Console.WriteLine("11. Show Elements with Name Containing Substring");
            Console.WriteLine("12. Show Elements Sorted by Price (Generator)");
            Console.WriteLine("13. Show Elements Sorted by Name (Generator)");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("#. --- Find Operations (using Predicate) ---");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("14. Find First Element by Criteria");
            Console.WriteLine("15. Find All Elements by Criteria");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("#. --- File Operations ---");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("16. Serialize Active Container to File");
            Console.WriteLine("17. Deserialize Container from File (Replaces Active)");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("#. --- Summary Information ---");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("18. Show Total Price of Items in Active Container");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("#. --- Min/Max/Average ---");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("19. Show Product with Minimum and Maximum Price");
            Console.WriteLine("20. Show Average Price for each Category");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("#. --- Exit ---");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("q. Exit");
            Console.ResetColor();
        }

        public static void PrintErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nERROR: {message}");
            Console.ResetColor();
        }
    }
}