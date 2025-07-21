using System;
using System.Collections.Generic;

class Solution
{
    static int L;
    static readonly int[] DR = { -1, 1, 0, 0 };
    static readonly int[] DC = { 0, 0, -1, 1 };
    const double Komi = 6.5;

    static void Main()
    {
        L = int.Parse(Console.ReadLine());
        var map = new char[L][];
        for (int i = 0; i < L; i++)
            map[i] = Console.ReadLine().ToCharArray();

        int blackStones = 0, whiteStones = 0;
        for (int i = 0; i < L; i++)
            for (int j = 0; j < L; j++)
            {
                if (map[i][j] == 'B') blackStones++;
                else if (map[i][j] == 'W') whiteStones++;
            }

        bool[,] visited = new bool[L, L];
        int blackTerr = 0, whiteTerr = 0;

        for (int i = 0; i < L; i++)
            for (int j = 0; j < L; j++)
            {
                if (map[i][j] != '.' || visited[i, j]) 
                    continue;

                var stack = new Stack<(int r, int c)>();
                var region = new List<(int r, int c)>();
                var edges  = new HashSet<char>();

                visited[i, j] = true;
                stack.Push((i, j));
                while (stack.Count > 0)
                {
                    var (r, c) = stack.Pop();
                    region.Add((r, c));

                    for (int d = 0; d < 4; d++)
                    {
                        int nr = r + DR[d], nc = c + DC[d];
                        if (nr < 0 || nr >= L || nc < 0 || nc >= L)
                        {
                            continue;
                        }

                        char ch = map[nr][nc];
                        if (ch == '.' && !visited[nr, nc])
                        {
                            visited[nr, nc] = true;
                            stack.Push((nr, nc));
                        }
                        else if (ch == 'B' || ch == 'W')
                        {
                            edges.Add(ch);
                        }
                    }
                }

                bool touchesB = edges.Contains('B');
                bool touchesW = edges.Contains('W');
                if (touchesB && !touchesW)
                    blackTerr += region.Count;
                else if (touchesW && !touchesB)
                    whiteTerr += region.Count;
            }

        double blackScore = blackStones + blackTerr;
        double whiteScore = whiteStones + whiteTerr + Komi;

        Console.WriteLine($"BLACK : {blackScore}");
        Console.WriteLine($"WHITE : {whiteScore:0.0}");
        Console.WriteLine(blackScore > whiteScore
            ? "BLACK WINS"
            : "WHITE WINS");
    }
}
