using System;
using System.Linq;

class Solution
{
    public static void Main()
    {
        var w = Console.ReadLine().Trim();
        var s = Console.ReadLine();
        var wLower = w.ToLower();
        var wLen = w.Length;
        var wFreq = new int[26];
        foreach (var c in wLower) if (c >= 'a' && c <= 'z') wFreq[c - 'a']++;

        var tokens = s
            .Split(new[] { ' ', ':', '.', ',', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);

        var keyIndex = -1;
        for (var i = 0; i < tokens.Length; i++)
        {
            var tok = tokens[i];
            if (tok.Length != wLen) continue;
            var lower = tok.ToLower();
            if (lower == wLower) continue;

            var freq = new int[26];
            foreach (var c in lower) if (c >= 'a' && c <= 'z') freq[c - 'a']++;
            var match = true;
            for (var j = 0; j < 26; j++)
                if (freq[j] != wFreq[j]) { match = false; break; }
            if (match) { keyIndex = i; break; }
        }

        if (keyIndex < 0)
        {
            Console.WriteLine("IMPOSSIBLE");
            return;
        }

        var beforeWords  = keyIndex % 10;
        var afterWords   = (tokens.Length - keyIndex - 1) % 10;
        var beforeLetters= tokens
            .Take(keyIndex)
            .Sum(t => t.Count(char.IsLetter)) % 10;
        var afterLetters = tokens
            .Skip(keyIndex + 1)
            .Sum(t => t.Count(char.IsLetter)) % 10;

        Console.WriteLine($"{beforeWords}.{afterWords}.{beforeLetters}.{afterLetters}");
    }
}
