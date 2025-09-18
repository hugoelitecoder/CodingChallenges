using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    public static void Main()
    {
        var sourceLines = new List<string>();
        string line;
        while ((line = Console.ReadLine()) != null)
        {
            sourceLines.Add(line);
        }

        if (sourceLines.Count > 1 && int.TryParse(sourceLines[0], out var lineCount) && lineCount == sourceLines.Count - 1)
        {
            sourceLines.RemoveAt(0);
        }

        var compiler = new PascalCompiler();
        var (output, error) = compiler.Run(sourceLines);
        if (error != null)
        {
            Console.WriteLine(error);
        }
        else
        {
            foreach (var instruction in output)
            {
                Console.WriteLine(instruction);
            }
        }
    }
}

public struct Token
{
    public string Text { get; }
    public int Line { get; }
    public Token(string text, int line) { Text = text; Line = line; }
}

public class Symbol
{
    public string Name { get; }
    public string Kind { get; }
    public int Level { get; }
    public int Value { get; }
    public int Address { get; }
    public Symbol(string name, string kind, int level, int value, int address)
    {
        Name = name;
        Kind = kind;
        Level = level;
        Value = value;
        Address = address;
    }
}

public class Instruction
{
    public string Opcode { get; }
    public int Level { get; set; }
    public int Argument { get; set; }
    public Instruction(string opcode, int level, int argument) { Opcode = opcode; Level = level; Argument = argument; }
}

