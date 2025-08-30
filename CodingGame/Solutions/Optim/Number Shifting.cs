using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Solution
{
    public static void Main(string[] args)
    {
        WriteLine("first_level");
        
        while (true)
        {
            var line = ReadLine();
            if (string.IsNullOrEmpty(line)) break;

            var inputs = line.Split(' ');
            var width = int.Parse(inputs[0]);
            var height = int.Parse(inputs[1]);
            var cells = new int[height, width];
            for (var i = 0; i < height; i++)
            {
                inputs = ReadLine().Split(' ');
                for (var j = 0; j < width; j++)
                {
                    cells[i, j] = int.Parse(inputs[j]);
                }
            }
            WriteError($"[DEBUG] Received level: {width}x{height}");

            var initialGrid = new Grid(width, height, cells);
            var solver = new LahcSolver(initialGrid, WriteError);

            var stopwatch = Stopwatch.StartNew();
            var solutionMoves = solver.Solve();
            stopwatch.Stop();
            WriteError($"[DEBUG] Total calculation time: {stopwatch.ElapsedMilliseconds}ms");

            if (solutionMoves != null)
            {
                foreach (var move in solutionMoves)
                {
                    WriteLine(move.ToString());
                }
            }
        }
    }

    private enum Direction { U, D, L, R }
    private enum Operation { Add, Subtract }

    private struct Move
    {
        public int X;
        public int Y;
        public Direction Dir;
        public Operation Op;

        public Move(int x, int y, Direction dir, Operation op)
        {
            X = x;
            Y = y;
            Dir = dir;
            Op = op;
        }

        public override string ToString()
        {
            var dirChar = Dir switch
            {
                Direction.U => 'U',
                Direction.D => 'D',
                Direction.L => 'L',
                Direction.R => 'R',
                _ => '?'
            };
            var opChar = Op == Operation.Add ? '+' : '-';
            return $"{X} {Y} {dirChar} {opChar}";
        }
    }

    private class Grid
    {
        public int Width;
        public int Height;
        public int[,] Cells;

        public Grid(int width, int height, int[,] cells)
        {
            Width = width;
            Height = height;
            Cells = cells;
        }

        public Grid(Grid other)
        {
            Width = other.Width;
            Height = other.Height;
            Cells = new int[Height, Width];
            Array.Copy(other.Cells, this.Cells, other.Cells.Length);
        }

        public bool ApplyMove(Move move)
        {
            var value = Cells[move.Y, move.X];
            if (value == 0) return false;

            var (dx, dy) = GetDestination(move.X, move.Y, move.Dir, value);

            if (dx < 0 || dx >= Width || dy < 0 || dy >= Height) return false;
            
            var destValue = Cells[dy, dx];
            if (destValue == 0) return false;

            var newValue = move.Op == Operation.Add ? destValue + value : Math.Abs(destValue - value);

            Cells[dy, dx] = newValue;
            Cells[move.Y, move.X] = 0;

            return true;
        }

        public int CalculateScore()
        {
            var numNonZero = 0;
            var totalSum = 0;
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    if (Cells[y, x] > 0)
                    {
                        numNonZero++;
                        totalSum += Cells[y, x];
                    }
                }
            }
            return numNonZero * 10000 + totalSum;
        }

        public List<Move> GetAllPossibleMoves()
        {
            var moves = new List<Move>();
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    if (Cells[y, x] > 0)
                    {
                        var value = Cells[y, x];
                        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                        {
                            var (dx, dy) = GetDestination(x, y, dir, value);
                            if (dx >= 0 && dx < Width && dy >= 0 && dy < Height && Cells[dy, dx] > 0)
                            {
                                moves.Add(new Move(x, y, dir, Operation.Add));
                                moves.Add(new Move(x, y, dir, Operation.Subtract));
                            }
                        }
                    }
                }
            }
            return moves;
        }
        
        private (int, int) GetDestination(int x, int y, Direction dir, int value)
        {
            return dir switch
            {
                Direction.U => (x, y - value),
                Direction.D => (x, y + value),
                Direction.L => (x - value, y),
                Direction.R => (x + value, y),
                _ => (-1, -1)
            };
        }
    }

    private class LahcSolver
    {
        private readonly Grid _initialGrid;
        private readonly Random _random = new Random();
        private readonly Action<string> _debugLogger;
        private const int ListLength = 1000;
        private const int MaxSolutionLength = 150;
        private const int TimeLimitMs = 950;

        public LahcSolver(Grid initialGrid, Action<string> debugLogger)
        {
            _initialGrid = initialGrid;
            _debugLogger = debugLogger ?? (_ => {});
        }

        public List<Move> Solve()
        {
            var stopwatch = Stopwatch.StartNew();
            var bestSolution = new List<Move>();
            var bestScore = _initialGrid.CalculateScore();
            if (bestScore == 0) return bestSolution;
            
            while (stopwatch.ElapsedMilliseconds < TimeLimitMs)
            {
                var currentSolution = new List<Move>();
                if (bestSolution.Count > 0 && _random.Next(2) == 0)
                {
                    currentSolution = new List<Move>(bestSolution);
                }

                var (currentScore, isValid) = Evaluate(currentSolution);
                if (!isValid) currentScore = int.MaxValue;
                
                var costHistory = new int[ListLength];
                for (var i = 0; i < ListLength; i++) costHistory[i] = currentScore;
                
                for (var iteration = 0; iteration < 50000; iteration++)
                {
                    if (stopwatch.ElapsedMilliseconds >= TimeLimitMs) break;

                    var candidateSolution = Mutate(currentSolution);
                    var (candidateScore, isCandidateValid) = Evaluate(candidateSolution);

                    if (!isCandidateValid) continue;
                    
                    var historyIndex = iteration % ListLength;
                    if (candidateScore <= costHistory[historyIndex])
                    {
                        currentSolution = candidateSolution;
                        currentScore = candidateScore;
                    }

                    costHistory[historyIndex] = currentScore;

                    if (currentScore < bestScore)
                    {
                        bestScore = currentScore;
                        bestSolution = new List<Move>(currentSolution);
                        _debugLogger($"[DEBUG] New best: score={bestScore}, moves={bestSolution.Count}, time={stopwatch.ElapsedMilliseconds}ms");
                        if (bestScore == 0)
                        {
                            _debugLogger($"[DEBUG] Solution found! Time: {stopwatch.ElapsedMilliseconds}ms");
                            return bestSolution;
                        }
                    }
                }
            }
            _debugLogger($"[DEBUG] Time out. Best score: {bestScore}");
            return bestSolution;
        }

        private (int score, bool isValid) Evaluate(List<Move> solution)
        {
            var grid = new Grid(_initialGrid);
            foreach (var move in solution)
            {
                if (!grid.ApplyMove(move))
                {
                    return (int.MaxValue, false);
                }
            }
            return (grid.CalculateScore(), true);
        }

        private List<Move> Mutate(List<Move> solution)
        {
            var newSolution = new List<Move>(solution);
            var mutationType = _random.Next(100);

            if (newSolution.Count == 0 || (mutationType < 50 && newSolution.Count < MaxSolutionLength))
            {
                var tempGrid = new Grid(_initialGrid);
                foreach (var move in newSolution) { tempGrid.ApplyMove(move); }
                var possibleMoves = tempGrid.GetAllPossibleMoves();
                if (possibleMoves.Count > 0)
                {
                    newSolution.Add(possibleMoves[_random.Next(possibleMoves.Count)]);
                }
            }
            else if (newSolution.Count > 0 && mutationType < 75)
            {
                newSolution.RemoveAt(_random.Next(newSolution.Count));
            }
            else if (newSolution.Count > 0 && mutationType < 90)
            {
                var index = _random.Next(newSolution.Count);
                var move = newSolution[index];
                move.Op = move.Op == Operation.Add ? Operation.Subtract : Operation.Add;
                newSolution[index] = move;
            }
            else if (newSolution.Count > 1) 
            {
                var index = _random.Next(1, newSolution.Count);
                newSolution.RemoveRange(index, newSolution.Count - index);
            }
            return newSolution;
        }
    }

    private static void WriteLine(string message) => Console.WriteLine(message);
    private static string ReadLine() => Console.ReadLine();
    private static void WriteError(string message) => Console.Error.WriteLine(message);
}