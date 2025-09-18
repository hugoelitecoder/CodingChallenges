using System;
using System.Globalization;
using System.Linq;

class Solution
{
    static readonly int[] Denoms = new int[] { 5000, 2000, 1000, 500, 200, 100, 50, 20, 10, 5, 2, 1 };

    static void Main()
    {
        string init = Console.ReadLine().Trim();
        var parts = init.Split('+');
        int[] counts = new int[Denoms.Length];
        bool[] jam = new bool[Denoms.Length];
        foreach (var part in parts)
        {
            var t = part.Split('X');
            string c = t[0];
            int denom = ToCents(t[1]);
            int idx = Array.IndexOf(Denoms, denom);
            if (c.Contains("J")) jam[idx] = true;
            string numStr = new string(c.TakeWhile(char.IsDigit).ToArray());
            counts[idx] = numStr.Length > 0 ? int.Parse(numStr) : 0;
        }

        int n = int.Parse(Console.ReadLine());
        bool error = false;
        for (int i = 0; i < n; i++)
        {
            var inp = Console.ReadLine().Split();
            decimal bill = decimal.Parse(inp[0], CultureInfo.InvariantCulture);
            string givenNotation = inp[1];

            int billC = (int)(bill * 100);
            var givenCounts = ParseNotation(givenNotation);
            int givenSum = 0;
            for (int d = 0; d < Denoms.Length; d++)
            {
                givenSum += givenCounts[d] * Denoms[d];
                counts[d] += givenCounts[d];
            }
            int change = givenSum - billC;
            if (change == 0)
            {
                Console.WriteLine("0");
                continue;
            }
            var use = new int[Denoms.Length];
            int rem = change;
            for (int d = 0; d < Denoms.Length; d++)
            {
                int can = Math.Min(rem / Denoms[d], counts[d]);
                if (can > 0)
                {
                    if (jam[d]) { Console.WriteLine("ERROR: JAM"); error = true; break; }
                    use[d] = can;
                    rem -= can * Denoms[d];
                }
            }
            if (error) break;
            if (rem > 0)
            {
                Console.WriteLine("ERROR: OUT OF MONEY");
                break;
            }
            for (int d = 0; d < Denoms.Length; d++) counts[d] -= use[d];

            bool first = true;
            for (int d = 0; d < Denoms.Length; d++)
            {
                if (use[d] > 0)
                {
                    if (!first) Console.Write("+"); first = false;
                    Console.Write(use[d] + "X" + FormatDenom(Denoms[d]));
                }
            }
            Console.WriteLine();
        }
    }

    static int ToCents(string s)
    {
        decimal v = decimal.Parse(s, CultureInfo.InvariantCulture);
        return (int)(v * 100);
    }

    static int[] ParseNotation(string note)
    {
        var arr = new int[Denoms.Length];
        foreach (var part in note.Split('+'))
        {
            var t = part.Split('X');
            string c = t[0];
            int denom = ToCents(t[1]);
            int idx = Array.IndexOf(Denoms, denom);
            string numStr = new string(c.TakeWhile(char.IsDigit).ToArray());
            arr[idx] = numStr.Length > 0 ? int.Parse(numStr) : 0;
        }
        return arr;
    }

    static string FormatDenom(int cents)
    {
        if (cents % 100 == 0) return (cents / 100).ToString();
        decimal v = cents / 100m;
        return v.ToString("0.00", CultureInfo.InvariantCulture);
    }
}