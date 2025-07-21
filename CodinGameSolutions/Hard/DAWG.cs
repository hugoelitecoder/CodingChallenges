using System;
using System.Text;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var nText = Console.ReadLine();
        var N = int.Parse(nText);
        var dawg = new Dawg();
        for (int i = 0; i < N; i++)
        {
            string s = Console.ReadLine();
            dawg.Insert(s);
        }

        Console.WriteLine(dawg.GetMinimumNodeCount());
    }
}

class Dawg
{
    private class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; } = new Dictionary<char, TrieNode>();
        public bool IsEndOfWord { get; set; }
    }

    private readonly TrieNode _root = new TrieNode();
    private readonly Dictionary<string, int> _registry = new Dictionary<string, int>();
    private readonly List<SortedDictionary<char, int>> _canonicalNodes = new List<SortedDictionary<char, int>>();
    private readonly Dictionary<TrieNode, int> _memo = new Dictionary<TrieNode, int>();
    private const char EOW_MARKER = (char)1;

    public Dawg()
    {
        _canonicalNodes.Add(new SortedDictionary<char, int>());
        _registry["SINK"] = 0;
    }

    public void Insert(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            _root.IsEndOfWord = true;
            return;
        }

        var current = _root;
        foreach (var c in word)
        {
            if (!current.Children.TryGetValue(c, out var child))
            {
                child = new TrieNode();
                current.Children[c] = child;
            }
            current = child;
        }
        current.IsEndOfWord = true;
    }

    public int GetMinimumNodeCount()
    {
        GetCanonicalId(_root);
        return _canonicalNodes.Count;
    }

    private int GetCanonicalId(TrieNode node)
    {
        if (_memo.TryGetValue(node, out var memoizedId))
        {
            return memoizedId;
        }

        var transitions = new SortedDictionary<char, int>();
        foreach (var entry in node.Children)
        {
            var childId = GetCanonicalId(entry.Value);
            transitions[entry.Key] = childId;
        }

        if (node.IsEndOfWord)
        {
            transitions[EOW_MARKER] = 0;
        }

        var sb = new StringBuilder();
        foreach (var entry in transitions)
        {
            sb.Append(entry.Key);
            sb.Append(entry.Value);
            sb.Append('|');
        }
        var signature = sb.ToString();

        if (!_registry.TryGetValue(signature, out var id))
        {
            id = _canonicalNodes.Count;
            _canonicalNodes.Add(transitions);
            _registry[signature] = id;
        }
        
        _memo[node] = id;
        return id;
    }
}
