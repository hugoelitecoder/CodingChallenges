using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

class Solution
{
    public static void Main(string[] args)
    {
        var lines = ReadInputLines();

        LogInput(lines);

        var stopwatch = Stopwatch.StartNew();
        var interpreter = new CGFungeInterpreter();
        var success = interpreter.Run(lines);
        stopwatch.Stop();

        if (success)
        {
            Console.WriteLine(interpreter.Output);
        }
        else
        {
            Console.Error.WriteLine($"[DEBUG] Execution failed: {interpreter.DebugMessage}");
        }

        Console.Error.WriteLine($"[DEBUG] Execution time: {stopwatch.ElapsedMilliseconds}ms");
    }

    private static List<string> ReadInputLines()
    {
        var lines = new List<string>();
        var lineCountStr = Console.ReadLine();
        if (!int.TryParse(lineCountStr, out var lineCount))
        {
            return lines;
        }

        for (var i = 0; i < lineCount; i++)
        {
            lines.Add(Console.ReadLine() ?? "");
        }
        return lines;
    }

    private static void LogInput(List<string> lines)
    {
        Console.Error.WriteLine("[DEBUG] Program Grid:");
        foreach (var line in lines)
        {
            Console.Error.WriteLine($"[DEBUG] {line}");
        }
    }
}

public class CGFungeInterpreter
{
    private const int MAX_TURNS = 3000;
    private char[][] _grid;
    private int _rows;
    private int _cols;
    private readonly Stack<int> _stack;
    private int _r;
    private int _c;
    private int _dr;
    private int _dc;
    private bool _stringMode;
    private readonly StringBuilder _outputBuilder;
    public string Output => _outputBuilder.ToString();
    public string DebugMessage { get; private set; }

    public CGFungeInterpreter()
    {
        _stack = new Stack<int>();
        _outputBuilder = new StringBuilder();
        DebugMessage = "";
    }

    public bool Run(List<string> lines)
    {
        InitializeGrid(lines);
        InitializeState();

        var turns = 0;
        while (turns++ < MAX_TURNS)
        {
            if (_r < 0 || _r >= _rows || _c < 0 || _c >= _cols)
            {
                DebugMessage = $"Pointer out of bounds at ({_r},{_c}) after {turns} turns";
                return false;
            }

            var instruction = _grid[_r][_c];
            if (!_stringMode && instruction == 'E')
            {
                return true;
            }

            if (!ProcessInstruction(instruction))
            {
                DebugMessage += $" (turn {turns})";
                return false;
            }

            MovePointer();
        }

        DebugMessage = "Max turns exceeded.";
        return false;
    }

    private void InitializeGrid(List<string> lines)
    {
        _rows = lines.Count;
        _cols = _rows > 0 ? lines.Max(l => l?.Length ?? 0) : 0;
        _grid = new char[_rows][];
        for (var i = 0; i < _rows; i++)
        {
            _grid[i] = new char[_cols];
            var line = lines[i] ?? "";
            for (var j = 0; j < _cols; j++)
            {
                _grid[i][j] = j < line.Length ? line[j] : ' ';
            }
        }
    }

    private void InitializeState()
    {
        _stack.Clear();
        _r = 0;
        _c = 0;
        _dr = 0;
        _dc = 1;
        _stringMode = false;
        _outputBuilder.Clear();
    }

    private void MovePointer()
    {
        _r += _dr;
        _c += _dc;
    }

    private bool ProcessInstruction(char instruction)
    {
        if (_stringMode)
        {
            if (instruction == '"')
            {
                _stringMode = false;
            }
            else
            {
                _stack.Push(instruction);
            }
            return true;
        }

        if (instruction >= '0' && instruction <= '9')
        {
            _stack.Push(instruction - '0');
            return true;
        }

        switch (instruction)
        {
            case ' ': return true;
            case '>': _dr = 0; _dc = 1; return true;
            case '<': _dr = 0; _dc = -1; return true;
            case '^': _dr = -1; _dc = 0; return true;
            case 'v': _dr = 1; _dc = 0; return true;
            case 'S': MovePointer(); return true;
            case '"': _stringMode = true; return true;
            case '+': return HandleArithmetic('+');
            case '-': return HandleArithmetic('-');
            case '*': return HandleArithmetic('*');
            case '/': return HandleArithmetic('/');
            case 'P': return HandlePop();
            case 'X': return HandleSwap();
            case 'D': return HandleDuplicate();
            case '_': return HandleHorizontalIf();
            case '|': return HandleVerticalIf();
            case 'I': return HandleOutputInteger();
            case 'C': return HandleOutputChar();
            default: return true;
        }
    }

    private bool Pop(out int value)
    {
        if (_stack.Count < 1)
        {
            value = 0;
            return false;
        }
        value = _stack.Pop();
        return true;
    }

    private bool Pop2(out int val1, out int val2)
    {
        val1 = 0;
        val2 = 0;
        if (_stack.Count < 2) return false;
        val1 = _stack.Pop();
        val2 = _stack.Pop();
        return true;
    }

    private bool HandleArithmetic(char op)
    {
        if (!Pop2(out var a, out var b))
        {
            DebugMessage = $"Stack underflow for {op}";
            return false;
        }
        switch (op)
        {
            case '+': _stack.Push((b + a) & 0xFF); break;
            case '-': _stack.Push((b - a) & 0xFF); break;
            case '*': _stack.Push((b * a) & 0xFF); break;
            case '/':
                if (a == 0)
                {
                    DebugMessage = "Division by zero";
                    return false;
                }
                _stack.Push((b / a) & 0xFF);
                break;
        }
        return true;
    }

    private bool HandlePop()
    {
        if (!Pop(out _))
        {
            DebugMessage = "Stack underflow for P";
            return false;
        }
        return true;
    }

    private bool HandleSwap()
    {
        if (!Pop2(out var a, out var b))
        {
            DebugMessage = "Stack underflow for X";
            return false;
        }
        _stack.Push(a);
        _stack.Push(b);
        return true;
    }

    private bool HandleDuplicate()
    {
        if (_stack.Count == 0)
        {
            DebugMessage = "Stack underflow for D";
            return false;
        }
        _stack.Push(_stack.Peek());
        return true;
    }

    private bool HandleHorizontalIf()
    {
        if (!Pop(out var val))
        {
            DebugMessage = "Stack underflow for _";
            return false;
        }
        _dr = 0;
        _dc = (val == 0) ? 1 : -1;
        return true;
    }

    private bool HandleVerticalIf()
    {
        if (!Pop(out var val))
        {
            DebugMessage = "Stack underflow for |";
            return false;
        }
        _dr = (val == 0) ? 1 : -1;
        _dc = 0;
        return true;
    }

    private bool HandleOutputInteger()
    {
        if (!Pop(out var val))
        {
            DebugMessage = "Stack underflow for I";
            return false;
        }
        _outputBuilder.Append(val);
        return true;
    }

    private bool HandleOutputChar()
    {
        if (!Pop(out var val))
        {
            DebugMessage = "Stack underflow for C";
            return false;
        }
        _outputBuilder.Append((char)val);
        return true;
    }
}
