using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Solution
{
    static void Main(string[] args)
    {
        var inputLines = new List<string>();
        var n = int.Parse(Console.ReadLine());
        for (var i = 0; i < n; i++)
        {
            inputLines.Add(Console.ReadLine());
        }

        var assembler = new Assembler();
        (var output, var error) = assembler.Assemble(inputLines);

        if (error != null)
        {
            Console.WriteLine(error);
        }
        else
        {
            foreach (var line in output)
            {
                Console.WriteLine(line);
            }
        }
    }
}

class Assembler
{
    private class AssemblyException : Exception
    {
        public int LineNumber { get; }
        public AssemblyException(string message, int lineNumber) : base(message)
        {
            LineNumber = lineNumber;
        }
    }

    private static readonly Dictionary<string, RegisterInfo> Registers = new(StringComparer.OrdinalIgnoreCase)
    {
        { "AX", new RegisterInfo("AX", 0, 16) }, { "CX", new RegisterInfo("CX", 1, 16) },
        { "DX", new RegisterInfo("DX", 2, 16) }, { "BX", new RegisterInfo("BX", 3, 16) },
        { "AL", new RegisterInfo("AL", 0, 8) }, { "CL", new RegisterInfo("CL", 1, 8) },
        { "DL", new RegisterInfo("DL", 2, 8) }, { "BL", new RegisterInfo("BL", 3, 8) },
        { "AH", new RegisterInfo("AH", 4, 8) }, { "CH", new RegisterInfo("CH", 5, 8) },
        { "DH", new RegisterInfo("DH", 6, 8) }, { "BH", new RegisterInfo("BH", 7, 8) }
    };

    public (List<string> output, string error) Assemble(List<string> sourceLines)
    {
        var outputLines = new List<string>();
        var currentAddress = 0;
        try
        {
            for (var i = 0; i < sourceLines.Count; i++)
            {
                var line = sourceLines[i].TrimEnd();
                var originalLine = line;
                
                var commentIndex = FindCommentStartIndex(line);
                var instructionPart = commentIndex >= 0 ? line.Substring(0, commentIndex) : line;
                instructionPart = instructionPart.Trim();
                
                string formattedLine;
                if (string.IsNullOrWhiteSpace(instructionPart))
                {
                    formattedLine = $"     |                   | {originalLine}";
                }
                else
                {
                    var parts = instructionPart.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    var command = parts[0];
                    var argsStr = parts.Length > 1 ? parts[1] : "";
                    var args = SplitArguments(argsStr);
                    
                    var addressBefore = currentAddress;
                    var byteCode = AssembleCommand(command, args, addressBefore, i + 1);
                    
                    var hexCode = string.Join(" ", byteCode.Select(b => b.ToString("X2")));
                    formattedLine = $"{addressBefore:X4} | {hexCode.ToUpper().PadRight(17)} | {originalLine}";
                    
                    currentAddress += byteCode.Length;
                }
                outputLines.Add(formattedLine);
            }
            return (outputLines, null);
        }
        catch (AssemblyException ex)
        {
            return (null, $"Line {ex.LineNumber}: {ex.Message}");
        }
    }
    
