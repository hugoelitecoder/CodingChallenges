using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var s = Console.ReadLine().Trim();
        int D = int.Parse(Console.ReadLine());

        int n = s.Length;
        int bestValue = -1;

        void CheckCandidate(string t)
        {
            if (t.Length == 0 || t.Length > 10) return;
            if (!long.TryParse(t, out var val)) return;
            if (val % D == 0 && val > bestValue)
                bestValue = (int)val;
        }

        CheckCandidate(s);

        for (int i = 0; i < n; i++)
        {
            var t1 = s.Remove(i, 1);
            CheckCandidate(t1);
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                var t2 = s.Remove(j, 1).Remove(i, 1);
                CheckCandidate(t2);
            }
        }

        Console.WriteLine(bestValue < 0 ? 0 : bestValue);
    }
}
