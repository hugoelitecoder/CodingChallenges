using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

class Solution
{
    static void Main(string[] args)
    {
        var watch = Stopwatch.StartNew();

        var line = Console.ReadLine().Trim();
        Console.Error.WriteLine($"[DEBUG] Input expression: \"{line}\"");

        var finalDistribution = DiceExpressionEvaluator.Evaluate(line);

        finalDistribution.Print(Console.Out);

        watch.Stop();
        Console.Error.WriteLine($"[DEBUG] Execution time: {watch.ElapsedMilliseconds} ms");
    }
}

public class DiceProbability
{
    private readonly IReadOnlyDictionary<int, double> _distribution;

    public DiceProbability(int constantValue)
    {
        _distribution = new Dictionary<int, double> { { constantValue, 1.0 } };
    }

    private DiceProbability(IReadOnlyDictionary<int, double> distribution)
    {
        _distribution = distribution;
    }

    public static DiceProbability Roll(int sides)
    {
        if (sides <= 0)
        {
            throw new ArgumentException("Die must have a positive number of sides.");
        }
        var prob = 1.0 / sides;
        var dist = Enumerable.Range(1, sides).ToDictionary(i => i, i => prob);
        return new DiceProbability(dist);
    }

    public void Print(TextWriter writer)
    {
        var culture = CultureInfo.InvariantCulture;
        foreach (var kvp in _distribution.OrderBy(kv => kv.Key))
        {
            if (kvp.Value > 1e-12)
            {
                writer.WriteLine($"{kvp.Key} {(kvp.Value * 100).ToString("F2", culture)}");
            }
        }
    }

    private static DiceProbability Combine(DiceProbability a, DiceProbability b, Func<int, int, int> operation)
    {
        var result = new Dictionary<int, double>();
        foreach (var (valA, probA) in a._distribution)
        {
            foreach (var (valB, probB) in b._distribution)
            {
                var newVal = operation(valA, valB);
                var newProb = probA * probB;
                result[newVal] = result.GetValueOrDefault(newVal, 0) + newProb;
            }
        }
        return new DiceProbability(result);
    }

    public static DiceProbability operator +(DiceProbability a, DiceProbability b) => Combine(a, b, (x, y) => x + y);
    public static DiceProbability operator -(DiceProbability a, DiceProbability b) => Combine(a, b, (x, y) => x - y);
    public static DiceProbability operator *(DiceProbability a, DiceProbability b) => Combine(a, b, (x, y) => x * y);

    public static DiceProbability operator >(DiceProbability a, DiceProbability b)
    {
        var successProb = 0.0;
        foreach (var (valA, probA) in a._distribution)
        {
            foreach (var (valB, probB) in b._distribution)
            {
                if (valA > valB)
                {
                    successProb += probA * probB;
                }
            }
        }
        var result = new Dictionary<int, double>
        {
            { 1, successProb },
            { 0, 1.0 - successProb }
        };
        return new DiceProbability(result);
    }

    public static DiceProbability operator <(DiceProbability a, DiceProbability b) => b > a;
}

public static class DiceExpressionEvaluator
{
    private static readonly Regex TokenizerRegex = new(@"d\d+|\d+|[+\-*>]|\(|\)", RegexOptions.Compiled);
    private static readonly Dictionary<char, int> Precedence = new() { { '*', 3 }, { '+', 2 }, { '-', 2 }, { '>', 1 } };

    public static DiceProbability Evaluate(string expression)
    {
        var tokens = TokenizerRegex.Matches(expression).Cast<Match>().Select(m => m.Value).ToList();
        Console.Error.WriteLine($"[DEBUG] Tokens: {string.Join(" ", tokens)}");
        var rpn = ConvertToRPN(tokens);
        Console.Error.WriteLine($"[DEBUG] RPN: {string.Join(" ", rpn)}");
        return EvaluateRPN(rpn);
    }

    private static List<string> ConvertToRPN(List<string> tokens)
    {
        var output = new List<string>();
        var ops = new Stack<string>();
        foreach (var token in tokens)
        {
            if (char.IsDigit(token[0]) || token[0] == 'd')
            {
                output.Add(token);
            }
            else if (token == "(")
            {
                ops.Push(token);
            }
            else if (token == ")")
            {
                while (ops.Peek() != "(")
                {
                    output.Add(ops.Pop());
                }
                ops.Pop();
            }
            else
            {
                var tokenChar = token[0];
                while (ops.Count > 0 && ops.Peek() != "(" && Precedence.GetValueOrDefault(ops.Peek()[0], 0) >= Precedence[tokenChar])
                {
                    output.Add(ops.Pop());
                }
                ops.Push(token);
            }
        }
        while (ops.Count > 0)
        {
            output.Add(ops.Pop());
        }
        return output;
    }

    private static DiceProbability EvaluateRPN(List<string> rpnTokens)
    {
        var stack = new Stack<DiceProbability>();
        foreach (var token in rpnTokens)
        {
            if (char.IsDigit(token[0]))
            {
                stack.Push(new DiceProbability(int.Parse(token)));
            }
            else if (token[0] == 'd')
            {
                stack.Push(DiceProbability.Roll(int.Parse(token.Substring(1))));
            }
            else
            {
                var b = stack.Pop();
                var a = stack.Pop();
                DiceProbability result = token switch
                {
                    "+" => a + b,
                    "-" => a - b,
                    "*" => a * b,
                    ">" => a > b,
                    _ => throw new ArgumentException($"Unknown operator: {token}")
                };
                stack.Push(result);
            }
        }
        return stack.Pop();
    }
}

