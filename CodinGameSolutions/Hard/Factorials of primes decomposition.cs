using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var n = Console.ReadLine();
        var decomposer = new FactorialDecomposer(2000);
        var result = decomposer.Decompose(n);
        Console.WriteLine(result);
    }
}

class FactorialDecomposer
{
    private readonly List<int> _primes;
    public FactorialDecomposer(int maxPrime)
    {
        _primes = Sieve(maxPrime);
    }
    public string Decompose(string nStr)
    {
        var exponents = GetPrimeFactorizationOfN(nStr);
        var result = new SortedDictionary<int, int>(Comparer<int>.Create((a, b) => b.CompareTo(a)));
        for (var i = _primes.Count - 1; i >= 0; i--)
        {
            var p = _primes[i];
            var coeff = 0;
            if (exponents.ContainsKey(p))
            {
                coeff = exponents[p];
            }
            if (coeff == 0)
            {
                continue;
            }
            result[p] = coeff;
            for (var j = 0; j <= i; j++)
            {
                var q = _primes[j];
                var expInFact = GetExponentInFactorial(p, q);
                if (expInFact > 0)
                {
                    if (!exponents.ContainsKey(q))
                    {
                        exponents[q] = 0;
                    }
                    exponents[q] -= coeff * expInFact;
                }
            }
        }
        return string.Join(" ", result.Select(kvp => $"{kvp.Key}#{kvp.Value}"));
    }
    private Dictionary<int, int> GetPrimeFactorizationOfN(string nStr)
    {
        var map = new Dictionary<int, int>();
        if (nStr.Contains("/"))
        {
            var parts = nStr.Split('/');
            var num = int.Parse(parts[0]);
            var den = int.Parse(parts[1]);
            AddPrimeFactorsToMap(map, num, 1);
            AddPrimeFactorsToMap(map, den, -1);
        }
        else
        {
            var number = int.Parse(nStr);
            AddPrimeFactorsToMap(map, number, 1);
        }
        return map;
    }
    private void AddPrimeFactorsToMap(Dictionary<int, int> map, int number, int sign)
    {
        var tempNum = number;
        foreach (var p in _primes)
        {
            if ((long)p * p > tempNum)
            {
                break;
            }
            if (tempNum % p == 0)
            {
                while (tempNum % p == 0)
                {
                    if (!map.ContainsKey(p))
                    {
                        map[p] = 0;
                    }
                    map[p] += sign;
                    tempNum /= p;
                }
            }
        }
        if (tempNum > 1)
        {
            if (!map.ContainsKey(tempNum))
            {
                map[tempNum] = 0;
            }
            map[tempNum] += sign;
        }
    }
    private int GetExponentInFactorial(int n, int p)
    {
        var exponent = 0;
        var powerOfP = (long)p;
        while (powerOfP <= n)
        {
            exponent += (int)(n / powerOfP);
            if (p > long.MaxValue / powerOfP)
            {
                break;
            }
            powerOfP *= p;
        }
        return exponent;
    }
    private static List<int> Sieve(int limit)
    {
        var primes = new List<int>();
        var isPrime = new bool[limit + 1];
        for (var i = 2; i <= limit; i++)
        {
            isPrime[i] = true;
        }
        for (var p = 2; (long)p * p <= limit; p++)
        {
            if (isPrime[p])
            {
                for (var i = p * p; i <= limit; i += p)
                {
                    isPrime[i] = false;
                }
            }
        }
        for (var p = 2; p <= limit; p++)
        {
            if (isPrime[p])
            {
                primes.Add(p);
            }
        }
        return primes;
    }
}
