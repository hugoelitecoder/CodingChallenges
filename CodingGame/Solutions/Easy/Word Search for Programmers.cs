using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static void Main() {
        int size = int.Parse(Console.ReadLine());
        char[][] grid = new char[size][];
        bool[][] keep = new bool[size][];

        for (int i = 0; i < size; i++) {
            grid[i] = Console.ReadLine().ToCharArray();
            keep[i] = new bool[size];
        }

        var clues = Console.ReadLine().ToUpper().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var directions = new (int dx, int dy)[] {
            (0, 1), (1, 0), (1, 1), (1, -1),
            (0, -1), (-1, 0), (-1, -1), (-1, 1)
        };

        bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < size && y < size;

        foreach (var word in clues) {
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    foreach (var (dx, dy) in directions) {
                        bool matched = true;
                        for (int k = 0; k < word.Length; k++) {
                            int nx = x + k * dx;
                            int ny = y + k * dy;
                            if (!InBounds(nx, ny) || grid[ny][nx] != word[k]) {
                                matched = false;
                                break;
                            }
                        }
                        if (matched) {
                            for (int k = 0; k < word.Length; k++) {
                                int nx = x + k * dx;
                                int ny = y + k * dy;
                                keep[ny][nx] = true;
                            }
                        }
                    }
                }
            }
        }

        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                Console.Write(keep[y][x] ? grid[y][x] : ' ');
            }
            Console.WriteLine();
        }
    }
}
