using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var solver = new BloodTypeSolver();

        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split();
            var answers = solver.Solve(parts[0], parts[1], parts[2]);
            Console.WriteLine(answers.Count == 0
                ? "impossible"
                : string.Join(" ", answers));
        }
    }
}

class BloodTypeSolver
{
    private static readonly string[] AllTypes =
        { "A+", "A-", "AB+", "AB-", "B+", "B-", "O+", "O-" };

    public List<string> Solve(string p1, string p2, string c)
    {
        return c == "?"
            ? ChildrenOf(p1, p2)
            : (p2 == "?"
               ? ParentsFor(p1, c)
               : ParentsFor(p2, c));
    }

    private List<string> ChildrenOf(string a, string b)
    {
        var set = new HashSet<string>();
        foreach (var gA in Genes(a))
        foreach (var gB in Genes(b))
        foreach (var x in gA)
        foreach (var y in gB)
        {
            string abo = ABO(x, y);
            foreach (char rh in Rh(a[^1], b[^1]))
                set.Add(abo + rh);
        }
        return set.OrderBy(s => s, StringComparer.Ordinal).ToList();
    }

    private List<string> ParentsFor(string known, string child)
        => AllTypes
            .Where(pt => ChildrenOf(known, pt).Contains(child))
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();

    private IEnumerable<string> Genes(string bt)
    {
        string abo = bt[..^1];
        return abo switch
        {
            "A"  => new[] { "AA", "AO", "OA" },
            "B"  => new[] { "BB", "BO", "OB" },
            "AB" => new[] { "AB", "BA" },
            "O"  => new[] { "OO" },
            _    => Array.Empty<string>()
        };
    }

    private string ABO(char g1, char g2)
        => (g1, g2) switch
        {
            ('A','B') or ('B','A') => "AB",
            _ when g1=='A' || g2=='A' => "A",
            _ when g1=='B' || g2=='B' => "B",
            _ => "O"
        };

    private IEnumerable<string> GenesRh(char r)
        => r == '+'
            ? new[] { "++", "+-", "-+" }
            : new[] { "--" };

    private IEnumerable<char> Rh(char r1, char r2)
    {
        var set = new HashSet<char>();
        foreach (var g1 in GenesRh(r1))
        foreach (var g2 in GenesRh(r2))
        foreach (var c1 in g1)
        foreach (var c2 in g2)
            set.Add((c1=='+'||c2=='+')?'+':'-');
        return set;
    }
}
