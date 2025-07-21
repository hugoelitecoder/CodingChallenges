using System;
using System.Collections.Generic;
using System.Linq;

public class Solution
{
    public static void Main(string[] args)
    {
        var x = long.Parse(Console.ReadLine());
        var c = int.Parse(Console.ReadLine());
        var solver = new PrimesSolver();
        for (var i = 0; i < c; i++)
        {
            var line = Console.ReadLine().Split();
            var a = long.Parse(line[0]);
            var b = long.Parse(line[1]);
            solver.AddConstraint(a, b);
        }
        var n = solver.Decode(x);
        Console.WriteLine(n);
    }
}

public class PrimesSolver
{
    private readonly Dictionary<long, HashSet<long>> _k = new Dictionary<long, HashSet<long>>();
    private readonly HashSet<long> _rm = new HashSet<long>();

    public void AddConstraint(long a, long b)
    {
        var ka = Factorize(a);
        var kb = Factorize(b);
        var nk = new Dictionary<long, HashSet<long>>();
        foreach (var pairA in ka)
        {
            foreach (var pairB in kb)
            {
                if (pairA.Value == pairB.Value)
                {
                    if (!nk.ContainsKey(pairA.Key))
                    {
                        nk[pairA.Key] = new HashSet<long>();
                    }
                    nk[pairA.Key].Add(pairB.Key);
                }
            }
        }

        var nkKeys = nk.Keys.ToList();
        foreach (var en in nkKeys)
        {
            var currentPossibilities = nk[en];
            if (!_k.ContainsKey(en))
            {
                var newSet = new HashSet<long>(currentPossibilities);
                newSet.ExceptWith(_rm);
                _k[en] = newSet;
            }
            else
            {
                var tempSet = new HashSet<long>(currentPossibilities);
                tempSet.ExceptWith(_rm);
                _k[en].IntersectWith(tempSet);
            }

            if (_k.TryGetValue(en, out var possibilities) && possibilities.Count == 1)
            {
                var determined = possibilities.First();
                if (_rm.Add(determined))
                {
                    var kKeys = _k.Keys.ToList();
                    foreach (var e in kKeys)
                    {
                        if (e != en)
                        {
                            _k[e].Remove(determined);
                        }
                    }
                }
            }
        }
    }

    public long Decode(long x)
    {
        var n = 1L;
        var i = 2L;
        while (x > 1)
        {
            if (i * i > x)
            {
                n *= _k[x].First();
                break;
            }
            if (x % i == 0)
            {
                n *= _k[i].First();
                x /= i;
            }
            else
            {
                i++;
            }
        }
        return n;
    }
    
    private static Dictionary<long, int> Factorize(long n)
    {
        var factors = new Dictionary<long, int>();
        var i = 2L;
        while (n > 1)
        {
            if (i * i > n)
            {
                if (!factors.ContainsKey(n))
                {
                    factors[n] = 0;
                }
                factors[n]++;
                break;
            }
            while (n % i == 0)
            {
                if (!factors.ContainsKey(i))
                {
                    factors[i] = 0;
                }
                factors[i]++;
                n /= i;
            }
            i++;
        }
        return factors;
    }
}