using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

class Solution
{
    static void Main()
    {
        bool encode = int.Parse(Console.ReadLine()) == 1;
        var hdr   = Console.ReadLine().Trim();
        var pass  = Console.ReadLine();
        var pos   = Console.ReadLine().Split().Select(int.Parse).ToArray();
        var key   = Console.ReadLine().Trim().Select(c => c - '0').ToArray();
        var msg   = Console.ReadLine();

        var cipher = new StraddlingCipher(hdr, pass, pos[0], pos[1], key);
        Console.WriteLine(cipher.Process(msg, encode));
    }
}

class StraddlingCipher
{
    readonly Dictionary<string,char> board;
    readonly int[] key;
    public const int HeaderSize = 10;
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const char Slash = '/';
    public const char Dot = '.';
    public const char Space = ' ';
    public const char CharZero = '0';

    public StraddlingCipher(string header, string pass, int posSlash, int posDot, int[] keyDigits)
    {
        key = keyDigits;
        board = BuildBoard(header, pass, posSlash, posDot);
    }

    public string Process(string message, bool encode)
    {
        var clean  = Clean(message, encode);
        var codes  = ToCodes(clean);
        var mixed  = ApplyKey(codes, encode);
        return Decode(mixed, encode);
    }

    Dictionary<string,char> BuildBoard(string header, string pass, int ps, int pd)
    {
        var map = new Dictionary<string,char>(HeaderSize);
        for (int i = 0; i < HeaderSize; i++)
            map[header[i].ToString()] = pass[i];

        var rem    = Alphabet.Except(pass.Replace(Space.ToString(), "")).ToList();
        var blanks = map.Where(kv => kv.Value == Space)
                        .Select(kv => kv.Key)
                        .OrderBy(k => k)
                        .ToArray();

        for (int row = 0; row < 2; row++)
            for (int i = 0; i < HeaderSize; i++)
            {
                int idx = row * HeaderSize + i;
                string key = blanks[row] + header[i];
                char ch   = idx == ps ? Slash
                            : idx == pd ? Dot
                            : rem[0];
                if (ch != Slash && ch != Dot)
                    rem.RemoveAt(0);
                map[key] = ch;
            }
        return map;
    }

    string Clean(string message, bool encode)
    {
        var sb = new StringBuilder();
        foreach (var c in message.ToUpper())
        {
            if (char.IsDigit(c))
            {
                if (encode) sb.Append(Slash);
                sb.Append(c);
            }
            else if (c == Slash && !encode)
                sb.Append(c);
            else if (char.IsLetter(c) || c == Dot)
                sb.Append(c);
        }
        return sb.ToString();
    }

    string ToCodes(string clean)
    {
        var sb = new StringBuilder();
        foreach (var c in clean)
        {
            if (char.IsDigit(c)) sb.Append(c);
            else sb.Append(board.First(kv => kv.Value == c).Key);
        }
        return sb.ToString();
    }

    string ApplyKey(string codes, bool encode)
    {
        var sb = new StringBuilder(codes.Length);
        for (int i = 0; i < codes.Length; i++)
        {
            int d = codes[i] - CharZero;
            int k = key[i % key.Length];
            int x = encode ? (d + k) % HeaderSize
                           : (d - k + HeaderSize) % HeaderSize;
            sb.Append((char)(CharZero + x));
        }
        return sb.ToString();
    }

    string Decode(string mixed, bool encode)
    {
        var sb = new StringBuilder();
        int i = 0;
        while (i < mixed.Length)
        {
            string s1 = mixed[i++].ToString();
            if (board[s1] == Space)
            {
                string s2 = s1 + mixed[i++];
                char ch  = board[s2];
                if (!encode && ch == Slash)
                    sb.Append(mixed[i++]);
                else
                    sb.Append(ch);
            }
            else sb.Append(board[s1]);
        }
        return sb.ToString();
    }
}