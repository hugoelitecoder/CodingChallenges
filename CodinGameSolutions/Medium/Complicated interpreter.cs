using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        var interp = new Interpreter();
        interp.OnOutput += s => Console.WriteLine(s);
        interp.OnError  += s => Console.WriteLine(s);

        int n = int.Parse(Console.ReadLine());
        var program = new List<List<string>>();
        for (int i = 0; i < n; i++)
        {
            var tokens = Console.ReadLine()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            program.Add(tokens);
        }

        try
        {
            foreach (var line in program)
                interp.Execute(line);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public class Interpreter
    {
        private readonly Dictionary<string, int> _registers = new Dictionary<string, int>();
        private readonly List<string> _registerOrder = new List<string>();
        private readonly Dictionary<string, string> _functions = new Dictionary<string, string>();

        public event Action<string> OnOutput;
        public event Action<string> OnError;

        public void Execute(List<string> tokens)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                var t = tokens[i];
                if (t.StartsWith("//"))
                    break;

                if (t == "=")
                {
                    var name = tokens[i - 1];
                    var val = Parse(tokens[i + 1]);
                    if (!_registers.ContainsKey(name))
                        _registerOrder.Add(name);
                    _registers[name] = val;
                    continue;
                }

                if (t == "add" || t == "sub" || t == "mult")
                {
                    var op = t;
                    var name = tokens[i - 1];
                    var val = Parse(tokens[i + 1]);
                    if (!_registers.ContainsKey(name))
                        Error("ERROR");
                    switch (op)
                    {
                        case "add": _registers[name] += val; break;
                        case "sub": _registers[name] -= val; break;
                        default:     _registers[name] *= val; break;
                    }
                    continue;
                }

                if (t == "delete")
                {
                    var name = tokens[i + 1];
                    if (!_registers.ContainsKey(name))
                        Error("ERROR");
                    _registers.Remove(name);
                    _registerOrder.Remove(name);
                    continue;
                }

                if (t == "function")
                {
                    var fname = tokens[i - 1];
                    var body = string.Join(" ", tokens.Skip(i + 1));
                    _functions[fname] = body;
                    break;
                }

                if (t.EndsWith("()"))
                {
                    var fname = t.Substring(0, t.Length - 2);
                    if (!_functions.ContainsKey(fname))
                        Error("ERROR");
                    var bodyTokens = _functions[fname]
                        .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                    Execute(bodyTokens);
                    continue;
                }

                if (t == "loop")
                {
                    var times = Parse(tokens[i + 1]);
                    if (i + 2 >= tokens.Count || tokens[i + 2] != "do")
                        Error("ERROR");
                    var bodyTokens = tokens.Skip(i + 3).ToList();
                    for (int rep = 0; rep < times; rep++)
                        Execute(bodyTokens);
                    break;
                }

                if (t == "if")
                {
                    var line = string.Join(" ", tokens);
                    var parts = line.Split(new[] { "then" }, StringSplitOptions.None);
                    if (parts.Length != 2)
                        Error("ERROR");
                    var cond = parts[0].Replace("if", "").Trim();
                    var bodyTokens = parts[1]
                        .Trim()
                        .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                    if (Logic(cond))
                        Execute(bodyTokens);
                    break;
                }

                if (t == "print")
                {
                    var output = string.Join(" ",
                        _registerOrder.Select(k => _registers[k]));
                    OnOutput?.Invoke(output);
                    throw new OperationCanceledException();
                }
            }
        }

        private int Parse(string token)
        {
            if (token.StartsWith("$"))
            {
                var name = token.Substring(1);
                if (_registers.TryGetValue(name, out var value))
                    return value;
                Error("ERROR");
            }
            else if (int.TryParse(token, out var value))
            {
                return value;
            }
            Error("ERROR");
            return 0;
        }

        private bool Logic(string cond)
        {
            var parts = cond
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (parts.Contains("or"))
            {
                if (parts.Count != 7) Error("ERROR");
                return EvaluateComparison(parts[0], parts[1], parts[2]) ||
                       EvaluateComparison(parts[4], parts[5], parts[6]);
            }

            if (parts.Contains("and"))
            {
                if (parts.Count != 7) Error("ERROR");
                return EvaluateComparison(parts[0], parts[1], parts[2]) &&
                       EvaluateComparison(parts[4], parts[5], parts[6]);
            }

            if (parts.Contains("==") || parts.Contains("!="))
                return EvaluateComparison(parts[0], parts[1], parts[2]);

            Error("ERROR");
            return false;
        }

        private bool EvaluateComparison(string left, string op, string right)
        {
            var l = Parse(left);
            var r = Parse(right);
            switch (op)
            {
                case "==": return l == r;
                case "!=" : return l != r;
                default:    Error("ERROR"); return false;
            }
        }

        private void Error(string message)
        {
            OnError?.Invoke(message);
            throw new OperationCanceledException();
        }
    }
}