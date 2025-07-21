using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        var invalid = new List<string>();

        for (int i = 0; i < N; i++)
        {
            var s = Console.ReadLine().Trim();
            if (!IsValidISBN(s))
                invalid.Add(s);
        }

        Console.WriteLine($"{invalid.Count} invalid:");
        foreach (var s in invalid)
            Console.WriteLine(s);
    }

    static bool IsValidISBN(string s)
    {
        if (s.Length == 10)
            return IsValidISBN10(s);
        else if (s.Length == 13)
            return IsValidISBN13(s);
        else
            return false;
    }

    static bool IsValidISBN10(string s)
    {
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            if (s[i] < '0' || s[i] > '9')
                return false;
            sum += (s[i] - '0') * (10 - i);
        }
        int provided;
        char c = s[9];
        if (c == 'X')
            provided = 10;
        else if (c >= '0' && c <= '9')
            provided = c - '0';
        else
            return false;

        int r = sum % 11;
        int needed = (11 - r) % 11;
        return needed == provided;
    }

    static bool IsValidISBN13(string s)
    {
        int sum = 0;
        for (int i = 0; i < 13; i++)
        {
            if (s[i] < '0' || s[i] > '9')
                return false;
            int digit = s[i] - '0';
            if (i < 12)
            {
                sum += digit * (i % 2 == 0 ? 1 : 3);
            }
        }
        int provided = s[12] - '0';
        int r = sum % 10;
        int needed = (10 - r) % 10;
        return needed == provided;
    }
}
