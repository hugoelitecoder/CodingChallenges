using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Solution
{
    public static void Main()
    {
        var watch = Stopwatch.StartNew();
        var line = Console.ReadLine();
        if (string.IsNullOrEmpty(line)) return;
        var n = int.Parse(line);
        var nodes = new int[n];
        for (var i = 0; i < n; i++)
        {
            var val = Console.ReadLine();
            nodes[i] = int.Parse(val);
        }
        var counter = new CircularLinkedListCounter(1000000007);
        var result = counter.CountDistinct(nodes);
        Console.WriteLine(result);
        watch.Stop();
        Console.Error.WriteLine($"[DEBUG] N: {n}");
        Console.Error.WriteLine($"[DEBUG] Calculation Time: {watch.ElapsedMilliseconds}ms");
    }
}

public class CircularLinkedListCounter
{
    private readonly long _MOD;
    private long[] _fact;
    private long[] _inv;
    public CircularLinkedListCounter(long mod)
    {
        _MOD = mod;
    }
    public long CountDistinct(int[] nodes)
    {
        var n = nodes.Length;
        if (n == 0) return 0;
        Array.Sort(nodes);
        var frequencies = GetFrequencies(nodes);
        PrecomputeMath(n);
        var g = GetGCD(frequencies);
        var totalSum = 0L;
        for (var d = 1; d * d <= g; d++)
        {
            if (g % d == 0)
            {
                totalSum = (totalSum + (CalculatePHI(d) * CalculateMultinomial(n / d, frequencies, d)) % _MOD) % _MOD;
                if (d * d != g)
                {
                    var d2 = g / d;
                    totalSum = (totalSum + (CalculatePHI(d2) * CalculateMultinomial(n / d2, frequencies, d2)) % _MOD) % _MOD;
                }
            }
        }
        return (totalSum * Power(n, _MOD - 2)) % _MOD;
    }
    private List<int> GetFrequencies(int[] nodes)
    {
        var list = new List<int>();
        var count = 1;
        for (var i = 1; i < nodes.Length; i++)
        {
            if (nodes[i] == nodes[i - 1])
            {
                count++;
            }
            else
            {
                list.Add(count);
                count = 1;
            }
        }
        list.Add(count);
        return list;
    }
    private void PrecomputeMath(int n)
    {
        _fact = new long[n + 1];
        _inv = new long[n + 1];
        _fact[0] = 1;
        for (var i = 1; i <= n; i++)
        {
            _fact[i] = (_fact[i - 1] * i) % _MOD;
        }
        _inv[n] = Power(_fact[n], _MOD - 2);
        for (var i = n - 1; i >= 0; i--)
        {
            _inv[i] = (_inv[i + 1] * (i + 1)) % _MOD;
        }
    }
    private long CalculateMultinomial(int nDivD, List<int> freqs, int d)
    {
        var res = _fact[nDivD];
        foreach (var f in freqs)
        {
            res = (res * _inv[f / d]) % _MOD;
        }
        return res;
    }
    private int GetGCD(List<int> freqs)
    {
        var res = freqs[0];
        foreach (var f in freqs)
        {
            res = ComputeGCD(res, f);
        }
        return res;
    }
    private int ComputeGCD(int a, int b)
    {
        while (b != 0)
        {
            a %= b;
            var t = a;
            a = b;
            b = t;
        }
        return a;
    }
    private long CalculatePHI(int n)
    {
        var res = n;
        for (var i = 2; i * i <= n; i++)
        {
            if (n % i == 0)
            {
                while (n % i == 0) n /= i;
                res -= res / i;
            }
        }
        if (n > 1) res -= res / n;
        return res;
    }
    private long Power(long b, long e)
    {
        var res = 1L;
        b %= _MOD;
        while (e > 0)
        {
            if (e % 2 == 1) res = (res * b) % _MOD;
            b = (b * b) % _MOD;
            e /= 2;
        }
        return res;
    }
}