using System;
using System.Text;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var N = int.Parse(Console.ReadLine());
        var parts = new List<(int, string)>();
        for (var i = 0; i < N; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var agentCode = inputs[0];
            var share = inputs[1];
            parts.Add((int.Parse(agentCode), share));
        }
        var secret = SecretInterpolator.Reveal(parts);
        Console.WriteLine(secret);
    }
}

public static class SecretInterpolator
{
    private const string ALPHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
    private const int MODULUS = 53;
    private static readonly Dictionary<char, int> CharToIndex = new Dictionary<char, int>();

    static SecretInterpolator()
    {
        for (var i = 0; i < ALPHABET.Length; i++)
        {
            CharToIndex[ALPHABET[i]] = i;
        }
    }

    public static string Reveal(List<(int agentId, string share)> parts)
    {
        if (parts == null || parts.Count == 0)
        {
            return string.Empty;
        }
        var shareLength = parts[0].share.Length;
        if (shareLength == 0)
        {
            return string.Empty;
        }
        var revealedSecret = new StringBuilder(shareLength);
        for (var i = 0; i < shareLength; i++)
        {
            var points = new List<(int x, int y)>();
            foreach (var part in parts)
            {
                var x = part.agentId;
                var y = CharToIndex[part.share[i]];
                points.Add((x, y));
            }
            var secretCharValue = LagrangeInterpolateAtZero(points);
            revealedSecret.Append(ALPHABET[secretCharValue]);
        }
        return revealedSecret.ToString();
    }

    private static int LagrangeInterpolateAtZero(List<(int x, int y)> points)
    {
        var totalSum = 0L;
        for (var i = 0; i < points.Count; i++)
        {
            var p_i = points[i];
            var currentTerm = (long)p_i.y;
            for (var j = 0; j < points.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }
                var p_j = points[j];
                var numerator = (long)-p_j.x;
                var denominator = (long)p_i.x - p_j.x;
                var inverseDenominator = ModInverse(denominator);
                currentTerm = (currentTerm * numerator % MODULUS + MODULUS) % MODULUS;
                currentTerm = (currentTerm * inverseDenominator % MODULUS + MODULUS) % MODULUS;
            }
            totalSum = (totalSum + currentTerm) % MODULUS;
        }
        return (int)((totalSum + MODULUS) % MODULUS);
    }

    private static long ModInverse(long n)
    {
        return ModPow((n % MODULUS + MODULUS) % MODULUS, MODULUS - 2);
    }

    private static long ModPow(long baseVal, long exp)
    {
        var res = 1L;
        baseVal %= MODULUS;
        while (exp > 0)
        {
            if (exp % 2 == 1)
            {
                res = (res * baseVal) % MODULUS;
            }
            baseVal = (baseVal * baseVal) % MODULUS;
            exp /= 2;
        }
        return res;
    }
}