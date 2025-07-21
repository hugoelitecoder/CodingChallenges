using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    const long A = 1103515245, C = 12345, MASK = (1L << 31) - 1;

    static void Main()
    {
        var parts = Console.ReadLine().Split();
        int N = int.Parse(parts[0]), M = int.Parse(parts[1]);
        long X = long.Parse(parts[2]) & MASK;

        IEnumerable<(long V, long E)> classical =
            Enumerable.Range(0, N)
                      .Select(_ => Console.ReadLine().Split())
                      .Select(a => (V: long.Parse(a[0]), E: long.Parse(a[1])));

        IEnumerable<(long V, long E)> congruential =
            Enumerable.Range(0, M)
                      .Select(_ =>
                      {
                          var v = X; 
                          X = (A * X + C) & MASK;
                          var e = X;
                          X = (A * X + C) & MASK;
                          return (V: v, E: e);
                      });

        var horses = classical
                     .Concat(congruential)
                     .OrderBy(h => h.V)
                     .ToList();

        long best = long.MaxValue;
        for (int i = 0, n = horses.Count; i < n - 1; i++)
        {
            var (Vi, Ei) = horses[i];
            for (int j = i + 1; j < n; j++)
            {
                var dV = horses[j].V - Vi;
                if (dV >= best) break;
                var dE = horses[j].E >= Ei ? horses[j].E - Ei : Ei - horses[j].E;
                var d  = dV + dE;
                if (d < best) best = d;
            }
        }

        Console.WriteLine(best);
    }
}
