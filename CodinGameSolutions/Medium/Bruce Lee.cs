using System;
using System.Text;

class Solution {
    static void Main() {
        var cipher = new ChuckNorrisCipher();
        var input = Console.ReadLine();
        try {
            Console.WriteLine(cipher.Decode(input));
        } catch {
            Console.WriteLine("INVALID");
        }
    }
}

public class ChuckNorrisCipher
{
    private const string TokenSep    = " ";
    private const string HeaderOne   = "0";
    private const string HeaderZero  = "00";
    private const char   BodyChar    = '0';
    private const int    BitsPerChar = 7;

    public string Decode(string unary)
    {
        var tokens = unary.Split(TokenSep, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length % 2 != 0) 
            throw new FormatException();

        var bitBuf = new StringBuilder(tokens.Length * 3);
        for (int i = 0; i < tokens.Length; i += 2)
        {
            string hdr  = tokens[i];
            string body = tokens[i+1];
            if (hdr.Length > 2 || body.Length == 0) throw new FormatException();
            foreach (char c in body)
                if (c != BodyChar) throw new FormatException();
            
            char bit = (char)('0' + (2 - hdr.Length));
            bitBuf.Append(bit, body.Length);
        }

        string bits = bitBuf.ToString();
        if (bits.Length % BitsPerChar != 0)
            throw new FormatException();

        var sb = new StringBuilder(bits.Length / BitsPerChar);
        for (int i = 0; i < bits.Length; i += BitsPerChar)
        {
            int val = Convert.ToInt32(bits.Substring(i, BitsPerChar), 2);
            sb.Append((char)val);
        }
        return sb.ToString();
    }

    public string Encode(string message)
    {
        var bitBuilder = new StringBuilder(message.Length * BitsPerChar);
        foreach (char ch in message)
        {
            string bin = Convert.ToString(ch, 2).PadLeft(BitsPerChar, '0');
            bitBuilder.Append(bin);
        }

        string bstr = bitBuilder.ToString();
        var unary = new StringBuilder(bstr.Length);

        for (int i = 0; i < bstr.Length;)
        {
            char b = bstr[i];
            int j = i;
            while (j < bstr.Length && bstr[j] == b) j++;

            string hdr = b == '1' ? HeaderOne : HeaderZero;
            unary.Append(hdr)
                 .Append(TokenSep)
                 .Append(new string(BodyChar, j - i));

            i = j;
            if (i < bstr.Length)
                unary.Append(TokenSep);
        }

        return unary.ToString();
    }
}
