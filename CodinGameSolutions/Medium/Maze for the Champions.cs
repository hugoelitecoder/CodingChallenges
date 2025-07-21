using System;
using System.Collections.Generic;


class Board {
    private string[][] board;
    public int Width { get; }
    public int Height { get; }

    public Board() { }

    public Board(string[][] board, int width, int height) {
        this.board = board;
        this.Width = width;
        this.Height = height;
    }

    public void Write(List<Point> points) {
        foreach (var p in points) {
            board[p.X][p.Y] = p.Motif;
        }
    }

    public void Display() {
        for (int row = 0; row < Height; row++) {
            string line = "";
            for (int col = 0; col < Width; col++) {
                line += board[col][row];
            }
            Console.WriteLine(line);
        }
    }

    public Point GetEntryPoint() {
        for (int x = 0; x < Width; x++)
            if (board[x][0] == "v") return new Point(x, 0, "v");
        for (int x = 0; x < Width; x++)
            if (board[x][Height - 1] == "^") return new Point(x, Height - 1, "^");
        for (int y = 0; y < Height; y++)
            if (board[0][y] == ">") return new Point(0, y, ">");
        for (int y = 0; y < Height; y++)
            if (board[Width - 1][y] == "<") return new Point(Width - 1, y, "<");
        return null;
    }

    public Point GetEndPoint() {
        for (int x = 0; x < Width; x++)
            if (board[x][0] == "^") return new Point(x, 0, "^");
        for (int x = 0; x < Width; x++)
            if (board[x][Height - 1] == "v") return new Point(x, Height - 1, "v");
        for (int y = 0; y < Height; y++)
            if (board[0][y] == "<") return new Point(0, y, "<");
        for (int y = 0; y < Height; y++)
            if (board[Width - 1][y] == ">") return new Point(Width - 1, y, ">");
        return null;
    }

    public Point GetPoint(string s) {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                if (board[x][y] == s)
                    return new Point(x, y, s);
        return null;
    }
}

class Point {
    public int X { get; set; }
    public int Y { get; set; }
    public string Motif { get; set; }

    public Point(int x, int y, string motif) {
        X = x;
        Y = y;
        Motif = motif;
    }

    public override string ToString() {
        return $"Point [{X},{Y}],motif={Motif}]";
    }
}

class Node {
    public int X { get; }
    public int Y { get; }
    public int Dist { get; }

    public Node(int x, int y, int dist) {
        X = x;
        Y = y;
        Dist = dist;
    }

    public override string ToString() {
        return $"Node [x={X}, y={Y}, dist={Dist}]";
    }
}

class Maze {
    private static readonly int[] RowRegular = { -1, 0, 0, 1 };
    private static readonly int[] ColRegular = { 0, -1, 1, 0 };
    private static readonly int[] RowElf     = { -1, 0, 0, 1, -1, 1, -1, 1 };
    private static readonly int[] ColElf     = {  0, -1, 1, 0, -1, 1,  1, -1 };

    private static bool IsValid(string[][] mat, bool[][] visited, int r, int c) {
        return r >= 0 && r < mat.Length
            && c >= 0 && c < mat[0].Length
            && mat[r][c] == "."
            && !visited[r][c];
    }

    private static bool IsWall(string[][] mat, bool[][] visited, int r, int c) {
        return r >= 0 && r < mat.Length
            && c >= 0 && c < mat[0].Length
            && mat[r][c] == "#"
            && !visited[r][c];
    }

    public static List<Point> FindShortestPathLengthElf(string[][] mat, Point start, Point end) {
        return FindShortestPathLength(RowElf.Length, RowElf, ColElf, mat, start, end);
    }

    public static List<Point> FindShortestPathLengthWarrior(string[][] mat, Point start, Point end) {
        return FindShortestPathLength(RowRegular.Length, RowRegular, ColRegular, mat, start, end);
    }

    public static List<Point> FindShortestPathLengthMage(List<Point> path) {
        var result = new List<Point>();
        string motif = "";
        foreach (var p in path) {
            if (motif == "") {
                motif = p.Motif;
                result.Add(p);
            } else if (motif != p.Motif) {
                motif = p.Motif;
                result.Add(p);
            }
        }
        return result;
    }

    private static List<Point> FindShortestPathLength(int steps, int[] dRow, int[] dCol,
                                                      string[][] mat, Point start, Point end) {
        int M = mat.Length, N = mat[0].Length;
        var visited = new bool[M][];
        for (int i = 0; i < M; i++) visited[i] = new bool[N];

        var queue = new Queue<Point>();
        var paths = new List<List<Point>>();

        visited[start.X][start.Y] = true;
        queue.Enqueue(new Point(start.X, start.Y, start.Motif));
        paths.Add(new List<Point> { new Point(start.X, start.Y, start.Motif) });

        while (queue.Count > 0) {
            var node = queue.Dequeue();
            int x = node.X, y = node.Y;

            for (int k = 0; k < steps; k++) {
                int nx = x + dRow[k], ny = y + dCol[k];
                if (nx >= 0 && ny >= 0 && ny < N && nx < M
                    && mat[nx][ny] == end.Motif
                    && !IsSamePoint(start, new Point(nx, ny, start.Motif))) {
                    string dir = SetDirection(dRow[k], dCol[k]);
                    foreach (var path in paths)
                        foreach (var p in path)
                            if (p.X == x && p.Y == y) {
                                p.Motif = dir;
                                path[0].Motif = start.Motif;
                                return path;
                            }
                }

                if (IsValid(mat, visited, nx, ny)) {
                    visited[nx][ny] = true;
                    string dir = SetDirection(dRow[k], dCol[k]);
                    var np = new Point(nx, ny, dir);
                    queue.Enqueue(np);

                    var newPaths = new List<List<Point>>();
                    foreach (var path in paths) {
                        var last = path[path.Count - 1];
                        if (last.X == x && last.Y == y) {
                            var copy = new List<Point>();
                            foreach (var pt in path) {
                                last.Motif = dir;
                                copy.Add(new Point(pt.X, pt.Y, pt.Motif));
                            }
                            copy.Add(new Point(last.X + dRow[k], last.Y + dCol[k], last.Motif));
                            newPaths.Add(copy);
                        }
                    }
                    paths.AddRange(newPaths);
                }
            }
        }

        return null;
    }

