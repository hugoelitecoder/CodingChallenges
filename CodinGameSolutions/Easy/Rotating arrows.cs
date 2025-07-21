using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var wh = Console.ReadLine()!.Split();
        int W = int.Parse(wh[0]), H = int.Parse(wh[1]);
        var xy = Console.ReadLine()!.Split();
        int startX = int.Parse(xy[0]), startY = int.Parse(xy[1]);

        // Read grid
        var grid = new char[H][];
        for (int r = 0; r < H; r++)
            grid[r] = Console.ReadLine()!.ToCharArray();

        // Rotation map: ^->>, >->v, v-><, <->^
        var rot = new Dictionary<char,char> {
            {'^','>'}, {'>','v'}, {'v','<'}, {'<','^'}
        };
        // Direction vectors for char after rotation
        var dx = new Dictionary<char,int> {
            {'^', 0}, {'>', 1}, {'v', 0}, {'<', -1}
        };
        var dy = new Dictionary<char,int> {
            {'^', -1}, {'>', 0}, {'v', 1}, {'<', 0}
        };

        int x = startX, y = startY;
        int count = 0;

        while (true)
        {
            // Rotate this arrow
            char old = grid[y][x];
            char nw  = rot[old];
            grid[y][x] = nw;
            count++;

            // Compute neighbor
            int nx = x + dx[nw];
            int ny = y + dy[nw];

            // If points outside, stop
            if (nx < 0 || nx >= W || ny < 0 || ny >= H)
                break;
            // If points back to start, stop
            if (nx == startX && ny == startY)
                break;

            // Continue cascade
            x = nx;
            y = ny;
        }

        Console.WriteLine(count);
    }
}
