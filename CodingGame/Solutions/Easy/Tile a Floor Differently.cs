using System;

class Program
{
    // mirror‐maps for b↔d,p↔q in horizontal, vertical, and diagonal
    static readonly string BOOK = "bdqp", DOWN = "bpqd", DIAG = "bqpd";

    static void Main()
    {
        int q = int.Parse(Console.ReadLine()!);
        var tl = new char[q, q];
        for (int i = 0; i < q; i++)
        {
            var line = Console.ReadLine()!;
            for (int j = 0; j < q; j++)
                tl[i, j] = line[j];
        }

        int full = q * 2, step = full + 1, F = 2 * step + 1;
        var tile = new char[full, full];

        static char Mirror(char c, string map)
        {
            int k = map.IndexOf(c);
            return k < 0 ? c : map[k ^ 1];
        }
        static char SwapSlash(char c) =>
            c == '/' ? '\\' : c == '\\' ? '/' : c;

        // Build the 2q×2q tile
        for (int i = 0; i < q; i++)
            for (int j = 0; j < q; j++)
            {
                var c = tl[i, j];
                tile[i, j]                                = c;                                         // TL
                tile[i, full - 1 - j]                     = SwapSlash(Mirror(c, BOOK));                // TR
                tile[full - 1 - i, j]                     = SwapSlash(Mirror(c, DOWN));                // BL
                tile[full - 1 - i, full - 1 - j] = Mirror(c, DIAG);                                  // BR (no slash‐swap)
            }

        // Prepare floor with grout
        var G = new char[F, F];
        for (int r = 0; r < F; r++)
            for (int c = 0; c < F; c++)
                G[r, c] = (r % step, c % step) switch
                {
                    (0, 0) => '+',
                    (0, _) => '-',
                    (_, 0) => '|',
                    _      => ' '
                };

        // Tile it 2×2
        for (int tr = 0; tr < 2; tr++)
            for (int tc = 0; tc < 2; tc++)
            {
                int or = tr * step + 1, oc = tc * step + 1;
                for (int i = 0; i < full; i++)
                    for (int j = 0; j < full; j++)
                        G[or + i, oc + j] = tile[i, j];
            }

        // Output
        for (int r = 0; r < F; r++)
        {
            for (int c = 0; c < F; c++)
                Console.Write(G[r, c]);
            Console.WriteLine();
        }
    }
}
