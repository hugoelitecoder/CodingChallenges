using System;
using System.Linq;

class Program
{
    const int MaxWidth  = 100;
    const int MaxHeight = 10;
    const int MaxCount  = 100;

    static void Main()
    {
        var parts       = Console.ReadLine().Split();
        int width       = int.Parse(parts[0]);
        int height      = int.Parse(parts[1]);
        int tumbleCount = int.Parse(Console.ReadLine());
        if (width  <= 0 || width  >= MaxWidth  ||
            height <= 0 || height >= MaxHeight ||
            tumbleCount <= 0 || tumbleCount >= MaxCount)
            return;

        char[][] map = new char[height][];
        for (int i = 0; i < height; i++)
            map[i] = Console.ReadLine().ToCharArray();

        for (int t = 0; t < tumbleCount; t++)
        {
            map = RotateLeft(map);
            ApplyGravity(map);
        }

        foreach (var row in map)
            Console.WriteLine(new string(row));
    }

    static char[][] RotateLeft(char[][] m)
    {
        int h = m.Length, w = m[0].Length;
        char[][] r = new char[w][];
        for (int i = 0; i < w; i++)
        {
            r[i] = new char[h];
            for (int j = 0; j < h; j++)
                r[i][j] = m[j][w - 1 - i];
        }
        return r;
    }

    static void ApplyGravity(char[][] m)
    {
        int h = m.Length, w = m[0].Length;
        for (int c = 0; c < w; c++)
        {
            int count = 0;
            for (int r = 0; r < h; r++)
                if (m[r][c] == '#') count++;
            for (int r = 0; r < h - count; r++)
                m[r][c] = '.';
            for (int r = h - count; r < h; r++)
                m[r][c] = '#';
        }
    }
}
