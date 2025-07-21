using System;
using System.Collections.Generic;
using System.Text;

class Solution
{
    static void Main()
    {
        var p = Console.ReadLine().Split();
        int w = int.Parse(p[0]), h = int.Parse(p[1]);
        long cur = long.Parse(p[2]);
        int bound = (int)(Math.Sqrt(Math.Min(int.MaxValue, cur + (long)w * h)) + 1);
        var primes = Sieve(bound);
        bool dash = false;

        for (int i = 0; i < h; i++)
            PrintLine(w, ref cur, primes, ref dash);
    }

    static void PrintLine(int w, ref long cur, List<int> primes, ref bool dash)
    {
        if (dash)
        {
            Console.WriteLine(new string('-', w));
            return;
        }

        var sb = new StringBuilder(w);
        int used = 0;

        while (true)
        {
            string repr = cur + "=" + string.Join("*", Factorize(cur, primes));
            int need = repr.Length + (used > 0 ? 1 : 0);

            if (used == 0 && need > w)
            {
                dash = true;
                Console.WriteLine(new string('-', w));
                return;
            }
            if (used + need > w) break;

            if (used > 0) { sb.Append(','); used++; }
            sb.Append(repr);
            used += repr.Length;
            cur++;
        }

        if (used < w) sb.Append('-', w - used);
        Console.WriteLine(sb);
    }

    static List<int> Sieve(int n)
    {
        var c = new bool[n + 1];
        var p = new List<int>();
        for (int i = 2; i <= n; i++)
            if (!c[i])
            {
                p.Add(i);
                for (long j = (long)i * i; j <= n; j += i)
                    c[j] = true;
            }
        return p;
    }

    static List<long> Factorize(long m, List<int> primes)
    {
        var f = new List<long>();
        foreach (int pr in primes)
        {
            if ((long)pr * pr > m) break;
            while (m % pr == 0)
            {
                f.Add(pr);
                m /= pr;
            }
        }
        if (m > 1) f.Add(m);
        return f;
    }
}