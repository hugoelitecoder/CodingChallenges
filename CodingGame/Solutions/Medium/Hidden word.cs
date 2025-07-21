using System;
using System.Collections.Generic;
using System.Text;

class Solution
{
    static readonly int[] dr = { -1,-1, 0, 1, 1, 1, 0,-1 };
    static readonly int[] dc = {  0, 1, 1, 1, 0,-1,-1,-1 };

    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var words = new string[n];
        for (int i = 0; i < n; i++)
            words[i] = Console.ReadLine().Trim();

        var dims = Console.ReadLine().Split();
        int H = int.Parse(dims[0]), W = int.Parse(dims[1]);
        var grid = new char[H][];
        for (int r = 0; r < H; r++)
            grid[r] = Console.ReadLine().ToCharArray();

        var posByChar = new Dictionary<char, List<(int r,int c)>>();
        for (char ch = 'A'; ch <= 'Z'; ch++)
            posByChar[ch] = new List<(int,int)>();
        for (int r = 0; r < H; r++)
            for (int c = 0; c < W; c++)
                posByChar[grid[r][c]].Add((r,c));

        var used = new bool[H][];
        for (int r = 0; r < H; r++)
            used[r] = new bool[W];

        foreach (var w in words)
            FindAndMark(grid, used, posByChar[w[0]], w);

        var sb = new StringBuilder();
        for (int r = 0; r < H; r++)
            for (int c = 0; c < W; c++)
                if (!used[r][c])
                    sb.Append(grid[r][c]);

        Console.WriteLine(sb);
    }

    static void FindAndMark(
        char[][] grid,
        bool[][] used,
        List<(int r,int c)> starts,
        string word
    )
    {
        int H = grid.Length, W = grid[0].Length, L = word.Length;
        foreach (var (r0,c0) in starts)
        {
            for (int d = 0; d < 8; d++)
            {
                int k;
                for (k = 0; k < L; k++)
                {
                    int r = r0 + dr[d]*k, c = c0 + dc[d]*k;
                    if (r<0 || r>=H || c<0 || c>=W || grid[r][c] != word[k])
                        break;
                }
                if (k == L)
                {
                    for (int i = 0; i < L; i++)
                    {
                        int r = r0 + dr[d]*i, c = c0 + dc[d]*i;
                        used[r][c] = true;
                    }
                    return;
                }
            }
        }
    }
}
