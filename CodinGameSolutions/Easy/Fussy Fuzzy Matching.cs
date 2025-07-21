using System;
using System.Numerics;

class Solution
{
    static void Main()
    {
        bool letterCase = bool.Parse(Console.ReadLine()!);
        int  letterFuzz = int.Parse(Console.ReadLine()!);
        int  numberFuzz = int.Parse(Console.ReadLine()!);
        bool otherExact = bool.Parse(Console.ReadLine()!);

        string template = Console.ReadLine()!;
        int N = int.Parse(Console.ReadLine()!);

        for (int k = 0; k < N; k++)
        {
            string candidate = Console.ReadLine()!;
            Console.WriteLine(
                Matches(template, candidate,
                        letterCase, letterFuzz,
                        numberFuzz, otherExact)
                ? "true" : "false");
        }
    }

    static bool Matches(string tpl, string s,
                        bool caseSensitive,
                        int lFuzz,
                        int nFuzz,
                        bool otherExact)
    {
        int i = 0, j = 0, L = tpl.Length, M = s.Length;
        while (i < L && j < M)
        {
            char ct = tpl[i], cc = s[j];

            // LETTER vs LETTER
            if (char.IsLetter(ct) && char.IsLetter(cc))
            {
                // Case alignment
                if (caseSensitive &&
                   ((char.IsUpper(ct) && !char.IsUpper(cc)) ||
                    (char.IsLower(ct) && !char.IsLower(cc))))
                    return false;

                // Alphabetical fuzz
                int a = char.ToUpperInvariant(ct) - 'A';
                int b = char.ToUpperInvariant(cc) - 'A';
                if (Math.Abs(a - b) > lFuzz) 
                    return false;

                i++; j++;
            }
            // NUMBER vs NUMBER
            else if (char.IsDigit(ct) && char.IsDigit(cc))
            {
                int si = i, sj = j;
                while (i < L && char.IsDigit(tpl[i])) i++;
                while (j < M && char.IsDigit(s[j]))  j++;

                var tnum = BigInteger.Parse(tpl.Substring(si, i - si));
                var cnum = BigInteger.Parse(s.Substring(sj, j - sj));
                if (BigInteger.Abs(tnum - cnum) > nFuzz) 
                    return false;
            }
            // OTHER vs OTHER
            else if (!char.IsLetterOrDigit(ct) && !char.IsLetterOrDigit(cc))
            {
                if (otherExact && ct != cc)
                    return false;
                i++; j++;
            }
            else
            {
                // Mismatched categories
                return false;
            }
        }
        return i == L && j == M;
    }
}
