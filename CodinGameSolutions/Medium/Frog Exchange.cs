using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static Dictionary<char, int> Directions;

    static void Main()
    {
        string start = Console.ReadLine().Replace(" ", "");
        string target = string.Concat(start.Reverse());

        Directions = new Dictionary<char, int>
        {
            ['f'] = start[0] == 'f' ? 1 : -1,
            ['m'] = start[0] == 'm' ? 1 : -1
        };

        var stack = new Stack<string>();
        var discovered = new HashSet<string>();
        var cameFrom = new Dictionary<string, string>();

        stack.Push(start);
        string current = start;

        while (stack.Count > 0)
        {
            current = stack.Pop();
            if (current == target) break;
            if (!discovered.Add(current)) continue;
            foreach (var next in GetMoves(current))
            {
                if (!cameFrom.ContainsKey(next))
                    cameFrom[next] = current;
                stack.Push(next);
            }
        }

        var path = new List<string> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        foreach (var state in path)
            Console.WriteLine(string.Join(' ', state.ToCharArray()));
    }

    static IEnumerable<string> GetMoves(string v)
    {
        foreach (var w in MovesFor(v, 'm')) yield return w;
        foreach (var w in MovesFor(v, 'f')) yield return w;
    }

    static IEnumerable<string> MovesFor(string v, char f)
    {
        int n = v.Length;
        int dir = Directions[f];
        for (int i = 0; i < n; i++)
        {
            if (v[i] != f) continue;
            int j = i + dir;
            if (j >= 0 && j < n && v[j] == 's')
                yield return Swap(v, i, j);
            j += dir;
            int mid = i + dir;
            if (j >= 0 && j < n && v[mid] != 's' && v[j] == 's')
                yield return Swap(v, i, j);
        }
    }

    static string Swap(string v, int i, int j)
    {
        var a = v.ToCharArray();
        var t = a[i]; a[i] = a[j]; a[j] = t;
        return new string(a);
    }
}
