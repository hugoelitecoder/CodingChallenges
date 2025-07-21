using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static readonly string Brackets = "()[]{}<>";
    static readonly Dictionary<(int, int), int> memo = new Dictionary<(int, int), int>();

    static int DFS(char[] bs, int i, int j)
    {
        if (i > j) return 0;
        if ((j - i + 1) % 2 == 1) return int.MaxValue;
        if (memo.TryGetValue((i, j), out var val))
            return val;
        int best = int.MaxValue;
        for (int k = i + 1; k <= j; k += 2)
        {
            int t1 = Brackets.IndexOf(bs[i]) / 2;
            int t2 = Brackets.IndexOf(bs[k]) / 2;
            if (t1 != t2) continue;
            int cost = (Brackets.IndexOf(bs[i]) % 2 == 0 ? 0 : 1)
                     + (Brackets.IndexOf(bs[k]) % 2 == 1 ? 0 : 1);

            int left = DFS(bs, i + 1, k - 1);
            if (left == int.MaxValue) continue;
            int right = DFS(bs, k + 1, j);
            if (right == int.MaxValue) continue;

            best = Math.Min(best, cost + left + right);
        }
        memo[(i, j)] = best;
        return best;
    }

    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        while (N-- > 0)
        {
            var bs = Console.ReadLine().Where(c => Brackets.Contains(c)).ToArray();
            memo.Clear();
            int res = DFS(bs, 0, bs.Length - 1);
            Console.WriteLine(res == int.MaxValue ? -1 : res);
        }
    }
}
