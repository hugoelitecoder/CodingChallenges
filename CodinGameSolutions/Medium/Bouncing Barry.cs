using System;
using System.Collections.Generic;
using System.Linq;

class Solution {
    static readonly Dictionary<string,(int dx,int dy)> Dirs = new() {
        ["N"] = ( 0,-1), ["S"] = ( 0, 1),
        ["E"] = ( 1, 0), ["W"] = (-1, 0)
    };

    static void Main() {
        var dirs    = Console.ReadLine().Split();
        var bounces = Console.ReadLine().Split().Select(int.Parse).ToArray();
        var toggled = new HashSet<(int x,int y)>();
        int x = 0, y = 0;

        for (int i = 0; i < dirs.Length; i++) {
            var (dx,dy) = Dirs[dirs[i]];
            for (int c = bounces[i]; c-- > 0; ) {
                x += dx; y += dy;
                var p = (x,y);
                if (!toggled.Add(p)) toggled.Remove(p);
            }
        }
        
        if (toggled.Count == 0) {
            Console.WriteLine(".");
            return;
        }
        int minX = toggled.Min(p => p.x),
            maxX = toggled.Max(p => p.x),
            minY = toggled.Min(p => p.y),
            maxY = toggled.Max(p => p.y),
            w = maxX - minX + 1;

        var rows = toggled
            .GroupBy(p => p.y - minY, p => p.x - minX)
            .ToDictionary(g => g.Key, g => g.ToHashSet());

        var line = new char[w];
        for (int ry = 0; ry <= maxY - minY; ry++) {
            for (int i = 0; i < w; i++) line[i] = '.';
            if (rows.TryGetValue(ry, out var cols))
                foreach (var cx in cols) line[cx] = '#';
            Console.WriteLine(line);
        }
    }
}
