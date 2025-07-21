using System;
using System.Linq;

class Player
{
    static void Main()
    {
        var dx = new[] { 0, 1, 0, -1 };
        var dy = new[] { 1, 0, -1, 0 };
        var dirMap = "v>^<";

        var input = Console.ReadLine().Split();
        int w = int.Parse(input[0]), h = int.Parse(input[1]);
        var grid = new char[h, w];
        var visits = new int[h, w];
        int x = 0, y = 0, d = 0;

        for (int i = 0; i < h; i++)
        {
            var line = Console.ReadLine();
            for (int j = 0; j < w; j++)
            {
                grid[i, j] = line[j];
                if (dirMap.Contains(grid[i, j]))
                {
                    x = j; y = i; d = dirMap.IndexOf(grid[i, j]);
                    grid[i, j] = '0';
                }
            }
        }

        int turn = Console.ReadLine() == "L" ? 3 : 1;
        int sx = x, sy = y;

        do
        {
            int nd = (d + 4 - turn) % 4;
            for (int i = 0; i < 4; i++, nd = (nd + turn) % 4)
            {
                int nx = x + dx[nd], ny = y + dy[nd];
                if (ny >= 0 && ny < h && nx >= 0 && nx < w && grid[ny, nx] != '#')
                {
                    x = nx; y = ny; d = nd;
                    visits[y, x]++;
                    break;
                }
            }
        } while (x != sx || y != sy);

        for (int i = 0; i < h; i++, Console.WriteLine())
            for (int j = 0; j < w; j++)
                Console.Write(grid[i, j] == '#' ? '#' : visits[i, j].ToString());
    }
}
