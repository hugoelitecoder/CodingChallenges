using System;
using System.Linq;

class Solution {
    
    static void Main() {
        int N = int.Parse(Console.ReadLine());
        var ply = new int[N, N];
        int w = N, h = N, f = 0, t = 1;
        var pat = new[] { true, false, false, true };

        while (w > 1 || h > 1) {
            t <<= 1;
            if ((f & 1) == 0) w >>= 1; else h >>= 1;
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++) {
                int idx = ((f & 1) == 0 ? x / w : y / h) & 3;
                bool flip = pat[idx] ^ (f < 2);
                if (flip) ply[y, x] = t - 1 - ply[y, x];
            }
            f++;
        }

        var msg = new char[N * N];
        for (int y = 0; y < N; y++) {
            var line = Console.ReadLine();
            for (int x = 0; x < N; x++)
                msg[N * N - 1 - ply[y, x]] = line[x];
        }

        Console.WriteLine(new string(msg));
    }
}
