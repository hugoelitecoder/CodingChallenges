using System;
using System.Text;
using System.Diagnostics;

public class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        int n = int.Parse(Console.ReadLine());
        string[] faceLines = new string[n * CubeMaze.FaceCount];
        for (int i = 0; i < faceLines.Length; i++)
            faceLines[i] = Console.ReadLine();

        string wallFollowSide = Console.ReadLine();

        CubeMaze maze = CubeMazeBuilder.BuildFromInput(n, faceLines);
        PikaptchaSimulator simulator = new PikaptchaSimulator(maze, wallFollowSide);
        simulator.Run();

        Console.Write(simulator.GetResult());
        Console.Error.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms");
    }
}

public class CubeMaze
{
    public const int FaceCount = 6;
    public const char Wall = '#';
    public const char Empty = '0';

    public int Size { get; }
    public char[][][] Faces { get; }
    public PikaptchaPosition StartPosition { get; }

    public CubeMaze(int size, char[][][] faces, PikaptchaPosition start)
    {
        Size = size;
        Faces = faces;
        StartPosition = start;
    }

    public char GetCell(int face, int row, int col) => Faces[face][row][col];

    public void SetCell(int face, int row, int col, char value) => Faces[face][row][col] = value;
}

public struct PikaptchaPosition
{
    public int Face, Row, Col, Direction;
    public PikaptchaPosition(int face, int row, int col, int direction)
    {
        Face = face;
        Row = row;
        Col = col;
        Direction = direction;
    }
}

public static class CubeMazeBuilder
{
    public static CubeMaze BuildFromInput(int n, string[] faceLines)
    {
        var faces = new char[CubeMaze.FaceCount][][];
        PikaptchaPosition? start = null;

        for (int f = 0; f < CubeMaze.FaceCount; f++)
        {
            faces[f] = new char[n][];
            for (int r = 0; r < n; r++)
            {
                faces[f][r] = faceLines[f * n + r].ToCharArray();
                for (int c = 0; c < n; c++)
                {
                    char ch = faces[f][r][c];
                    if (ch != CubeMaze.Empty && ch != CubeMaze.Wall)
                    {
                        int d = ch switch
                        {
                            '^' => 0,
                            '>' => 1,
                            'v' => 2,
                            '<' => 3,
                            _ => throw new Exception($"Unknown start direction: {ch}")
                        };
                        start = new PikaptchaPosition(f, r, c, d);
                        faces[f][r][c] = CubeMaze.Empty;
                    }
                }
            }
        }
        if (!start.HasValue)
            throw new Exception("No Pikaptcha starting position found.");
        return new CubeMaze(n, faces, start.Value);
    }
}

public class PikaptchaSimulator
{
    private static readonly int[] DR = { -1, 0, 1, 0 };
    private static readonly int[] DC = { 0, 1, 0, -1 };
    private static readonly int[] RightWallFollow = { 1, 0, -1, 2 };
    private static readonly int[] LeftWallFollow = { -1, 0, 1, 2 };

    private readonly CubeMaze _maze;
    private readonly string _wallSide;
    private readonly int[][][] _visitCounts;

    private int _face, _row, _col, _dir;
    private readonly int _startFace, _startRow, _startCol;

    public PikaptchaSimulator(CubeMaze maze, string wallSide)
    {
        _maze = maze;
        _wallSide = wallSide;
        _visitCounts = new int[CubeMaze.FaceCount][][];
        for (int f = 0; f < CubeMaze.FaceCount; f++)
        {
            _visitCounts[f] = new int[_maze.Size][];
            for (int r = 0; r < _maze.Size; r++)
                _visitCounts[f][r] = new int[_maze.Size];
        }

        _face = _startFace = maze.StartPosition.Face;
        _row = _startRow = maze.StartPosition.Row;
        _col = _startCol = maze.StartPosition.Col;
        _dir = maze.StartPosition.Direction;
    }

    public void Run()
    {
        var turnDeltas = _wallSide == "R" ? RightWallFollow : LeftWallFollow;
        do
        {
            bool moved = false;
            foreach (var turn in turnDeltas)
            {
                int nd = (_dir + turn + 4) % 4;
                int nr = _row + DR[nd];
                int nc = _col + DC[nd];
                int nf = _face, finalR = nr, finalC = nc, finalD = nd;

                if (nr < 0 || nr >= _maze.Size || nc < 0 || nc >= _maze.Size)
                    (nf, finalR, finalC, finalD) = GetCubeTransition(_face, _row, _col, nd, _maze.Size);

                if (_maze.GetCell(nf, finalR, finalC) != CubeMaze.Wall)
                {
                    _face = nf; _row = finalR; _col = finalC; _dir = finalD;
                    _visitCounts[_face][_row][_col]++;
                    moved = true;
                    break;
                }
            }
            if (!moved) break;
        }
        while (_face != _startFace || _row != _startRow || _col != _startCol);
    }

    public string GetResult()
    {
        var sb = new StringBuilder();
        for (int f = 0; f < CubeMaze.FaceCount; f++)
            for (int r = 0; r < _maze.Size; r++)
            {
                for (int c = 0; c < _maze.Size; c++)
                {
                    char cell = _maze.GetCell(f, r, c);
                    sb.Append(cell == CubeMaze.Wall ? CubeMaze.Wall : (char)('0' + _visitCounts[f][r][c]));
                }
                sb.Append('\n');
            }
        return sb.ToString();
    }

    private (int, int, int, int) GetCubeTransition(int f, int r, int c, int d, int n)
    {
        var N_1 = n - 1;
        switch ((f, d))
        {
            case (0, 0): return (5, N_1, c, 0);
            case (0, 1): return (3, 0, N_1 - r, 2);
            case (0, 2): return (2, 0, c, 2);
            case (0, 3): return (1, 0, r, 2);
            case (1, 0): return (0, c, 0, 1);
            case (1, 1): return (2, r, 0, 1);
            case (1, 2): return (4, N_1 - c, 0, 1);
            case (1, 3): return (5, N_1 - r, 0, 1);
            case (2, 0): return (0, N_1, c, 0);
            case (2, 1): return (3, r, 0, 1);
            case (2, 2): return (4, 0, c, 2);
            case (2, 3): return (1, r, N_1, 3);
            case (3, 0): return (0, N_1 - c, N_1, 3);
            case (3, 1): return (5, N_1 - r, N_1, 3);
            case (3, 2): return (4, c, N_1, 3);
            case (3, 3): return (2, r, N_1, 3);
            case (4, 0): return (2, N_1, c, 0);
            case (4, 1): return (3, N_1, r, 0);
            case (4, 2): return (5, 0, c, 2);
            case (4, 3): return (1, N_1, N_1 - r, 0);
            case (5, 0): return (4, N_1, c, 0);
            case (5, 1): return (3, N_1 - r, N_1, 3);
            case (5, 2): return (0, 0, c, 2);
            case (5, 3): return (1, N_1 - r, 0, 1);
        }
        return (-1, -1, -1, -1);
    }
}
