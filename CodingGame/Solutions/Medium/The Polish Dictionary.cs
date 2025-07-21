using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        string[] tokens = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var converter = new InfixConverter();
        Console.WriteLine(converter.Convert(tokens));
    }
}

class InfixConverter
{
    private struct Expr
    {
        public string Text;
        public int Prec;
        public Expr(string text, int prec)
        {
            Text = text;
            Prec = prec;
        }
    }

    public string Convert(string[] tokens)
    {
        var stack = new Stack<Expr>();
        foreach (var token in tokens)
        {
            if (IsOperator(token))
            {
                var right = stack.Pop();
                var left = stack.Pop();
                stack.Push(CreateOperatorExpr(token, left, right));
            }
            else
            {
                stack.Push(new Expr(token, int.MaxValue));
            }
        }
        return stack.Pop().Text;
    }

    private Expr CreateOperatorExpr(string op, Expr left, Expr right)
    {
        int prec = GetPrecedence(op);
        string lt = left.Prec < prec ? "(" + left.Text + ")" : left.Text;
        string rt = right.Prec < prec || (right.Prec == prec && (op == "-" || op == "/"))
            ? "(" + right.Text + ")" : right.Text;
        return new Expr(lt + " " + op + " " + rt, prec);
    }

    private int GetPrecedence(string op)
    {
        switch (op)
        {
            case "+":
            case "-": return 1;
            case "*":
            case "/": return 2;
            default: return int.MaxValue;
        }
    }

    private bool IsOperator(string token)
        => token == "+" || token == "-" || token == "*" || token == "/";
}


