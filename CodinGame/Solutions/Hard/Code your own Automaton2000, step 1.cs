using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    private static Dictionary<string, Dictionary<string, int>> _tree = new Dictionary<string, Dictionary<string, int>>();
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        for (var i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            AddLine(line);
            if (line.Contains("Automaton2000"))
            {
                Console.WriteLine(Say("__START__", 0).Trim());
            }
        }
    }
    private static void AddLine(string line)
    {
        if (line.Length <= 11) return;
        line = line.Substring(11);
        var idx = line.IndexOf(':');
        if (idx == -1) return;
        line = line.Substring(idx + 1);
        var words = line.Split(' ').Where(w => !string.IsNullOrEmpty(w) && w != "Automaton2000").ToList();
        if (words.Count == 0) return;
        AddWord("__START__", words[0]);
        for (var i = 1; i < words.Count; ++i)
            AddWord(words[i - 1], words[i]);
        AddWord(words[words.Count - 1], "__END__");
    }
    private static void AddWord(string previous, string next)
    {
        if (!_tree.ContainsKey(previous))
            _tree[previous] = new Dictionary<string, int>();
        if (!_tree[previous].ContainsKey(next))
            _tree[previous][next] = 0;
        _tree[previous][next]++;
    }
    private static string Say(string word, int depth)
    {
        if (depth >= 30 || !_tree.ContainsKey(word)) return "";
        string max = null;
        foreach (var next in _tree[word].Keys)
        {
            if (max == null ||
                _tree[word][next] > _tree[word][max] ||
                (_tree[word][next] == _tree[word][max] && string.Compare(next, max, StringComparison.Ordinal) < 0))
            {
                max = next;
            }
        }
        if (max == "__END__" || max == null) return "";
        return max + " " + Say(max, depth + 1);
    }
}
