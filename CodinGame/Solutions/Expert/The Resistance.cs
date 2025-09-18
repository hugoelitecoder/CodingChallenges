using System;
using System.Collections.Generic;
using System.Text;

public class Solution
{
    public static void Main(string[] args)
    {
        var morseSequence = Console.ReadLine();
        var n = int.Parse(Console.ReadLine());
        var decoder = new MorseDecoder();
        for (var i = 0; i < n; i++)
        {
            var word = Console.ReadLine();
            decoder.AddDictionaryWord(word);
        }
        var result = decoder.CountMessages(morseSequence);
        Console.WriteLine(result);
    }
}

internal class MorseDecoder
{
    public void AddDictionaryWord(string word)
    {
        var morseWord = MorseTranslator.ToMorse(word);
        var currentNode = _root;
        foreach (var morseChar in morseWord)
        {
            if (morseChar == '.')
            {
                if (currentNode.DotChild == null)
                {
                    currentNode.DotChild = new TrieNode();
                }
                currentNode = currentNode.DotChild;
            }
            else
            {
                if (currentNode.DashChild == null)
                {
                    currentNode.DashChild = new TrieNode();
                }
                currentNode = currentNode.DashChild;
            }
        }
        currentNode.WordCount++;
    }

    public long CountMessages(string morseSequence)
    {
        var len = morseSequence.Length;
        var dp = new long[len + 1];
        dp[len] = 1;
        for (var i = len - 1; i >= 0; i--)
        {
            var currentNode = _root;
            for (var j = i; j < len; j++)
            {
                var morseChar = morseSequence[j];
                if (morseChar == '.')
                {
                    currentNode = currentNode.DotChild;
                }
                else
                {
                    currentNode = currentNode.DashChild;
                }
                if (currentNode == null)
                {
                    break;
                }
                if (currentNode.WordCount > 0)
                {
                    dp[i] += currentNode.WordCount * dp[j + 1];
                }
            }
        }
        return dp[0];
    }

    private class TrieNode
    {
        public TrieNode DotChild { get; set; }
        public TrieNode DashChild { get; set; }
        public int WordCount { get; set; }
    }
    
    private readonly TrieNode _root = new TrieNode();
}

internal static class MorseTranslator
{
    public static string ToMorse(string word)
    {
        var sb = new StringBuilder();
        foreach (var c in word)
        {
            sb.Append(_codeMap[c]);
        }
        return sb.ToString();
    }

    private static readonly Dictionary<char, string> _codeMap = new Dictionary<char, string>
    {
        {'A', ".-"}, {'B', "-..."}, {'C', "-.-."}, {'D', "-.."}, {'E', "."},
        {'F', "..-."}, {'G', "--."}, {'H', "...."}, {'I', ".."}, {'J', ".---"},
        {'K', "-.-"}, {'L', ".-.."}, {'M', "--"}, {'N', "-."}, {'O', "---"},
        {'P', ".--."}, {'Q', "--.-"}, {'R', ".-."}, {'S', "..."}, {'T', "-"},
        {'U', "..-"}, {'V', "...-"}, {'W', ".--"}, {'X', "-..-"}, {'Y', "-.--"},
        {'Z', "--.."}
    };
}