using System;
using System.Collections.Generic;

class Sudoku
{
    private readonly int N, B;
    private readonly int[,] grid;
    private readonly int[] rowMask, colMask, boxMask;
    private readonly List<(int r,int c)> empties = new List<(int,int)>();

    public Sudoku(string[] lines)
    {
        N = lines.Length;
        B = (int)Math.Sqrt(N);
        if (B * B != N) throw new ArgumentException("Size must be a perfect square");
        grid = new int[N,N];
        rowMask = new int[N];
        colMask = new int[N];
        boxMask = new int[N];

        for (int r = 0; r < N; r++)
        {
            if (lines[r].Length != N) throw new ArgumentException("Line length mismatch");
            for (int c = 0; c < N; c++)
            {
                int v = lines[r][c] - '0';
                grid[r,c] = v;
                if (v == 0)
                    empties.Add((r,c));
                else
                {
                    int bit = 1 << v;
                    rowMask[r] |= bit;
                    colMask[c] |= bit;
                    boxMask[(r/B)*B + c/B] |= bit;
                }
            }
        }
    }

    public bool Solve() => Backtrack(0);

    private bool Backtrack(int idx)
    {
        if (idx == empties.Count) return true;
        int best = idx, minCount = N+1;
        for (int k = idx; k < empties.Count; k++)
        {
            var (r,c) = empties[k];
            int used = rowMask[r] | colMask[c] | boxMask[(r/B)*B + c/B];
            int avail = (~used) & ((1 << (N+1)) - 2);
            int cnt = BitCount(avail);
            if (cnt < minCount) { minCount = cnt; best = k; if (cnt == 1) break; }
        }
        (empties[idx], empties[best]) = (empties[best], empties[idx]);

        var (row,col) = empties[idx];
        int b = (row/B)*B + col/B;
        int availMask = (~(rowMask[row] | colMask[col] | boxMask[b])) & ((1 << (N+1)) - 2);
        for (int d = 1; d <= N; d++)
        {
            int bit = 1 << d;
            if ((availMask & bit) == 0) continue;

            grid[row,col] = d;
            rowMask[row] |= bit;
            colMask[col] |= bit;
            boxMask[b] |= bit;

            if (Backtrack(idx+1)) return true;

            grid[row,col] = 0;
            rowMask[row] ^= bit;
            colMask[col] ^= bit;
            boxMask[b] ^= bit;
        }

        (empties[idx], empties[best]) = (empties[best], empties[idx]);
        return false;
    }

    private static int BitCount(int x)
    {
        int c = 0;
        while (x != 0) { x &= x - 1; c++; }
        return c;
    }

    public string[] GetSolutionLines()
    {
        var outLines = new string[N];
        for (int r = 0; r < N; r++)
        {
            var buf = new char[N];
            for (int c = 0; c < N; c++)
                buf[c] = (char)('0' + grid[r,c]);
            outLines[r] = new string(buf);
        }
        return outLines;
    }
}

class Solution
{
    static void Main()
    {
        var lines = new string[9];
        for (int i = 0; i < 9; i++)
            lines[i] = Console.ReadLine().Trim();

        var sudoku = new Sudoku(lines);
        if (sudoku.Solve())
        {
            foreach (var l in sudoku.GetSolutionLines())
                Console.WriteLine(l);
        }
    }
}
