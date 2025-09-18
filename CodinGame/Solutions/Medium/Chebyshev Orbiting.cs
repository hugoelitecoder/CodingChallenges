using System;
using System.Linq;
using System.Collections.Generic;

class Solution {
    static void Main() {
        var a = Console.ReadLine()
                       .Split()
                       .Select(int.Parse)
                       .ToArray();
        int R = a[0], x = a[1], y = a[2], vx = a[3], vy = a[4], T = a[5];

        var seen = new Dictionary<(int x, int y, int vx, int vy), int> {
            [(x,y,vx,vy)] = 0
        };

        int t = 0, period = 0;
        bool crashed = false;
        while (t < T && !crashed) {
            t++;
            x += vx;  y += vy;
            vx -= Math.Sign(x);
            vy -= Math.Sign(y);

            if (Math.Max(Math.Abs(x), Math.Abs(y)) <= R) {
                crashed = true;
                break;
            }

            var state = (x, y, vx, vy);
            if (period == 0 && seen.TryGetValue(state, out var prev)) {
                period = t - prev;
                t = T - (T - t) % period;
            } else {
                seen[state] = t;
            }
        }

        Console.WriteLine($"{x} {y} {(crashed ? 1 : 0)}");
    }
}
