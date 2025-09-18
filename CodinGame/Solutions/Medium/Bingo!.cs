using System;
using System.Linq;
class Solution {
    const int S = 5;
    static void Main() {
        int n = int.Parse(Console.ReadLine());
        var rawCards = new int[n][][];
        for (int k = 0; k < n; k++) {
            rawCards[k] = new int[S][];
            for (int i = 0; i < S; i++)
                rawCards[k][i] = Console.ReadLine().Split().Select(int.Parse).ToArray();
        }
        var calls = Console.ReadLine().Split().Select(int.Parse).ToArray();
        var idx   = calls.Select((v, i) => (v, i + 1))
                         .ToDictionary(x => x.v, x => x.Item2);

        int bestLine = int.MaxValue, bestFull = int.MaxValue;
        for (int k = 0; k < n; k++) {
            var rowMax = new int[S];
            var colMax = new int[S];
            int d0 = 0, d1 = 0, full = 0;
            for (int i = 0; i < S; i++)
            for (int j = 0; j < S; j++) {
                int t = rawCards[k][i][j] == 0 ? 0 : idx[rawCards[k][i][j]];
                full = Math.Max(full, t);
                rowMax[i] = Math.Max(rowMax[i], t);
                colMax[j] = Math.Max(colMax[j], t);
                if (i == j)       d0 = Math.Max(d0, t);
                if (i + j == S-1) d1 = Math.Max(d1, t);
            }
            int line = new[] { rowMax.Min(), colMax.Min(), d0, d1 }.Min();
            bestLine = Math.Min(bestLine, line);
            bestFull = Math.Min(bestFull, full);
        }
        Console.WriteLine(bestLine);
        Console.WriteLine(bestFull);
    }
}
