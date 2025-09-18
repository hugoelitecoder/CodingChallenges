using System;
using System.Collections.Generic;
using System.Linq;

class P
{
    const int n = 5;
    static readonly char[] D = { 'D', 'L', 'R', 'U' };
    static readonly int[] dr = { +1,  0,  0, -1 };
    static readonly int[] dc = {  0, -1, +1,  0 };

    static void Main()
    {
        var startMap = new SortedDictionary<char, (int, int)>();
        for (int i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            for (int j = 0; j < n; j++)
                if (line[j] != '.')
                    startMap[line[j]] = (i, j);
        }

        var start = Encode(startMap);
        var queue = new Queue<string>();
        var parent = new Dictionary<string, (string prev, string mv)> {{ start, (null, null) }};
        queue.Enqueue(start);

        string goal = null;
        while (queue.Count > 0)
        {
            var key = queue.Dequeue();
            var map = Decode(key);

            if (map['X'] == (2, 2)) { goal = key; break; }

            var grid = new char[n, n];
            foreach (var kv in map)
                grid[kv.Value.Item1, kv.Value.Item2] = kv.Key;

            foreach (var kv in map)
            {
                char token = kv.Key;
                var (r, c) = kv.Value;

                for (int d = 0; d < 4; d++)
                {
                    int br = r + dr[d], bc = c + dc[d];
                    while (br >= 0 && br < n && bc >= 0 && bc < n && grid[br, bc] == '\0')
                    {
                        br += dr[d]; bc += dc[d];
                    }
                    if (br < 0 || br >= n || bc < 0 || bc >= n) continue;

                    int nr = br - dr[d];
                    int nc = bc - dc[d];
                    if (nr == r && nc == c) continue;

                    var nextMap = new SortedDictionary<char, (int, int)>(map) { [token] = (nr, nc) };
                    var nextKey = Encode(nextMap);
                    if (parent.ContainsKey(nextKey)) continue;

                    parent[nextKey] = (key, $"{token}{D[d]}");
                    queue.Enqueue(nextKey);
                }
            }
        }

        var moves = new List<string>();
        for (var k = goal; k != null; k = parent[k].prev)
            if (parent[k].mv != null)
                moves.Add(parent[k].mv);
        moves.Reverse();

        Console.WriteLine(string.Join(" ", moves));
        Console.WriteLine();

        var final = Decode(goal);
        var outGrid = new char[n, n];
        foreach (var kv in final)
            outGrid[kv.Value.Item1, kv.Value.Item2] = kv.Key;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                Console.Write(outGrid[i, j] == '\0' ? '.' : outGrid[i, j]);
            Console.WriteLine();
        }
    }

    static string Encode(IDictionary<char, (int, int)> m)
        => string.Join(";", m.Select(k => $"{k.Key}{k.Value.Item1}{k.Value.Item2}"));

    static SortedDictionary<char, (int, int)> Decode(string key)
    {
        var map = new SortedDictionary<char, (int, int)>();
        foreach (var part in key.Split(';'))
            map[part[0]] = (part[1] - '0', part[2] - '0');
        return map;
    }
}
