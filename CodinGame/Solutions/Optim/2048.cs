using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Solution
{
    // --- Performance Tuning Parameters ---
    const int NORMAL_TURN_BEAM_WIDTH = 60;
    const int FIRST_TURN_BEAM_WIDTH = 100;
    const int ABSOLUTE_MAX_DEPTH = 1200;
    const double SAFE_PATH_RATIO = 1.5;

    // --- Game Logic Constants ---
    const int BOARD_SIZE = 4;
    static readonly char[] POSSIBLE_MOVES = { 'U', 'D', 'L', 'R' };
    static readonly double[] HEURISTIC_WEIGHTS = { 97.30119, 90.68706, 13.228226, 67.991486, 12.232829 };

    // --- State ---
    static bool _isFirstTurn = true;
    private static readonly int[] _freeCellBuffer = new int[BOARD_SIZE * BOARD_SIZE];

    public static void Main(string[] args)
    {
        while (true)
        {
            string seedLine = Console.ReadLine();
            if (seedLine == null) return;

            long seed = long.Parse(seedLine);
            int.Parse(Console.ReadLine());
            var grid = new int[BOARD_SIZE, BOARD_SIZE];
            for (var i = 0; i < BOARD_SIZE; i++)
            {
                var inputs = Console.ReadLine().Split(' ');
                for (var j = 0; j < BOARD_SIZE; j++) grid[i, j] = int.Parse(inputs[j]);
            }
            var moveSequence = FindBestMoveSequence(grid, seed);
            _isFirstTurn = false;
            Console.WriteLine(moveSequence);
        }
    }

    struct SearchNode
    {
        public int[,] grid;
        public long seed;
        public int scoreGain;
        public int depth;
        public string path;
        public int mergedCells;
        public int biggestCell;
        public double heuristicScore;

        public SearchNode(int[,] grid, long seed, int scoreGain, int depth, string path, int mergedCells, int biggestCell)
        {
            this.grid = grid;
            this.seed = seed;
            this.scoreGain = scoreGain;
            this.depth = depth;
            this.path = path;
            this.mergedCells = mergedCells;
            this.biggestCell = biggestCell;
            this.heuristicScore = CalculateHeuristicScore(this);
        }
    }

    static string FindBestMoveSequence(int[,] grid, long seed)
    {
        var stopwatch = Stopwatch.StartNew();
        long timeLimitMs = _isFirstTurn ? 950 : 45;

        var currentBeam = new List<SearchNode> { new SearchNode(Clone(grid), seed, 0, 0, "", 0, 0) };
        string bestPathFound = "U";

        for (int depth = 0; depth < ABSOLUTE_MAX_DEPTH; depth++)
        {
            var nextBeam = new List<SearchNode>();
            int beamWidth = _isFirstTurn ? FIRST_TURN_BEAM_WIDTH : NORMAL_TURN_BEAM_WIDTH;
            int nodesToExpand = Math.Min(currentBeam.Count, beamWidth);

            for (var i = 0; i < nodesToExpand; i++)
            {
                var node = currentBeam[i];
                foreach (var move in POSSIBLE_MOVES)
                {
                    var moveResult = Move(node.grid, move);
                    if (!moveResult.isSameGrid)
                    {
                        var spawnedGrid = moveResult.grid;
                        var nextTileSpawn = GetNextTileSpawn(spawnedGrid, node.seed);
                        if (!double.IsNaN(nextTileSpawn.x))
                        {
                            spawnedGrid[(int)nextTileSpawn.x, (int)nextTileSpawn.y] = nextTileSpawn.value;
                        }
                        var path = node.path + move;
                        var child = new SearchNode(spawnedGrid, nextTileSpawn.nextSeed, moveResult.score, node.depth + 1, path, moveResult.mergedCells, moveResult.biggestCell);
                        nextBeam.Add(child);
                    }
                }
            }

            if (nextBeam.Count == 0) break;

            nextBeam.Sort((a, b) => b.heuristicScore.CompareTo(a.heuristicScore));
            currentBeam = nextBeam;
            bestPathFound = currentBeam[0].path;

            if (stopwatch.ElapsedMilliseconds > timeLimitMs) break;
        }

        var cut = (int)Math.Round(bestPathFound.Length / SAFE_PATH_RATIO);
        if (cut <= 0) cut = 1;
        if (cut > bestPathFound.Length) cut = bestPathFound.Length;
        return bestPathFound.Substring(0, cut);
    }

    static (int[,] grid, int score, bool isSameGrid, int mergedCells, int biggestCell) Move(int[,] grid, char m)
    {
        var sameGrid = true;
        var _grid = Clone(grid);
        var score = 0;
        var mergedCells = 0;
        var biggestCell = 0;

        var indexStart = (m == 'U' || m == 'L') ? 0 : BOARD_SIZE - 1;
        var indexEnd = (m == 'U' || m == 'L') ? BOARD_SIZE - 1 : 0;
        var indexInc = (m == 'U' || m == 'L') ? 1 : -1;

        for (var i = 0; i < BOARD_SIZE; i++)
        {
            for (var j = indexStart; j != indexEnd + indexInc; j += indexInc)
            {
                var index1 = (m == 'U' || m == 'D') ? j : i;
                var index2 = (m == 'U' || m == 'D') ? i : j;

                if (_grid[index1, index2] == 0) continue;

                var k = j + indexInc;
                while (k >= 0 && k < BOARD_SIZE)
                {
                    var kIndex1 = (m == 'U' || m == 'D') ? k : i;
                    var kIndex2 = (m == 'U' || m == 'D') ? i : k;

                    if (_grid[kIndex1, kIndex2] == 0) { k += indexInc; continue; }
                    if (_grid[kIndex1, kIndex2] == _grid[index1, index2])
                    {
                        var newval = _grid[index1, index2] * 2;
                        _grid[index1, index2] = newval;
                        _grid[kIndex1, kIndex2] = 0;
                        score += newval;
                        mergedCells++;
                        sameGrid = false;
                    }
                    break;
                }
            }

            for (var j = indexStart; j != indexEnd + indexInc; j += indexInc)
            {
                var index1 = (m == 'U' || m == 'D') ? j : i;
                var index2 = (m == 'U' || m == 'D') ? i : j;

                if (_grid[index1, index2] != 0) continue;

                var k = j + indexInc;
                while (k >= 0 && k < BOARD_SIZE)
                {
                    var kIndex1 = (m == 'U' || m == 'D') ? k : i;
                    var kIndex2 = (m == 'U' || m == 'D') ? i : k;
                    if (_grid[kIndex1, kIndex2] != 0)
                    {
                        _grid[index1, index2] = _grid[kIndex1, kIndex2];
                        _grid[kIndex1, kIndex2] = 0;
                        sameGrid = false;
                        break;
                    }
                    k += indexInc;
                }
            }
        }

        for (int i = 0; i < BOARD_SIZE; i++) for (int j = 0; j < BOARD_SIZE; j++) if (_grid[i, j] > biggestCell) biggestCell = _grid[i, j];

        return (_grid, score, sameGrid, mergedCells, biggestCell);
    }

    static (double x, double y, int value, long nextSeed) GetNextTileSpawn(int[,] grid, long seed)
    {
        int freeCellCount = 0;
        for (var y = 0; y < BOARD_SIZE; y++)
            for (var x = 0; x < BOARD_SIZE; x++)
                if (grid[x, y] == 0) _freeCellBuffer[freeCellCount++] = x + y * BOARD_SIZE;

        if (freeCellCount == 0) return (double.NaN, double.NaN, 0, seed);
        var spawnIndex = _freeCellBuffer[(int)(seed % freeCellCount)];
        var value = ((seed & 0x10) == 0) ? 2 : 4;
        var sx = spawnIndex % BOARD_SIZE;
        var sy = spawnIndex / BOARD_SIZE;
        var nextSeed = (seed * seed) % 50515093L;
        return (sx, sy, value, nextSeed);
    }

    static double CalculateHeuristicScore(SearchNode node)
    {
        var grid = node.grid;
        var monotonicity = CalculateMonotonicity(grid);
        var score = node.scoreGain * HEURISTIC_WEIGHTS[0] + node.mergedCells * HEURISTIC_WEIGHTS[1] + node.biggestCell * HEURISTIC_WEIGHTS[3];
        score -= monotonicity * HEURISTIC_WEIGHTS[4];
        if (grid[0, 0] == node.biggestCell || grid[0, 3] == node.biggestCell || grid[3, 0] == node.biggestCell || grid[3, 3] == node.biggestCell) score *= HEURISTIC_WEIGHTS[2];
        return score;
    }

    static int CalculateMonotonicity(int[,] grid)
    {
        var monotonicity = 0;
        for (var i = 0; i < BOARD_SIZE; i++)
        {
            for (var j = 0; j < BOARD_SIZE - 1; j++)
            {
                monotonicity += Math.Abs(grid[i, j] - grid[i, j + 1]);
            }
        }
        return monotonicity;
    }

    static int[,] Clone(int[,] src)
    {
        var dst = new int[BOARD_SIZE, BOARD_SIZE];
        Array.Copy(src, dst, src.Length);
        return dst;
    }
}