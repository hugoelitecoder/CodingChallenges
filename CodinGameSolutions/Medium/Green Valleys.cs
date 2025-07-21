using System;
using System.Linq;

class Solution {
    

    static void Main() {
        int H = int.Parse(Console.ReadLine());
        int N = int.Parse(Console.ReadLine());
        var grid = Enumerable.Range(0, N)
                             .Select(_ => Console.ReadLine()
                                                  .Split()
                                                  .Select(int.Parse)
                                                  .ToArray())
                             .ToArray();

        var visited = new bool[N, N];
        int bestSize = 0, bestMin = 0;
        for (int r = 0; r < N; r++)
        for (int c = 0; c < N; c++)
            if (!visited[r, c] && grid[r][c] <= H) {
                int size = 0, minv = int.MaxValue;
                DFS(grid, visited, N, H, r, c, ref size, ref minv);
                if (size > bestSize || (size == bestSize && minv < bestMin)) {
                    bestSize = size;
                    bestMin = minv;
                }
            }

        Console.WriteLine(bestSize > 0 ? bestMin.ToString() : "0");
    }
    
    static readonly (int dr, int dc)[] DIRS = {(-1,0),(1,0),(0,-1),(0,1)};
    private static void DFS(int[][] grid, bool[,] visited, int N, int H,
                            int r, int c, ref int size, ref int minv) {
        visited[r, c] = true;
        size++;
        minv = Math.Min(minv, grid[r][c]);
        foreach (var (dr, dc) in DIRS) {
            int nr = r + dr, nc = c + dc;
            if (nr >= 0 && nr < N && nc >= 0 && nc < N
             && !visited[nr, nc] && grid[nr][nc] <= H) {
                DFS(grid, visited, N, H, nr, nc, ref size, ref minv);
            }
        }
    }
}
