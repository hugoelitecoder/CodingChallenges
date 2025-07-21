using System;
using System.Collections.Generic;
using System.Text;

class Solution
{
    static void Main(string[] args)
    {
        var meta = Console.ReadLine().Split();
        var E = int.Parse(meta[0]);
        var L = int.Parse(meta[1]);
        var N = int.Parse(meta[2]);
        var baseRow = Console.ReadLine().Split();
        Console.WriteLine(EvalIterative(baseRow, E, L, N));
    }

    static string EvalIterative(string[] baseRow, int E, int L, int N)
    {
        var cache = new Dictionary<(int, int), string>();

        for (int i = 1; i <= E; i++)
            cache[(1, i)] = baseRow[i - 1];

        for (int l = 2; l <= L; l++)
        {
            int start = Math.Max(1, N - (L - l));
            int end = Math.Min(E + l - 1, N);
            for (int n = start; n <= end; n++)
            {
                var left = cache.TryGetValue((l - 1, n - 1), out var lv) ? lv : "";
                var right = cache.TryGetValue((l - 1, n), out var rv) ? rv : "";

                if (int.TryParse(left, out var a) && int.TryParse(right, out var b))
                    cache[(l, n)] = (a + b).ToString();
                else
                    cache[(l, n)] = left + right;
            }
        }

        return cache[(L, N)];
    }
}
