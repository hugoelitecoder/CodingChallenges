using System;
using System.Collections.Generic;
using System.Text;

class Solution {
    
    static void Main() {
        var parts = Console.ReadLine().Split();
        int count = int.Parse(parts[0]), dim = int.Parse(parts[1]);
        var pts = new List<(string label, int[] coord)>(count);
        for (int i = 0; i < count; i++) {
            var line = Console.ReadLine().Split();
            var coord = new int[dim];
            for (int j = 0; j < dim; j++)
                coord[j] = int.Parse(line[j + 1]);
            pts.Add((line[0], coord));
        }

        var curr = new int[dim];
        var prevSign = new int[dim];
        var sb = new StringBuilder();

        while (pts.Count > 0) {
            int bestIdx = 0;
            long bestDist = long.MaxValue;
            for (int i = 0; i < pts.Count; i++) {
                long d = 0;
                var c = pts[i].coord;
                for (int k = 0; k < dim; k++) {
                    long diff = c[k] - curr[k];
                    d += diff * diff;
                }
                if (d < bestDist) {
                    bestDist = d;
                    bestIdx = i;
                }
            }

            var (label, coord) = pts[bestIdx];
            var sign = new int[dim];
            for (int k = 0; k < dim; k++)
                sign[k] = coord[k] > 0 ? 1 : coord[k] < 0 ? -1 : 0;

            bool addSpace = sb.Length > 0;
            if (addSpace) {
                addSpace = false;
                for (int k = 0; k < dim; k++) {
                    if (prevSign[k] != 0 && sign[k] != 0 && prevSign[k] != sign[k]) {
                        addSpace = true;
                        break;
                    }
                }
            }
            if (addSpace) sb.Append(' ');
            sb.Append(label);

            pts.RemoveAt(bestIdx);
            curr = coord;
            prevSign = sign;
        }

        Console.WriteLine(sb);
    }
}
