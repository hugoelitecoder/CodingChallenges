using System;
using System.Collections.Generic;

class Solution
{
    const int MOD = 1_000_000_007;

    static void Main()
    {
        var parts = Console.ReadLine().Split();
        int A = int.Parse(parts[0]), B = int.Parse(parts[1]);

        var primes = Sieve((int)Math.Sqrt(A));
        var factors = Factorize(A, primes);

        long answer = long.MaxValue;
        foreach (var (p, e) in factors)
        {
            long count = CountExponentInFactorial(B, p);
            answer = Math.Min(answer, count / e);
        }

        Console.WriteLine(answer == long.MaxValue ? 0 : answer);
    }

    static List<int> Sieve(int limit)
    {
        var isComp = new bool[limit + 1];
        var list   = new List<int>();
        for (int i = 2; i <= limit; i++)
        {
            if (!isComp[i])
            {
                list.Add(i);
                for (long j = (long)i * i; j <= limit; j += i)
                    isComp[j] = true;
            }
        }
        return list;
    }

    static Dictionary<int,int> Factorize(int n, List<int> primes)
    {
        var dict = new Dictionary<int,int>();
        int tmp = n;
        foreach (int p in primes)
        {
            if (p * p > tmp) break;
            while (tmp % p == 0)
            {
                if (!dict.ContainsKey(p)) dict[p] = 0;
                dict[p]++;
                tmp /= p;
            }
        }
        if (tmp > 1) dict[tmp] = dict.GetValueOrDefault(tmp) + 1;
        return dict;
    }

    static long CountExponentInFactorial(int n, int p)
    {
        long sum = 0;
        for (long v = p; v <= n; v *= p)
            sum += n / v;
        return sum;
    }
}