    private int FindCommentStartIndex(string line)
    {
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\'')
            {
                i++;
                if (i < line.Length && line[i] == '\\')
                {
                    i++; // Move past escape character '\'
                }
                if (i < line.Length)
                {
                    i++; // Move past the character itself to (what should be) the closing quote
                }
            }
            else if (c == ';')
            {
                return i;
            }
        }
        return -1; // No comment found
    }

    private static string[] SplitArguments(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return new string[0];
    
        var args = new List<string>();
        int parenLevel = 0;
        int start = 0;

        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '\'')
            {
                i++; // move past opening quote
                if (i < s.Length && s[i] == '\\') i++; // move past escape char
                if (i < s.Length) i++; // move past actual character to the closing quote
            }
            else if (c == '(') parenLevel++;
            else if (c == ')') parenLevel--;
            else if (c == ',' && parenLevel == 0)
            {
                args.Add(s.Substring(start, i - start).Trim());
                start = i + 1;
            }
        }
        args.Add(s.Substring(start).Trim());
        return args.Where(a => !string.IsNullOrEmpty(a)).ToArray();
    }


    private byte[] AssembleCommand(string command, string[] args, int address, int lineNum)
    {
        return command.ToUpper() switch
        {
            "MOV" => AssembleMov(args, address, lineNum),
            "ADD" => AssembleAdd(args, address, lineNum),
            "INT" => AssembleInt(args, address, lineNum),
            _ => throw new AssemblyException("Unknown command", lineNum)
        };
    }

    private Operand ParseOperand(string s, int address, int lineNum)
    {
        s = s.Trim();
        if (Registers.TryGetValue(s, out var regInfo))
        {
            return new RegisterOperand(regInfo);
        }
        if (string.IsNullOrEmpty(s))
        {
            throw new AssemblyException("Invalid expression or argument", lineNum);
        }
        try
        {
            (long val, bool hasDollar) = ExpressionParser.Parse(s, address);
            return new ImmediateOperand(val, hasDollar);
        }
        catch
        {
            throw new AssemblyException("Invalid expression or argument", lineNum);
        }
    }

    private byte[] AssembleMov(string[] args, int address, int lineNum)
    {
        if (args.Length != 2) throw new AssemblyException("Invalid number of arguments", lineNum);
        var op1 = ParseOperand(args[0], address, lineNum);
        var op2 = ParseOperand(args[1], address, lineNum);
        
        if (op1 is RegisterOperand tRegOp)
        {
            var t = tRegOp.Info;
            if (op2 is RegisterOperand sRegOp)
            {
                var s = sRegOp.Info;
                if (t.Size != s.Size) throw new AssemblyException("Invalid operand", lineNum);
                var w = t.Is16Bit ? 1 : 0;
                return new[] { (byte)(0x88 + w), (byte)(0xC0 + 8 * s.Code + t.Code) };
            }
            if (op2 is ImmediateOperand sImmOp)
            {
                var val = sImmOp.Value;
                if (t.Is16Bit)
                {
                    if (val < -32768 || val > 65535) throw new AssemblyException("Invalid operand", lineNum);
                    return new[] { (byte)(0xB8 + t.Code), (byte)val, (byte)(val >> 8) };
                }
                if (t.Is8Bit)
                {
                    if (val < -128 || val > 255) throw new AssemblyException("Invalid operand", lineNum);
                    return new[] { (byte)(0xB0 + t.Code), (byte)val };
                }
            }
        }
        throw new AssemblyException("Invalid operand", lineNum);
    }
    
    private byte[] AssembleAdd(string[] args, int address, int lineNum)
    {
        if (args.Length != 2) throw new AssemblyException("Invalid number of arguments", lineNum);
        var op1 = ParseOperand(args[0], address, lineNum);
        var op2 = ParseOperand(args[1], address, lineNum);

        if (op1 is RegisterOperand tRegOp)
        {
            var t = tRegOp.Info;
            if (op2 is RegisterOperand sRegOp)
            {
                var s = sRegOp.Info;
                if (t.Size != s.Size) throw new AssemblyException("Invalid operand", lineNum);
                var w = t.Is16Bit ? 1 : 0;
                return new[] { (byte)(0x00 + w), (byte)(0xC0 + 8 * s.Code + t.Code) };
            }
            if (op2 is ImmediateOperand sImmOp)
            {
                var val = sImmOp.Value;
                if (t.Is8Bit)
                {
                    if (val < -128 || val > 255) throw new AssemblyException("Invalid operand", lineNum);
                    return t.Name == "AL"
                        ? new[] { (byte)0x04, (byte)val }
                        : new[] { (byte)0x80, (byte)(0xC0 + t.Code), (byte)val };
                }
                if (t.Is16Bit)
                {
                    var canBeSignExtendedI8 = (short)val == (sbyte)((short)val);

                    if (canBeSignExtendedI8 && !sImmOp.HasDollar)
                    {
                        return new[] { (byte)0x83, (byte)(0xC0 + t.Code), (byte)val };
                    }
                    
                    if (val < -32768 || val > 65535) throw new AssemblyException("Invalid operand", lineNum);

                    if (t.Name == "AX")
                    {
                        return new[] { (byte)0x05, (byte)val, (byte)(val >> 8) };
                    }
                    else
                    {
                        return new[] { (byte)0x81, (byte)(0xC0 + t.Code), (byte)val, (byte)(val >> 8) };
                    }
                }
            }
        }
        throw new AssemblyException("Invalid operand", lineNum);
    }
    
    private byte[] AssembleInt(string[] args, int address, int lineNum)
    {
        if (args.Length != 1) throw new AssemblyException("Invalid number of arguments", lineNum);
        var op = ParseOperand(args[0], address, lineNum);
        if (op is ImmediateOperand immOp)
        {
            var val = immOp.Value;
            if (val < 0 || val > 255) throw new AssemblyException("Invalid operand", lineNum);
            return new[] { (byte)0xCD, (byte)val };
        }
        throw new AssemblyException("Invalid operand", lineNum);
    }
}

record RegisterInfo(string Name, int Code, int Size)
{
    public bool Is16Bit => Size == 16;
    public bool Is8Bit => Size == 8;
}
abstract record Operand;
record RegisterOperand(RegisterInfo Info) : Operand;
record ImmediateOperand(long Value, bool HasDollar) : Operand;

