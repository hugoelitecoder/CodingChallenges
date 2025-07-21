using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine()!);
        var cyborgs = new List<string>();
        for (int i = 0; i < N; i++)
            cyborgs.Add(Console.ReadLine()!);

        int M = int.Parse(Console.ReadLine()!);
        var mayhemAttrs = new Dictionary<string, string>();
        for (int i = 0; i < M; i++)
            ParseReport(Console.ReadLine()!, mayhemAttrs, null);

        int C = int.Parse(Console.ReadLine()!);
        var cyborgAttrs = new Dictionary<string, Dictionary<string, string>>();
        foreach (var name in cyborgs)
            cyborgAttrs[name] = new Dictionary<string, string>();
        for (int i = 0; i < C; i++)
        {
            string line = Console.ReadLine()!;
            ParseReport(line, null, cyborgAttrs);
        }

        // Filter candidates
        var candidates = new List<string>();
        foreach (var name in cyborgs)
        {
            var attrs = cyborgAttrs[name];
            bool ok = true;

            // Check simple attributes
            foreach (var kv in mayhemAttrs)
            {
                string attr = kv.Key, val = kv.Value;
                if (attr == "word")
                {
                    // Mayhem's word must appear in catchphrase or match word
                    if (attrs.TryGetValue("word", out var w))
                    {
                        if (w != val) { ok = false; break; }
                    }
                    else if (attrs.TryGetValue("catchphrase", out var phrase))
                    {
                        if (!phrase.Contains(val)) { ok = false; break; }
                    }
                }
                else
                {
                    if (attrs.TryGetValue(attr, out var cv) && cv != val)
                    {
                        ok = false;
                        break;
                    }
                }
            }

            if (ok)
                candidates.Add(name);
        }

        // Output result
        if (candidates.Count == 1)
            Console.WriteLine(candidates[0]);
        else if (candidates.Count == 0)
            Console.WriteLine("MISSING");
        else
            Console.WriteLine("INDETERMINATE");
    }

    // If mayhemMap != null, subject must be "Mayhem" and we fill mayhemMap[attr]=val
    // Otherwise, we fill into cyborgAttrs[name][attr]=val
    static void ParseReport(string line,
                            Dictionary<string,string>? mayhemMap,
                            Dictionary<string,Dictionary<string,string>>? cyborgMap)
    {
        // Split at " is "
        int idx = line.IndexOf(" is ");
        var left = line.Substring(0, idx);
        var right = line.Substring(idx + 4);

        // Parse subject and attribute
        var parts = left.Split(new[]{ "'s " }, StringSplitOptions.None);
        string subj = parts[0], attr = parts[1];

        // Clean value
        var val = right.Trim();
        if (val.StartsWith("\"") && val.EndsWith("\""))
            val = val[1..^1];
        else if (val.StartsWith("a "))
            val = val[2..].Trim();
        else if (val.StartsWith("an "))
            val = val[3..].Trim();

        if (mayhemMap != null && subj == "Mayhem")
        {
            mayhemMap[attr] = val;
        }
        else if (cyborgMap != null && cyborgMap.ContainsKey(subj))
        {
            cyborgMap[subj]![attr] = val;
        }
    }
}
