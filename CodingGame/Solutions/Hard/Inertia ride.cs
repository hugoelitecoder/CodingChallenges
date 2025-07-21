using System;
class Solution
{
    public static void Main(string[] args)
    {
        var inertia = int.Parse(Console.ReadLine());
        var wh = Console.ReadLine().Split(' ');
        var w = int.Parse(wh[0]);
        var h = int.Parse(wh[1]);
        var grid = new char[h, w];
        for (var i = 0; i < h; i++)
        {
            var line = Console.ReadLine();
            for (var j = 0; j < w; j++)
            {
                grid[i, j] = line[j];
            }
        }
        var track = ExtractTrack(grid, h, w);
        var pos = Simulate(track, inertia);
        Console.WriteLine(pos);
    }
    static char[] ExtractTrack(char[,] grid, int h, int w)
    {
        var track = new char[w];
        for (var j = 0; j < w; j++)
        {
            for (var i = 0; i < h; i++)
            {
                if (grid[i, j] != '.')
                {
                    track[j] = grid[i, j];
                    break;
                }
            }
        }
        return track;
    }
    static int Simulate(char[] track, int inertia)
    {
        var w = track.Length;
        var pos = 0;
        var dir = 1;
        var v = inertia;
        var swap = false;
        while (true)
        {
            var t = track[pos];
            if (swap)
            {
                if (t == '/')
                    t = '\\';
                else if (t == '\\')
                    t = '/';
            }
            if (t == '_')
                v -= 1;
            else if (t == '\\')
                v += 9;
            else
                v -= 10;
            if (v < 0)
            {
                v = -v;
                dir = -dir;
                swap = !swap;
            }
            if (v == 0 && t == '_')
                return pos;
            if (v == 0)
                continue;
            var next = pos + dir;
            if (next < 0 || next >= w)
                return Math.Max(0, Math.Min(w - 1, next));
            pos = next;
        }
    }
}
