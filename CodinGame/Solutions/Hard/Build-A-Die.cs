using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

class Solution
{
    public static void Main(string[] args)
    {
        var totalClock = Stopwatch.StartNew();
        var readClock = Stopwatch.StartNew();

        var labels = Console.ReadLine() ?? "";
        var imageCount = int.Parse(Console.ReadLine() ?? "0");

        var images = new string[5];
        for (var i = 0; i < 5; i++)
            images[i] = Console.ReadLine() ?? "";

        var size = (Console.ReadLine() ?? "").Split(
            new[] { ' ' },
            StringSplitOptions.RemoveEmptyEntries
        );

        var width = int.Parse(size[0]);
        var height = int.Parse(size[1]);
        var netLines = new string[height];

        for (var i = 0; i < height; i++)
            netLines[i] = Console.ReadLine() ?? "";

        readClock.Stop();

        var solveClock = Stopwatch.StartNew();

        int permutationCount;
        int validCount;

        var templates = DieSolver.Solve(
            labels,
            images,
            imageCount,
            netLines,
            width,
            height,
            out permutationCount,
            out validCount
        );

        solveClock.Stop();

        var outputClock = Stopwatch.StartNew();
        var outputLineCount = 1;

        Console.WriteLine(templates.Count);

        for (var templateIndex = 0; templateIndex < templates.Count; templateIndex++)
        {
            var lines = templates[templateIndex].Split('\n');

            for (var row = 0; row < lines.Length; row++)
            {
                Console.WriteLine(lines[row].TrimEnd());
                outputLineCount++;
            }
        }

        outputClock.Stop();
        totalClock.Stop();

        PrintDebug(
            labels,
            imageCount,
            width,
            height,
            permutationCount,
            validCount,
            templates.Count,
            outputLineCount,
            readClock.Elapsed,
            solveClock.Elapsed,
            outputClock.Elapsed,
            totalClock.Elapsed
        );
    }