public class PascalCompiler
{
    private List<Token> _tokens;
    private int _tokenPointer;
    private List<List<Symbol>> _symbolTable;
    private int _level;
    private Stack<int> _addressStack;
    private List<Instruction> _instructions;
    private string _errorMessage;
    private static readonly HashSet<string> _keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "const", "var", "procedure", "begin", "end", "if", "then", "while", "do", "call", "odd" };
    private class ParsingException : Exception { }

    public (List<string> output, string error) Run(List<string> sourceLines)
    {
        _tokens = new List<Token>();
        _tokenPointer = 0;
        _symbolTable = new List<List<Symbol>>();
        _level = 0;
        _addressStack = new Stack<int>();
        _instructions = new List<Instruction>();
        _errorMessage = null;

        Tokenize(sourceLines);
        if (_tokens.Count == 0) return (new List<string>(), null);

        _symbolTable.Add(new List<Symbol>());
        _addressStack.Push(3);

        try
        {
            ParseProgram();
        }
        catch (ParsingException)
        {
            return (null, _errorMessage);
        }

        if (_tokenPointer < _tokens.Count)
        {
            Error(Peek().Line, "Invalid statement");
            return (null, _errorMessage);
        }

        var result = _instructions.Select(i => $"{i.Opcode} {i.Level}, {i.Argument}").ToList();
        return (result, null);
    }

    private void Tokenize(List<string> sourceLines)
    {
        for (var i = 0; i < sourceLines.Count; i++)
        {
            var s = sourceLines[i];
            var l = i + 1;
            var j = 0;
            while (j < s.Length)
            {
                while (j < s.Length && char.IsWhiteSpace(s[j])) j++;
                if (j >= s.Length) break;

                if (char.IsLetter(s[j]))
                {
                    var start = j;
                    while (j < s.Length && char.IsLetterOrDigit(s[j])) j++;
                    _tokens.Add(new Token(s.Substring(start, j - start), l));
                }
                else if (char.IsDigit(s[j]))
                {
                    var start = j;
                    while (j < s.Length && char.IsDigit(s[j])) j++;
                    _tokens.Add(new Token(s.Substring(start, j - start), l));
                }
                else
                {
                    var t = "";
                    if (j + 1 < s.Length && (s[j] == '<' || s[j] == '>') && s[j + 1] == '=')
                    {
                        t = s.Substring(j, 2); j += 2;
                    }
                    else if (j + 1 < s.Length && s[j] == ':' && s[j + 1] == '=')
                    {
                        t = ":="; j += 2;
                    }
                    else
                    {
                        t = s[j].ToString(); j++;
                    }
                    _tokens.Add(new Token(t, l));
                }
            }
        }
    }

    private Token Peek() => _tokenPointer < _tokens.Count ? _tokens[_tokenPointer] : new Token("", _tokens.LastOrDefault().Line);
    private bool Accept(string text, bool caseSensitive = false)
    {
        if (_tokenPointer < _tokens.Count)
        {
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            if (string.Equals(_tokens[_tokenPointer].Text, text, comparison))
            {
                _tokenPointer++;
                return true;
            }
        }
        return false;
    }

    private void Expect(string text, string message, bool caseSensitive = false)
    {
        if (!Accept(text, caseSensitive))
        {
            var token = _tokenPointer > 0 ? _tokens[_tokenPointer - 1] : Peek();
            Error(token.Line, $"{message} missing");
        }
    }

    private void Error(int line, string message)
    {
        if (_errorMessage == null) _errorMessage = $"Line {line}: {message}";
        throw new ParsingException();
    }
    
    private void ParseProgram() { ParseBlock(); if (!Accept(".", true)) Error(Peek().Line, "Invalid program"); }
    
    private void ParseBlock()
    {
        var jumpIndex = _instructions.Count;
        EmitInstruction("jmp", 0, 0);

        _level++;
        _symbolTable.Add(new List<Symbol>());
        _addressStack.Push(3);

        if (Accept("const"))
        {
            do
            {
                var idToken = Peek();
                if (!char.IsLetter(idToken.Text.Length > 0 ? idToken.Text[0] : '\0')) Error(idToken.Line, "Invalid const");
                Accept(idToken.Text, true);
                if (FindSymbolInCurrentScope(idToken.Text) != null) Error(idToken.Line, "const already defined");
                Expect("=", ";", true);
                var numToken = Peek();
                if (!int.TryParse(numToken.Text, out var value)) Error(numToken.Line, "Invalid const value");
                Accept(numToken.Text, true);
                _symbolTable[^1].Add(new Symbol(idToken.Text, "const", _level - 1, value, 0));
            } while (Accept(",", true));
            Expect(";", ";", true);
        }

        if (Accept("var"))
        {
            do
            {
                var idToken = Peek();
                if (!char.IsLetter(idToken.Text.Length > 0 ? idToken.Text[0] : '\0')) Error(idToken.Line, "Invalid var");
                Accept(idToken.Text, true);
                if (FindSymbolInCurrentScope(idToken.Text) != null) Error(idToken.Line, "var already defined");
                _symbolTable[^1].Add(new Symbol(idToken.Text, "var", _level - 1, 0, _addressStack.Peek()));
                _addressStack.Pop();
                _addressStack.Push(_symbolTable[^1][^1].Address + 1);
            } while (Accept(",", true));
            Expect(";", ";", true);
        }

        while (Accept("procedure"))
        {
            var idToken = Peek();
            if (!char.IsLetter(idToken.Text.Length > 0 ? idToken.Text[0] : '\0')) Error(idToken.Line, "Invalid procedure");
            Accept(idToken.Text, true);
            if (FindSymbolInCurrentScope(idToken.Text) != null) Error(idToken.Line, "procedure already defined");
            var procAddress = _instructions.Count;
            _symbolTable[^1].Add(new Symbol(idToken.Text, "proc", _level - 1, 0, procAddress));
            Expect(";", ";", true);
            ParseBlock();
            Expect(";", ";", true);
        }

        _instructions[jumpIndex].Argument = _instructions.Count;
        EmitInstruction("int", 0, _addressStack.Peek());
        ParseStatement();
        EmitInstruction("opr", 0, 0);

        _level--;
        _symbolTable.RemoveAt(_symbolTable.Count - 1);
        _addressStack.Pop();
    }

    private Symbol FindSymbol(string name)
    {
        for (var i = _symbolTable.Count - 1; i >= 0; i--)
        {
            var symbol = _symbolTable[i].FirstOrDefault(s => s.Name == name);
            if (symbol != null) return symbol;
        }
        return null;
    }

    private Symbol FindSymbolInCurrentScope(string name) => _symbolTable[^1].FirstOrDefault(s => s.Name == name);

    private void ParseStatement()
    {
        var token = Peek();
        if (char.IsLetter(token.Text.Length > 0 ? token.Text[0] : '\0') && !_keywords.Contains(token.Text))
        {
            _tokenPointer++;
            var symbol = FindSymbol(token.Text);
            if (symbol == null) Error(token.Line, "Unknown var");
            if (symbol.Kind == "const") Error(token.Line, "Invalid statement");
            Expect(":=", ":=", true);
            ParseExpression();
            EmitInstruction("sto", _level - 1 - symbol.Level, symbol.Address);
        }
        else if (Accept("call"))
        {
            var idToken = Peek();
            _tokenPointer++;
            var symbol = FindSymbol(idToken.Text);
            if (symbol == null || symbol.Kind != "proc") Error(idToken.Line, "Unknown var");
            EmitInstruction("cal", _level - 1 - symbol.Level, symbol.Address);
        }
        else if (Accept("?", true))
        {
            var idToken = Peek();
            _tokenPointer++;
            var symbol = FindSymbol(idToken.Text);
            if (symbol == null || symbol.Kind != "var") Error(idToken.Line, "Unknown var");
            EmitInstruction("opr", 0, 14);
            EmitInstruction("sto", _level - 1 - symbol.Level, symbol.Address);
        }
        else if (Accept("!", true)) { ParseExpression(); EmitInstruction("opr", 0, 13); }
        else if (Accept("begin"))
        {
            ParseStatement();
            while (Accept(";", true))
            {
                if (Peek().Text.Equals("end", StringComparison.OrdinalIgnoreCase)) break;
                ParseStatement();
            }
            Expect("end", "end");
        }
        else if (Accept("if"))
        {
            ParseCondition();
            Expect("then", "then");
            var jumpIndex = _instructions.Count;
            EmitInstruction("jpc", 0, 0);
            ParseStatement();
            _instructions[jumpIndex].Argument = _instructions.Count;
        }
        else if (Accept("while"))
        {
            var loopStart = _instructions.Count;
            ParseCondition();
            Expect("do", "do");
            var jumpIndex = _instructions.Count;
            EmitInstruction("jpc", 0, 0);
            ParseStatement();
            EmitInstruction("jmp", 0, loopStart);
            _instructions[jumpIndex].Argument = _instructions.Count;
        }
    }

    private void ParseCondition()
    {
        if (Accept("odd"))
        {
            ParseExpression();
            EmitInstruction("opr", 0, 6);
        }
        else
        {
            ParseExpression();
            var opToken = Peek();
            if (Accept("=", true)) { ParseExpression(); EmitInstruction("opr", 0, 7); }
            else if (Accept("#", true)) { ParseExpression(); EmitInstruction("opr", 0, 8); }
            else if (Accept("<", true)) { ParseExpression(); EmitInstruction("opr", 0, 9); }
            else if (Accept(">=", true)) { ParseExpression(); EmitInstruction("opr", 0, 10); }
            else if (Accept(">", true)) { ParseExpression(); EmitInstruction("opr", 0, 11); }
            else if (Accept("<=", true)) { ParseExpression(); EmitInstruction("opr", 0, 12); }
            else Error(opToken.Line, "Invalid condition");
        }
    }

    private void ParseExpression()
    {
        var isNegative = Accept("-", true);
        if(!isNegative) Accept("+", true);

        ParseTerm();
        if (isNegative) EmitInstruction("opr", 0, 1);

        while (true)
        {
            if (Accept("+", true)) { ParseTerm(); EmitInstruction("opr", 0, 2); }
            else if (Accept("-", true)) { ParseTerm(); EmitInstruction("opr", 0, 3); }
            else break;
        }
    }

    private void ParseTerm()
    {
        ParseFactor();
        while (true)
        {
            if (Accept("*", true)) { ParseFactor(); EmitInstruction("opr", 0, 4); }
            else if (Accept("/", true)) { ParseFactor(); EmitInstruction("opr", 0, 5); }
            else break;
        }
    }

    private void ParseFactor()
    {
        var token = Peek();
        if (char.IsLetter(token.Text.Length > 0 ? token.Text[0] : '\0') && !_keywords.Contains(token.Text))
        {
            _tokenPointer++;
            var symbol = FindSymbol(token.Text);
            if (symbol == null) Error(token.Line, "Unknown var");
            if (symbol.Kind == "const") EmitInstruction("lit", 0, symbol.Value);
            else if (symbol.Kind == "var") EmitInstruction("lod", _level - 1 - symbol.Level, symbol.Address);
            else Error(token.Line, "Unknown var");
        }
        else if (int.TryParse(token.Text, out var value)) { _tokenPointer++; EmitInstruction("lit", 0, value); }
        else if (Accept("(", true)) { ParseExpression(); Expect(")", ")", true); }
        else Error(token.Line, "Invalid expr");
    }

    private void EmitInstruction(string opcode, int level, int argument) => _instructions.Add(new Instruction(opcode, level, argument));
}