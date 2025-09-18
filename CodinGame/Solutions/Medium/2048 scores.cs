using System;
using System.Linq;
using System.Collections.Generic;

class Solution {
    static void Main() {
        var tiles = Enumerable.Range(0, 4)
            .SelectMany(_ => Console.ReadLine().Split().Select(int.Parse))
            .ToArray();

        int fours = int.Parse(Console.ReadLine());
        var memo = new Dictionary<int, long> { { 0, 0 }, { 2, 0 } };
        Func<int, long> getScore = null;
        getScore = v => memo.TryGetValue(v, out var s)
            ? s
            : (memo[v] = 2 * getScore(v / 2) + v);

        long score = tiles.Sum(getScore) - 4L * fours;
        long totalTilesNeeded = tiles.Sum(v => v / 2);
        long turns = totalTilesNeeded - (2 + fours);

        Console.WriteLine(score);
        Console.WriteLine(turns);
    }
}
