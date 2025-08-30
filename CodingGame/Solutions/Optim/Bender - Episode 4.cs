using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

record Point(int X, int Y);
record SwitchInfo(Point SwitchPos, Point BlockPos, bool InitialState);
record State(Point Pos, int Mask);

class Solution
{
    public static void Main(string[] args)
    {
        var stopwatch = Stopwatch.StartNew();
        var inputs = Console.ReadLine().Split(' ');
        var width = int.Parse(inputs[0]);
        var height = int.Parse(inputs[1]);
        var map = new char[height][];
        for (var i = 0; i < height; i++) map[i] = Console.ReadLine().ToCharArray();
        inputs = Console.ReadLine().Split(' ');
        var startPos = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));
        inputs = Console.ReadLine().Split(' ');
        var targetPos = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));
        var switchCount = int.Parse(Console.ReadLine());
        var switches = new SwitchInfo[switchCount];
        for (var i = 0; i < switchCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            switches[i] = new SwitchInfo(
                new Point(int.Parse(inputs[0]), int.Parse(inputs[1])),
                new Point(int.Parse(inputs[2]), int.Parse(inputs[3])),
                int.Parse(inputs[4]) == 1);
        }
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                if (map[y][x] == '+') map[y][x] = '#';

        Console.Error.WriteLine($"[DEBUG] Parsing done in {stopwatch.ElapsedMilliseconds} ms");
        var findPathWatch = Stopwatch.StartNew();
        var path = FindPathBFS(width, height, map, startPos, targetPos, switches);
        findPathWatch.Stop();
        Console.Error.WriteLine($"[DEBUG] BFS took {findPathWatch.ElapsedMilliseconds} ms. Path length: {(path == null ? -1 : path.Length)}");
        var compressWatch = Stopwatch.StartNew();
        var compressedPath = CompressPath(path);
        compressWatch.Stop();
        Console.Error.WriteLine($"[DEBUG] Compression took {compressWatch.ElapsedMilliseconds} ms");
        Console.Error.WriteLine($"[DEBUG] Compressed path length: {compressedPath.Length}");
        Console.WriteLine(compressedPath);
    }

    private static string FindPathBFS(int width, int height, char[][] map, Point startPos, Point targetPos, SwitchInfo[] switches)
    {
        var switchIndexMap = new Dictionary<Point, int>();
        var blockIndexMap = new Dictionary<Point, int>();
        for (var i = 0; i < switches.Length; i++)
        {
            switchIndexMap[switches[i].SwitchPos] = i;
            blockIndexMap[switches[i].BlockPos] = i;
        }
        var queue = new Queue<State>();
        var predecessors = new Dictionary<State, (State Parent, char Move)>();
        var initialMask = 0;
        for (var i = 0; i < switches.Length; i++)
            if (switches[i].InitialState) initialMask |= (1 << i);
        var startState = new State(startPos, initialMask);
        queue.Enqueue(startState);
        predecessors[startState] = (null, ' ');
        State finalState = null;
        var moves = new[] { ('U', 0, -1), ('D', 0, 1), ('L', -1, 0), ('R', 1, 0) };
        while (queue.Count > 0)
        {
            var currentState = queue.Dequeue();
            if (currentState.Pos.Equals(targetPos))
            {
                finalState = currentState;
                break;
            }
            foreach (var (direction, dx, dy) in moves)
            {
                var nextPos = new Point(currentState.Pos.X + dx, currentState.Pos.Y + dy);
                if (nextPos.X < 0 || nextPos.X >= width || nextPos.Y < 0 || nextPos.Y >= height || map[nextPos.Y][nextPos.X] == '#')
                    continue;
                var newMask = currentState.Mask;
                if (switchIndexMap.TryGetValue(nextPos, out var switchIdx)) newMask ^= (1 << switchIdx);
                if (blockIndexMap.TryGetValue(nextPos, out var blockIdx) && (newMask & (1 << blockIdx)) != 0) continue;
                var nextState = new State(nextPos, newMask);
                if (!predecessors.ContainsKey(nextState))
                {
                    predecessors[nextState] = (currentState, direction);
                    queue.Enqueue(nextState);
                }
            }
        }
        return ReconstructPath(finalState, predecessors);
    }

    private static string ReconstructPath(State finalState, Dictionary<State, (State Parent, char Move)> predecessors)
    {
        if (finalState == null) return "IMPOSSIBLE";
        var path = new StringBuilder();
        var curr = finalState;
        while (predecessors.ContainsKey(curr) && predecessors[curr].Parent != null)
        {
            var (parent, move) = predecessors[curr];
            path.Append(move);
            curr = parent;
        }
        var charArray = path.ToString().ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    private static string CompressPath(string path)
    {
        if (string.IsNullOrEmpty(path) || path == "IMPOSSIBLE") return path;
        var mainPath = path;
        var functions = new List<string>();
        for (var funcNum = 1; funcNum <= 9; funcNum++)
        {
            var bestSub = "";
            var bestScore = 0;
            var candidates = new Dictionary<string, int>();
            var allSegments = new List<string> { mainPath };
            allSegments.AddRange(functions);
            foreach (var segment in allSegments)
                for (var len = 2; len < segment.Length; len++)
                    for (var i = 0; i <= segment.Length - len; i++)
                    {
                        var sub = segment.Substring(i, len);
                        if (sub.Any(c => c >= '1' && c <= '9')) continue;
                        if (!candidates.ContainsKey(sub))
                        {
                            var totalCount = allSegments.Sum(s => CountOccurrences(s, sub));
                            if (totalCount > 1) candidates[sub] = totalCount;
                        }
                    }
            if (!candidates.Any()) break;
            foreach (var (sub, count) in candidates)
            {
                var score = (count - 1) * (sub.Length - 1) - (sub.Length + 1);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestSub = sub;
                }
            }
            if (bestScore <= 0) break;
            var funcChar = (char)('0' + funcNum);
            mainPath = ReplaceNonOverlapping(mainPath, bestSub, funcChar.ToString());
            for (var i = 0; i < functions.Count; i++)
                functions[i] = ReplaceNonOverlapping(functions[i], bestSub, funcChar.ToString());
            functions.Add(bestSub);
            Console.Error.WriteLine($"[COMPRESS] Function {funcNum}: '{bestSub}' x{candidates[bestSub]} (score {bestScore})");
        }
        if (functions.Any())
            return $"{mainPath};{string.Join(";", functions)}";
        return mainPath;
    }

    private static string ReplaceNonOverlapping(string text, string pattern, string replacement)
    {
        if (string.IsNullOrEmpty(pattern) || text.Length < pattern.Length) return text;
        var sb = new StringBuilder();
        int i = 0;
        while (i <= text.Length - pattern.Length)
        {
            if (text.Substring(i, pattern.Length) == pattern)
            {
                sb.Append(replacement);
                i += pattern.Length;
            }
            else
            {
                sb.Append(text[i]);
                i++;
            }
        }
        sb.Append(text.Substring(i));
        return sb.ToString();
    }

    private static int CountOccurrences(string text, string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) return 0;
        var count = 0;
        var i = 0;
        while ((i = text.IndexOf(pattern, i, StringComparison.Ordinal)) != -1)
        {
            i += pattern.Length;
            count++;
        }
        return count;
    }
}
