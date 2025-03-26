

namespace LB_2
{
    class LB_2
    {
        const string GREEN = "\u001b[32m";
        const string RED = "\u001b[31m";
        const string CYAN = "\u001b[36m";
        const string YELLOW = "\u001b[33m";
        const string RESET = "\u001b[0m";

        static Random rand = new Random();
        
        static bool IsZeroRow(int[] row)
        {
            foreach (int num in row)
            {
                if (num != 0)
                    return false;
            }
            return true;
        }

        static int[][] DeleteZeroRows(int[][] array)
        {
            int count = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (!IsZeroRow(array[i]))
                {
                    count++;
                }
            }

            int[][] jagged = new int[count][];
            int index = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (!IsZeroRow(array[i]))
                {
                    jagged[index] = array[i];
                    index++;
                }
            }
            return jagged;
        }

        static int Sum(int[] row)
        {
            int sum = 0;
            foreach (int x in row)
                sum += x;
            return sum;
        }
        static void PrintArray(int[][] array, string title)
        {
            Console.WriteLine(RESET + $"\n{title}:");
            for (int i = 0; i < array.Length; i++)
            {
                Console.Write($"Row {i + 1}{YELLOW}:{RESET} ");
                if (array[i].Length == 0)
                    Console.WriteLine(RED + "(empty)" + RESET);
                else if (IsZeroRow(array[i]))
                    Console.WriteLine(RED + string.Join(" ", array[i]).PadRight(25) + $" {CYAN}[Sum: {Sum(array[i])}]" + RESET);
                else
                    Console.WriteLine(string.Join(" ", array[i]).PadRight(25) + $" {CYAN}[Sum: {Sum(array[i])}]" + RESET);
            }
        }

        static int[][] ManualInput()
        {
            Console.Write($"Enter the number of rows:{RESET} ");
            int rows = int.Parse(Console.ReadLine());
            int[][] arr = new int[rows][];

            for (int i = 0; i < rows; i++)
            {
                Console.Write($"{GREEN}Enter the number of elements in {CYAN}Row {i + 1}{GREEN}:{RESET} ");
                int columns = int.Parse(Console.ReadLine());
                arr[i] = new int[columns];

                Console.Write($"{GREEN}Enter {CYAN}{columns} {GREEN}elements: {RESET}");
                string[] elements = Console.ReadLine().Split(' ');
                for (int j = 0; j < columns; j++)
                {
                    arr[i][j] = int.Parse(elements[j]);
                }
            }
            return arr;
        }

        static void Main()
        {
            while (true)
            {
                Console.WriteLine(
                    $"\n{GREEN}Menu:" +
                    $"\n1. Random array" +
                    $"\n2. Manualy input array" +
                    $"\n3. Exit\n"
                    );
                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine();

                int[][] jagged;

                switch (choice)
                {
                    case "1":
                        int rows = rand.Next(1, 7);
                        jagged = new int[rows][];
                        for (int i = 0; i < rows; i++)
                        {
                            int len = rand.Next(0, 7);
                            jagged[i] = new int[len];
                            for (int j = 0; j < len; j++) 
                                jagged[i][j] = rand.Next(-7, 7);
                        }
                        break;

                    case "2":
                        jagged = ManualInput();
                        break;

                    case "3":
                        return;

                    default:
                        Console.WriteLine(RED + "Try agian\n" + RESET);
                        continue;
                }

                PrintArray(jagged, "Original array");

                jagged = DeleteZeroRows(jagged);
                PrintArray(jagged, "\nArray after deleting zero rows");

                for (int i = 0; i < jagged.Length - 1; i++)
                {
                    for (int j = i + 1; j < jagged.Length; j++)
                    {
                        if (Sum(jagged[i]) > Sum(jagged[j]))
                        {
                            (jagged[i], jagged[j]) = (jagged[j], jagged[i]);
                        }
                    }
                }
                PrintArray(jagged, "\nSorted array in ascending order");
            }
        }
    }
}