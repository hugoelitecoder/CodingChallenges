using System;
using System.Linq;

class Solution {

    static void Main() {
        var nm = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int n = nm[0], t = nm[1];
        var ok = new bool[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                ok[i, j] = true;

        while (t-- > 0) {
            var a = Console.ReadLine().Split().Select(int.Parse);
            var seenO = new bool[n];
            var seenE = new bool[n];
            foreach (var x in a) {
                if ((x & 1) == 1) seenO[(x - 1) / 2] = true;
                else              seenE[(x / 2) - 1] = true;
            }
            for (int i = 0; i < n; i++) if (seenO[i])
                for (int j = 0; j < n; j++) if (seenE[j])
                    ok[i, j] = false;
        }

        var adj = Enumerable.Range(0, n)
                            .Select(i => Enumerable.Range(0, n).Where(j => ok[i, j]).ToArray())
                            .ToArray();
        var matchR = Enumerable.Repeat(-1, n).ToArray();

        bool Dfs(int u, bool[] seen) {
            foreach (var v in adj[u]) {
                if (seen[v]) continue;
                seen[v] = true;
                if (matchR[v] < 0 || Dfs(matchR[v], seen)) {
                    matchR[v] = u;
                    return true;
                }
            }
            return false;
        }

        for (int u = 0; u < n; u++)
            Dfs(u, new bool[n]);

        var result = Enumerable.Range(0, n)
                               .Select(i => 2 * (Array.IndexOf(matchR, i) + 1))
                               .ToArray();

        Console.WriteLine(string.Join(" ", result));
    }
}
