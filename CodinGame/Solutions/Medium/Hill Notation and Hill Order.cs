using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var formulas = new List<ChemicalFormula>();
        for (var i = 0; i < n; i++)
            formulas.Add(ChemicalFormula.Parse(Console.ReadLine()));
        var hillFormulas = formulas
            .Select(f => f.ToHill())
            .Distinct()
            .ToList();
        hillFormulas.Sort(HillComparer.Instance);
        foreach (var hf in hillFormulas)
            Console.WriteLine(hf);
    }
}

class ChemicalFormula
{
    public Dictionary<string, int> Elements { get; }
    private ChemicalFormula(Dictionary<string, int> elements)
    {
        Elements = elements;
    }
    public static ChemicalFormula Parse(string formula)
    {
        var parser = new FormulaParser(formula);
        var elements = parser.Parse();
        return new ChemicalFormula(elements);
    }
    public string ToHill()
    {
        var builder = new StringBuilder();
        var keys = Elements.Keys.ToList();
        if (keys.Contains("C"))
        {
            builder.Append("C");
            if (Elements["C"] > 1) builder.Append(Elements["C"]);
            if (Elements.ContainsKey("H"))
            {
                builder.Append("H");
                if (Elements["H"] > 1) builder.Append(Elements["H"]);
            }
            foreach (var el in keys.Where(x => x != "C" && x != "H").OrderBy(x => x))
            {
                builder.Append(el);
                if (Elements[el] > 1) builder.Append(Elements[el]);
            }
        }
        else
        {
            foreach (var el in keys.OrderBy(x => x))
            {
                builder.Append(el);
                if (Elements[el] > 1) builder.Append(Elements[el]);
            }
        }
        return builder.ToString();
    }
}

class FormulaParser
{
    private readonly string _formula;
    private int _index;
    public FormulaParser(string formula)
    {
        _formula = formula;
        _index = 0;
    }
    public Dictionary<string, int> Parse()
    {
        var stack = new Stack<Dictionary<string, int>>();
        stack.Push(new Dictionary<string, int>());
        while (_index < _formula.Length)
        {
            var c = _formula[_index];
            if (c == '(')
            {
                _index++;
                stack.Push(new Dictionary<string, int>());
            }
            else if (c == ')')
            {
                _index++;
                var mult = ReadNumber();
                if (mult == 0) mult = 1;
                var top = stack.Pop();
                foreach (var kv in top)
                {
                    if (!stack.Peek().ContainsKey(kv.Key))
                        stack.Peek()[kv.Key] = 0;
                    stack.Peek()[kv.Key] += kv.Value * mult;
                }
            }
            else if (char.IsUpper(c))
            {
                var element = ReadElement();
                var cnt = ReadNumber();
                if (cnt == 0) cnt = 1;
                if (!stack.Peek().ContainsKey(element))
                    stack.Peek()[element] = 0;
                stack.Peek()[element] += cnt;
            }
            else
            {
                _index++;
            }
        }
        return stack.Pop();
    }
    private string ReadElement()
    {
        var start = _index;
        _index++;
        while (_index < _formula.Length && char.IsLower(_formula[_index])) _index++;
        return _formula.Substring(start, _index - start);
    }
    private int ReadNumber()
    {
        var start = _index;
        while (_index < _formula.Length && char.IsDigit(_formula[_index])) _index++;
        if (start < _index)
            return int.Parse(_formula.Substring(start, _index - start));
        return 0;
    }
}

class HillComparer : IComparer<string>
{
    public static HillComparer Instance { get; } = new HillComparer();
    public int Compare(string a, string b)
    {
        var aTokens = HillToken.Split(a);
        var bTokens = HillToken.Split(b);
        for (int i = 0; i < Math.Min(aTokens.Count, bTokens.Count); i++)
        {
            var cmp = string.Compare(aTokens[i].Element, bTokens[i].Element, StringComparison.Ordinal);
            if (cmp != 0) return cmp;
            if (aTokens[i].Count != bTokens[i].Count) return aTokens[i].Count.CompareTo(bTokens[i].Count);
        }
        return aTokens.Count.CompareTo(bTokens.Count);
    }
}

class HillToken
{
    public string Element { get; }
    public int Count { get; }
    public HillToken(string element, int count)
    {
        Element = element;
        Count = count;
    }
    public static List<HillToken> Split(string s)
    {
        var list = new List<HillToken>();
        var i = 0;
        while (i < s.Length)
        {
            var start = i;
            i++;
            while (i < s.Length && char.IsLower(s[i])) i++;
            var element = s.Substring(start, i - start);
            var countStart = i;
            while (i < s.Length && char.IsDigit(s[i])) i++;
            var count = countStart < i ? int.Parse(s.Substring(countStart, i - countStart)) : 1;
            list.Add(new HillToken(element, count));
        }
        return list;
    }
}
