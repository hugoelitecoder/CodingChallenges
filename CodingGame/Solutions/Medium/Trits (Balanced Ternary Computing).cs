using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var lhs = new BalancedTernary(Console.ReadLine());
        var op  = Console.ReadLine();
        var rhs = Console.ReadLine();

        BalancedTernary res = op switch
        {
            "<<" => lhs.ShiftLeft(new BalancedTernary(rhs).ToInt()),
            ">>" => lhs.ShiftRight(new BalancedTernary(rhs).ToInt()),
            "+"  => lhs + new BalancedTernary(rhs),
            "-"  => lhs - new BalancedTernary(rhs),
            "*"  => lhs * new BalancedTernary(rhs),
            _    => throw new InvalidOperationException()
        };

        Console.WriteLine(res);
    }
}

class BalancedTernary
{
    private readonly string _digits;
    private readonly int _value;
    private const int Radix = 3;

    public BalancedTernary(string s)
    {
        _digits = Normalize(s);
        _value  = FromDigits(_digits);
    }

    private BalancedTernary(int v)
    {
        _value  = v;
        _digits = ToDigits(v);
    }

    public static BalancedTernary operator +(BalancedTernary a, BalancedTernary b)
        => new BalancedTernary(a._value + b._value);

    public static BalancedTernary operator -(BalancedTernary a, BalancedTernary b)
        => new BalancedTernary(a._value - b._value);

    public static BalancedTernary operator *(BalancedTernary a, BalancedTernary b)
        => new BalancedTernary(a._value * b._value);

    public BalancedTernary ShiftLeft(int k)
        => new BalancedTernary(_digits + new string('0', k));

    public BalancedTernary ShiftRight(int k)
        => _digits.Length <= k
            ? new BalancedTernary("0")
            : new BalancedTernary(_digits[..^k]);

    public override string ToString() => _digits;

    public int ToInt() => _value;

    static string Normalize(string s)
    {
        if (s == "0") return s;
        s = s.TrimStart('0');
        return s.Length > 0 ? s : "0";
    }

    static int FromDigits(string s)
    {
        int n = 0;
        foreach (char c in s)
        {
            n = n * Radix + (c == '1' ? 1 : c == 'T' ? -1 : 0);
        }
        return n;
    }

    static string ToDigits(int v)
    {
        if (v == 0) return "0";
        var cs = new List<char>();
        while (v != 0)
        {
            int r = v % Radix;
            v /= Radix;
            if (r == 2) { r = -1; v++; }
            else if (r == -2) { r = 1; v--; }
            cs.Add(r == 1 ? '1' : r == -1 ? 'T' : '0');
        }
        cs.Reverse();
        return new string(cs.ToArray());
    }
}
