using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        var data = Console.ReadLine().Split(',');
        var parsed = data.Select(p => p.Split(':')).ToList();
        var itemName = "-" + parsed[0][0] + "-";
        var rarity = parsed[1][0];
        var attributes = parsed.Skip(2)
                               .Select(x => x[0] + " " + x[1])
                               .ToList();
        var width = new[] { attributes.Max(a => a.Length), itemName.Length, rarity.Length }.Max();
        var frame = _frames[rarity];

        if (rarity != "Legendary")
        {
            var centeredRarity = Centered(rarity, width);
            var centeredName   = Centered(itemName, width);
            attributes.Insert(0, centeredRarity);
            attributes.Insert(0, centeredName);

            for (var i = 0; i < attributes.Count; i++)
            {
                var pad = new string(' ', width - attributes[i].Length);
                attributes[i] = frame[1] + " " + attributes[i] + pad + " " + frame[1];
            }

            Console.WriteLine(frame[0][0] 
                + new string(frame[0][1], width + 2) 
                + frame[0][2]);
        }
        else
        {
            var centeredRarity = Centered(rarity, width);
            var centeredName   = Centered(itemName, width);
            attributes.Insert(0, centeredRarity);
            attributes.Insert(0, centeredName);

            for (var i = 0; i < attributes.Count; i++)
            {
                var pad = new string(' ', width - attributes[i].Length);
                attributes[i] = i == 0
                    ? "[ " + attributes[i] + pad + " ]"
                    : "| " + attributes[i] + pad + " |";
            }

            var d   = (width - 3) / 2;
            var mod = (width + 1) % 2;
            Console.WriteLine("X-" 
                + new string('-', d) 
                + "\\_" 
                + new string('_', mod) 
                + "/" 
                + new string('-', d) 
                + "-X");
        }

        foreach (var line in attributes)
            Console.WriteLine(line);

        Console.WriteLine(frame[2][0] 
            + new string(frame[2][1], width + 2) 
            + frame[2][2]);
    }

    private static string Centered(string s, int width)
    {
        var totalPad = width - s.Length;
        var left     = totalPad / 2 + totalPad % 2;
        var right    = totalPad / 2;
        return new string(' ', left) + s + new string(' ', right);
    }

    private static readonly Dictionary<string,string[]> _frames = new Dictionary<string,string[]>
    {
        { "Common",    new[]{ "###", "#",   "###" } },
        { "Rare",      new[]{ "/#\\", "#",  "\\#/" } },
        { "Epic",      new[]{ "/-\\", "|",  "\\_/" } },
        { "Legendary", new[]{ "",    "",   "X_X" } }
    };
}
