using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    const int Width = 19;
    const int Height = 25;
    static string[,] field = new string[Height, Width];

    static void Main() {
        var input = Console.ReadLine();
        InitializeField();
        foreach (var token in input.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            ProcessInstruction(token);
        PrintField();
    }

    static void InitializeField() {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                field[y, x] = "{}";
    }

    static void ProcessInstruction(string token) {
        string mode = "MOW";
        if (token.StartsWith("PLANTMOW")) {
            mode = "PLANTMOW";
            token = token[8..];
        } else if (token.StartsWith("PLANT")) {
            mode = "PLANT";
            token = token[5..];
        }

        int cx = token[0] - 'a';
        int cy = token[1] - 'a';
        int diameter = int.Parse(token[2..]);
        double radius = diameter / 2.0;

        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                double dist = Math.Sqrt(Math.Pow(x - cx, 2) + Math.Pow(y - cy, 2));
                if (dist <= radius)
                    ApplyCircleEffect(x, y, mode);
            }
        }
    }

    static void ApplyCircleEffect(int x, int y, string mode) {
        switch (mode) {
            case "MOW":
                field[y, x] = "  ";
                break;
            case "PLANT":
                field[y, x] = "{}";
                break;
            case "PLANTMOW":
                field[y, x] = field[y, x] == "{}" ? "  " : "{}";
                break;
        }
    }

    static void PrintField() {
        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++)
                Console.Write(field[y, x]);
            Console.WriteLine();
        }
    }
}
