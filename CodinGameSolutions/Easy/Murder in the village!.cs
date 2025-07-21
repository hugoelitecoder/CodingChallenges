using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

class Solution {

    static void Main() {

        int n = int.Parse(Console.ReadLine());
        var rx = new Regex(@"^(?<name>\w+): I was in the (?<loc>\w+)(?:, alone| with (?<lst>[\w ]+(?: and [\w ]+)*))\.$");
        var people = Enumerable.Range(0, n)
            .Select(_ => {
                var m = rx.Match(Console.ReadLine());
                return new {
                    Name = m.Groups["name"].Value,
                    Loc = m.Groups["loc"].Value,
                    Pals = m.Groups["lst"].Success
                        ? m.Groups["lst"].Value.Split(" and ").ToList()
                        : new List<string>()
                };
            })
            .ToList();

        string killer = people
            .Select(c => c.Name)
            .FirstOrDefault(c =>
                people.Where(p => p.Name != c).All(p => {
                    var group = p.Pals.Append(p.Name).ToList();
                    bool mutual = p.Pals.All(q =>
                        q != c &&
                        people.First(x => x.Name == q).Loc == p.Loc &&
                        people.First(x => x.Name == q).Pals.Contains(p.Name)
                    );
                    bool aloneOk = p.Pals.Count == 0 && !people.Any(x => x.Name != p.Name && x.Name != c && x.Loc == p.Loc);
                    bool groupOk = p.Pals.Count>0 && mutual && !people.Any(x => !group.Contains(x.Name) && x.Name!=c && x.Loc==p.Loc);
                    return aloneOk || groupOk;
                })
            );
            
        Console.WriteLine(killer != null ? $"{killer} did it!" : "It was me!");
    }
}
