using System;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        var N = int.Parse(Console.ReadLine().Trim());
        var input = new List<string>(N);
        for (var i = 0; i < N; i++)
            input.Add(Console.ReadLine());

        // Build list of distinct words and map each word to its distinct index
        var distinctMap = new Dictionary<string, int>();
        var distinct = new List<string>();
        foreach (var w in input)
            if (!distinctMap.ContainsKey(w))
            {
                distinctMap[w] = distinct.Count;
                distinct.Add(w);
            }

        // Build a trie counting how many distinct words pass through each node
        var root = new Node();
        foreach (var w in distinct)
        {
            var cur = root;
            foreach (var ch in w)
            {
                if (!cur.Children.TryGetValue(ch, out var nxt))
                {
                    nxt = new Node();
                    cur.Children[ch] = nxt;
                }
                nxt.Count++;
                cur = nxt;
            }
        }

        // For each distinct word, find shortest prefix whose trie‐node count == 1
        var prefixes = new string[distinct.Count];
        for (var i = 0; i < distinct.Count; i++)
        {
            var w = distinct[i];
            var cur = root;
            var prefixLen = 0;
            foreach (var ch in w)
            {
                cur = cur.Children[ch];
                prefixLen++;
                if (cur.Count == 1)
                    break;
            }
            prefixes[i] = w.Substring(0, prefixLen);
        }

        // Output prefix for each original input word
        foreach (var w in input)
        {
            var idx = distinctMap[w];
            Console.WriteLine(prefixes[idx]);
        }
    }

    private class Node
    {
        public readonly Dictionary<char, Node> Children = new Dictionary<char, Node>();
        public int Count;
    }
}
