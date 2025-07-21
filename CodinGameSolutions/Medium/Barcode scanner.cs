using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        string scan = Console.ReadLine().Trim();
        string result = Ean13Decoder.Decode(scan)
                     ?? Ean13Decoder.Decode(new string(scan.Reverse().ToArray()));
        Console.WriteLine(result ?? "INVALID SCAN");
    }
}

static class Ean13Decoder
{
    private static readonly Regex BarcodePattern = new Regex(
        @"^101((?:\d{7}){6})01010((?:\d{7}){6})101$",
        RegexOptions.Compiled
    );

    private static readonly (string L, string G, string R)[] CodeTable = {
        ("0001101","0100111","1110010"),
        ("0011001","0110011","1100110"),
        ("0010011","0011011","1101100"),
        ("0111101","0100001","1000010"),
        ("0100011","0011101","1011100"),
        ("0110001","0111001","1001110"),
        ("0101111","0000101","1010000"),
        ("0111011","0010001","1000100"),
        ("0110111","0001001","1001000"),
        ("0001011","0010111","1110100"),
    };

    private static readonly Dictionary<string,int> FirstDigitByPattern = new()
    {
        ["LLLLLL"] = 0, ["LLGLGG"] = 1, ["LLGGLG"] = 2, ["LLGGGL"] = 3,
        ["LGLLGG"] = 4, ["LGGLLG"] = 5, ["LGGGLL"] = 6, ["LGLGLG"] = 7,
        ["LGLGGL"] = 8, ["LGGLGL"] = 9
    };

    public static string Decode(string scan)
    {
        var m = BarcodePattern.Match(scan);
        if (!m.Success) return null;

        string leftBits  = m.Groups[1].Value;
        string rightBits = m.Groups[2].Value;

        if (!TryDecodeLeft(leftBits, out int[] leftDigits, out string parity))
            return null;

        if (!FirstDigitByPattern.TryGetValue(parity, out int firstDigit))
            return null;

        if (!TryDecodeRight(rightBits, out int[] rightDigits))
            return null;

        var all = new[] { firstDigit }
            .Concat(leftDigits)
            .Concat(rightDigits)
            .ToArray();
        return IsValidChecksum(all)
            ? string.Concat(all)
            : null;
    }

    private static bool TryDecodeLeft(string bits, out int[] digits, out string parity)
    {
        digits = new int[6];
        var sb = new char[6];
        for (int i = 0; i < 6; i++)
        {
            string chunk = bits.Substring(7 * i, 7);
            int d = Array.FindIndex(CodeTable, t => t.L == chunk);
            if (d >= 0) { digits[i] = d; sb[i] = 'L'; continue; }
            d = Array.FindIndex(CodeTable, t => t.G == chunk);
            if (d >= 0) { digits[i] = d; sb[i] = 'G'; continue; }
            parity = null; return false;
        }
        parity = new string(sb);
        return true;
    }

    private static bool TryDecodeRight(string bits, out int[] digits)
    {
        digits = new int[6];
        for (int i = 0; i < 6; i++)
        {
            string chunk = bits.Substring(7 * i, 7);
            int d = Array.FindIndex(CodeTable, t => t.R == chunk);
            if (d < 0) return false;
            digits[i] = d;
        }
        return true;
    }

    private static bool IsValidChecksum(int[] d)
    {
        int sum = d
            .Select((val, idx) => val * (idx % 2 == 0 ? 1 : 3))
            .Sum();
        return sum % 10 == 0;
    }
}
