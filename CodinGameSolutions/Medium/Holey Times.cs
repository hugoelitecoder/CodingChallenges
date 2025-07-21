using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

class Solution
{
    private class Patterns
    {
        public string A { get; init; }
        public string B { get; init; }
        public List<string> Partials { get; init; }
        public string Result { get; init; }
    }

    private class Multiplication
    {
        public BigInteger A { get; init; }
        public BigInteger B { get; init; }
        public List<string> Partials { get; init; }
        public string Result { get; init; }
    }

    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var raw = new string[n];
        for (int i = 0; i < n; i++)
            raw[i] = Console.ReadLine();
        int partialCount = n - 5;

        var patterns = ExtractPatterns(raw, partialCount);
        var solution = FindSolution(patterns);
        Print(raw, solution);
    }
    
    private static Patterns ExtractPatterns(string[] raw, int p)
    {
        static string Clean(string s) => Regex.Replace(s, "[^0-9*]", "");
        return new Patterns
        {
            A = Clean(raw[0]),
            B = Clean(raw[1]),
            Partials = Enumerable.Range(0, p)
                                 .Select(i => Clean(raw[3 + i]))
                                 .ToList(),
            Result = Clean(raw[4 + p])
        };
    }

    private static Multiplication FindSolution(Patterns pat)
    {
        IEnumerable<BigInteger> Dfs(string pattern, int idx, char[] cur)
        {
            if (idx == pattern.Length)
            {
                if (cur.Length > 1 && cur[0] == '0') yield break;
                yield return BigInteger.Parse(new string(cur));
                yield break;
            }

            if (pattern[idx] != '*')
            {
                cur[idx] = pattern[idx];
                foreach (var num in Dfs(pattern, idx + 1, cur))
                    yield return num;
            }
            else
            {
                for (char d = '0'; d <= '9'; d++)
                {
                    if (idx == 0 && cur.Length > 1 && d == '0') continue;
                    cur[idx] = d;
                    foreach (var num in Dfs(pattern, idx + 1, cur))
                        yield return num;
                }
            }
        }
        
        bool Fits(string pattern, string value) =>
            pattern.Length == value.Length &&
            !pattern.Where((c, i) => c != '*' && c != value[i]).Any();

        var As = Dfs(pat.A, 0, new char[pat.A.Length]).ToList();
        var Bs = Dfs(pat.B, 0, new char[pat.B.Length]).ToList();

        foreach (var a in As)
        {
            foreach (var b in Bs)
            {
                var prodS = (a * b).ToString();
                if (!Fits(pat.Result, prodS)) continue;

                var bStr = b.ToString();
                var vals = pat.Partials
                    .Select((pattern, i) =>
                    {
                        int digit = bStr[bStr.Length - 1 - i] - '0';
                        string val = (a * digit).ToString() + new string('0', i);
                        return (pattern, val);
                    })
                    .ToList();

                if (vals.All(p => Fits(p.pattern, p.val)))
                {
                    return new Multiplication
                    {
                        A = a,
                        B = b,
                        Partials = vals.Select(p => p.val).ToList(),
                        Result = prodS
                    };
                }
            }
        }
        throw new InvalidOperationException("No solution found");
    }

    private static void Print(string[] raw, Multiplication sol)
    {
        var queue = new Queue<string>();
        queue.Enqueue(sol.A.ToString());
        queue.Enqueue(sol.B.ToString());
        foreach (var part in sol.Partials) queue.Enqueue(part);
        queue.Enqueue(sol.Result);

        foreach (var line in raw)
        {
            var m = Regex.Match(line, "[0-9*]+");
            if (!m.Success)
                Console.WriteLine(line);
            else
                Console.WriteLine(
                    line[..m.Index]
                    + queue.Dequeue()
                    + line[(m.Index + m.Length)..]
                );
        }
    }
}
