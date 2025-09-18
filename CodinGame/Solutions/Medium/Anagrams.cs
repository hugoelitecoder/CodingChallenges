using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var scrambled = Console.ReadLine();
        var unscrambler = new PhraseUnscrambler();
        Console.WriteLine(unscrambler.Unscramble(scrambled));
    }
}

class PhraseUnscrambler
{
    public string Unscramble(string input)
    {
        var spaces  = GetSpacePositions(input);
        var letters = ExtractLetters(input);

        letters = RotateGroup(letters, k: 4, shift: +1);
        letters = RotateGroup(letters, k: 3, shift: -1);
        letters = ReverseGroup(letters, k: 2);

        var withSpaces = ReinsertSpaces(letters, spaces, input.Length);

        return RestoreWordOrder(withSpaces);
    }

    private HashSet<int> GetSpacePositions(string s)
    {
        return s.Select((c, i) => (c, i))
                .Where(t => t.c == ' ')
                .Select(t => t.i)
                .ToHashSet();
    }

    private char[] ExtractLetters(string s)
    {
        return s.Where(c => c != ' ').ToArray();
    }

    private char[] ReinsertSpaces(char[] letters, HashSet<int> spaces, int totalLength)
    {
        var result = new char[totalLength];
        int li = 0;
        for (int i = 0; i < totalLength; i++)
        {
            if (spaces.Contains(i)) result[i] = ' ';
            else                result[i] = letters[li++];
        }
        return result;
    }

    private string RestoreWordOrder(char[] withSpaces)
    {
        var words = new string(withSpaces).Split(' ');
        var flat  = string.Concat(words);
        var lens  = words.Select(w => w.Length).Reverse().ToArray();

        var resultWords = new List<string>();
        int p = 0;
        foreach (var len in lens)
        {
            resultWords.Add(flat.Substring(p, len));
            p += len;
        }
        return string.Join(" ", resultWords);
    }

    private char[] RotateGroup(char[] arr, int k, int shift)
    {
        var idx  = FindGroupIndices(arr, k);
        int m    = idx.Length;
        if (m < 2) return arr;

        shift = ((shift % m) + m) % m;
        var buf = idx.Select(i => arr[i]).ToArray();
        var copy= arr.ToArray();

        for (int j = 0; j < m; j++)
            copy[idx[j]] = buf[(j - shift + m) % m];

        return copy;
    }

    private char[] ReverseGroup(char[] arr, int k)
    {
        var idx  = FindGroupIndices(arr, k);
        int n    = idx.Length;
        var copy = arr.ToArray();

        for (int j = 0; j < n/2; j++)
        {
            copy[idx[j]]         = arr[idx[n-1-j]];
            copy[idx[n-1-j]]     = arr[idx[j]];
        }
        return copy;
    }

    private int[] FindGroupIndices(char[] arr, int k)
    {
        return arr.Select((c, i) => (c, i))
                  .Where(t => GetAlphaIndex(t.c) % k == 0)
                  .Select(t => t.i)
                  .ToArray();
    }

    private int GetAlphaIndex(char c)
    {
        if (char.IsLetter(c))
            return char.ToUpper(c) - 'A' + 1;
        return 0;
    }
}