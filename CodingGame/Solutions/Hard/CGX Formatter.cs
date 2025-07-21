using System;
using System.Text;
using System.Collections.Generic;

public class Solution
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        var cgxLines = new List<string>();
        for (int i = 0; i < n; i++)
            cgxLines.Add(Console.ReadLine());

        var formatter = new CgxFormatter();
        Console.Write(formatter.Format(cgxLines));
    }
}

public class CgxFormatter
{
    private const int IndentSize = 4;
    private const char Quote = '\'';
    private const char OpenParen = '(';
    private const char CloseParen = ')';
    private const char Semicolon = ';';
    private const string NewLine = "\n";

    private int _tabs;
    private readonly StringBuilder _builder;
    private bool _inString;
    private char _lastChar;

    public CgxFormatter()
    {
        _tabs = 0;
        _builder = new StringBuilder();
        _inString = false;
        _lastChar = '\0';
    }

    public string Format(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            foreach (var c in line)
            {
                if (_inString)
                {
                    _builder.Append(c);
                    if (c == Quote)
                        _inString = false;
                    _lastChar = c;
                    continue;
                }

                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                HandleBreak(c);

                switch (c)
                {
                    case Quote:
                        _inString = true;
                        _builder.Append(c);
                        break;
                    case OpenParen:
                        _builder.Append(c);
                        _tabs++;
                        break;
                    case CloseParen:
                        _tabs--;
                        AppendIndent();
                        _builder.Append(c);
                        break;
                    case Semicolon:
                        _builder.Append(c);
                        AppendIndent();
                        break;
                    default:
                        _builder.Append(c);
                        break;
                }

                _lastChar = c;
            }
        }
        return _builder.ToString();
    }

    private void HandleBreak(char current)
    {
        if (_lastChar == CloseParen)
        {
            if (current != Semicolon && current != CloseParen && current != OpenParen)
                AppendIndent();
        }
        else if (_lastChar == OpenParen)
        {
            if (current != CloseParen)
                AppendIndent();
        }
        else if (_lastChar == '=' && current == OpenParen)
        {
            AppendIndent();
        }
    }

    private void AppendIndent()
    {
        _builder.Append(NewLine)
                .Append(new string(' ', _tabs * IndentSize));
    }
}