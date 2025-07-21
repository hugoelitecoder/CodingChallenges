using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static int n;
    static char[][] grid;
    static List<(int x, int y)> bestPath;
    static List<(int dx, int dy)> dirs = new() { (0, 1), (1, 0), (0, -1), (-1, 0) };

    static void Main() {
        n = int.Parse(Console.ReadLine());
        grid = new char[n][];
        for (int i = 0; i < n; i++)
            grid[i] = Console.ReadLine().ToCharArray();

        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
                if (grid[y][x] == 'a')
                    DFS(x, y, 1, new List<(int, int)> { (x, y) });

        var output = new char[n][];
        for (int y = 0; y < n; y++) {
            output[y] = Enumerable.Repeat('-', n).ToArray();
        }

        if (bestPath != null) {
            foreach (var (x, y) in bestPath)
                output[y][x] = grid[y][x];
        }

        foreach (var line in output)
            Console.WriteLine(new string(line));
    }

    static void DFS(int x, int y, int level, List<(int x, int y)> path) {
        if (level == 26) {
            bestPath = new List<(int, int)>(path);
            return;
        }

        char next = (char)('a' + level);
        foreach (var (dx, dy) in dirs) {
            int nx = x + dx, ny = y + dy;
            if (nx >= 0 && ny >= 0 && nx < n && ny < n && grid[ny][nx] == next) {
                path.Add((nx, ny));
                DFS(nx, ny, level + 1, path);
                path.RemoveAt(path.Count - 1);
                if (bestPath != null) return; // early exit if found
            }
        }
    }
}

