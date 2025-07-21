using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int target = int.Parse(Console.ReadLine());
        var initial = Console.ReadLine()
                             .Split()
                             .Select(int.Parse)
                             .OrderBy(x => x)
                             .ToList();

        var visited = new HashSet<string> { Key(initial) };
        var reachable = new HashSet<int>(initial);

        int ops = BFS(new List<List<int>> { initial }, 0, target, visited, reachable);

        if (ops >= 0)
        {
            Console.WriteLine("POSSIBLE");
            Console.WriteLine(ops);
        }
        else
        {
            int bestDiff = reachable.Min(v => Math.Abs(v - target));
            Console.WriteLine("IMPOSSIBLE");
            Console.WriteLine(bestDiff);
        }
    }

    private static int BFS(
        List<List<int>> frontier,
        int depth,
        int target,
        HashSet<string> visited,
        HashSet<int> reachable)
    {
        if (frontier.Count == 0)
            return -1;

        var nextFrontier = new List<List<int>>();

        foreach (var state in frontier)
        {
            if (state.Contains(target))
                return depth;

            int n = state.Count;
            for (int i = 0; i < n - 1; i++)
            for (int j = i + 1; j < n; j++)
            {
                int a = state[i], b = state[j];
                var results = new List<int> { a + b, a * b };
                if (a > b)           results.Add(a - b);
                if (b > a)           results.Add(b - a);
                if (b != 0 && a % b == 0) results.Add(a / b);
                if (a != 0 && b % a == 0) results.Add(b / a);

                foreach (int r in results)
                {
                    var next = state
                        .Where((_, idx) => idx != i && idx != j)
                        .Append(r)
                        .OrderBy(x => x)
                        .ToList();

                    string key = Key(next);
                    if (visited.Add(key))
                    {
                        reachable.Add(r);
                        nextFrontier.Add(next);
                    }
                }
            }
        }

        return BFS(nextFrontier, depth + 1, target, visited, reachable);
    }

    private static string Key(IEnumerable<int> seq) =>
        string.Join(",", seq);
}
