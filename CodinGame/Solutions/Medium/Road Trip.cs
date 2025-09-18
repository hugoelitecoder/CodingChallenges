using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        var tokens = Console.ReadLine().Split();
        int N = int.Parse(tokens[0]);
        int C = int.Parse(tokens[1]);
        int P = int.Parse(tokens[2]);

        var friends = Enumerable.Range(0, N)
                                .Select(_ => Console.ReadLine().Split()
                                                 .Select(int.Parse)
                                                 .ToArray())
                                .Select(arr => (budget: arr[0], joy: arr[1]))
                                .ToArray();

        long bestH = 0;
        for (int x = 1; x <= N; x++)
        {
            double price = P + C / (double)(x + 1);
            var joys = friends
                       .Where(f => f.budget >= price)
                       .Select(f => f.joy)
                       .OrderByDescending(j => j)
                       .Take(x)
                       .ToArray();
            if (joys.Length == x)
            {
                long sumJoy = joys.Sum();
                if (sumJoy > bestH)
                    bestH = sumJoy;
            }
        }

        Console.WriteLine(bestH);
    }
}