using System;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        int k = int.Parse(Console.ReadLine());
        Console.WriteLine(CountDigitOccurrences(n, k));
    }

    static int CountDigitOccurrences(int n, int k)
    {
        int count = 0;
        int factor = 1;

        while (n / factor > 0)
        {
            int lower = n % factor;
            int current = (n / factor) % 10;
            int higher = n / (factor * 10);

            if (current < k)
                count += higher * factor;
            else if (current == k)
                count += higher * factor + lower + 1;
            else
                count += (higher + 1) * factor;

            factor *= 10;
        }

        return count;
    }
}
