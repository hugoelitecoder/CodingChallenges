using System;
using System.Collections.Generic;

class Solution
{
    static string[] board;
    static int n, m;
    static char roadChar, settlementChar;
    static int maxLen;
    static readonly int[] di = { -1, 1, 0, 0 };
    static readonly int[] dj = { 0, 0, -1, 1 };

    static void Main()
    {
        n = int.Parse(Console.ReadLine());
        board = new string[n];
        for (int i = 0; i < n; i++)
            board[i] = Console.ReadLine();
        m = board[0].Length;

        bool[] present = new bool[26];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                if (char.IsLower(board[i][j]))
                    present[board[i][j] - 'a'] = true;

        char bestPlayer = '\0';
        int bestLen = 0;

        foreach (int idx in GetIndices(present))
        {
            roadChar = (char)('a' + idx);
            settlementChar = char.ToUpper(roadChar);
            maxLen = 0;
            var visited = new HashSet<(int, int)>();

            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    if (board[i][j] == roadChar)
                    {
                        visited.Clear();
                        visited.Add((i, j));
                        DFS(i, j, 1, visited);
                    }

            if (maxLen > bestLen)
            {
                bestLen = maxLen;
                bestPlayer = (char)('A' + idx);
            }
        }

        if (bestLen >= 5)
            Console.WriteLine($"{bestPlayer} {bestLen}");
        else
            Console.WriteLine("0");
    }

    static void DFS(int i, int j, int length, HashSet<(int, int)> visited)
    {
        if (length > maxLen)
            maxLen = length;

        for (int d = 0; d < 4; d++)
        {
            int ni = i + di[d], nj = j + dj[d];
            if (ni < 0 || ni >= n || nj < 0 || nj >= m) continue;
            char c = board[ni][nj];
            if ((c == roadChar || c == settlementChar) && visited.Add((ni, nj)))
            {
                int add = c == roadChar ? 1 : 0;
                DFS(ni, nj, length + add, visited);
                visited.Remove((ni, nj));
            }
        }
    }

    static IEnumerable<int> GetIndices(bool[] present)
    {
        for (int i = 0; i < present.Length; i++)
            if (present[i]) yield return i;
    }
}