
namespace LB_1
{
    class LB_1
    {
        const string GREEN = "\u001b[32m";
        const string RED = "\u001b[31m";
        const string RESET = "\u001b[0m";

        static string RemoveShortWords (string s)
        {
            List<string> words = new List<string>();
            foreach (string word in s.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (word.Length < 3)
                    words.Add(word);
            }
            return string.Join(" ", words);
        }

        static void Main()
        {
            while (true)
            {
                Console.WriteLine(
                    $"\n{GREEN}Menu:" +
                    $"\n1. Enter sentence" +
                    $"\n2. Exit\n{RESET}"
                    );
                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine();

                int[][] jagged;

                switch (choice)
                {
                    case "1":
                        Console.Write("\nEnter string: \n");
                        string input = Console.ReadLine();
                        string res = RemoveShortWords(input);
                        Console.WriteLine("\nResult: \n" + res + "\n"); 
                        break;

                    case "2":
                        return;

                    default:
                        Console.WriteLine(RED + "Try agian\n" + RESET);
                        continue;
                }
            }
        }
    }
}
