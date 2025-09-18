using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var codeBuilder = new StringBuilder();
        for (var i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            codeBuilder.Append(line).Append(" ");
        }

        var interpreter = new Interpreter();
        interpreter.Run(codeBuilder.ToString());
    }
}

class CallFrame
{
    public List<string> Instructions { get; }
    public int InstructionPointer { get; set; }

    public CallFrame(List<string> instructions)
    {
        this.Instructions = instructions;
        this.InstructionPointer = 0;
    }
}

class Interpreter
{
    public void Run(string code)
    {
        var mainTokens = ParseDefinitions(code);
        _callStack.Push(new CallFrame(mainTokens));

        while (_callStack.Count > 0)
        {
            var currentFrame = _callStack.Peek();
            if (currentFrame.InstructionPointer >= currentFrame.Instructions.Count)
            {
                _callStack.Pop();
                continue;
            }

            var token = currentFrame.Instructions[currentFrame.InstructionPointer];
            currentFrame.InstructionPointer++;
            ExecuteToken(token);
        }
    }

    private readonly Stack<long> _dataStack = new Stack<long>();
    private readonly Stack<CallFrame> _callStack = new Stack<CallFrame>();
    private readonly Dictionary<string, List<string>> _functions = new Dictionary<string, List<string>>();

    private List<string> ParseDefinitions(string code)
    {
        var tokens = code.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var mainProgramTokens = new List<string>();
        var i = 0;
        while (i < tokens.Length)
        {
            if (tokens[i] == "DEF")
            {
                i++;
                var name = tokens[i++];
                var body = new List<string>();
                while (i < tokens.Length && tokens[i] != "END")
                {
                    body.Add(tokens[i++]);
                }
                _functions[name] = body;
                if (i < tokens.Length)
                {
                    i++;
                }
            }
            else
            {
                mainProgramTokens.Add(tokens[i++]);
            }
        }
        return mainProgramTokens;
    }

    private void ExecuteToken(string token)
    {
        if (long.TryParse(token, out var number))
        {
            _dataStack.Push(number);
            return;
        }

        switch (token)
        {
            case "ADD": { var b = _dataStack.Pop(); var a = _dataStack.Pop(); _dataStack.Push(a + b); break; }
            case "SUB": { var b = _dataStack.Pop(); var a = _dataStack.Pop(); _dataStack.Push(a - b); break; }
            case "MUL": { var b = _dataStack.Pop(); var a = _dataStack.Pop(); _dataStack.Push(a * b); break; }
            case "DIV": { var b = _dataStack.Pop(); var a = _dataStack.Pop(); _dataStack.Push(a / b); break; }
            case "MOD": { var b = _dataStack.Pop(); var a = _dataStack.Pop(); _dataStack.Push(a % b); break; }
            case "POP": { _dataStack.Pop(); break; }
            case "DUP": { _dataStack.Push(_dataStack.Peek()); break; }
            case "SWP": { var b = _dataStack.Pop(); var a = _dataStack.Pop(); _dataStack.Push(b); _dataStack.Push(a); break; }
            case "ROT": { var c = _dataStack.Pop(); var b = _dataStack.Pop(); var a = _dataStack.Pop(); _dataStack.Push(b); _dataStack.Push(c); _dataStack.Push(a); break; }
            case "OVR": { var b = _dataStack.Pop(); var a = _dataStack.Peek(); _dataStack.Push(b); _dataStack.Push(a); break; }
            case "POS": { var a = _dataStack.Pop(); _dataStack.Push(a >= 0 ? 1L : 0L); break; }
            case "NOT": { var a = _dataStack.Pop(); _dataStack.Push(a == 0 ? 1L : 0L); break; }
            case "OUT": { Console.WriteLine(_dataStack.Pop()); break; }
            case "IF": HandleIf(); break;
            case "ELS": HandleEls(); break;
            case "FI": break;
            default:
                if (_functions.TryGetValue(token, out var funcBody))
                {
                    _callStack.Push(new CallFrame(funcBody));
                }
                break;
        }
    }

    private void HandleIf()
    {
        var condition = _dataStack.Pop();
        if (condition != 0)
        {
            return;
        }

        var frame = _callStack.Peek();
        var ip = frame.InstructionPointer;
        var tokens = frame.Instructions;
        var nestLevel = 0;
        while (ip < tokens.Count)
        {
            var currentToken = tokens[ip];
            if (currentToken == "IF")
            {
                nestLevel++;
            }
            else if (currentToken == "FI")
            {
                if (nestLevel == 0)
                {
                    frame.InstructionPointer = ip + 1;
                    return;
                }
                nestLevel--;
            }
            else if (currentToken == "ELS" && nestLevel == 0)
            {
                frame.InstructionPointer = ip + 1;
                return;
            }
            ip++;
        }
    }

    private void HandleEls()
    {
        var frame = _callStack.Peek();
        var ip = frame.InstructionPointer;
        var tokens = frame.Instructions;
        var nestLevel = 0;
        while (ip < tokens.Count)
        {
            var currentToken = tokens[ip];
            if (currentToken == "IF")
            {
                nestLevel++;
            }
            else if (currentToken == "FI")
            {
                if (nestLevel == 0)
                {
                    frame.InstructionPointer = ip + 1;
                    return;
                }
                nestLevel--;
            }
            ip++;
        }
    }
}