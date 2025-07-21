using System;
using System.Linq;
using System.Collections.Generic;


class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var balancer = new CentrifugeBalancer(n);
        var m = balancer.CountPossibleKValues();
        Console.WriteLine(m);
    }
}

public class CentrifugeBalancer
{
    private readonly int _n;
    private readonly bool[] _isSumOfPrimeFactors;

    public CentrifugeBalancer(int n)
    {
        _n = n;
        var primeFactors = GetDistinctPrimeFactors(n);
        _isSumOfPrimeFactors = GenerateSemigroup(n, primeFactors);
    }

    public int CountPossibleKValues()
    {
        var count = 0;
        for (var k = 1; k <= _n; k++)
        {
            if (_isSumOfPrimeFactors[k] && _isSumOfPrimeFactors[_n - k])
            {
                count++;
            }
        }
        return count;
    }

    private static List<int> GetDistinctPrimeFactors(int number)
    {
        var factors = new HashSet<int>();
        var temp = number;
        for (var i = 2; i * i <= temp; i++)
        {
            if (temp % i == 0)
            {
                factors.Add(i);
                while (temp % i == 0)
                {
                    temp /= i;
                }
            }
        }
        if (temp > 1)
        {
            factors.Add(temp);
        }
        return factors.ToList();
    }

    private static bool[] GenerateSemigroup(int limit, List<int> generators)
    {
        var isPossible = new bool[limit + 1];
        isPossible[0] = true;
        foreach (var p in generators)
        {
            for (var i = p; i <= limit; i++)
            {
                isPossible[i] = isPossible[i] || isPossible[i - p];
            }
        }
        return isPossible;
    }
}
