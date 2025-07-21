using System;
using System.Numerics;
using System.Linq;

class Program
{
    static void Main()
    {
        int T = int.Parse(Console.ReadLine()!);
        var queries = Enumerable.Range(0, T)
                                .Select(_ => int.Parse(Console.ReadLine()!))
                                .ToArray();

        int maxN = queries.Max();
        var dp = new BigInteger[maxN + 1];

        dp[0] = 1;
        for (int part = 1; part <= maxN; part++) {
            for (int total = part; total <= maxN; total++) {
                dp[total] += dp[total - part];
            }
        }

        Console.WriteLine(string.Join(Environment.NewLine, queries.Select(n => dp[n])));
    }

}
