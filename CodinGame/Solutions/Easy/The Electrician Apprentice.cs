using System;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        var C = int.Parse(Console.ReadLine().Trim());
        var circuits = new List<Circuit>(C);
        var switchStates = new Dictionary<string, bool>(StringComparer.Ordinal);

        for (var i = 0; i < C; i++)
        {
            var line = Console.ReadLine().Trim();
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var equip = parts[0];
            var segments = new List<Segment>();
            Segment current = null;
            for (var j = 1; j < parts.Length; j++)
            {
                var tok = parts[j];
                if (tok == "-" || tok == "=")
                {
                    current = new Segment { IsParallel = tok == "=" };
                    segments.Add(current);
                }
                else
                {
                    current.Switches.Add(tok);
                    if (!switchStates.ContainsKey(tok))
                        switchStates[tok] = false;
                }
            }
            circuits.Add(new Circuit { Name = equip, Segments = segments });
        }

        var A = int.Parse(Console.ReadLine().Trim());
        for (var i = 0; i < A; i++)
        {
            var sw = Console.ReadLine().Trim();
            switchStates[sw] = !switchStates[sw];
        }

        foreach (var c in circuits)
        {
            var on = true;
            foreach (var seg in c.Segments)
            {
                if (seg.IsParallel)
                {
                    var anyOn = false;
                    foreach (var sw in seg.Switches)
                        if (switchStates[sw]) { anyOn = true; break; }
                    if (!anyOn) { on = false; break; }
                }
                else
                {
                    foreach (var sw in seg.Switches)
                        if (!switchStates[sw]) { on = false; break; }
                    if (!on) break;
                }
            }
            Console.WriteLine($"{c.Name} is {(on?"ON":"OFF")}");
        }
    }

    private class Circuit
    {
        public string Name;
        public List<Segment> Segments;
    }

    private class Segment
    {
        public bool IsParallel;
        public List<string> Switches = new List<string>();
    }
}
