using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var variables = Console.ReadLine().Split(' ');
        var m = int.Parse(Console.ReadLine());
        var equations = new List<Equation>();
        for (var i = 0; i < m; i++)
            equations.Add(Equation.Parse(Console.ReadLine()));
        var system = new SymbolicSystem(variables, equations);
        var solved = system.Solve();
        if (solved == null)
            Console.WriteLine("No solution!");
        else
            foreach (var v in variables)
                Console.WriteLine($"{v} -> {solved[v]}");
    }
}

class Equation
{
    public string Variable { get; }
    public string Function { get; }
    public List<string> Arguments { get; }
    public Equation(string variable, string function, List<string> arguments)
    {
        Variable = variable;
        Function = function;
        Arguments = arguments;
    }
    public static Equation Parse(string line)
    {
        var eq = line.Split('=');
        var lhs = eq[0].Trim();
        var rhs = eq[1].Trim();
        var fun = rhs.Substring(0, rhs.IndexOf('(')).Trim();
        var inside = rhs.Substring(rhs.IndexOf('(') + 1, rhs.LastIndexOf(')') - rhs.IndexOf('(') - 1);
        var args = inside.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        return new Equation(lhs, fun, args);
    }
}

class SymbolicSystem
{
    private readonly HashSet<string> _variables;
    private readonly Dictionary<string, Equation> _equations;
    private readonly Dictionary<string, string> _resolved = new Dictionary<string, string>();
    private readonly HashSet<string> _visiting = new HashSet<string>();
    private bool _fail;

    public SymbolicSystem(IEnumerable<string> variables, IEnumerable<Equation> equations)
    {
        _variables = new HashSet<string>(variables);
        _equations = equations.ToDictionary(e => e.Variable, e => e);
    }

    public Dictionary<string, string> Solve()
    {
        _fail = false;
        foreach (var v in _variables)
        {
            _visiting.Clear();
            Expand(v);
            if (_fail) return null;
        }
        var result = new Dictionary<string, string>();
        foreach (var v in _variables)
            result[v] = _resolved.ContainsKey(v) ? _resolved[v] : v;
        return result;
    }

    private string Expand(string v)
    {
        if (_resolved.ContainsKey(v)) return _resolved[v];
        if (_visiting.Contains(v)) { _fail = true; return ""; }
        _visiting.Add(v);
        if (!_equations.ContainsKey(v)) { _visiting.Remove(v); return v; }
        var eq = _equations[v];
        if (eq.Arguments.Contains(v)) { _fail = true; return ""; }
        var expandedArgs = new List<string>();
        foreach (var a in eq.Arguments)
        {
            expandedArgs.Add(Expand(a));
            if (_fail) return "";
        }
        var result = eq.Function + " ( " + string.Join(" ", expandedArgs) + " )";
        _visiting.Remove(v);
        _resolved[v] = result;
        return result;
    }
}
