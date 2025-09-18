using System;
using System.Collections.Generic;

public class Solution
{
    public static void Main(string[] args)
    {
        var line = Console.ReadLine();
        var parts = line.Split(' ');
        var l = long.Parse(parts[0]);
        var r = long.Parse(parts[1]);
        var luckyCounter = new LuckyCounter();
        var result = luckyCounter.Solve(r) - luckyCounter.Solve(l - 1);
        Console.WriteLine(result);
    }
}

public class LuckyCounter
{
    private static readonly long[,] _pre;

    static LuckyCounter()
    {
        _pre = new long[22, 3];
        Precompute();
    }

    public long Solve(long limit)
    {
        if (limit < 1)
        {
            return 0;
        }
        var digits = new List<int>();
        var tempLimit = limit;
        while (tempLimit > 0)
        {
            digits.Add((int)(tempLimit % 10));
            tempLimit /= 10;
        }

        long ans = 0;
        var has6 = false;
        var has8 = false;

        for (int i = digits.Count - 1; i >= 0; i--)
        {
            if (has6 && has8)
            {
                break;
            }

            var currentDigit = digits[i];
            for (int j = 0; j < currentDigit; j++)
            {
                if (j != 6 && j != 8)
                {
                    if (!has6 && !has8)
                    {
                        ans += _pre[i, 1] + _pre[i, 2];
                    }
                    else if (has6)
                    {
                        ans += _pre[i, 1] + _pre[i, 0];
                    }
                    else
                    {
                        ans += _pre[i, 2] + _pre[i, 0];
                    }
                }
                else if (j == 6 && !has8)
                {
                    ans += _pre[i, 0] + _pre[i, 1];
                }
                else if (j == 8 && !has6)
                {
                    ans += _pre[i, 0] + _pre[i, 2];
                }
            }

            if (currentDigit == 6)
            {
                has6 = true;
            }
            if (currentDigit == 8)
            {
                has8 = true;
            }
        }

        if (has6 ^ has8)
        {
            ans++;
        }
        return ans;
    }

    private static void Precompute()
    {
        _pre[0, 0] = 1;
        for (int i = 0; i < 20; i++)
        {
            _pre[i + 1, 0] += _pre[i, 0] * 8;
            _pre[i + 1, 1] += _pre[i, 0] + _pre[i, 1] * 9;
            _pre[i + 1, 2] += _pre[i, 0] + _pre[i, 2] * 9;
        }
    }
}