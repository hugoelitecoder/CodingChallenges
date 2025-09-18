using System;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        int fullWidth = 4 * N - 1;

        for (int i = 1; i <= N; i++)
        {
            int stars = 2 * i - 1;
            int indent = (fullWidth - stars) / 2;
            if (i == 1)
            {
                Console.Write('.');
                Console.Write(new string(' ', indent - 1));
                Console.WriteLine(new string('*', stars));
            }
            else
            {
                Console.Write(new string(' ', indent));
                Console.WriteLine(new string('*', stars));
            }
        }

        for (int i = 1; i <= N; i++)
        {
            int stars = 2 * i - 1;
            int indent = (2 * N - 1 - stars) / 2;
            int midSpaces = fullWidth - 2 * (indent + stars);
            Console.Write(new string(' ', indent));
            Console.Write(new string('*', stars));
            Console.Write(new string(' ', midSpaces));
            Console.WriteLine(new string('*', stars));
        }
    }
}
