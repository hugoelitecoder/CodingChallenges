using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var rc = Console.ReadLine().Split();
        int R = int.Parse(rc[0]), C = int.Parse(rc[1]);
        var grid = new int[R][];
        for (int i = 0; i < R; i++)
        {
            var row = Console.ReadLine().Split();
            grid[i] = new int[C];
            for (int j = 0; j < C; j++)
                grid[i][j] = row[j][0] - '0';
        }
        int maxLen = Math.Max(R, C);
        int maxNum = 1;
        for (int i = 0; i < maxLen; i++) maxNum *= 10;
        int bound = (int)Math.Sqrt(maxNum) + 1;
        var primes = Sieve(bound);
        var found = new HashSet<int>();
        for (int i = 0; i < R; i++)
            for (int a = 0; a < C; a++)
            {
                int num = 0;
                for (int b = a; b < C; b++)
                {
                    num = num * 10 + grid[i][b];
                    if (IsPrime(num, primes))
                        found.Add(num);
                }
            }
        for (int j = 0; j < C; j++)
            for (int a = 0; a < R; a++)
            {
                int num = 0;
                for (int b = a; b < R; b++)
                {
                    num = num * 10 + grid[b][j];
                    if (IsPrime(num, primes))
                        found.Add(num);
                }
            }
        Console.WriteLine(found.Count);
    }

    static List<int> Sieve(int n)
    {
        var sieve = new bool[n + 1];
        var list = new List<int>();
        for (int i = 2; i <= n; i++)
            if (!sieve[i])
            {
                list.Add(i);
                for (int j = i * i; j <= n; j += i)
                    sieve[j] = true;
            }
        return list;
    }

    static bool IsPrime(int x, List<int> primes)
    {
        if (x < 2) return false;
        foreach (int p in primes)
        {
            if ((long)p * p > x) break;
            if (x % p == 0) return x == p;
        }
        return true;
    }
}
