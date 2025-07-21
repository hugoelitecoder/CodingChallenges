using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {

    static void Main() {
        int n = int.Parse(Console.ReadLine());
        var grid = ReadGrid(n);
        var result = FindIslandWithMaxCoast(n, grid);
        Console.WriteLine(result);
    }

    static char[][] ReadGrid(int n) {
        var grid = new char[n][];
        for (int i = 0; i < n; i++)
            grid[i] = Console.ReadLine().ToCharArray();
        return grid;
    }

    static string FindIslandWithMaxCoast(int n, char[][] grid) {
        var visited = new bool[n][];
        for (int i = 0; i < n; i++)
            visited[i] = new bool[n];

        int bestIndex = 1, maxCoast = -1, islandId = 0;

        for (int y = 0; y < n; y++) {
            for (int x = 0; x < n; x++) {
                if (grid[y][x] == '#' && !visited[y][x]) {
                    islandId++;
                    int coast = ComputeIslandCoast(n, grid, visited, x, y);
                    if (coast > maxCoast) {
                        maxCoast = coast;
                        bestIndex = islandId;
                    }
                }
            }
        }

        return islandId == 0 ? "0 0" : $"{bestIndex} {Math.Max(0, maxCoast)}";
    }

    static int ComputeIslandCoast(int n, char[][] grid, bool[][] visited, int startX, int startY) {
        var q = new Queue<(int x, int y)>();
        var coast = new HashSet<(int x, int y)>();
        var directions = new (int dx, int dy)[] { (0, -1), (1, 0), (0, 1), (-1, 0) };

        q.Enqueue((startX, startY));
        visited[startY][startX] = true;

        while (q.Count > 0) {
            var (x, y) = q.Dequeue();
            foreach (var (dx, dy) in directions) {
                int nx = x + dx;
                int ny = y + dy;

                if (nx >= 0 && nx < n && ny >= 0 && ny < n) {
                    if (grid[ny][nx] == '~') {
                        coast.Add((nx, ny));
                    } else if (grid[ny][nx] == '#' && !visited[ny][nx]) {
                        visited[ny][nx] = true;
                        q.Enqueue((nx, ny));
                    }
                }
            }
        }

        return coast.Count;
    }
}













