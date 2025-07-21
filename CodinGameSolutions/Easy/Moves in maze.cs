using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static void Main(string[] args) {
        var V = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var D = new (int y, int x)[] { (-1, 0), (0, -1), (1, 0), (0, 1) };
        var i = Console.ReadLine().Split().Select(int.Parse).ToArray();
        var (w, h) = (i[0], i[1]);
        var m = new char[h, w];
        var q = new Queue<(int y, int x, int d)>();
        for (int y = 0; y < h; y++) {
            var row = Console.ReadLine();
            for (int x = 0; x < w; x++) {
                m[y, x] = row[x];
                if (row[x] == 'S') {
                    q.Enqueue((y, x, 0));
                    m[y, x] = '0';
                }
            }
        }
        while (q.Any()) {
            var (y, x, d) = q.Dequeue();
            foreach (var (dy, dx) in D) {
                var ny = (y + dy + h) % h;
                var nx = (x + dx + w) % w;
                if (m[ny, nx] == '.') {
                    m[ny, nx] = V[d + 1];
                    q.Enqueue((ny, nx, d + 1));
                }
            }
        }
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) Console.Write(m[y, x]);
            Console.WriteLine();
        }
    }
}
