using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var start = Console.ReadLine().Split();
        var goal  = Console.ReadLine().Split();
        var startState = string.Concat(start);
        var goalState  = string.Concat(goal);

        var prev  = new Dictionary<string, string> { [startState] = null };
        var queue = new Queue<string>(new[] { startState });

        BFS(queue, goalState, prev);

        var path = new List<string>();
        for (var cur = goalState; cur != null; cur = prev[cur])
            path.Add(cur);
        path.Reverse();

        path.ForEach(s => Console.WriteLine(string.Join(' ', s.ToCharArray())));
    }

    static bool BFS(Queue<string> q, string goal, Dictionary<string, string> prev)
    {
        if (q.Count == 0) return false;
        var s = q.Dequeue();
        if (s == goal) return true;

        var arr = s.ToCharArray();
        var fs  = arr[0];

        var nbrs = new[] { -1 }.Concat(Enumerable.Range(1, 3))
                         .Where(i => i < 0 || arr[i] == fs)
                         .Select(i => Toggle(arr, 0, i))
                         .OrderBy(x => x);

        foreach (var ns in nbrs)
        {
            if (prev.ContainsKey(ns) || !IsValid(ns)) continue;
            prev[ns] = s;
            q.Enqueue(ns);
        }
        return BFS(q, goal, prev);
    }

    static string Toggle(char[] a, int f, int c)
    {
        var b = (char[])a.Clone();
        b[f] = b[f] == 'L' ? 'R' : 'L';
        if (c >= 0) b[c] = b[f];
        return new string(b);
    }

    static bool IsValid(string s)
    {
        var a = s.ToCharArray();
        if (a[0] != a[2] && (a[1] == a[2] || a[2] == a[3]))
            return false;
        return true;
    }
}