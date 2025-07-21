using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var tokens = Console.ReadLine().Split();
        string player1 = tokens[0], player2 = tokens[1];
        int bof = int.Parse(Console.ReadLine());
        string hexPoints = Console.ReadLine();

        string binPoints = HexToBin(hexPoints);
        var scoreMap = new object[] { 0, 15, 30, 40, "AV", "-" };

        int[] sets = { 0, 0 };
        var games = new List<List<int>> { new List<int> { 0 }, new List<int> { 0 } };
        int[] pts = { 0, 0 };
        bool tieBreak = false;
        int maxi = 4;
        int winner = -1;

        while (binPoints.Length > 0)
        {
            (pts, binPoints) = Game(binPoints, maxi);
            if (Math.Max(pts[0], pts[1]) >= maxi && Math.Abs(pts[0] - pts[1]) > 1)
            {
                int j = pts[1] > pts[0] ? 1 : 0;
                games[j][games[j].Count - 1]++;
                pts[0] = pts[1] = 0;

                int g0 = games[0].Last(), g1 = games[1].Last();
                if (games[j].Last() >= 6)
                {
                    if (Math.Abs(g0 - g1) > 1 || games[j].Last() == 7)
                    {
                        sets[j]++;
                        tieBreak = false;
                        maxi = 4;
                        if (sets.Max() == (bof + 1) / 2)
                        {
                            winner = j;
                            break;
                        }
                        games[0].Add(0);
                        games[1].Add(0);
                    }
                    else if (g0 == 6 && g1 == 6)
                    {
                        tieBreak = true;
                        maxi = (sets.Sum() == bof - 1) ? 10 : 7;
                    }
                }
            }
        }

        void PrintLine(string name, List<int> gs, int point)
        {
            Console.Write(name.PadRight(15, '.'));
            foreach (int g in gs) Console.Write(" " + g);
            if (winner < 0)
            {
                if (tieBreak) Console.Write(" | " + point);
                else
                {
                    int idx = point + 2 * ((pts[1 - (name == player1 ? 0 : 1)] == 4) ? 1 : 0);
                    Console.Write(" | " + scoreMap[idx]);
                }
            }
            Console.WriteLine();
        }

        PrintLine(player1, games[0], pts[0]);
        PrintLine(player2, games[1], pts[1]);
        Console.WriteLine(winner < 0 ? "Game in progress" : (winner == 0 ? player1 : player2) + " wins");
    }

    static string HexToBin(string hex)
    {
        string h = hex.Replace(" ", "");
        var sb = new System.Text.StringBuilder();
        foreach (char c in h)
        {
            int v = Convert.ToInt32(c.ToString(), 16);
            sb.Append(Convert.ToString(v, 2).PadLeft(4, '0'));
        }
        return sb.ToString();
    }

    static (int[] pts, string rem) Game(string bits, int maxi)
    {
        int[] pts = { 0, 0 };
        int i;
        for (i = 0; i < bits.Length; i++)
        {
            int p = bits[i] == '1' ? 1 : 0;
            pts[p]++;
            if (Math.Max(pts[0], pts[1]) >= maxi)
            {
                if (Math.Abs(pts[0] - pts[1]) > 1)
                    break;
                else if (pts[0] == pts[1] && pts[0] == maxi)
                {
                    pts[0]--;
                    pts[1]--;
                }
            }
        }
        string rem = (i + 1 < bits.Length) ? bits.Substring(i + 1) : string.Empty;
        return (pts, rem);
    }
}
