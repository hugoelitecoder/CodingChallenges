using System;
class Solution
{
    public static void Main(string[] args)
    {
        var parts = Console.ReadLine().Split();
        var _H = int.Parse(parts[0]);
        var _W = int.Parse(parts[1]);
        var _grid = new char[_H, _W];
        var _visited = new bool[_H, _W];
        var startX = 0;
        var startY = 0;

        for (var i = 0; i < _H; i++)
        {
            var line = Console.ReadLine();
            for (var j = 0; j < _W; j++)
            {
                _grid[i, j] = line[j];
                if (line[j] == 'X')
                {
                    startX = i;
                    startY = j;
                }
            }
        }

        _visited[startX, startY] = true;
        var maxGold = DFS(startX, startY, _grid, _visited, _H, _W);
        Console.WriteLine(maxGold);
    }

    private static int DFS(int x, int y, char[,] grid, bool[,] visited, int H, int W)
    {
        var best = 0;
        var dx = new[] { -1, 1, 0, 0 };
        var dy = new[] { 0, 0, -1, 1 };

        for (var dir = 0; dir < 4; dir++)
        {
            var nx = x + dx[dir];
            var ny = y + dy[dir];
            if (nx < 0 || nx >= H || ny < 0 || ny >= W) continue;
            if (visited[nx, ny] || grid[nx, ny] == '#') continue;

            visited[nx, ny] = true;
            var gold = grid[nx, ny] >= '1' && grid[nx, ny] <= '9'
                       ? grid[nx, ny] - '0'
                       : 0;
            var collected = gold + DFS(nx, ny, grid, visited, H, W);
            if (collected > best) best = collected;
            visited[nx, ny] = false;
        }

        return best;
    }
}
