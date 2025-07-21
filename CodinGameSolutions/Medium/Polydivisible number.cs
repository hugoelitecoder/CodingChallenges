using System;
using System.Linq;
using System.Numerics;

class Solution
{
    static void Main()
    {
        var tokens = Console.ReadLine().Trim().Split(' ');
        var digits = tokens.Select(int.Parse).ToArray();
        int maxDigit = digits.Max();
        for (int b = Math.Max(2, maxDigit + 1); b <= 36; b++)
        {
            BigInteger value = 0;
            foreach (int d in digits)
                value = value * b + d;
            string dec = value.ToString();
            bool ok = true;
            for (int j = 1; j <= dec.Length; j++)
            {
                BigInteger prefix = BigInteger.Parse(dec.Substring(0, j));
                if (prefix % j != 0)
                {
                    ok = false;
                    break;
                }
            }
            if (ok)
                Console.WriteLine(b);
        }
    }
}
