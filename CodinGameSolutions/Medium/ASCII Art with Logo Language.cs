using System;
using System.Linq;
using System.Collections.Generic;

class Program {
    static void Main() {
        var instrs = Enumerable.Range(0, int.Parse(Console.ReadLine() ?? "0"))
            .SelectMany(_ => Console.ReadLine()?
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                ?? Array.Empty<string>())
            .ToArray();

        int x = 0, y = 0, dir = 0;
        bool penDown = true;
        char penChar = '#', bgChar = ' ';
        var screen = new Dictionary<(int x, int y), char>();
        int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
        (int dx, int dy)[] dirs = { (0,1), (1,0), (0,-1), (-1,0) };

        void Step() {
            if (penDown) {
                screen[(x,y)] = penChar;
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
            }
            x += dirs[dir].dx;
            y += dirs[dir].dy;
        }

        foreach (var instr in instrs) {
            var parts = instr.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0].ToUpperInvariant();
            var arg = parts.Length > 1 ? parts[1] : null;

            switch(cmd) {
                case "CS":
                    bgChar = arg![0];
                    screen.Clear();
                    minX = int.MaxValue; maxX = int.MinValue;
                    minY = int.MaxValue; maxY = int.MinValue;
                    break;

                case "FD":
                    int steps = int.Parse(arg!);
                    for (int i = 0; i < steps; i++) Step();
                    break;

                case "RT":
                    dir = (dir + int.Parse(arg!) / 90) & 3;
                    break;

                case "LT":
                    dir = (dir - int.Parse(arg!) / 90 + 4) & 3;
                    break;

                case "PU":
                    penDown = false;
                    break;

                case "PD":
                    penDown = true;
                    break;

                case "SETPC":
                    penChar = arg![0];
                    break;
            }
        }

        for (int yy = maxY; yy >= minY; yy--) {
            var line = string.Concat(
                Enumerable.Range(minX, maxX - minX + 1)
                          .Select(xx => screen.TryGetValue((xx, yy), out var ch) ? ch : bgChar)
            ).TrimEnd();
            Console.WriteLine(line);
        }
    }
}