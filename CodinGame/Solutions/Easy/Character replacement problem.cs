using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var instr = (Console.ReadLine() ?? string.Empty).Split(' ');
        var map = new Dictionary<char, char>();
        foreach (var token in instr)
        {
            if (token.Length != 2) continue;
            char src = token[0], dst = token[1];
            if (src == dst) continue;
            if (map.ContainsKey(src) && map[src] != dst)
            {
                Console.WriteLine("ERROR");
                return;
            }
            map[src] = dst;
        }
        
        foreach (var kv in map)
        {
            var seen = new HashSet<char>();
            char cur = kv.Key;
            while (map.TryGetValue(cur, out var nxt))
            {
                if (seen.Contains(nxt))
                {
                    Console.WriteLine("ERROR");
                    return;
                }
                seen.Add(nxt);
                cur = nxt;
            }
        }

        var finalMap = new Dictionary<char, char>();
        foreach (var c in map.Keys)
        {
            char cur = c;
            while (map.TryGetValue(cur, out var nxt)) cur = nxt;
            finalMap[c] = cur;
        }
        
        int n = int.Parse(Console.ReadLine() ?? "0");
        var output = new string[n];
        for (int i = 0; i < n; i++)
        {
            var line = Console.ReadLine() ?? string.Empty;
            var sb = new System.Text.StringBuilder(line.Length);
            foreach (char ch in line)
            {
                if (finalMap.TryGetValue(ch, out var rep)) sb.Append(rep);
                else sb.Append(ch);
            }
            output[i] = sb.ToString();
        }
        
        foreach (var l in output)
            Console.WriteLine(l);
    }
}