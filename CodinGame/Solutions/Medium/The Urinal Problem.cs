using System;
using System.Linq;

class Solution {
    static void Main() {
        int N = int.Parse(Console.ReadLine());
        string B = Console.ReadLine();
        var occupied = B
            .Select((ch, i) => (ch, i))
            .Where(x => x.ch == '!')
            .Select(x => x.i)
            .ToArray();

        int bestIdx = -1, bestDist = -1;
        for (int i = 0; i < N; i++) {
            if (B[i] != 'U') continue;
            int dist = occupied.Min(j => Math.Abs(i - j));
            if (dist > bestDist) {
                bestDist = dist;
                bestIdx = i;
            }
        }

        Console.WriteLine(bestIdx);
    }
}
