using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        var S = Console.ReadLine()
            .Split(' ')
            .Select(int.Parse)
            .OrderBy(x => x)
            .ToArray(); // length 10

        int s0 = S[0], s1 = S[1], s9 = S[9], s8 = S[8];
        // Try each possible s_k = x2+x3 from S[2]..S[8]
        for (int k = 2; k <= 8; k++)
        {
            int sk = S[k];
            // x1 = (s0 + s1 - sk) / 2
            int sum01 = s0 + s1 - sk;
            if (sum01 < 0 || (sum01 & 1) != 0) continue;
            int x1 = sum01 / 2;
            int x2 = s0 - x1;
            int x3 = s1 - x1;
            if (x1 <= 0 || x2 <= 0 || x3 <= 0) continue;
            if (!(x1 <= x2 && x2 <= x3)) continue;

            // derive x5 and x4
            int x5 = s8 - x3;
            int x4 = s9 - x5;
            if (x4 < x3 || x5 < x4) continue;

            var xs = new[] { x1, x2, x3, x4, x5 };
            // generate all pairwise sums
            var sums = new int[10];
            int idx = 0;
            for (int i = 0; i < 5; i++)
                for (int j = i + 1; j < 5; j++)
                    sums[idx++] = xs[i] + xs[j];
            Array.Sort(sums);
            // compare
            bool ok = true;
            for (int i = 0; i < 10; i++)
                if (sums[i] != S[i]) { ok = false; break; }
            if (!ok) continue;

            // found solution
            Array.Sort(xs);
            Console.WriteLine(string.Join(" ", xs));
            return;
        }
    }
}
