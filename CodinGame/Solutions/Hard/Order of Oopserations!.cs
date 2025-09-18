using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var expr = Console.ReadLine();
        var parser = new LowIQParser(expr);
        var result = parser.Eval();
        Console.WriteLine(result);
    }
}

class LowIQParser
{
    private readonly string _expr;
    private int _pos;
    public LowIQParser(string expr)
    {
        _expr = expr;
        _pos = 0;
    }
    public int Eval()
    {
        _pos = 0;
        return ParseMult();
    }
    private int ParseMult()
    {
        var val = ParseSub();
        while (Match('*'))
            val *= ParseSub();
        return val;
    }
    private int ParseSub()
    {
        var val = ParseDiv();
        while (Match('-'))
            val -= ParseDiv();
        return val;
    }
    private int ParseDiv()
    {
        var val = ParseAdd();
        while (Match('/'))
            val /= ParseAdd();
        return val;
    }
    private int ParseAdd()
    {
        var val = ParseUnary();
        while (Match('+'))
            val += ParseUnary();
        return val;
    }
    private int ParseUnary()
    {
        if (Match('-'))
            return -ParseUnary();
        return ParseNumber();
    }
    private int ParseNumber()
    {
        int start = _pos;
        while (_pos < _expr.Length && char.IsDigit(_expr[_pos])) _pos++;
        return int.Parse(_expr.Substring(start, _pos - start));
    }
    private bool Match(char c)
    {
        if (_pos < _expr.Length && _expr[_pos] == c)
        {
            _pos++;
            return true;
        }
        return false;
    }
}
