using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        while (n-- > 0)
        {
            var parts = Console.ReadLine().Split(' ');
            var A = parts[0];
            var B = parts[2];
            var minLen = Math.Min(A.Length, B.Length);

            var results = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int k = minLen; k >= 1 && results.Count == 0; k--)
            {
                CollectWithOverlap(A, B, k, minLen, results);
                CollectWithOverlap(B, A, k, minLen, results);
            }

            Console.Write($"{A} plus {B} = ");
            if (results.Count == 0)
                Console.WriteLine("NONE");
            else
                Console.WriteLine(string.Join(" ", results));
        }
    }

    static void CollectWithOverlap(string A, string B, int k, int minLen, ISet<string> output)
    {
        var aLow = A.ToLowerInvariant();
        var bLow = B.ToLowerInvariant();
        int la = A.Length, lb = B.Length;

        for (int i = k; i <= la; i++)
        {
            var suffix = aLow.Substring(i - k, k);
            int idx = 0;
            while ((idx = bLow.IndexOf(suffix, idx, StringComparison.Ordinal)) >= 0)
            {
                var left = A.Substring(0, i - k);
                var right = B.Substring(idx + k);
                if (left.Length > 0 
                    && right.Length > 0 
                    && left.Length + k + right.Length >= minLen)
                {
                    var blend = left + A.Substring(i - k, k) + right;
                    if (!blend.Equals(A, StringComparison.OrdinalIgnoreCase)
                     && !blend.Equals(B, StringComparison.OrdinalIgnoreCase))
                    {
                        var proper = char.ToUpper(blend[0]) + blend.Substring(1).ToLower();
                        output.Add(proper);
                    }
                }
                idx++;
            }
        }
    }
}
