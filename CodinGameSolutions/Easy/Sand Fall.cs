using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    const char Empty = ' ';
    const int Padding = 1;

    static void Main(string[] args) {
        var dims = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int w = dims[0], h = dims[1], n = int.Parse(Console.ReadLine());

        var box = InitBox(w, h);

        for (int i = 0; i < n; i++) {
            var parts = Console.ReadLine().Split();
            var ch = parts[0][0];
            var x = int.Parse(parts[1]) + Padding;
            DropGrain(box, ch, x, h, w);
        }

        PrintBox(box, h, w);
    }

    static char[,] InitBox(int w, int h) {
        var box = new char[h + 2 * Padding, w + 2 * Padding];
        for (int y = 0; y < box.GetLength(0); y++)
            for (int x = 0; x < box.GetLength(1); x++)
                box[y, x] = Empty;
        return box;
    }

    static void DropGrain(char[,] box, char ch, int x, int h, int w) {
        int y = 0;
        while (y + 1 <= h && box[y + 1, x] == Empty) y++;

        while (true) {
            if (y + 1 <= h && box[y + 1, x] == Empty) {
                y++;
                continue;
            }

            bool moved = TryMove(ref x, ref y, ch, box, h, w);
            if (!moved) break;
        }

        box[y, x] = ch;
    }

    static bool TryMove(ref int x, ref int y, char ch, char[,] box, int h, int w) {
        var directions = char.IsLower(ch)
            ? new[] { +1, -1 } // Right then Left
            : new[] { -1, +1 }; // Left then Right

        foreach (var dx in directions) {
            int nx = x + dx, ny = y + 1;
            if (nx >= Padding && nx <= w && ny <= h &&
                box[y, nx] == Empty && box[ny, nx] == Empty) {
                x = nx; y = ny;
                return true;
            }
        }
        return false;
    }

    static void PrintBox(char[,] box, int h, int w) {
        for (int y = 1; y <= h; y++) {
            Console.Write("|");
            for (int x = 1; x <= w; x++)
                Console.Write(box[y, x]);
            Console.WriteLine("|");
        }
        Console.WriteLine("+" + new string('-', w) + "+");
    }
}




