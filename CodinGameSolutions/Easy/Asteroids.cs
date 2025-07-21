using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {

    static void Main() {
        var parts = Console.ReadLine().Split();
        int w = int.Parse(parts[0]);
        int h = int.Parse(parts[1]);
        int t1 = int.Parse(parts[2]);
        int t2 = int.Parse(parts[3]);
        int t3 = int.Parse(parts[4]);
        int dt = t2 - t1;
        int dt3 = t3 - t1;

        var sky1 = new char[h][];
        var sky2 = new char[h][];
        var positions = new Dictionary<char, (int x, int y)>();

        for (int y = 0; y < h; y++) {
            var row = Console.ReadLine().Split();
            sky1[y] = row[0].ToCharArray();
            sky2[y] = row[1].ToCharArray();

            for (int x = 0; x < w; x++) {
                char a = sky1[y][x];
                if (a != '.' && !positions.ContainsKey(a))
                    positions[a] = (x, y);
            }
        }

        var motion = new Dictionary<char, (double dx, double dy)>();
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                char a = sky2[y][x];
                if (a != '.' && positions.ContainsKey(a)) {
                    var (x1, y1) = positions[a];
                    double dx = (x - x1) / (double)dt;
                    double dy = (y - y1) / (double)dt;
                    motion[a] = (dx, dy);
                }
            }
        }

        var sky3 = new char[h, w];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                sky3[y, x] = '.';

        foreach (char a in motion.Keys) {
            var (x1, y1) = positions[a];
            var (dx, dy) = motion[a];
            int xf = (int)Math.Floor(x1 + dx * dt3);
            int yf = (int)Math.Floor(y1 + dy * dt3);
            if (xf >= 0 && xf < w && yf >= 0 && yf < h) {
                if (sky3[yf, xf] == '.' || a < sky3[yf, xf])
                    sky3[yf, xf] = a;
            }
        }

        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++)
                Console.Write(sky3[y, x]);
            Console.WriteLine();
        }
    }
}
