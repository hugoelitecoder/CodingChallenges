using System;
using System.Text;
using System.Globalization;
class Solution
{
    public static void Main(string[] args)
    {
        var shipStr = Console.ReadLine();
        var wormStr = Console.ReadLine();
        var ship = ParseVector(shipStr);
        var worm = ParseVector(wormStr);
        var rawDx = worm.i - ship.i;
        var rawDy = worm.j - ship.j;
        var rawDz = worm.k - ship.k;
        var gcd = Gcd(Math.Abs(rawDx), Math.Abs(rawDy), Math.Abs(rawDz));
        var dx = rawDx;
        var dy = rawDy;
        var dz = rawDz;
        if (gcd > 1) { dx /= gcd; dy /= gcd; dz /= gcd; }
        var dirVec = FormatVector(dx, dy, dz);
        if (dirVec == "") dirVec = "0";
        Console.WriteLine("Direction: " + dirVec);
        var distRaw = Math.Sqrt(rawDx * rawDx + rawDy * rawDy + rawDz * rawDz);
        var dist = Math.Round(distRaw, 2);
        Console.WriteLine("Distance: " + dist.ToString("F2", CultureInfo.InvariantCulture));
    }
    private static (int i, int j, int k) ParseVector(string s)
    {
        var str = s.Replace(" ", "");
        if (str.Length > 0 && str[0] != '+' && str[0] != '-') str = "+" + str;
        var i = 0; var j = 0; var k = 0; var idx = 0;
        while (idx < str.Length)
        {
            var sign = str[idx] == '-' ? -1 : 1; idx++;
            var numStart = idx;
            while (idx < str.Length && char.IsDigit(str[idx])) idx++;
            var numStr = str.Substring(numStart, idx - numStart);
            var num = numStr == "" ? 1 : int.Parse(numStr);
            var axis = str[idx++]; var val = sign * num;
            switch (axis) { case 'i': i += val; break; case 'j': j += val; break; case 'k': k += val; break; }
        }
        return (i, j, k);
    }
    private static int Gcd(int a, int b)
    {
        while (b != 0) { var t = b; b = a % b; a = t; }
        return a;
    }
    private static int Gcd(int a, int b, int c) => Gcd(Gcd(a, b), c);
    private static string FormatVector(int di, int dj, int dk)
    {
        var sb = new StringBuilder(); var first = true;
        foreach (var (comp, axis) in new[] { (di, 'i'), (dj, 'j'), (dk, 'k') })
        {
            if (comp == 0) continue;
            var sign = comp < 0 ? '-' : '+'; var mag = comp < 0 ? -comp : comp;
            var term = (mag == 1 ? "" : mag.ToString()) + axis;
            if (first) { if (sign == '-') sb.Append(sign); sb.Append(term); first = false; }
            else { sb.Append(sign); sb.Append(term); }
        }
        return sb.ToString();
    }
}
