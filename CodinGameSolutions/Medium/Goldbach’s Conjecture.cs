using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        var ms = Enumerable
                    .Range(0, int.Parse(Console.ReadLine()))
                    .Select(_ => int.Parse(Console.ReadLine()))
                    .ToArray();
        int maxM = ms.Max();

        var isPrime = new bool[maxM + 1];
        for (int i = 2; i <= maxM; i++) isPrime[i] = true;
        for (int p = 2; p * p <= maxM; p++)
            if (isPrime[p])
                for (int j = p * p; j <= maxM; j += p)
                    isPrime[j] = false;

        var primes = Enumerable.Range(2, maxM - 1)
                               .Where(i => isPrime[i])
                               .ToArray();

        var results = ms
            .Select(m => primes
                .TakeWhile(p => p <= m / 2)
                .Count(p => isPrime[m - p])
            );

        Console.WriteLine(string.Join(Environment.NewLine, results));
    }
}
