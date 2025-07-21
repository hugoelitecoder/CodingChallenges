using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var tributes = new List<string>();
        for (var i = 0; i < n; i++)
            tributes.Add(Console.ReadLine());

        var kills = new Dictionary<string, List<string>>();
        var killedBy = new Dictionary<string, string>();
        foreach (var name in tributes)
            kills[name] = new List<string>();

        var t = int.Parse(Console.ReadLine());
        for (var i = 0; i < t; i++)
        {
            var info = Console.ReadLine();
            var parts = info.Split(new[] { " killed " }, StringSplitOptions.None);
            var killer = parts[0];
            var victims = parts[1].Split(new[] { ", " }, StringSplitOptions.None);
            foreach (var victim in victims)
            {
                kills[killer].Add(victim);
                killedBy[victim] = killer;
            }
        }

        tributes.Sort(StringComparer.Ordinal);
        for (var i = 0; i < tributes.Count; i++)
        {
            var name = tributes[i];
            var killedList = kills[name];
            killedList.Sort(StringComparer.Ordinal);

            var killedOutput = killedList.Count > 0
                ? string.Join(", ", killedList)
                : "None";

            var killerOutput = killedBy.ContainsKey(name)
                ? killedBy[name]
                : "Winner";

            Console.WriteLine($"Name: {name}");
            Console.WriteLine($"Killed: {killedOutput}");
            Console.WriteLine($"Killer: {killerOutput}");

            if (i != tributes.Count - 1)
                Console.WriteLine();
        }
    }
}
