using System;
using System.Collections.Generic;
using System.Diagnostics;
class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var mode = Console.ReadLine();
        if (string.IsNullOrEmpty(mode)) return;
        var v1 = ReadLines();
        var v2 = ReadLines();
        var diffUtil = new TextDiffUtility<string>();
        var results = mode == "BY_NUMBER" 
            ? diffUtil.CompareByNumber(v1, v2) 
            : diffUtil.CompareByContent(v1, v2);
        if (results.Count == 0)
        {
            Console.WriteLine("No Diffs");
        }
        else
        {
            results.Sort(StringComparer.Ordinal);
            foreach (var line in results)
            {
                Console.WriteLine(line);
            }
        }
        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] Total lines: {v1.Length + v2.Length}");
        Console.Error.WriteLine($"[DEBUG] Diff count: {results.Count}");
        Console.Error.WriteLine($"[DEBUG] Execution time: {sw.ElapsedMilliseconds}ms");
    }
    private static string[] ReadLines()
    {
        var line = Console.ReadLine();
        if (string.IsNullOrEmpty(line)) return Array.Empty<string>();
        var n = int.Parse(line);
        var res = new string[n];
        for (var i = 0; i < n; i++)
        {
            res[i] = Console.ReadLine();
        }
        return res;
    }
}
public class TextDiffUtility<T> where T : IEquatable<T>
{
    public List<string> CompareByNumber(T[] v1, T[] v2)
    {
        var diffs = new List<string>();
        var min = Math.Min(v1.Length, v2.Length);
        for (var i = 0; i < min; i++)
        {
            if (!v1[i].Equals(v2[i]))
            {
                diffs.Add($"CHANGE: {v1[i]} ---> {v2[i]}");
            }
        }
        for (var i = min; i < v1.Length; i++)
        {
            diffs.Add($"DELETE: {v1[i]}");
        }
        for (var i = min; i < v2.Length; i++)
        {
            diffs.Add($"ADD: {v2[i]}");
        }
        return diffs;
    }
    public List<string> CompareByContent(T[] v1, T[] v2)
    {
        var diffs = new List<string>();
        var map1 = new Dictionary<T, int>(v1.Length);
        for (var i = 0; i < v1.Length; i++)
        {
            map1[v1[i]] = i + 1;
        }
        var map2 = new Dictionary<T, int>(v2.Length);
        for (var i = 0; i < v2.Length; i++)
        {
            map2[v2[i]] = i + 1;
        }
        foreach (var item in v1)
        {
            if (map2.TryGetValue(item, out var newIdx))
            {
                var oldIdx = map1[item];
                if (oldIdx != newIdx)
                {
                    diffs.Add($"MOVE: {item} @:{oldIdx} >>> @:{newIdx}");
                }
            }
            else
            {
                diffs.Add($"DELETE: {item}");
            }
        }
        foreach (var item in v2)
        {
            if (!map1.ContainsKey(item))
            {
                diffs.Add($"ADD: {item}");
            }
        }
        return diffs;
    }
}