    private static void PrintDebug(
        string labels,
        int imageCount,
        int width,
        int height,
        int permutationCount,
        int validCount,
        int templateCount,
        int outputLineCount,
        TimeSpan readTime,
        TimeSpan solveTime,
        TimeSpan outputTime,
        TimeSpan totalTime)
    {
        Console.Error.WriteLine("[DEBUG] ----------------------------------------");
        Console.Error.WriteLine("[DEBUG] Die Template Solver Report");
        Console.Error.WriteLine("[DEBUG] ----------------------------------------");

        Console.Error.WriteLine("[DEBUG] Input:");
        Console.Error.WriteLine("[DEBUG]   Face labels: " + labels);
        Console.Error.WriteLine("[DEBUG]   Die images: " + imageCount);
        Console.Error.WriteLine("[DEBUG]   Net size: " + width + " columns by " + height + " rows");
        Console.Error.WriteLine("[DEBUG]   Input was read in " + FormatTime(readTime) + ".");

        Console.Error.WriteLine("[DEBUG] Search:");
        Console.Error.WriteLine("[DEBUG]   Generated " + permutationCount + " possible label assignments.");
        Console.Error.WriteLine("[DEBUG]   Found " + validCount + " assignments matching every image.");
        Console.Error.WriteLine("[DEBUG]   Kept " + templateCount + " unique cube template(s).");
        Console.Error.WriteLine("[DEBUG]   Calculation finished in " + FormatTime(solveTime) + ".");

        Console.Error.WriteLine("[DEBUG] Output:");
        Console.Error.WriteLine("[DEBUG]   Wrote " + outputLineCount + " line(s).");
        Console.Error.WriteLine("[DEBUG]   Output finished in " + FormatTime(outputTime) + ".");

        Console.Error.WriteLine("[DEBUG] Total execution time: " + FormatTime(totalTime) + ".");
        Console.Error.WriteLine("[DEBUG] ----------------------------------------");
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture) + " ms";
    }

    private static class DieSolver
    {
        private static readonly int[][] FaceRing =
        {
            new[] { 1, 5, 3, 4 },
            new[] { 0, 4, 2, 5 },
            new[] { 1, 4, 3, 5 },
            new[] { 0, 5, 2, 4 },
            new[] { 0, 3, 2, 1 },
            new[] { 0, 1, 2, 3 }
        };

        private static readonly int[] OppFace = { 2, 3, 0, 1, 5, 4 };

        public static List<string> Solve(
            string labelText,
            string[] images,
            int imageCount,
            string[] netLines,
            int width,
            int height,
            out int permutationCount,
            out int validCount)
        {
            var rules = ParseRules(images, imageCount);

            var permutations = new List<string>(720);
            var chars = labelText.ToCharArray();

            Permute(chars, 0, permutations);

            permutationCount = permutations.Count;
            validCount = 0;

            var net = new CubeNet(netLines, width, height);
            var bestByCube = new Dictionary<string, string>(StringComparer.Ordinal);

            for (var i = 0; i < permutations.Count; i++)
            {
                var labels = permutations[i].ToCharArray();

                if (!Fits(labels, rules))
                    continue;

                validCount++;

                var cubeKey = CubeKey(labels);
                var template = net.Render(labels);

                string previous;
                if (!bestByCube.TryGetValue(cubeKey, out previous) ||
                    string.CompareOrdinal(template, previous) < 0)
                {
                    bestByCube[cubeKey] = template;
                }
            }

            var result = new List<string>(bestByCube.Values);
            result.Sort(StringComparer.Ordinal);

            return result;
        }

        private static List<string> ParseRules(string[] images, int imageCount)
        {
            var rules = new List<string>(imageCount);

            for (var start = 0; start < imageCount * 6; start += 6)
            {
                var marker = CharAt(images[0], start);

                if (marker == '\\')
                {
                    rules.Add(new string(new[]
                    {
                        CharAt(images[0], start + 2),
                        CharAt(images[3], start),
                        CharAt(images[3], start + 4)
                    }));
                }
                else if (marker == ' ')
                {
                    rules.Add(new string(new[]
                    {
                        CharAt(images[1], start + 2),
                        CharAt(images[3], start + 2)
                    }));
                }
                else
                {
                    rules.Add(new string(new[]
                    {
                        CharAt(images[2], start),
                        CharAt(images[2], start + 4)
                    }));
                }
            }

            return rules;
        }

        private static char CharAt(string text, int index)
        {
            return index < text.Length ? text[index] : ' ';
        }

        private static void Permute(char[] values, int start, List<string> result)
        {
            if (start == values.Length)
            {
                result.Add(new string(values));
                return;
            }

            for (var i = start; i < values.Length; i++)
            {
                var duplicate = false;

                for (var j = start; j < i; j++)
                {
                    if (values[j] == values[i])
                    {
                        duplicate = true;
                        break;
                    }
                }

                if (duplicate)
                    continue;

                var temp = values[start];
                values[start] = values[i];
                values[i] = temp;

                Permute(values, start + 1, result);

                temp = values[start];
                values[start] = values[i];
                values[i] = temp;
            }
        }

        private static bool Fits(char[] labels, List<string> rules)
        {
            for (var i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];

                if (rule.Length == 2)
                {
                    if (!HasEdge(labels, rule[0], rule[1]) ||
                        !HasEdge(labels, rule[1], rule[0]))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!HasCorner(labels, rule[0], rule[1], rule[2]) ||
                        !HasCorner(labels, rule[1], rule[2], rule[0]) ||
                        !HasCorner(labels, rule[2], rule[0], rule[1]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool HasEdge(char[] labels, char center, char neighbor)
        {
            for (var face = 0; face < 6; face++)
            {
                if (labels[face] != center)
                    continue;

                var ring = FaceRing[face];

                for (var i = 0; i < 4; i++)
                {
                    if (labels[ring[i]] == neighbor)
                        return true;
                }
            }

            return false;
        }

        private static bool HasCorner(char[] labels, char center, char first, char second)
        {
            for (var face = 0; face < 6; face++)
            {
                if (labels[face] != center)
                    continue;

                var ring = FaceRing[face];

                for (var i = 0; i < 4; i++)
                {
                    if (labels[ring[i]] == first &&
                        labels[ring[(i + 1) & 3]] == second)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static string CubeKey(char[] labels)
        {
            string best = null;

            for (var top = 0; top < 6; top++)
            {
                var ring = FaceRing[top];

                for (var front = 0; front < 4; front++)
                {
                    var key = new string(new[]
                    {
                        labels[top],
                        labels[ring[front]],
                        labels[OppFace[top]],
                        labels[ring[(front + 2) & 3]],
                        labels[ring[(front + 3) & 3]],
                        labels[ring[(front + 1) & 3]]
                    });

                    if (best == null || string.CompareOrdinal(key, best) < 0)
                        best = key;
                }
            }

            return best;
        }
    }

    private sealed class CubeNet
    {
        private static readonly int[] Dy = { -1, 0, 1, 0 };
        private static readonly int[] Dx = { 0, 1, 0, -1 };

        private readonly char[][] _grid;
        private readonly int[][] _faceAt;
        private readonly int _width;
        private readonly int _height;

        public CubeNet(string[] lines, int width, int height)
        {
            _width = width;
            _height = height;
            _grid = new char[height][];
            _faceAt = new int[height][];

            for (var y = 0; y < height; y++)
            {
                _grid[y] = new char[width];
                _faceAt[y] = new int[width];

                for (var x = 0; x < width; x++)
                {
                    _grid[y][x] = x < lines[y].Length ? lines[y][x] : ' ';
                    _faceAt[y][x] = -1;
                }
            }

            Fold();
        }

        public string Render(char[] labels)
        {
            var output = new char[_height * (_width + 1) - 1];
            var index = 0;

            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    output[index++] = _grid[y][x] == '?'
                        ? labels[_faceAt[y][x]]
                        : _grid[y][x];
                }

                if (y + 1 < _height)
                    output[index++] = '\n';
            }

            return new string(output);
        }

        private void Fold()
        {
            var seen = new bool[_height][];
            var pose = new FacePose[_height][];

            var startX = -1;
            var startY = -1;

            for (var y = 0; y < _height; y++)
            {
                seen[y] = new bool[_width];
                pose[y] = new FacePose[_width];

                for (var x = 0; x < _width; x++)
                {
                    if (startY == -1 && _grid[y][x] == '?')
                    {
                        startX = x;
                        startY = y;
                    }
                }
            }

            var queue = new Queue<int>(6);

            seen[startY][startX] = true;
            pose[startY][startX] = new FacePose(
                new Axis3(0, 0, 1),
                new Axis3(0, 1, 0),
                new Axis3(1, 0, 0)
            );

            queue.Enqueue(startY * _width + startX);

            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();
                var y = pos / _width;
                var x = pos - y * _width;

                for (var dir = 0; dir < 4; dir++)
                {
                    var nextY = y + Dy[dir];
                    var nextX = x + Dx[dir];

                    if (nextY < 0 || nextY >= _height ||
                        nextX < 0 || nextX >= _width ||
                        _grid[nextY][nextX] != '?' ||
                        seen[nextY][nextX])
                    {
                        continue;
                    }

                    seen[nextY][nextX] = true;
                    pose[nextY][nextX] = Turn(pose[y][x], dir);

                    queue.Enqueue(nextY * _width + nextX);
                }
            }

            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    if (_grid[y][x] == '?')
                        _faceAt[y][x] = FaceFor(pose[y][x].Normal);
                }
            }
        }

        private static FacePose Turn(FacePose face, int dir)
        {
            if (dir == 0)
            {
                return new FacePose(
                    face.Up,
                    Axis3.Neg(face.Normal),
                    face.Right
                );
            }

            if (dir == 1)
            {
                return new FacePose(
                    face.Right,
                    face.Up,
                    Axis3.Neg(face.Normal)
                );
            }

            if (dir == 2)
            {
                return new FacePose(
                    Axis3.Neg(face.Up),
                    face.Normal,
                    face.Right
                );
            }

            return new FacePose(
                Axis3.Neg(face.Right),
                face.Up,
                face.Normal
            );
        }

        private static int FaceFor(Axis3 normal)
        {
            if (normal.Y == 1)
                return 0;

            if (normal.Z == 1)
                return 1;

            if (normal.Y == -1)
                return 2;

            if (normal.Z == -1)
                return 3;

            if (normal.X == -1)
                return 4;

            return 5;
        }
    }

    private struct Axis3
    {
        public int X;
        public int Y;
        public int Z;

        public Axis3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Axis3 Neg(Axis3 axis)
        {
            return new Axis3(-axis.X, -axis.Y, -axis.Z);
        }
    }

    private struct FacePose
    {
        public Axis3 Normal;
        public Axis3 Up;
        public Axis3 Right;

        public FacePose(Axis3 normal, Axis3 up, Axis3 right)
        {
            Normal = normal;
            Up = up;
            Right = right;
        }
    }
}