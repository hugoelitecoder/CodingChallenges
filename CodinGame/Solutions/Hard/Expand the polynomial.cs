using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var input = Console.ReadLine();
        var polys = ParseFactors(input);
        var poly = Polynomial.One;
        foreach (var p in polys)
            poly = poly.Multiply(p);
        Console.WriteLine(poly.ToString());
    }

    static List<Polynomial> ParseFactors(string s)
    {
        var factors = new List<Polynomial>();
        var i = 0;
        while (i < s.Length)
        {
            while (i < s.Length && (s[i] == '*' || s[i] == ' ')) i++;
            if (i == s.Length) break;
            if (s[i] == '(')
            {
                var j = i + 1;
                var depth = 1;
                while (j < s.Length && depth > 0)
                {
                    if (s[j] == '(') depth++;
                    if (s[j] == ')') depth--;
                    j++;
                }
                var inside = s.Substring(i + 1, j - i - 2);
                var power = 1;
                if (j < s.Length && s[j] == '^')
                {
                    var k = j + 1;
                    while (k < s.Length && (char.IsDigit(s[k]) || s[k] == '-')) k++;
                    power = int.Parse(s.Substring(j + 1, k - (j + 1)));
                    j = k;
                }
                var poly = Polynomial.Parse(inside).Power(power);
                factors.Add(poly);
                i = j;
            }
            else
            {
                var j = i;
                while (j < s.Length && s[j] != '*') j++;
                var seg = s.Substring(i, j - i).Trim();
                if (seg != "")
                    factors.Add(Polynomial.Parse(seg));
                i = j;
            }
        }
        if (factors.Count == 0)
            factors.Add(Polynomial.Parse(s));
        return factors;
    }
}

class Polynomial
{
    List<int> _coeffs;
    public static Polynomial One => new Polynomial(new List<int> { 1 });

    public Polynomial(List<int> coeffs)
    {
        _coeffs = Trim(coeffs);
    }

    static List<int> Trim(List<int> coeffs)
    {
        var n = coeffs.Count - 1;
        while (n > 0 && coeffs[n] == 0) n--;
        var res = new List<int>();
        for (var i = 0; i <= n; i++) res.Add(coeffs[i]);
        return res;
    }

    public static Polynomial Parse(string f)
    {
        var res = new List<int>();
        f = f.Replace("-", "+-");
        var terms = new List<string>();
        var sb = "";
        foreach (var ch in f)
        {
            if (ch == '+')
            {
                if (sb != "") { terms.Add(sb); sb = ""; }
            }
            else sb += ch;
        }
        if (sb != "") terms.Add(sb);
        foreach (var t in terms)
        {
            var s = t.Trim();
            if (s == "") continue;
            if (s.Contains("x^"))
            {
                var k = s.IndexOf("x^");
                var c = s.Substring(0, k);
                if (c == "" || c == "+") c = "1";
                if (c == "-") c = "-1";
                var d = int.Parse(s.Substring(k + 2));
                while (res.Count <= d) res.Add(0);
                res[d] += int.Parse(c);
            }
            else if (s.Contains("x"))
            {
                var k = s.IndexOf("x");
                var c = s.Substring(0, k);
                if (c == "" || c == "+") c = "1";
                if (c == "-") c = "-1";
                while (res.Count <= 1) res.Add(0);
                res[1] += int.Parse(c);
            }
            else
            {
                var c = s;
                while (res.Count <= 0) res.Add(0);
                res[0] += int.Parse(c);
            }
        }
        return new Polynomial(res);
    }

    public Polynomial Multiply(Polynomial other)
    {
        var a = _coeffs;
        var b = other._coeffs;
        var c = new List<int>(new int[a.Count + b.Count - 1]);
        for (var i = 0; i < a.Count; i++)
            for (var j = 0; j < b.Count; j++)
                c[i + j] += a[i] * b[j];
        return new Polynomial(c);
    }

    public Polynomial Power(int p)
    {
        var res = One;
        for (var i = 0; i < p; i++)
            res = res.Multiply(this);
        return res;
    }

    public override string ToString()
    {
        var sb = "";
        for (var i = _coeffs.Count - 1; i >= 0; i--)
        {
            var c = _coeffs[i];
            if (c == 0) continue;
            if (sb.Length > 0)
                sb += c > 0 ? "+" : "";
            if (i == 0)
                sb += c.ToString();
            else
            {
                if (c == 1)
                    sb += "x";
                else if (c == -1)
                    sb += "-x";
                else
                    sb += c.ToString() + "x";
                if (i > 1) sb += "^" + i.ToString();
            }
        }
        return sb == "" ? "0" : sb;
    }
}
