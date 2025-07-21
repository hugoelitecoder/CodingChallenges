using System;
using System.Text;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        string operation = Console.ReadLine();
        if (operation != TwinPrimeCipher.ENCODE && operation != TwinPrimeCipher.DECODE)
        {
            Console.WriteLine(TwinPrimeCipher.ErrorMessage);
            return;
        }

        if (!int.TryParse(Console.ReadLine(), out int key))
        {
            Console.WriteLine(TwinPrimeCipher.ErrorMessage);
            return;
        }

        string message = Console.ReadLine() ?? "";

        var cipher = new TwinPrimeCipher(key);
        string output = operation == TwinPrimeCipher.ENCODE
            ? cipher.Encode(message)
            : cipher.Decode(message);

        Console.WriteLine(output);
    }
}

class TwinPrimeCipher
{
    public const char Separator = 'G';
    public const string SpaceSeparator = "GG";
    public const char MinLetter = 'A';
    public const char MaxLetter = 'Z';
    public const string ENCODE = "ENCODE";
    public const string DECODE = "DECODE";
    public const string ErrorMessage = "ERROR !!";

    private readonly Dictionary<char,string> map;
    private readonly Dictionary<string,char> reverseMap;

    public TwinPrimeCipher(int key)
    {
        map = new Dictionary<char,string>();
        long last = key + key;
        map[MinLetter] = last.ToString("X");
        for (char c = (char)(MinLetter + 1); c <= MaxLetter; c++)
        {
            long centre = GetNextCentre(last);
            last = centre + key;
            map[c] = last.ToString("X");
        }

        reverseMap = new Dictionary<string,char>();
        foreach (var kv in map)
            reverseMap[kv.Value] = kv.Key;
    }

    public string Encode(string message)
    {
        var sb = new StringBuilder();
        int n = message.Length;
        for (int i = 0; i < n; i++)
        {
            char ch = message[i];
            if (ch == ' ')
            {
                sb.Append(SpaceSeparator);
                continue;
            }
            if (!map.TryGetValue(ch, out string hex))
                return ErrorMessage;
            sb.Append(hex);
            if (i + 1 < n && message[i + 1] != ' ')
                sb.Append(Separator);
        }
        return sb.ToString();
    }

    public string Decode(string message)
    {
        var sb = new StringBuilder();
        int i = 0, n = message.Length;
        while (i < n)
        {
            if (i + 1 < n && message[i] == Separator && message[i + 1] == Separator)
            {
                sb.Append(' ');
                i += 2;
                continue;
            }
            if (!IsHexDigit(message[i]))
                return ErrorMessage;
            int start = i;
            while (i < n && IsHexDigit(message[i]))
                i++;
            string token = message.Substring(start, i - start);
            if (!reverseMap.TryGetValue(token, out char decoded))
                return ErrorMessage;
            sb.Append(decoded);
            if (i < n && message[i] == Separator && (i + 1 == n || message[i + 1] != Separator))
                i++;
        }
        return sb.ToString();
    }

    private static bool IsHexDigit(char c)
        => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F');

    private static long GetNextCentre(long after)
    {
        long n = after + 1;
        while (!(IsPrime(n - 1) && IsPrime(n + 1)))
            n++;
        return n;
    }

    private static bool IsPrime(long x)
    {
        if (x < 2) return false;
        if (x == 2) return true;
        if (x % 2 == 0) return false;
        for (long i = 3; i * i <= x; i += 2)
            if (x % i == 0)
                return false;
        return true;
    }
}
