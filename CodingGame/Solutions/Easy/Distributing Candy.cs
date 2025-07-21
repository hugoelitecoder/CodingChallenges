using System;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        var nm = Console.ReadLine().Split(' ');
        int n = int.Parse(nm[0]);
        int m = int.Parse(nm[1]);
        var candies = Console.ReadLine()
                            .Split(' ')
                            .Select(int.Parse)
                            .OrderBy(x => x)
                            .ToArray();

        int minUnfairness = int.MaxValue;
        for (int i = 0; i + m - 1 < n; i++)
        {
            int diff = candies[i + m - 1] - candies[i];
            if (diff < minUnfairness)
                minUnfairness = diff;
        }

        Console.WriteLine(minUnfairness);
    }
}
