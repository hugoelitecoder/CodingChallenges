using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        var input = new string(Console.ReadLine()
                             .Where(c => !char.IsWhiteSpace(c))
                             .ToArray());
        int pos = 0;
        var result = ParseExpr(input, ref pos);
        var output = Enumerable.Range(-100, 201)
                               .Where(x => result[x + 100]);
        Console.WriteLine(output.Any()
            ? string.Join(" ", output)
            : "EMPTY");
    }

    static bool[] ParseExpr(string s, ref int pos)
    {
        var acc = ParseFactor(s, ref pos);
        while (pos < s.Length && "UI-".Contains(s[pos]))
        {
            char op = s[pos++];
            var rhs = ParseFactor(s, ref pos);
            acc = acc.Zip(rhs, (a, b) =>
                op == 'U' ? a | b :
                op == 'I' ? a & b :
                            a & !b
            ).ToArray();
        }
        return acc;
    }

    static bool[] ParseFactor(string s, ref int pos) => s[pos] switch
    {
        '{'       => ParseBrace(s, ref pos),
        '[' or ']'=> ParseInterval(s, ref pos),
        '('       => ParseParens(s, ref pos),
        _         => throw new InvalidOperationException()
    };

    static bool[] ParseBrace(string s, ref int pos)
    {
        pos++; 
        var set = new bool[201];
        do
        {
            int v = ParseInt(s, ref pos);
            set[v + 100] = true;
        }
        while (s[pos] == ';' && ++pos > 0);
        pos++; 
        return set;
    }

    static bool[] ParseInterval(string s, ref int pos)
    {
        char left = s[pos++], right;
        int a = ParseInt(s, ref pos);
        pos++; 
        int b = ParseInt(s, ref pos);
        right = s[pos++];
        var set = new bool[201];
        int lo = left == '[' ? a : a + 1;
        int hi = right == ']' ? b : b - 1;
        for (int x = lo; x <= hi; x++)
            if (x >= -100 && x <= 100)
                set[x + 100] = true;
        return set;
    }

    static bool[] ParseParens(string s, ref int pos)
    {
        pos++;
        var inner = ParseExpr(s, ref pos);
        pos++;
        return inner;
    }

    static int ParseInt(string s, ref int pos)
    {
        bool neg = s[pos] == '-' && ++pos > 0;
        int v = 0;
        while (pos < s.Length && char.IsDigit(s[pos]))
            v = v * 10 + (s[pos++] - '0');
        return neg ? -v : v;
    }
}
