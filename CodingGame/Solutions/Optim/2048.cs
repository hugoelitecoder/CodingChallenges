using System;
using System.Collections.Generic;

public class Solution
{
    const int BOARD_SIZE = 4;
    static readonly int[,] BLANK_GRID = new int[BOARD_SIZE, BOARD_SIZE];
    const int BEAM_WIDTH = 40;
    const int FIRST_TURN_BEAM_WIDTH = 50;
    static readonly char[] POSSIBLE_MOVE = { 'U', 'D', 'L', 'R' };
    static readonly double[] POIDS = { 97.30119, 90.68706, 13.228226, 67.991486, 12.232829 };
    const int MAX_DEPTH = 60;
    const int FIRST_TURN_MAX_DEPTH = 1500;
    const double SAFE_PATH = 1.5;

    static bool firstTurn = true;

    public static void Main(string[] args)
    {
        while (true)
        {
            var seedLine = Console.ReadLine();
            if (seedLine == null) return;
            var seed = long.Parse(seedLine);
            var score = int.Parse(Console.ReadLine());
            var grid = new int[BOARD_SIZE, BOARD_SIZE];
            for (var i = 0; i < BOARD_SIZE; i++)
            {
                var inputs = Console.ReadLine().Split(' ');
                for (var j = 0; j < BOARD_SIZE; j++) grid[i, j] = int.Parse(inputs[j]);
            }
            var m = BestMove(grid, seed);
            firstTurn = false;
            Console.WriteLine(m);
        }
    }

    class TreeGrid
    {
        public int[,] grid;
        public long seed;
        public int score;
        public int depth;
        public string path;
        public int margedCell;
        public int biggestCell;
        public double heuristique;
        public TreeGrid(int[,] grid, long seed, int score, int depth, string path, int margedCell, int biggestCell)
        {
            this.grid = grid;
            this.seed = seed;
            this.score = score;
            this.depth = depth;
            this.path = path;
            this.margedCell = margedCell;
            this.biggestCell = biggestCell;
            this.heuristique = Heuristique(this);
        }
    }

    static string BestMove(int[,] grid, long seed)
    {
        var nodes = new List<TreeGrid> { new TreeGrid(Clone(grid), seed, 0, 0, "", 0, 0) };
        var maxDepth = firstTurn ? FIRST_TURN_MAX_DEPTH : MAX_DEPTH;
        while (nodes[0].depth != maxDepth)
        {
            var next = new List<TreeGrid>();
            var n = firstTurn ? FIRST_TURN_BEAM_WIDTH : BEAM_WIDTH;
            var take = Math.Min(nodes.Count, n);
            for (var i = 0; i < take; i++)
            {
                var node = nodes[i];
                for (var iMove = 0; iMove < 4; iMove++)
                {
                    var res = Move(node.grid, POSSIBLE_MOVE[iMove]);
                    var _grid = res.grid;
                    var gained = res.score;
                    var isSameGrid = res.isSameGrid;
                    var merged = res.margedCell;
                    var biggest = res.biggestCell;
                    if (!isSameGrid)
                    {
                        var preshot = PreshotSpawn(_grid, node.seed);
                        if (!double.IsNaN(preshot.x) && !double.IsNaN(preshot.y))
                        {
                            _grid[(int)preshot.x, (int)preshot.y] = preshot.value;
                        }
                        var path = node.path + POSSIBLE_MOVE[iMove];
                        var child = new TreeGrid(_grid, preshot.seed, gained, node.depth + 1, path, merged, biggest);
                        next.Add(child);
                    }
                }
            }
            if (next.Count == 0)
            {
                if (nodes[0].path == "") nodes[0].path = "U";
                break;
            }
            next.Sort((a, b) => b.heuristique.CompareTo(a.heuristique));
            nodes = next;
        }
        var pathOut = nodes[0].path;
        var cut = (int)Math.Round(pathOut.Length / SAFE_PATH);
        if (cut < 0) cut = 0;
        if (cut > pathOut.Length) cut = pathOut.Length;
        return pathOut.Substring(0, cut);
    }

