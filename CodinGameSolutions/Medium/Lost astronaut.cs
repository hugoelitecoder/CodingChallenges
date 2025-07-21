using System;
using System.Text;

class Program
{
    static void Main()
    {
        if (!int.TryParse(Console.ReadLine(), out _)) return;
        var tokens = Console.ReadLine().Split(' ');
        var decoder = new MixedBaseAsciiDecoder();
        Console.WriteLine(decoder.DecodeMessage(tokens));
    }
}

public class MixedBaseAsciiDecoder
{
    public string DecodeMessage(string[] tokens)
    {
        var sb = new StringBuilder();
        bool sawEmpty = false;
        foreach (var tok in tokens)
        {
            if (string.IsNullOrEmpty(tok))
            {
                if (!sawEmpty)
                {
                    sb.Append(' ');
                    sawEmpty = true;
                }
                continue;
            }
            sawEmpty = false;
            sb.Append(DecodeToken(tok));
        }
        return sb.ToString();
    }

    private char DecodeToken(string tok)
    {
        if (tok.Length == 1 && char.IsLetter(tok[0]))
            return tok[0];
        foreach (var fromBase in new[] { 2, 8, 16 })
            if (TryParse(tok, fromBase, out var ch))
                return ch;
        throw new InvalidOperationException($"Cannot decode token '{tok}'");
    }

    private bool TryParse(string tok, int fromBase, out char ch)
    {
        ch = '\0';
        try
        {
            int code = Convert.ToInt32(tok, fromBase);
            if (code == 32 || (code >= 'A' && code <= 'Z') || (code >= 'a' && code <= 'z'))
            {
                ch = (char)code;
                return true;
            }
        }
        catch { }
        return false;
    }
}

