using System;
using System.Linq;
using System.Text.RegularExpressions;

class Solution
{
    record Q(long R, long I, long J, long K)
    {
        public static Q operator *(Q a, Q b) => (a, b) switch
        {
            (var (r1,i1,j1,k1), var (r2,i2,j2,k2)) => new Q(
                r1*r2 - i1*i2 - j1*j2 - k1*k2,
                r1*i2 + i1*r2 + j1*k2 - k1*j2,
                r1*j2 - i1*k2 + j1*r2 + k1*i2,
                r1*k2 + i1*j2 - j1*i2 + k1*r2),
        };
    }

    static void Main()
    {
        var expr = Console.ReadLine().Trim();
        var product = expr
            .Split('(', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.Contains(')'))
            .Select(p => p[..p.IndexOf(')')])
            .Select(ParseQ)
            .Aggregate(new Q(1,0,0,0), (acc, q) => acc * q);

        Console.WriteLine(Format(product));
    }

    static Q ParseQ(string s)
    {
        var pattern = new Regex(@"([+-]?)(\d*)([ijk]?)");
        var matches = pattern.Matches(s);
        long r=0,i=0,j=0,k=0;
        foreach (Match m in matches)
        {
            if (string.IsNullOrEmpty(m.Value)) continue;
            var sign = m.Groups[1].Value == "-" ? -1L : 1L;
            var numText = m.Groups[2].Value;
            var num = (string.IsNullOrEmpty(numText) ? 1L : long.Parse(numText)) * sign;
            switch (m.Groups[3].Value)
            {
                case "i": i += num; break;
                case "j": j += num; break;
                case "k": k += num; break;
                default:  r += num; break;
            }
        }
        return new Q(r, i, j, k);
    }

    static string Format(Q q)
    {
        var parts = new[]
        {
            (q.I, "i"),
            (q.J, "j"),
            (q.K, "k"),
        }
        .Where(t => t.Item1 != 0)
        .Select(t => t.Item1 switch
        {
            1 => t.Item2,
            -1 => "-" + t.Item2,
            _ => t.Item1 + t.Item2
        })
        .ToList();

        if (q.R != 0 || parts.Count == 0)
            parts.Add(q.R.ToString());

        return string.Join("+", parts)
                     .Replace("+-", "-");
    }
}
