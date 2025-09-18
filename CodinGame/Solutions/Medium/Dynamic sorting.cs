using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

class Solution
{
    class Spec
    {
        public Func<Dictionary<string,string>, IComparable> Key;
        public bool Asc;
    }
    class Item
    {
        public int Id;
        public Dictionary<string,string> F;
    }

    static void Main()
    {
        var expr  = Console.ReadLine();
        var types = Console.ReadLine().Split(',');
        var specs = Regex.Matches(expr, @"([+-])([^+-]+)")
                         .Cast<Match>()
                         .Select((m, i) => new Spec {
                             Asc = m.Groups[1].Value == "+",
                             Key = types[i] == "int"
                                 ? (Func<Dictionary<string,string>, IComparable>)(d => int.Parse(d[m.Groups[2].Value]))
                                 : d => d[m.Groups[2].Value]
                         })
                         .ToArray();
        var items = Enumerable.Range(0, int.Parse(Console.ReadLine()))
                              .Select(_ => Console.ReadLine()
                                                .Split(',')
                                                .Select(p => p.Split(':'))
                                                .ToDictionary(a => a[0], a => a[1]))
                              .Select(d => new Item { Id = int.Parse(d["id"]), F = d });

        var ordered = specs.Aggregate(
            (IOrderedEnumerable<Item>)null,
            (ord, spec) => ord == null
                ? (spec.Asc
                    ? items.OrderBy(x => spec.Key(x.F))
                    : items.OrderByDescending(x => spec.Key(x.F)))
                : (spec.Asc
                    ? ord.ThenBy(x => spec.Key(x.F))
                    : ord.ThenByDescending(x => spec.Key(x.F)))
        )
        .ThenBy(x => x.Id);

        foreach (var it in ordered)
            Console.WriteLine(it.Id);
    }
}
