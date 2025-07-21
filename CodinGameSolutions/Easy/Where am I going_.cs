using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int h = int.Parse(Console.ReadLine());
        int w = int.Parse(Console.ReadLine());
        var grid = new char[h][];
        for (int i = 0; i < h; i++)
            grid[i] = Console.ReadLine().ToCharArray();

        var directions = new (int dy, int dx)[] { (0, 1), (1, 0), (0, -1), (-1, 0) }; // R, D, L, U
        int dir = 0; // Start facing right
        int y = 0, x = 0;

        if (grid[y][x] != '#') x++; // Move in if starting outside
        var visited = new HashSet<(int, int)> { (y, x) };

        var path = new List<string>();
        int steps = 1; // We already stepped into the first '#'

        while (true)
        {
            int ny = y + directions[dir].dy;
            int nx = x + directions[dir].dx;

            if (ny >= 0 && ny < h && nx >= 0 && nx < w && grid[ny][nx] == '#' && !visited.Contains((ny, nx)))
            {
                steps++;
                y = ny;
                x = nx;
                visited.Add((y, x));
            }
            else
            {
                if (steps > 0)
                {
                    path.Add(steps.ToString());
                    steps = 0;
                }

                bool turned = false;
                foreach (var (turnDir, newDir) in new (char, int)[] { ('R', (dir + 1) % 4), ('L', (dir + 3) % 4) })
                {
                    ny = y + directions[newDir].dy;
                    nx = x + directions[newDir].dx;
                    if (ny >= 0 && ny < h && nx >= 0 && nx < w && grid[ny][nx] == '#' && !visited.Contains((ny, nx)))
                    {
                        path.Add(turnDir.ToString());
                        dir = newDir;
                        turned = true;
                        break;
                    }
                }

                if (!turned) break;
            }
        }

        Console.WriteLine(string.Join("", path));
    }
}
