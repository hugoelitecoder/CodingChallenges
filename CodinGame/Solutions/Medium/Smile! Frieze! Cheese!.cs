using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        int numRows = int.Parse(Console.ReadLine());
        string[] rows = Enumerable.Range(0, numRows)
                     .Select(_ => Console.ReadLine())
                     .ToArray();

        int cols = rows[0].Length;
        int[][] grid = ToGrid(rows);

        double[] scores = ComputeScores(grid, numRows, cols);
        int period = FindPeriod(scores);

        string[] motif = ExtractMotif(rows, period);
        bool vert = HasVertical(motif);
        bool horiz = HasHorizontal(motif);
        bool rot = HasRotation(motif);
        bool glide = HasGlide(motif, period);

        string name = BuildName(vert, horiz, rot, glide);
        Console.WriteLine(name);
    }

    static int[][] ToGrid(string[] rows)
        => rows.Select(r => r.Select(c => c == '#' ? 1 : 0).ToArray())
               .ToArray();

    static double[] ComputeScores(int[][] grid, int numRows, int cols)
    {
        var scores = new double[cols];
        for (int shift = 1; shift < cols; shift++)
        {
            double match = 0;
            int len = cols - shift;
            for (int i = 0; i < numRows; i++)
                for (int j = 0; j < len; j++)
                    if (grid[i][j] == grid[i][j + shift]) match++;
            scores[shift] = match / len;
        }
        return scores;
    }

    static int FindPeriod(double[] scores)
        => Array.IndexOf(scores, scores.Max());

    static string[] ExtractMotif(string[] rows, int period)
        => rows.Select(r => r.Substring(0, period)).ToArray();

    static bool HasVertical(string[] motif)
        => motif.All(r => r.SequenceEqual(r.Reverse()));

    static bool HasHorizontal(string[] motif)
        => motif.SequenceEqual(motif.Reverse());

    static bool HasRotation(string[] motif)
    {
        int n = motif.Length;
        return Enumerable.Range(0, n)
                         .All(i => motif[i] == new string(motif[n - 1 - i]
                             .Reverse().ToArray()));
    }

    static bool HasGlide(string[] motif, int period)
    {
        if (period % 2 != 0) return false;
        int half = period / 2;
        int n = motif.Length;
        return Enumerable.Range(0, n)
                         .All(i => motif[i].Substring(0, half)
                             == motif[n - 1 - i].Substring(half));
    }

    static string BuildName(bool vert, bool horiz, bool rot, bool glide)
    {
        string name = "p";
        name += vert  ? 'm' : '1';
        name += horiz ? 'm' : glide ? 'a' : '1';
        name += rot   ? '2' : '1';
        return name == "pma1" ? "pma2" : name;
    }
}
