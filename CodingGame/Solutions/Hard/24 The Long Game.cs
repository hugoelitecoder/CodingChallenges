using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var numbers = new List<int>
        {
            int.Parse(inputs[0]),
            int.Parse(inputs[1]),
            int.Parse(inputs[2]),
            int.Parse(inputs[3])
        };

        var solutions = new Game24Solver().Solve(numbers);

        if (solutions.Count == 0)
        {
            Console.WriteLine("not possible");
        }
        else
        {
            Console.WriteLine(solutions.Count);
            var sortedSolutions = solutions
                .OrderBy(s => s.Count(c => c == '('))
                .ThenBy(s => s, StringComparer.Ordinal);
            foreach (var solution in sortedSolutions)
            {
                Console.WriteLine(solution);
            }
        }
    }
}

public class Game24Solver
{
    private const double Epsilon = 1e-9;

    public HashSet<string> Solve(List<int> numbers)
    {
        var solutions = new HashSet<string>();
        var initialExpressions = numbers.Select(n => (Expression)new Constant(n)).ToList();
        FindSolutions(initialExpressions, solutions);
        return solutions;
    }

    private void FindSolutions(List<Expression> expressions, HashSet<string> solutions)
    {
        if (expressions.Count == 1)
        {
            var expr = expressions[0];
            if (Math.Abs(expr.Value - 24) < Epsilon)
            {
                solutions.Add(expr.CanonicalString);
            }
            return;
        }

        for (var i = 0; i < expressions.Count; i++)
        {
            for (var j = i + 1; j < expressions.Count; j++)
            {
                var e1 = expressions[i];
                var e2 = expressions[j];
                
                var remaining = new List<Expression>();
                for(var k=0; k<expressions.Count; k++)
                {
                    if (k != i && k != j)
                    {
                        remaining.Add(expressions[k]);
                    }
                }

                var operations = new List<Expression> { new Operation(e1, '+', e2) };
                operations.Add(new Operation(e1, '-', e2));
                operations.Add(new Operation(e2, '-', e1));
                operations.Add(new Operation(e1, '*', e2));
                if (Math.Abs(e2.Value) > Epsilon) operations.Add(new Operation(e1, '/', e2));
                if (Math.Abs(e1.Value) > Epsilon) operations.Add(new Operation(e2, '/', e1));

                foreach (var opExpr in operations)
                {
                    var nextExpressions = new List<Expression>(remaining) { opExpr };
                    FindSolutions(nextExpressions, solutions);
                }
            }
        }
    }
}

public abstract class Expression : IComparable<Expression>
{
    private double? _value;
    public double Value => _value ??= Evaluate();

    private string _canonicalString;
    public string CanonicalString => _canonicalString ??= ToCanonical();

    protected abstract double Evaluate();
    protected abstract string ToCanonical();
    public abstract bool IsConstant();

    public int CompareTo(Expression other)
    {
        if (other == null) return 1;

        var isThisConstant = IsConstant();
        var isOtherConstant = other.IsConstant();

        if (isThisConstant && !isOtherConstant) return -1;
        if (!isThisConstant && isOtherConstant) return 1;

        if (isThisConstant && isOtherConstant)
        {
            return Value.CompareTo(other.Value);
        }

        var valueComparison = Value.CompareTo(other.Value);
        if (Math.Abs(Value - other.Value) > 1e-9) return valueComparison;

        return string.Compare(CanonicalString, other.CanonicalString, StringComparison.Ordinal);
    }
}

public class Constant : Expression
{
    private readonly double _val;
    public Constant(double val) { _val = val; }
    protected override double Evaluate() => _val;
    protected override string ToCanonical() => _val.ToString();
    public override bool IsConstant() => true;
}

public class Operation : Expression
{
    public Expression Left { get; }
    public char Operator { get; }
    public Expression Right { get; }

    public Operation(Expression left, char op, Expression right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
    
    public override bool IsConstant() => false;

    protected override double Evaluate()
    {
        switch (Operator)
        {
            case '+': return Left.Value + Right.Value;
            case '-': return Left.Value - Right.Value;
            case '*': return Left.Value * Right.Value;
            case '/': return Left.Value / Right.Value;
            default: throw new InvalidOperationException();
        }
    }

    protected override string ToCanonical()
    {
        if (Operator == '+' || Operator == '-')
        {
            var pos = new List<Expression>();
            var neg = new List<Expression>();
            CollectAddition(this, true, pos, neg);
            pos.Sort();
            neg.Sort();
            
            var sb = new StringBuilder();
            sb.Append(FormatChild(pos[0], ' '));
            for(var i = 1; i < pos.Count; i++) sb.Append('+').Append(FormatChild(pos[i], ' '));
            foreach(var term in neg) sb.Append('-').Append(FormatChild(term, ' '));
            return sb.ToString();
        }

        if (Operator == '*' || Operator == '/')
        {
            var num = new List<Expression>();
            var den = new List<Expression>();
            CollectMultiplication(this, true, num, den);
            num.Sort();
            den.Sort();

            var sb = new StringBuilder();
            sb.Append(FormatChild(num[0], ' '));
            for(var i = 1; i < num.Count; i++) sb.Append('*').Append(FormatChild(num[i], ' '));
            foreach(var term in den) sb.Append('/').Append(FormatChild(term, ' '));
            return sb.ToString();
        }
        
        throw new InvalidOperationException("Invalid operator");
    }
    
    private void CollectAddition(Expression expr, bool isPositive, List<Expression> pos, List<Expression> neg)
    {
        if (expr is Operation opExpr && (opExpr.Operator == '+' || opExpr.Operator == '-'))
        {
            if (opExpr.Operator == '+')
            {
                CollectAddition(opExpr.Left, isPositive, pos, neg);
                CollectAddition(opExpr.Right, isPositive, pos, neg);
            }
            else
            {
                CollectAddition(opExpr.Left, isPositive, pos, neg);
                CollectAddition(opExpr.Right, !isPositive, pos, neg);
            }
        }
        else
        {
            if (isPositive) pos.Add(expr); else neg.Add(expr);
        }
    }

    private void CollectMultiplication(Expression expr, bool isNumerator, List<Expression> num, List<Expression> den)
    {
        if (expr is Operation opExpr && (opExpr.Operator == '*' || opExpr.Operator == '/'))
        {
            if (opExpr.Operator == '*')
            {
                CollectMultiplication(opExpr.Left, isNumerator, num, den);
                CollectMultiplication(opExpr.Right, isNumerator, num, den);
            }
            else
            {
                CollectMultiplication(opExpr.Left, isNumerator, num, den);
                CollectMultiplication(opExpr.Right, !isNumerator, num, den);
            }
        }
        else
        {
            if (isNumerator) num.Add(expr); else den.Add(expr);
        }
    }
    
    private static int GetPrecedence(char op) => (op == '+' || op == '-') ? 1 : 2;

    private string FormatChild(Expression child, char side)
    {
        if (child is not Operation childOp)
        {
            return child.CanonicalString;
        }

        var childPrec = GetPrecedence(childOp.Operator);
        var parentPrec = GetPrecedence(this.Operator);
        
        var needsParens = childPrec < parentPrec || 
                         (childPrec == parentPrec && side == 'R' && (Operator == '-' || Operator == '/'));

        return needsParens ? $"({child.CanonicalString})" : child.CanonicalString;
    }
}