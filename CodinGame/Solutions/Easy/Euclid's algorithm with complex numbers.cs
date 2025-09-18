using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class Solution
{
    public static void Main()
    {
        var parts = Console.ReadLine().Split();
        var aR = int.Parse(parts[0]);
        var aI = int.Parse(parts[1]);
        parts = Console.ReadLine().Split();
        var bR = int.Parse(parts[0]);
        var bI = int.Parse(parts[1]);

        var origAR = aR;
        var origAI = aI;
        var origBR = bR;
        var origBI = bI;

        while (!(bR == 0 && bI == 0))
        {
            var denom = bR * (double)bR + bI * (double)bI;
            var x = (aR * (double)bR + aI * (double)bI) / denom;
            var y = (aI * (double)bR - aR * (double)bI) / denom;
            var qR = RoundNearest(x);
            var qI = RoundNearest(y);
            var rR = aR - (qR * bR - qI * bI);
            var rI = aI - (qR * bI + qI * bR);

            Console.WriteLine($"{PrintComplex(aR, aI)} = {PrintComplex(bR, bI)} * {PrintComplex(qR, qI)} + {PrintComplex(rR, rI)}");
            aR = bR; aI = bI;
            bR = rR; bI = rI;
        }

        Console.WriteLine($"GCD({PrintComplex(origAR, origAI)}, {PrintComplex(origBR, origBI)}) = {PrintComplex(aR, aI)}");
    }

    private static int RoundNearest(double v)
    {
        var f = Math.Floor(v);
        var frac = v - f;
        return frac > 0.5 ? (int)f + 1 : frac < 0.5 ? (int)f : (int)f + 1;
    }

    private static string PrintComplex(int re, int im)
    {
        if (re == 0 && im == 0)
            return "0j";
        if (re == 0)
            return im == 1 ? "1j" : im == -1 ? "-1j" : im + "j";
        if (im == 0)
            return $"({re}+0j)";
        var sign = im > 0 ? '+' : '-';
        var absI = Math.Abs(im);
        return $"({re}{sign}{absI}j)";
    }
}