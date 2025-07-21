using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var chars = Console.ReadLine().Split(' ').ToArray();
        var res = FindGreatest(chars);
        Console.WriteLine(res);
    }

    static string FindGreatest(string[] chars)
    {
        var used = new bool[chars.Length];
        var max = (string)null;
        Permute(chars, used, "", ref max);
        return max;
    }

    static void Permute(string[] chars, bool[] used, string cur, ref string max)
    {
        if (cur.Length == chars.Length)
        {
            var norm = Normalize(cur);
            if (norm == null) return;
            if (max == null || Compare(norm, max) > 0) max = norm;
            return;
        }
        for (var i = 0; i < chars.Length; ++i)
        {
            if (used[i]) continue;
            used[i] = true;
            Permute(chars, used, cur + chars[i], ref max);
            used[i] = false;
        }
    }

    static string Normalize(string s)
    {
        if (s.Count(c => c == '-') > 1 || s.Count(c => c == '.') > 1) return null;
        if (s.Contains("-") && s.IndexOf('-') != 0) return null;
        if (s.Contains("."))
        {
            var dot = s.IndexOf('.');
            if (dot == 0 || dot == s.Length - 1) return null;
            if (!char.IsDigit(s[dot - 1]) || !char.IsDigit(s[dot + 1])) return null;
        }
        var firstDigit = s.StartsWith("-") ? 1 : 0;
        while (s.Length > firstDigit + 1 && s[firstDigit] == '0' && s[firstDigit + 1] != '.') s = s.Remove(firstDigit, 1);
        if (s.Contains("."))
        {
            while (s.EndsWith("0")) s = s.Substring(0, s.Length - 1);
            if (s.EndsWith(".")) s = s.Substring(0, s.Length - 1);
        }
        if (s == "-" || s == "" || (s.Length == 1 && !char.IsDigit(s[0]))) return null;
        if (IsZero(s)) return "0";
        return s;
    }

    static bool IsZero(string s)
    {
        var num = s;
        if (num.StartsWith("-")) num = num.Substring(1);
        if (num.Contains("."))
        {
            num = num.Replace(".", "");
        }
        foreach (var c in num)
            if (c != '0') return false;
        return true;
    }

    static int Compare(string a, string b)
    {
        var da = decimal.Parse(a, CultureInfo.InvariantCulture);
        var db = decimal.Parse(b, CultureInfo.InvariantCulture);
        if (da > db) return 1;
        if (da < db) return -1;
        return string.CompareOrdinal(a, b);
    }
}
