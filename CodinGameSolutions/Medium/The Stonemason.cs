using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        long spans = long.Parse(Console.ReadLine());
        var coeff = new List<long> { spans, 0 };
        int spanIndex = 1;

        void Normalize()
        {
            // Rule A: replace two of same unit by one of next larger (shift left)
            int i = coeff.FindIndex(x => x > 1);
            if (i >= 0)
            {
                if (i == 0) { coeff.Insert(0, 0); spanIndex++; i = 1; }
                while (i + 2 >= coeff.Count) coeff.Add(0);
                coeff[i - 1]++; coeff[i] -= 2; coeff[i + 2]++;
                Normalize(); return;
            }
            // Rule B: replace two adjacent units by one of next larger
            var pair = coeff
                .Select((x, idx) => new { x, idx })
                .FirstOrDefault(p => p.x > 0 && p.idx + 1 < coeff.Count && coeff[p.idx + 1] > 0);
            if (pair != null)
            {
                int j = pair.idx;
                if (j == 0) { coeff.Insert(0, 0); spanIndex++; j = 1; }
                coeff[j - 1]++; coeff[j]--; coeff[j + 1]--;
                Normalize();
            }
        }
        Normalize();

        // Map remaining coefficients to unit labels
        var units = coeff
            .Select((count, idx) => (count, idx))
            .Where(t => t.count > 0)
            .Select(t =>
            {
                int delta = t.idx - (spanIndex - 1);
                return delta switch
                {
                    0 => "C",
                    < 0 => $"{-delta}L",
                    > 0 => $"{delta}R",
                };
            });

        Console.WriteLine(string.Join(" ", units));
    }
}