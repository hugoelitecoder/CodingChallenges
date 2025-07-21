using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var tokens = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var interpreter = new RpnInterpreter();
        var result = interpreter.Evaluate(tokens);
        Console.WriteLine(string.Join(' ', result));
    }
}

public class RpnInterpreter
{
    private readonly List<string> _stack = new List<string>();

    public IList<string> Evaluate(string[] tokens)
    {
        foreach (var token in tokens)
        {
            try
            {
                if (long.TryParse(token, out _))
                    _stack.Add(token);
                else
                    Execute(token);
            }
            catch
            {
                _stack.Add("ERROR");
                break;
            }
        }
        return _stack;
    }

    private void Execute(string inst)
    {
        switch (inst)
        {
            case "POP":
                {
                    var _ = _stack[_stack.Count - 1];
                    _stack.RemoveAt(_stack.Count - 1);
                    break;
                }
            case "DUP":
                {
                    var val = _stack[_stack.Count - 1];
                    _stack.Add(val);
                    break;
                }
            case "SWP":
                {
                    var a = _stack[_stack.Count - 1];
                    _stack.RemoveAt(_stack.Count - 1);
                    var b = _stack[_stack.Count - 1];
                    _stack.RemoveAt(_stack.Count - 1);
                    _stack.Add(a);
                    _stack.Add(b);
                    break;
                }
            case "ROL":
                {
                    var nStr = _stack[_stack.Count - 1];
                    _stack.RemoveAt(_stack.Count - 1);
                    int n = int.Parse(nStr);
                    int idx = _stack.Count - n;
                    var v = _stack[idx];
                    _stack.RemoveAt(idx);
                    _stack.Add(v);
                    break;
                }
            case "ADD":
                BinaryOp((x, y) => y + x);
                break;
            case "SUB":
                BinaryOp((x, y) => y - x);
                break;
            case "MUL":
                BinaryOp((x, y) => y * x);
                break;
            case "DIV":
                BinaryOp((x, y) => y / x);
                break;
            case "MOD":
                BinaryOp((x, y) => y % x);
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    private void BinaryOp(Func<long, long, long> op)
    {
        var xStr = _stack[_stack.Count - 1];
        _stack.RemoveAt(_stack.Count - 1);
        var yStr = _stack[_stack.Count - 1];
        _stack.RemoveAt(_stack.Count - 1);
        long x = long.Parse(xStr);
        long y = long.Parse(yStr);
        _stack.Add(op(x, y).ToString());
    }
}