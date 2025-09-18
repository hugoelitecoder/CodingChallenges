using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        var ct = Console.ReadLine();
        var w  = Console.ReadLine();
        const int F = 32, R = 95;

        for (int k = 0; k < R; k++)
        {
            var pt = string.Concat(ct.Select(c =>
                c < F || c > F + R - 1
                    ? c
                    : (char)((((c - F) - k) % R + R) % R + F)));

            if (pt
                .Split(new[] { ' ', ',', '.', '!', '?', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
                .Contains(w))
            {
                Console.WriteLine(k);
                Console.WriteLine(pt);
                return;
            }
        }
    }
}
