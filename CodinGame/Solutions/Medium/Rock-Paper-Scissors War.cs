using System;
using System.Linq;

class Solution {
    
    static void Main() {
        var whn = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int w = whn[0], h = whn[1], days = whn[2];
        var grid = Enumerable.Range(0, h)
                             .Select(_ => Console.ReadLine().ToCharArray())
                             .ToArray();

        var dirs = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
        static bool Beats(char a, char b) => a switch {
            'C' when b=='P'||b=='L' => true,
            'P' when b=='R'||b=='S' => true,
            'R' when b=='L'||b=='C' => true,
            'L' when b=='S'||b=='P' => true,
            'S' when b=='C'||b=='R' => true,
            _ => false
        };

        for (int day = 0; day < days; day++) {
            grid = grid
                .Select((row, r) => row
                    .Select((curr, c) => {
                        var attackers = dirs
                            .Select(d => (r + d.Item1, c + d.Item2))
                            .Where(p => p.Item1 >= 0 && p.Item1 < h && p.Item2 >= 0 && p.Item2 < w)
                            .Select(p => grid[p.Item1][p.Item2])
                            .Where(nb => Beats(nb, curr));
                        return attackers.Any()
                            ? attackers.Aggregate((x, y) => Beats(y, x) ? y : x)
                            : curr;
                    })
                    .ToArray()
                )
                .ToArray();
        }

        Console.Write(string.Join("\n", grid.Select(r => new string(r))));
    }
}
