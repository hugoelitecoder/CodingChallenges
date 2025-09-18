using System;
using System.Collections.Generic;
using System.Linq;

namespace MinesweeperSolver
{
    // Represents a grid cell coordinate
    public struct Coord
    {
        public int X { get; }
        public int Y { get; }
        public Coord(int x, int y) { X = x; Y = y; }
    }

    // Represents a clue cell with its numeric value
    public struct Clue : IEquatable<Clue>
    {
        public Coord Pos { get; }
        public int Value { get; }
        public Clue(Coord pos, int value) { Pos = pos; Value = value; }

        public bool Equals(Clue other) => Pos.X == other.Pos.X && Pos.Y == other.Pos.Y;
        public override int GetHashCode() => HashCode.Combine(Pos.X, Pos.Y);
    }

    class Program
    {
        static int n;
        static char[][] inputGrid;
        static char[][] outputGrid;
        static List<Clue> clues;

        static void Main(string[] args)
        {
            // Read grid size
            n = int.Parse(Console.ReadLine());
            inputGrid = new char[n][];
            outputGrid = new char[n][];

            // Initialize grids
            for (int y = 0; y < n; y++)
            {
                inputGrid[y] = Console.ReadLine().ToCharArray();
                outputGrid[y] = Enumerable.Repeat(' ', n).ToArray();
            }

            // Collect clues
            clues = new List<Clue>();
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
                if (inputGrid[y][x] != '.')
                    clues.Add(new Clue(new Coord(x, y), inputGrid[y][x] - '0'));

            // Solve obvious ones
            foreach (var clue in clues)
            {
                char c = inputGrid[clue.Pos.Y][clue.Pos.X];
                if (c == '0') FillRemaining(clue.Pos, '.');
                else if (IsInner(clue.Pos) && c == '9') FillRemaining(clue.Pos, '#');
                else if (IsEdge(clue.Pos) && c == '6') FillRemaining(clue.Pos, '#');
                else if (IsCorner(clue.Pos) && c == '4') FillRemaining(clue.Pos, '#');
            }

            // Iterative constraint propagation
            bool progress;
            do
            {
                progress = false;
                var statsToSolve = new List<Clue>();

                foreach (var clue in clues)
                {
                    var (unsolved, filled) = NeighbourStats(clue);
                    int remaining = clue.Value - filled;

                    if (remaining == 0)
                    {
                        FillRemaining(clue.Pos, '.');
                        statsToSolve.Add(clue);
                        progress = true;
                    }
                    else if (remaining == unsolved)
                    {
                        FillRemaining(clue.Pos, '#');
                        statsToSolve.Add(clue);
                        progress = true;
                    }
                }

                // Remove solved clues
                foreach (var c in statsToSolve)
                    clues.Remove(c);

            } while (progress && clues.Count > 0);

            // Output result
            foreach (var row in outputGrid)
                Console.WriteLine(new string(row));
        }

        // Fill all unsolved neighbors of pos with the given symbol
        static void FillRemaining(Coord pos, char symbol)
        {
            foreach (var nb in Neighbourhood(pos))
                if (outputGrid[nb.Y][nb.X] == ' ')
                    outputGrid[nb.Y][nb.X] = symbol;
        }

        // Compute how many neighbors are unsolved (' ') and filled ('#')
        static (int unsolved, int filled) NeighbourStats(Clue clue)
        {
            int unsolved = 0, filled = 0;
            foreach (var nb in Neighbourhood(clue.Pos))
            {
                char outc = outputGrid[nb.Y][nb.X];
                if (outc == ' ') unsolved++;
                if (outc == '#') filled++;
            }
            return (unsolved, filled);
        }

        // Return valid neighboring coordinates (including diagonals)
        static IEnumerable<Coord> Neighbourhood(Coord c)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = c.X + dx, ny = c.Y + dy;
                if (nx >= 0 && nx < n && ny >= 0 && ny < n)
                    yield return new Coord(nx, ny);
            }
        }

        static bool IsInner(Coord c) => c.X > 0 && c.X < n - 1 && c.Y > 0 && c.Y < n - 1;
        static bool IsCorner(Coord c) => (c.X == 0 || c.X == n - 1) && (c.Y == 0 || c.Y == n - 1);
        static bool IsEdge(Coord c) => ((c.X == 0 || c.X == n - 1) || (c.Y == 0 || c.Y == n - 1)) && !IsCorner(c);
    }
}
