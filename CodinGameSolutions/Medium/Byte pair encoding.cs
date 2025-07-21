using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class Solution
{
    static void Main()
    {
        var parts = Console.ReadLine().Split();
        int n = int.Parse(parts[0]);
        var sb = new StringBuilder();
        for (int i = 0; i < n; i++)
            sb.Append(Console.ReadLine());

        var compressor = new Compressor(sb.ToString());
        var rules = compressor.Compress().ToList();

        Console.WriteLine(compressor.Current);
        foreach (var (nt, pair) in rules)
            Console.WriteLine($"{nt} = {pair}");
    }
}

public class Compressor
{
    public string Current { get; private set; }
    private char _nextNt = 'Z';

    public Compressor(string input) => Current = input;

    public IEnumerable<(char NonTerminal, string Pair)> Compress()
    {
        while (true)
        {
            var best = Enumerable.Range(0, Current.Length - 1)
                .Select(i => Current.Substring(i, 2))
                .Distinct()
                .Select(p => new
                {
                    Pair  = p,
                    Count = Regex.Matches(Current, Regex.Escape(p)).Count,
                    First = Current.IndexOf(p, StringComparison.Ordinal)
                })
                .Where(x => x.Count > 1)
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.First)
                .FirstOrDefault();

            if (best == null)
                yield break;

            char nt = _nextNt--;
            yield return (nt, best.Pair);

            Current = Regex.Replace(
                Current,
                Regex.Escape(best.Pair),
                nt.ToString()
            );
        }
    }
}