static class ExpressionParser
{
    private static string _expr;
    private static int _pos;
    private static int _address;
    private static bool _hasDollar;

    public static (long, bool) Parse(string expression, int currentAddress)
    {
        _expr = expression;
        _pos = 0;
        _address = currentAddress;
        _hasDollar = false;
        var result = ParseSum();
        SkipWhitespace();
        if (_pos < _expr.Length) throw new FormatException("Unexpected characters at end of expression.");
        return (result, _hasDollar);
    }
    
    private static long ParseSum()
    {
        var val = ParseTerm();
        while (true)
        {
            SkipWhitespace();
            if (_pos < _expr.Length && _expr[_pos] == '+') { _pos++; val += ParseTerm(); }
            else if (_pos < _expr.Length && _expr[_pos] == '-') { _pos++; val -= ParseTerm(); }
            else return val;
        }
    }

    private static long ParseTerm()
    {
        var val = ParseFactor();
        while (true)
        {
            SkipWhitespace();
            if (_pos < _expr.Length && _expr[_pos] == '*') { _pos++; val *= ParseFactor(); }
            else if (_pos < _expr.Length && _expr[_pos] == '/')
            {
                _pos++;
                var divisor = ParseFactor();
                if (divisor == 0) throw new DivideByZeroException();
                val = (long)Math.Floor((double)val / divisor);
            }
            else return val;
        }
    }

    private static long ParseFactor()
    {
        SkipWhitespace();
        if (_pos >= _expr.Length) throw new FormatException("Unexpected end of expression.");
        var c = _expr[_pos];
        if (c == '+') { _pos++; return ParseFactor(); }
        if (c == '-') { _pos++; return -ParseFactor(); }
        if (c == '(')
        {
            _pos++;
            var val = ParseSum();
            SkipWhitespace();
            if (_pos >= _expr.Length || _expr[_pos] != ')') throw new FormatException("Mismatched parentheses.");
            _pos++;
            return val;
        }
        return ParseAtom();
    }
    
    private static long ParseAtom()
    {
        SkipWhitespace();
        if (_pos >= _expr.Length) throw new FormatException("Unexpected end of expression.");
        var c = _expr[_pos];
        if (c == '$') { _pos++; _hasDollar = true; return _address; }
        if (c == '\'') return ParseChar();
        
        var start = _pos;
        if (c == '0' && _pos + 1 < _expr.Length && char.ToLower(_expr[_pos + 1]) == 'x')
        {
            _pos += 2;
            start = _pos;
            while (_pos < _expr.Length && "0123456789abcdefABCDEF".Contains(_expr[_pos])) _pos++;
            var hex = _expr.Substring(start, _pos - start);
            if (string.IsNullOrEmpty(hex)) throw new FormatException("Invalid hex number.");
            return Convert.ToInt64(hex, 16);
        }

        while (_pos < _expr.Length && (char.IsLetterOrDigit(_expr[_pos]) || _expr[_pos] == '_')) _pos++;
        var numStr = _expr.Substring(start, _pos - start);
        if (string.IsNullOrEmpty(numStr)) throw new FormatException("Invalid number format.");

        var lastChar = char.ToLower(numStr.Last());
        if (lastChar == 'h')
        {
            var hexPart = numStr.Substring(0, numStr.Length - 1);
            if (!char.IsDigit(hexPart[0])) throw new FormatException("Invalid hex number format.");
            return Convert.ToInt64(hexPart, 16);
        }
        if (lastChar == 'b')
        {
            return Convert.ToInt64(numStr.Substring(0, numStr.Length - 1), 2);
        }
        if (!long.TryParse(numStr, out var result))
        {
             if(char.IsLetter(numStr[0])) throw new FormatException("Invalid identifier in expression.");
             throw new FormatException("Invalid number format.");
        }
        return result;
    }
    
    private static long ParseChar()
    {
        _pos++;
        if (_pos >= _expr.Length) throw new FormatException("Unterminated char literal.");
        long val;
        if (_expr[_pos] == '\\')
        {
            _pos++;
            if (_pos >= _expr.Length) throw new FormatException("Unterminated escape sequence.");
            var escapedChar = _expr[_pos];
            if (escapedChar != '\'' && escapedChar != '\\') throw new FormatException("Invalid escape sequence.");
            val = escapedChar;
        }
        else
        {
            val = _expr[_pos];
        }
        _pos++;
        if (_pos >= _expr.Length || _expr[_pos] != '\'') throw new FormatException("Char literal too long or unterminated.");
        _pos++;
        return val;
    }

    private static void SkipWhitespace()
    {
        while (_pos < _expr.Length && char.IsWhiteSpace(_expr[_pos])) _pos++;
    }
}