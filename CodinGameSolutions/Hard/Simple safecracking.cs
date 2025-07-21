using System;
using System.Linq;

class Solution
{
    public static void Main(string[] args)
    {
        var msg = Console.ReadLine();
        var decoder = new SafeDecoder();
        Console.WriteLine(decoder.Decode(msg));
    }
}

class SafeDecoder
{
    private static readonly string[] _nums = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
    public string Decode(string msg)
    {
        var key = 0;
        while (++key < 27)
        {
            var t = Rot(msg, key);
            if (t.Contains("safe"))
            {
                msg = t;
                break;
            }
        }
        var idx = msg.IndexOf(":") + 2;
        var codeWords = msg.Substring(idx).Split('-');
        var solution = "";
        foreach (var w in codeWords)
        {
            var d = Array.IndexOf(_nums, w);
            solution += d;
        }
        return solution;
    }
    private string Rot(string msg, int rot)
    {
        return new string(msg.Select(c =>
        {
            if (c >= 'a' && c <= 'z')
                return (char)((c - 'a' + rot) % 26 + 'a');
            if (c >= 'A' && c <= 'Z')
                return (char)((c - 'A' + rot) % 26 + 'A');
            return c;
        }).ToArray());
    }
}
