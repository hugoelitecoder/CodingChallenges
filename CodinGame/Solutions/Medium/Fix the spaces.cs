using System;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        var s = Console.ReadLine().Trim();
        var wordsArr = Console.ReadLine().Trim()
                         .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var n = s.Length;

        var freq = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var w in wordsArr)
            freq[w] = freq.GetValueOrDefault(w, 0) + 1;
        var words = new List<string>(freq.Keys);

        var path = new List<string>();
        var best = new List<string>();
        var solutions = 0;

        DFS(0, s, n, words, freq, path, ref solutions, best);

        if (solutions == 1)
            Console.WriteLine(string.Join(' ', best));
        else
            Console.WriteLine("Unsolvable");
    }

    private static void DFS(
        int idx,
        string s,
        int n,
        List<string> words,
        Dictionary<string,int> freq,
        List<string> path,
        ref int solutions,
        List<string> best)
    {
        if (solutions > 1)
            return;
        if (idx == n)
        {
            solutions++;
            best.Clear();
            best.AddRange(path);
            return;
        }
        foreach (var w in words)
        {
            if (solutions > 1)
                break;
            if (!freq.TryGetValue(w, out var count) || count == 0)
                continue;
            var len = w.Length;
            if (idx + len > n)
                continue;
            if (!s.AsSpan(idx, len).SequenceEqual(w))
                continue;

            freq[w]--;
            path.Add(w);

            DFS(idx + len, s, n, words, freq, path, ref solutions, best);

            path.RemoveAt(path.Count - 1);
            freq[w]++;
        }
    }
}