using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int valueToReach = int.Parse(Console.ReadLine().Trim());
        int N = int.Parse(Console.ReadLine().Trim());

        int[] counts = Console.ReadLine()
                              .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                              .Select(int.Parse)
                              .ToArray();
        int[] values = Console.ReadLine()
                              .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                              .Select(int.Parse)
                              .ToArray();

        var coins = new List<int>();
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < counts[i]; j++)
                coins.Add(values[i]);
        }

        long totalSum = coins.Sum(v => (long)v);
        if (totalSum < valueToReach)
        {
            Console.WriteLine("-1");
            return;
        }
        coins.Sort();
        long running = 0;
        for (int k = 1; k <= coins.Count; k++)
        {
            running += coins[k - 1];
            if (running >= valueToReach)
            {
                Console.WriteLine(k);
                return;
            }
        }
        Console.WriteLine("-1");
    }
}
