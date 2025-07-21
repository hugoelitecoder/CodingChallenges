using System;
using System.Collections.Generic;
using System.Linq;

class Solution {
    enum Dir { N, E, S, W }

    static readonly Dictionary<Dir, (int dx, int dy)> Delta = new() {
        [Dir.N] = (0, -1), [Dir.S] = (0, 1),
        [Dir.E] = (1, 0), [Dir.W] = (-1, 0)
    };

    static readonly Dictionary<int, Dictionary<Dir, Dir>> ExitMap =
        new Dictionary<int, Dictionary<Dir, Dir>> {
        [1]  = new() { [Dir.N]=Dir.S, [Dir.E]=Dir.S, [Dir.W]=Dir.S },
        [2]  = new() { [Dir.E]=Dir.W, [Dir.W]=Dir.E },
        [3]  = new() { [Dir.N]=Dir.S },
        [4]  = new() { [Dir.N]=Dir.W, [Dir.E]=Dir.S },
        [5]  = new() { [Dir.N]=Dir.E, [Dir.W]=Dir.S },
        [6]  = new() { [Dir.W]=Dir.E, [Dir.E]=Dir.W },
        [7]  = new() { [Dir.N]=Dir.S, [Dir.E]=Dir.S },
        [8]  = new() { [Dir.W]=Dir.S, [Dir.E]=Dir.S },
        [9]  = new() { [Dir.W]=Dir.S, [Dir.N]=Dir.S },
        [10] = new() { [Dir.N]=Dir.W },
        [11] = new() { [Dir.N]=Dir.E },
        [12] = new() { [Dir.E]=Dir.S },
        [13] = new() { [Dir.W]=Dir.S }
    };

    static Dir ParsePos(string p) => p[0] switch {
        'T' => Dir.N, 'L' => Dir.W, 'R' => Dir.E,
        _   => throw new ArgumentException()
    };

    static void Main() {
        var wh = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int W = wh[0], H = wh[1];
        var grid = Enumerable.Range(0, H)
                    .Select(_ => Console.ReadLine()
                                     .Split().Select(int.Parse).ToArray())
                    .ToArray();
        _ = Console.ReadLine();

        while (true) {
            var parts = Console.ReadLine().Split();
            int x = int.Parse(parts[0]), y = int.Parse(parts[1]);
            Dir inDir = ParsePos(parts[2]);
            int type = grid[y][x];
            var exitDir = ExitMap[type][inDir];
            var d = Delta[exitDir];
            Console.WriteLine($"{x + d.dx} {y + d.dy}");
        }
    }
}
