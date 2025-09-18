using System;
using System.Linq;

class Program
{
    static readonly double[] EngFreq = {
         8.08, 1.67, 3.18, 3.99,12.56, 2.17, 1.80, 5.27, 7.24, 0.14,
         0.63, 4.04, 2.60, 7.38, 7.47, 1.91, 0.09, 6.42, 6.59, 9.15,
         2.79, 1.00, 1.89, 0.21, 1.65, 0.07
    };

    static string Shift(string s, int k) =>
        new string(s.Select(c =>
            char.IsUpper(c)
                ? (char)('A' + (c - 'A' + k) % 26)
            : char.IsLower(c)
                ? (char)('a' + (c - 'a' + k) % 26)
            : c
        ).ToArray());

    static double Score(string s)
    {
        var letters = s.Where(char.IsLetter)
                       .Select(char.ToLower)
                       .ToArray();
        int n = letters.Length;
        if (n == 0) return double.MaxValue;

        var cnt = letters
            .GroupBy(c => c)
            .ToDictionary(g => g.Key, g => g.Count());

        return Math.Sqrt(EngFreq
            .Select((f, i) => {
                double obs = cnt.TryGetValue((char)('a' + i), out var c0)
                             ? c0 * 100.0 / n
                             : 0;
                double d = obs - f;
                return d * d;
            })
            .Sum()
        );
    }

    static void Main()
    {
        string msg = Console.ReadLine();
        string best = Enumerable.Range(0, 26)
            .Select(k => new { Text = Shift(msg, k), Sc = Score(Shift(msg, k)) })
            .OrderBy(x => x.Sc)
            .First()
            .Text;
        Console.WriteLine(best);
    }
}