    private static bool IsSamePoint(Point a, Point b) =>
        a.X == b.X && a.Y == b.Y && a.Motif == b.Motif;

    public static List<Point> FindShortestPathLengthDwarf(string[][] mat, Point start, Point end) {
        int M = mat.Length, N = mat[0].Length;
        var visited = new bool[M][];
        for (int i = 0; i < M; i++) visited[i] = new bool[N];

        var queue = new Queue<Point>();
        var paths = new List<List<Point>>();

        visited[start.X][start.Y] = true;
        queue.Enqueue(new Point(start.X, start.Y, start.Motif));
        paths.Add(new List<Point> { new Point(start.X, start.Y, start.Motif) });

        while (queue.Count > 0) {
            var node = queue.Dequeue();
            int x = node.X, y = node.Y;

            for (int k = 0; k < 4; k++) {
                int nx = x + RowRegular[k], ny = y + ColRegular[k];
                if (nx >= 0 && ny >= 0 && ny < N && nx < M
                    && mat[nx][ny] == end.Motif
                    && !IsSamePoint(start, new Point(nx, ny, start.Motif))) {
                    string dir = SetDirection(RowRegular[k], ColRegular[k]);
                    foreach (var path in paths)
                        foreach (var p in path)
                            if (p.X == x && p.Y == y) {
                                p.Motif = dir;
                                path[0].Motif = start.Motif;
                                return path;
                            }
                }

                int nx2 = x + RowRegular[k] * 2, ny2 = y + ColRegular[k] * 2;
                if (IsValid(mat, visited, nx, ny)
                    || (IsValid(mat, visited, nx2, ny2) && IsWall(mat, visited, nx, ny))) {
                    visited[nx][ny] = true;
                    string dir = SetDirection(RowRegular[k], ColRegular[k]);
                    var np = new Point(nx, ny, dir);
                    queue.Enqueue(np);

                    var newPaths = new List<List<Point>>();
                    foreach (var path in paths) {
                        var last = path[path.Count - 1];
                        if (last.X == x && last.Y == y) {
                            var copy = new List<Point>();
                            foreach (var pt in path) {
                                last.Motif = dir;
                                copy.Add(new Point(pt.X, pt.Y, pt.Motif));
                            }
                            copy.Add(new Point(last.X + RowRegular[k], last.Y + ColRegular[k], last.Motif));
                            newPaths.Add(copy);
                        }
                    }
                    paths.AddRange(newPaths);
                }
            }
        }

        return null;
    }

    public static string SetDirection(int row, int col) {
        if (row == -1 && col == 0) return "<";
        if (row == 1  && col == 0) return ">";
        if (row == 0  && col == -1) return "^";
        if (row == 0  && col == 1) return "v";
        if (row != 0 && col != 0) return "o";
        return "e";
    }
}

class Program {
    static void Main(string[] args) {
        int width  = int.Parse(Console.ReadLine());
        int height = int.Parse(Console.ReadLine());

        var board = new string[width][];
        for (int x = 0; x < width; x++)
            board[x] = new string[height];

        for (int y = 0; y < height; y++) {
            var line = Console.ReadLine();
            for (int x = 0; x < width; x++)
                board[x][y] = line[x].ToString();
        }

        var b = new Board(board, width, height);
        var entry   = b.GetEntryPoint();
        var outdoors= b.GetEndPoint();

        var chemins = new Dictionary<int, List<Point>>();
        chemins.Add(2, Maze.FindShortestPathLengthWarrior(board, entry, outdoors));
        chemins.Add(3, Maze.FindShortestPathLengthDwarf (board, entry, outdoors));
        chemins.Add(4, Maze.FindShortestPathLengthElf   (board, entry, outdoors));
        chemins.Add(5, Maze.FindShortestPathLengthMage  (chemins[2]));

        ConsolidationScore(chemins, b);
    }

    static void ConsolidationScore(Dictionary<int, List<Point>> chemins, Board board) {
        List<Point> bestPath = null;
        int bestScore = int.MaxValue;
        string champion = "";

        foreach (var kvp in chemins) {
            int length = kvp.Value?.Count ?? 0;
            int score = kvp.Key * (1 + length);
            if (score < bestScore) {
                bestScore = score;
                bestPath  = kvp.Value;
                champion  = kvp.Key switch {
                    2 => "WARRIOR",
                    3 => "DWARF",
                    4 => "ELF",
                    5 => "MAGE",
                    _ => ""
                };
            }
        }

        Console.WriteLine($"{champion} {bestScore}");
        board.Write(bestPath);
        board.Display();
    }
}
