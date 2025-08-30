using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Solution
{
    public static void Main(string[] args)
    {
        var formula = ReadFormula();
        var sw = Stopwatch.StartNew();
        var result = OrganicCompoundNamer.GetName(formula);
        sw.Stop();
        WriteResult(result);
        LogDebug(formula, sw.Elapsed);
    }

    private static string ReadFormula()
    {
        return Console.ReadLine();
    }

    private static void WriteResult(string result)
    {
        Console.WriteLine(result);
    }

    private static void LogDebug(string formula, TimeSpan time)
    {
        Console.Error.WriteLine($"[DEBUG] Input: {formula}");
        Console.Error.WriteLine($"[DEBUG] Calculation time: {time.TotalMilliseconds:F4} ms");
    }
}

public static class OrganicCompoundNamer
{
    private const string OtherCompound = "OTHERS";
    private const string AlkaneSuffix = "ane";
    private const string AlkeneSuffix = "ene";
    private const string AlcoholSuffix = "anol";
    private const string CarboxylicAcidSuffix = "anoic acid";
    private const string AldehydeSuffix = "anal";
    private const string KetoneSuffix = "anone";
    private const string AlcoholGroup = "OH";
    private const string CarboxylicAcidGroup = "COOH";
    private const string AldehydeGroup = "CHO";

    private static readonly Dictionary<int, string> Prefixes = new Dictionary<int, string>
    {
        {1, "meth"}, {2, "eth"}, {3, "prop"}, {4, "but"}, {5, "pent"},
        {6, "hex"}, {7, "hept"}, {8, "oct"}, {9, "non"}, {10, "dec"}
    };

    public static string GetName(string formula)
    {
        var (nC, nH, nO) = ParseFormula(formula);
        if (nC == 0) return OtherCompound;
        if (!Prefixes.TryGetValue(nC, out var prefix)) return OtherCompound;

        if (nH == 2 * nC + 2)
        {
            if (nO == 0) return prefix + AlkaneSuffix;
            if (nO == 1 && formula.EndsWith(AlcoholGroup)) return prefix + AlcoholSuffix;
        }
        else if (nH == 2 * nC)
        {
            if (nO == 0)
            {
                if (nC > 1) return prefix + AlkeneSuffix;
            }
            else if (nO == 1)
            {
                if (formula.EndsWith(AldehydeGroup)) return prefix + AldehydeSuffix;
                if (nC >= 3) return prefix + KetoneSuffix;
            }
            else if (nO == 2)
            {
                if (formula.EndsWith(CarboxylicAcidGroup)) return prefix + CarboxylicAcidSuffix;
            }
        }
        return OtherCompound;
    }

    private static (int nC, int nH, int nO) ParseFormula(string formula)
    {
        var nC = 0;
        var nH = 0;
        var nO = 0;
        for (var i = 0; i < formula.Length;)
        {
            var element = formula[i];
            i++;
            var start = i;
            while (i < formula.Length && char.IsDigit(formula[i]))
            {
                i++;
            }
            var count = 1;
            if (i > start)
            {
                count = int.Parse(formula.AsSpan(start, i - start));
            }
            switch (element)
            {
                case 'C': nC += count; break;
                case 'H': nH += count; break;
                case 'O': nO += count; break;
            }
        }
        return (nC, nH, nO);
    }
}
