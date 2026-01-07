using System;
using System.Collections.Generic;
using System.Diagnostics;
class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var inputN = Console.ReadLine();
        if (inputN == null) return;
        var n = int.Parse(inputN);
        var table = new int[n];
        var exists = new bool[81];
        for (var i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            if (line == null) continue;
            var parts = line.Split(' ');
            var id = Encode(parts);
            table[i] = id;
            exists[id] = true;
            Console.Error.WriteLine($"[DEBUG] Input: {line} -> ID: {id}");
        }
        var foundSet = false;
        for (var i = 0; i < n; i++)
        {
            for (var j = i + 1; j < n; j++)
            {
                if (exists[GetThird(table[i], table[j])])
                {
                    foundSet = true;
                    break;
                }
            }
            if (foundSet) break;
        }
        if (foundSet)
        {
            Console.WriteLine("1.0000");
        }
        else
        {
            var win = new bool[81];
            var count = 0;
            for (var i = 0; i < n; i++)
            {
                for (var j = i + 1; j < n; j++)
                {
                    var target = GetThird(table[i], table[j]);
                    if (!win[target])
                    {
                        win[target] = true;
                        count++;
                    }
                }
            }
            var deck = 81 - n;
            var prob = (double)count / deck;
            Console.Error.WriteLine($"[DEBUG] Possible completing cards: {count}");
            Console.Error.WriteLine($"[DEBUG] Remaining cards: {deck}");
            Console.WriteLine(prob.ToString("F4"));
        }
        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] Time: {sw.ElapsedMilliseconds}ms");
    }
    private static int Encode(string[] p)
    {
        var v1 = int.Parse(p[0]) - 1;
        var v2 = p[1] switch { "OUTLINED" => 0, "STRIPED" => 1, "SOLID" => 2, _ => 0 };
        var v3 = p[2] switch { "RED" => 0, "GREEN" => 1, "PURPLE" => 2, _ => 0 };
        var v4 = p[3] switch { "DIAMOND" => 0, "OVAL" => 1, "SQUIGGLE" => 2, _ => 0 };
        return v1 * 27 + v2 * 9 + v3 * 3 + v4;
    }
    private static int GetThird(int a, int b)
    {
        var res = 0;
        var m = 1;
        for (var i = 0; i < 4; i++)
        {
            var c1 = a % 3;
            var c2 = b % 3;
            var c3 = (c1 == c2) ? c1 : (3 - (c1 + c2));
            res += c3 * m;
            a /= 3;
            b /= 3;
            m *= 3;
        }
        return res;
    }
}