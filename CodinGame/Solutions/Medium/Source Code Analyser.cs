using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

class Solution
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        var sbCode = new StringBuilder();
        for (int i = 0; i < n; i++)
            sbCode.AppendLine(Console.ReadLine());

        var cleaned = Clean(sbCode.ToString());

        var reserved = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "and","array","echo","else","elseif","if","for","foreach",
            "function","or","return","while","new"
        };

        var userDefined = Regex.Matches(cleaned, @"\b(?:function|new)\s+([A-Za-z_]\w*)\s*\(", RegexOptions.IgnoreCase)
            .Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .ToHashSet();

        var counts = Regex.Matches(cleaned, @"(?<![$\w])([A-Za-z_]\w*)\s*\(")
            .Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .Where(name => !reserved.Contains(name) && !userDefined.Contains(name))
            .GroupBy(name => name)
            .ToDictionary(g => g.Key, g => g.Count());

        if (!counts.Any())
            Console.WriteLine("NONE");
        else
            foreach (var key in counts.Keys.OrderBy(k => k, StringComparer.Ordinal))
                Console.WriteLine($"{key} {counts[key]}");
    }

    static string Clean(string code)
    {
        var sb = new StringBuilder();
        bool inS = false, inD = false, inL = false, inB = false;

        for (int i = 0; i < code.Length; i++)
        {
            var c = code[i];
            var next = i + 1 < code.Length ? code[i + 1] : '\0';

            switch (c)
            {
                case '/' when !inS && !inD && !inB && next == '/':
                    inL = true; i++; break;
                case '*' when inB && next == '/':
                    inB = false; i++; break;
                case '/' when !inS && !inD && !inL && next == '*':
                    inB = true; i++; break;
                case '\n' when inL:
                    inL = false; sb.Append(c); break;
                case '\'' when !inD && !inL && !inB:
                    inS = !inS; break;
                case '"' when !inS && !inL && !inB:
                    inD = !inD; break;
                default:
                    if (inS || inD)
                    {
                        if (c == '\\')
                            i++;
                    }
                    else if (!inL && !inB)
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }

        return sb.ToString();
    }
}