    static (int[,] grid, int score, bool isSameGrid, int margedCell, int biggestCell) Move(int[,] grid, char m)
    {
        var sameGrid = true;
        var _grid = Clone(grid);
        var score = 0;
        var margedCell = 0;
        var biggestCell = 0;

        var indexStart = (m == 'U' || m == 'L') ? 0 : BOARD_SIZE - 1;
        var indexEnd = (m == 'U' || m == 'L') ? BOARD_SIZE - 1 : 0;
        var indexInc = (m == 'U' || m == 'L') ? 1 : -1;

        for (var i = indexStart; i != indexEnd + indexInc; i += indexInc)
        {
            for (var j = indexStart; j != indexEnd + indexInc; j += indexInc)
            {
                var index1 = (m == 'U' || m == 'D') ? i : j;
                var index2 = (m == 'U' || m == 'D') ? j : i;
                var end = indexEnd + indexInc;

                var k = ((m == 'U' || m == 'D') ? index1 : index2) + indexInc;

                if (_grid[index1, index2] == 0)
                {
                    for (; k != end; k += indexInc)
                    {
                        var kIndex1 = (m == 'U' || m == 'D') ? k : index1;
                        var kIndex2 = (m == 'U' || m == 'D') ? index2 : k;
                        if (_grid[kIndex1, kIndex2] != 0)
                        {
                            sameGrid = false;
                            _grid[index1, index2] = _grid[kIndex1, kIndex2];
                            _grid[kIndex1, kIndex2] = 0;
                            break;
                        }
                    }
                }

                if (_grid[index1, index2] != 0)
                {
                    for (; k != end; k += indexInc)
                    {
                        var kIndex1 = (m == 'U' || m == 'D') ? k : index1;
                        var kIndex2 = (m == 'U' || m == 'D') ? index2 : k;
                        if (_grid[kIndex1, kIndex2] == _grid[index1, index2])
                        {
                            sameGrid = false;
                            var cellNumber = _grid[index1, index2] * 2;
                            _grid[index1, index2] = cellNumber;
                            _grid[kIndex1, kIndex2] = 0;
                            score += cellNumber;
                            margedCell++;
                            break;
                        }
                        else if (_grid[kIndex1, kIndex2] != 0)
                        {
                            break;
                        }
                    }
                }

                if (_grid[index1, index2] > biggestCell) biggestCell = _grid[index1, index2];
            }
        }

        return (_grid, score, sameGrid, margedCell, biggestCell);
    }

    static (double x, double y, int value, long seed) PreshotSpawn(int[,] grid, long seed)
    {
        var free = new List<int>();
        for (var y = 0; y < BOARD_SIZE; y++)
        {
            for (var x = 0; x < BOARD_SIZE; x++)
            {
                if (grid[x, y] == 0) free.Add(x + y * BOARD_SIZE);
            }
        }
        if (free.Count == 0) return (double.NaN, double.NaN, 0, seed);
        var spawnIndex = free[(int)(seed % free.Count)];
        var value = ((seed & 0x10) == 0) ? 2 : 4;
        var sx = spawnIndex % BOARD_SIZE;
        var sy = spawnIndex / BOARD_SIZE;
        var nextSeed = (seed * seed) % 50515093L;
        return (sx, sy, value, nextSeed);
    }

    static double Heuristique(TreeGrid node)
    {
        var grid = node.grid;
        var monotonie = CalculMonotonie(grid);
        var score = node.score * POIDS[0] + node.margedCell * POIDS[1] + node.biggestCell * POIDS[3];
        score -= monotonie * POIDS[4];
        if (grid[0, 0] == node.biggestCell || grid[0, 3] == node.biggestCell || grid[3, 0] == node.biggestCell || grid[3, 3] == node.biggestCell) score *= POIDS[2];
        return score;
    }

    static int CalculMonotonie(int[,] grid)
    {
        var monotonie = 0;
        for (var i = 0; i < BOARD_SIZE; i++)
        {
            var rowMonotonie = 0;
            for (var j = 0; j < BOARD_SIZE - 1; j++)
            {
                var diff = Math.Abs(grid[i, j] - grid[i, j + 1]);
                rowMonotonie += diff;
            }
            monotonie += rowMonotonie;
        }
        return monotonie;
    }

    static int[,] Clone(int[,] src)
    {
        var r = src.GetLength(0);
        var c = src.GetLength(1);
        var dst = new int[r, c];
        for (var i = 0; i < r; i++)
            for (var j = 0; j < c; j++)
                dst[i, j] = src[i, j];
        return dst;
    }
}
