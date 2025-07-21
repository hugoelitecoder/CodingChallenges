using System;
using System.Collections.Generic;
using System.Linq;

abstract class Expr
{
    public abstract Expr Diff(string v);
    public abstract Expr Simplify();
    public abstract double Eval(Dictionary<string,double> env);
    public override string ToString() => this.Print();
    protected abstract string Print();
}

class Const : Expr
{
    public readonly double Value;
    public Const(double v) { Value = v; }
    public override Expr Diff(string v) => new Const(0);
    public override Expr Simplify() => this;
    public override double Eval(Dictionary<string,double> env) => Value;
    protected override string Print() => Value.ToString("G");
}

class Var : Expr
{
    public readonly string Name;
    public Var(string n) { Name = n; }
    public override Expr Diff(string v) => v == Name ? new Const(1) : new Const(0);
    public override Expr Simplify() => this;
    public override double Eval(Dictionary<string,double> env) => env[Name];
    protected override string Print() => Name;
}

class Add : Expr
{
    public Expr L, R;
    public Add(Expr l, Expr r) { L = l; R = r; }
    public override Expr Diff(string v)
        => new Add(L.Diff(v), R.Diff(v)).Simplify();
    public override Expr Simplify()
    {
        var l = L.Simplify();
        var r = R.Simplify();
        if (l is Const lc && lc.Value == 0) return r;
        if (r is Const rc && rc.Value == 0) return l;
        if (l is Const lc2 && r is Const rc2)
            return new Const(lc2.Value + rc2.Value);
        return new Add(l, r);
    }
    public override double Eval(Dictionary<string,double> env)
        => L.Eval(env) + R.Eval(env);
    protected override string Print() => $"({L}+{R})";
}

class Mul : Expr
{
    public Expr L, R;
    public Mul(Expr l, Expr r) { L = l; R = r; }
    public override Expr Diff(string v)
        => new Add(
               new Mul(L.Diff(v), R),
               new Mul(L, R.Diff(v))
           ).Simplify();
    public override Expr Simplify()
    {
        var l = L.Simplify();
        var r = R.Simplify();
        if (l is Const lc && lc.Value == 0) return new Const(0);
        if (r is Const rc && rc.Value == 0) return new Const(0);
        if (l is Const lc1 && lc1.Value == 1) return r;
        if (r is Const rc1 && rc1.Value == 1) return l;
        if (l is Const lc2 && r is Const rc2)
            return new Const(lc2.Value * rc2.Value);
        return new Mul(l, r);
    }
    public override double Eval(Dictionary<string,double> env)
        => L.Eval(env) * R.Eval(env);
    protected override string Print() => $"({L}*{R})";
}

class Pow : Expr
{
    public Expr Base, Exp;
    public Pow(Expr b, Expr e) { Base = b; Exp = e; }

    public override Expr Diff(string v)
    {
        var db = Base.Diff(v).Simplify();
        var de = Exp.Diff(v).Simplify();
        if (de is Const cde && cde.Value == 0)
        {
            Console.Error.WriteLine($"  -- Power rule (exp constant): base={Base}, exp={Exp}");
            return new Mul(
                Exp,
                new Mul(
                    new Pow(Base, new Add(Exp, new Const(-1))),
                    db
                )
            ).Simplify();
        }
        Console.Error.WriteLine($"  -- Exponent depends on '{v}', returning 0");
        return new Const(0);
    }

    public override Expr Simplify()
    {
        var b = Base.Simplify();
        var e = Exp.Simplify();
        if (b is Const cb && e is Const ce)
            return new Const(Math.Pow(cb.Value, ce.Value));
        return new Pow(b, e);
    }

    public override double Eval(Dictionary<string,double> env)
    {
        var vb = Base.Eval(env);
        var ve = Exp.Eval(env);
        return Math.Pow(vb, ve);
    }

    protected override string Print() => $"({Base}^{Exp})";
}

class Parser
{
    string s; int i;
    public Parser(string str) { s = str; i = 0; }
    public Expr ParseExpr()
    {
        Skip();
        if (i < s.Length && s[i] == '(')
        {
            i++; Skip();
            var left = ParseExpr();
            Skip();
            if (i < s.Length && s[i] == ')')
            {
                i++; return left;
            }
            var op = s[i++];
            Skip();
            var right = ParseExpr();
            Skip();
            if (i >= s.Length || s[i] != ')')
                throw new Exception("Expected ')'");
            i++;
            return op switch
            {
                '+' => new Add(left, right),
                '*' => new Mul(left, right),
                '^' => new Pow(left, right),
                _   => throw new Exception($"Unknown op '{op}'")
            };
        }
        Skip();
        if (i < s.Length && (char.IsDigit(s[i]) || s[i] == '-'))
        {
            int start = i++;
            while (i < s.Length && char.IsDigit(s[i])) i++;
            var tok = s[start..i];
            return new Const(double.Parse(tok));
        }
        if (i < s.Length && (char.IsLetter(s[i]) || s[i] == '_'))
        {
            int start = i++;
            while (i < s.Length && (char.IsLetterOrDigit(s[i])||s[i]=='_')) i++;
            return new Var(s[start..i]);
        }
        throw new Exception($"Unexpected '{s[i]}'");
    }
    void Skip() { while (i < s.Length && char.IsWhiteSpace(s[i])) i++; }
}

class Solution
{
    static void Main()
    {
        var formula = Console.ReadLine();
        Console.Error.WriteLine("Input formula: " + formula);
        var expr = new Parser(formula).ParseExpr().Simplify();
        Console.Error.WriteLine("Parsed & simplified: " + expr);

        var vars = Console.ReadLine().Split(' ');
        Console.Error.WriteLine("Differentiate in order: " + string.Join(", ", vars));

        foreach (var v in vars)
        {
            Console.Error.WriteLine($"\n--- d/d{v} ---");
            expr = expr.Diff(v).Simplify();
            Console.Error.WriteLine("Resulting expr: " + expr);
        }

        var parts = Console.ReadLine().Split(' ');
        var env = new Dictionary<string,double>();
        for (int i = 0; i+1 < parts.Length; i += 2)
            env[parts[i]] = double.Parse(parts[i+1]);

        Console.Error.WriteLine(
          "Environment: " +
          string.Join(", ", env.Select(kv=>$"{kv.Key}={kv.Value}"))
        );

        var value = expr.Eval(env);
        Console.WriteLine((long)Math.Round(value));
    }
}
