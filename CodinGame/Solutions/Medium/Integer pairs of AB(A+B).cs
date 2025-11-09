using System;
using System.Collections.Generic;
using System.Diagnostics;

class Solution
{
    public static void Main(string[] args)
    {
        var n_str = Console.ReadLine();
        var n = long.Parse(n_str);
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var result = Solve(n);
        
        stopwatch.Stop();

        Console.WriteLine(result);
        
        Console.Error.WriteLine($"[DEBUG] Input N: {n}");
        Console.Error.WriteLine($"[DEBUG] Calculation time: {stopwatch.ElapsedMilliseconds} ms");
    }

    private static long Solve(long n)
    {
        var primeFactors = GetPrimeFactorization(n);
        
        long divisorsOfNSquared = 1;
        foreach (var exponent in primeFactors.Values)
        {
            divisorsOfNSquared *= (2L * exponent + 1);
        }

        return 2 * divisorsOfNSquared - 1;
    }

    private static Dictionary<long, int> GetPrimeFactorization(long n)
    {
        var factors = new Dictionary<long, int>();
        if (n <= 1)
        {
            return factors;
        }

        var count = 0;
        while (n % 2 == 0)
        {
            count++;
            n /= 2;
        }
        if (count > 0)
        {
            factors[2] = count;
        }

        for (long i = 3; i * i <= n; i += 2)
        {
            count = 0;
            while (n % i == 0)
            {
                count++;
                n /= i;
            }
            if (count > 0)
            {
                factors[i] = count;
            }
        }
        
        if (n > 1)
        {
            factors[n] = 1;
        }
        
        return factors;
    }
}

