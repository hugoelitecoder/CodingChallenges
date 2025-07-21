using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var start = Console.ReadLine().Trim();
        var grammar = Grammar.Read(n);
        var t = int.Parse(Console.ReadLine());
        var parser = new CYKParser(grammar, start);
        for (int i = 0; i < t; i++)
        {
            var word = Console.ReadLine().Trim();
            Console.WriteLine(parser.CanParse(word) ? "true" : "false");
        }
    }
}

class Grammar
{
    public Dictionary<(string, string), List<string>> BinaryRules { get; }
    public Dictionary<string, List<string>> TerminalRules { get; }
    public Grammar()
    {
        BinaryRules = new Dictionary<(string, string), List<string>>();
        TerminalRules = new Dictionary<string, List<string>>();
    }
    public static Grammar Read(int n)
    {
        var grammar = new Grammar();
        for (int i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            var tokens = line.Replace(" ", "").Split("->");
            var left = tokens[0];
            var right = tokens[1];
            if (right.Length == 1 && char.IsLower(right[0]))
            {
                if (!grammar.TerminalRules.ContainsKey(right))
                    grammar.TerminalRules[right] = new List<string>();
                grammar.TerminalRules[right].Add(left);
            }
            else
            {
                var a = right.Substring(0, 1);
                var b = right.Substring(1, 1);
                var key = (a, b);
                if (!grammar.BinaryRules.ContainsKey(key))
                    grammar.BinaryRules[key] = new List<string>();
                grammar.BinaryRules[key].Add(left);
            }
        }
        return grammar;
    }
}

class CYKParser
{
    private readonly Grammar _grammar;
    private readonly string _start;
    public CYKParser(Grammar grammar, string start)
    {
        _grammar = grammar;
        _start = start;
    }
    public bool CanParse(string word)
    {
        var n = word.Length;
        if (n == 0) return false;
        var table = new HashSet<string>[n, n + 1];
        for (int i = 0; i < n; i++)
            for (int j = 0; j <= n; j++)
                table[i, j] = new HashSet<string>();
        for (int i = 0; i < n; i++)
        {
            var ch = word[i].ToString();
            if (_grammar.TerminalRules.ContainsKey(ch))
                foreach (var nt in _grammar.TerminalRules[ch])
                    table[i, i + 1].Add(nt);
        }
        for (int l = 2; l <= n; l++)
        {
            for (int i = 0; i + l <= n; i++)
            {
                int j = i + l;
                for (int k = i + 1; k < j; k++)
                {
                    foreach (var a in table[i, k])
                    foreach (var b in table[k, j])
                    {
                        var key = (a, b);
                        if (_grammar.BinaryRules.ContainsKey(key))
                            foreach (var nt in _grammar.BinaryRules[key])
                                table[i, j].Add(nt);
                    }
                }
            }
        }
        return table[0, n].Contains(_start);
    }
}
