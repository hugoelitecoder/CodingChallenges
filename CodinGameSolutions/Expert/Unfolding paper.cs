using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var N = int.Parse(Console.ReadLine());
        var inputs = Console.ReadLine().Split(' ');
        var W = int.Parse(inputs[0]);
        var H = int.Parse(inputs[1]);
        var grid = new char[H][];
        for (var i = 0; i < H; i++)
        {
            grid[i] = Console.ReadLine().ToCharArray();
        }

        var solver = new PaperFoldingSolver(N, W, H, grid);
        var result = solver.Solve();

        Console.WriteLine(result);
    }
}

class PaperFoldingSolver
{
    private readonly int _n;
    private readonly int _w;
    private readonly int _h;
    private readonly char[][] _grid;

    private const int TYPE_RIGHT = 1;
    private const int TYPE_TOP = 2;
    private const int TYPE_LEFT = 4;
    private const int TYPE_BOTTOM = 8;
    
    private static readonly Matrix _transitionMatrix = new Matrix(new long[16, 16] {
        {4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
        {0, 2, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 1
        {2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 2
        {0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 3
        {2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 4
        {0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 5
        {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 6
        {0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 7
        {0, 0, 2, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0}, // 8
        {0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0}, // 9
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0}, // 10
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0}, // 11
        {0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0}, // 12
        {0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0}, // 13
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0}, // 14
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}  // 15
    });

    public PaperFoldingSolver(int n, int w, int h, char[][] grid)
    {
        _n = n;
        _w = w;
        _h = h;
        _grid = grid;
    }

    public long Solve()
    {
        var initialParts = FindInitialComponents();
        if (_n == 0)
        {
            return initialParts.Sum();
        }

        var transitionPowered = Matrix.Power(_transitionMatrix, _n);
        
        var finalParts = new long[16];
        for (var j = 0; j < 16; j++)
        {
            long sum = 0;
            for (var i = 0; i < 16; i++)
            {
                sum += initialParts[i] * transitionPowered.Values[i, j];
            }
            finalParts[j] = sum;
        }

        return finalParts.Sum();
    }

    private long[] FindInitialComponents()
    {
        var parts = new long[16];
        var visited = new bool[_h, _w];
        for (var r = 0; r < _h; r++)
        {
            for (var c = 0; c < _w; c++)
            {
                if (_grid[r][c] == '#' && !visited[r, c])
                {
                    var type = GetPartType(r, c, visited);
                    parts[type]++;
                }
            }
        }
        return parts;
    }

    private int GetPartType(int startR, int startC, bool[,] visited)
    {
        var type = 0;
        var q = new Queue<(int r, int c)>();
        q.Enqueue((startR, startC));
        visited[startR, startC] = true;

        while (q.Count > 0)
        {
            var (r, c) = q.Dequeue();

            if (r == 0) type |= TYPE_TOP;
            if (r == _h - 1) type |= TYPE_BOTTOM;
            if (c == 0) type |= TYPE_LEFT;
            if (c == _w - 1) type |= TYPE_RIGHT;

            int[] dr = { -1, 1, 0, 0 };
            int[] dc = { 0, 0, -1, 1 };

            for (var i = 0; i < 4; i++)
            {
                var nr = r + dr[i];
                var nc = c + dc[i];

                if (nr >= 0 && nr < _h && nc >= 0 && nc < _w &&
                    _grid[nr][nc] == '#' && !visited[nr, nc])
                {
                    visited[nr, nc] = true;
                    q.Enqueue((nr, nc));
                }
            }
        }
        return type;
    }
}

class Matrix
{
    private const int SIZE = 16;
    public long[,] Values { get; }

    public Matrix(long[,] values)
    {
        Values = values;
    }

    public static Matrix Identity()
    {
        var values = new long[SIZE, SIZE];
        for (var i = 0; i < SIZE; i++)
        {
            values[i, i] = 1;
        }
        return new Matrix(values);
    }

    public static Matrix Multiply(Matrix a, Matrix b)
    {
        var resultValues = new long[SIZE, SIZE];
        for (var i = 0; i < SIZE; i++)
        {
            for (var j = 0; j < SIZE; j++)
            {
                long sum = 0;
                for (var k = 0; k < SIZE; k++)
                {
                    sum += a.Values[i, k] * b.Values[k, j];
                }
                resultValues[i, j] = sum;
            }
        }
        return new Matrix(resultValues);
    }

    public static Matrix Power(Matrix baseMatrix, int exp)
    {
        var result = Identity();
        var currentPower = baseMatrix;
        while (exp > 0)
        {
            if ((exp & 1) == 1)
            {
                result = Multiply(result, currentPower);
            }
            currentPower = Multiply(currentPower, currentPower);
            exp >>= 1;
        }
        return result;
    }
}