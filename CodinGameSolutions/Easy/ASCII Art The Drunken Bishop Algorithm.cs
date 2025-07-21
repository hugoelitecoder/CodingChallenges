using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    
    const int Width = 17;
    const int Height = 9;
    const int StartX = 8;
    const int StartY = 4;
    static readonly string Symbols = " .o+=*BOX@%&#/^";
    static readonly (int dx, int dy)[] MoveMap = {
        (-1, -1), // 00
        (1, -1),  // 01
        (-1, 1),  // 10
        (1, 1)    // 11
    };

    static void Main() {
        var fingerprint = Console.ReadLine();
        var grid = new int[Height, Width];
        var bytes = ParseFingerprint(fingerprint);
        SimulateWalk(bytes, grid);
        Render(grid);
    }

    static byte[] ParseFingerprint(string input) {
        return input.Split(':').Select(s => Convert.ToByte(s, 16)).ToArray();
    }

    static void SimulateWalk(byte[] bytes, int[,] grid) {
        int x = StartX, y = StartY;
        grid[y, x]++;

        foreach (var b in bytes) {
            for (int bit = 0; bit < 8; bit += 2) {
                int dir = (b >> bit) & 0b11;
                int nx = x + MoveMap[dir].dx;
                int ny = y + MoveMap[dir].dy;

                // Wall sliding logic
                if (nx < 0 || nx >= Width) nx = x;
                if (ny < 0 || ny >= Height) ny = y;

                x = nx;
                y = ny;
                grid[y, x]++;
            }
        }

        // Mark final position specially
        grid[StartY, StartX] = -1; // S
        grid[y, x] = -2;           // E
    }

    static void Render(int[,] grid) {
        Console.WriteLine("+---[CODINGAME]---+");
        for (int y = 0; y < Height; y++) {
            Console.Write("|");
            for (int x = 0; x < Width; x++) {
                if (grid[y, x] == -1) Console.Write('S');
                else if (grid[y, x] == -2) Console.Write('E');
                else Console.Write(Symbols[grid[y, x] % Symbols.Length]);
            }
            Console.WriteLine("|");
        }
        Console.WriteLine("+-----------------+");
    }
}

