using System;
using System.Collections.Generic;
using System.Linq;

class Solution {

    static void Main() {
        var dims = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int W = dims[0], H = dims[1];
        var grid = Enumerable.Range(0, H)
                             .Select(_ => Console.ReadLine().ToCharArray())
                             .ToArray();
        var visited = new bool[H, W];
        var blocks = new SortedDictionary<char,int>();

        for (int r = 0; r < H; r++) {
            for (int c = 0; c < W; c++) {
                char col = grid[r][c];
                if (col != '0' && !visited[r, c]) {
                    blocks[col] = blocks.GetValueOrDefault(col) + 1;
                    DFS(grid, visited, H, W, r, c, col);
                }
            }
        }

        if (blocks.Count == 0)
            Console.WriteLine("No coloring today");
        else
            foreach (var kv in blocks)
                Console.WriteLine($"{kv.Key} -> {kv.Value}");
    }
    
    static readonly (int dr, int dc)[] Dirs = { (-1,0), (1,0), (0,-1), (0,1) };

    private static void DFS(char[][] grid,bool[,] visited,int H,int W,int r,int c,char color) {
        visited[r, c] = true;
        foreach (var (dr, dc) in Dirs) {
            int nr = r + dr, nc = c + dc;
            if (nr >= 0 && nr < H && nc >= 0 && nc < W
             && !visited[nr, nc] && grid[nr][nc] == color) {
                DFS(grid, visited, H, W, nr, nc, color);
            }
        }
    }
}
