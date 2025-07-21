using System;
using System.Collections.Generic;
using System.Linq;

namespace ReverseCompose
{
    abstract class Expr { }
    class VarExpr   : Expr { public char Name; public VarExpr(char c)=>Name=c; }
    class ParExpr   : Expr { public Expr Sub;  public ParExpr(Expr e)=>Sub=e; }
    class AddExpr   : Expr { public Expr Left,Right; public AddExpr(Expr l,Expr r){Left=l;Right=r;} }
    class AppExpr   : Expr { public Expr Left,Right; public AppExpr(Expr l,Expr r){Left=l;Right=r;} }

    class Parser
    {
        readonly string[] _tokens;
        int _pos;
        public Parser(string input)
        {
            input = input
                .Replace("(", " ( ")
                .Replace(")", " ) ");
            _tokens = input
                .Split(new[]{' ','\t'}, StringSplitOptions.RemoveEmptyEntries);
        }

        string Peek() => _pos < _tokens.Length ? _tokens[_pos] : null;
        string Next() => _pos < _tokens.Length ? _tokens[_pos++] : null;

        public Expr ParseExpr()
        {
            var left = ParseTerm();
            while (Peek()=="+" )
            {
                Next();
                var right = ParseTerm();
                left = new AddExpr(left,right);
            }
            return left;
        }

        Expr ParseTerm()
        {
            var left = ParseFactor();
            while (Peek()=="." )
            {
                Next(); 
                var right = ParseFactor();
                left = new AppExpr(left,right);
            }
            return left;
        }

        Expr ParseFactor()
        {
            var tok = Peek();
            if (tok=="(")
            {
                Next();
                var e = ParseExpr();
                if (Next()!=")")
                    throw new Exception("Missing closing ')'");
                return new ParExpr(e);
            }
            if (tok!=null && tok.Length==1 && char.IsLower(tok[0]))
            {
                Next();
                return new VarExpr(tok[0]);
            }
            throw new Exception($"Unexpected token '{tok}'");
        }
    }

    class Printer
    {
        public static string Print(Expr e) => PrintExpr(e);

        static string PrintExpr(Expr e)
        {
            switch(e)
            {
                case AddExpr a:
                    return PrintExpr(a.Left) + " + " + PrintExpr(a.Right);
                case AppExpr _:
                case VarExpr _:
                case ParExpr _:
                    return PrintComp(e);
                default:
                    throw new Exception();
            }
        }

        static string PrintComp(Expr e)
        {
            var spine = new List<Expr>();
            void Collect(Expr x)
            {
                if (x is AppExpr app)
                {
                    Collect(app.Left);
                    Collect(app.Right);
                }
                else spine.Add(x);
            }
            Collect(e);

            if (spine.Count==1 && !(e is AppExpr))
                return PrintFactor(spine[0]);

            var parts = spine
                .AsEnumerable()
                .Reverse()
                .Select(PrintFactor);

            return string.Join(" |> ", parts);
        }

        static string PrintFactor(Expr e)
        {
            switch(e)
            {
                case VarExpr v:
                    return v.Name.ToString();
                case ParExpr p:
                    return "(" + PrintExpr(p.Sub) + ")";
                case AddExpr a:
                    return "(" + PrintExpr(a) + ")";
                default:
                    return PrintComp(e);
            }
        }
    }

    class Program
    {
        static void Main()
        {
            var line = Console.ReadLine();
            var parser = new Parser(line);
            var ast = parser.ParseExpr();
            Console.WriteLine(Printer.Print(ast));
        }
    }
}
