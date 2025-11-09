using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var ciphertext = Console.ReadLine();
        var keylength = int.Parse(Console.ReadLine());
        var word = Console.ReadLine();

        Console.Error.WriteLine($"[DEBUG] Ciphertext: {ciphertext.Substring(0, Math.Min(50, ciphertext.Length))}{(ciphertext.Length > 50 ? "..." : "")}");
        Console.Error.WriteLine($"[DEBUG] Key Length: {keylength}");
        Console.Error.WriteLine($"[DEBUG] Known Word: {word}");

        var cracker = new VigenereCracker();
        var key = cracker.FindKey(ciphertext, keylength, word);
        Console.Error.WriteLine($"[DEBUG] Found key: {key}");
        var plaintext = cracker.Decrypt(ciphertext, key);

        Console.WriteLine(key);
        Console.WriteLine(plaintext.Length > 900 ? plaintext.Substring(0, 900) : plaintext);

        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] Execution Time: {sw.ElapsedMilliseconds}ms");
    }
}

public class VigenereCracker
{
    public string FindKey(string ciphertext, int keylength, string word)
    {
        var candidates = FindCandidates(ciphertext, word.Length);
        var keyFound = new int[keylength];
        foreach (var (cWord, startLetterIdx) in candidates)
        {
            Array.Fill(keyFound, -1);
            var consistent = true;
            for (var j = 0; j < word.Length; j++)
            {
                var pVal = word[j] - 'A';
                var cVal = Char.ToUpper(cWord[j]) - 'A';
                var keyMod = (startLetterIdx + j) % keylength;
                var kVal = (cVal - pVal + 26) % 26;
                if (keyFound[keyMod] != -1 && keyFound[keyMod] != kVal)
                {
                    consistent = false;
                    break;
                }
                keyFound[keyMod] = kVal;
            }
            if (consistent)
            {
                var keyBuilder = new StringBuilder(keylength);
                for (var i = 0; i < keylength; i++)
                {
                    keyBuilder.Append((char)(keyFound[i] + 'A'));
                }
                return keyBuilder.ToString();
            }
        }
        return "";
    }

    public string Decrypt(string ciphertext, string key)
    {
        var keylength = key.Length;
        if (keylength == 0) return ciphertext;
        var keyVals = new int[keylength];
        for(var i = 0; i < keylength; i++)
        {
            keyVals[i] = key[i] - 'A';
        }
        var pt = new StringBuilder();
        var letterCount = 0;
        foreach (var c in ciphertext)
        {
            if (IsLetter(c))
            {
                var kVal = keyVals[letterCount % keylength];
                pt.Append(GetDecryptedChar(c, kVal));
                letterCount++;
            }
            else
            {
                pt.Append(c);
            }
        }
        return pt.ToString();
    }

    private List<(string CWord, int StartLetterIndex)> FindCandidates(string ciphertext, int wordLength)
    {
        var candidates = new List<(string, int)>();
        var sb = new StringBuilder();
        var letterCount = 0;
        var wordStartLetter = -1;
        for (var i = 0; i < ciphertext.Length; i++)
        {
            var c = ciphertext[i];
            if (IsLetter(c))
            {
                if (sb.Length == 0)
                {
                    wordStartLetter = letterCount;
                }
                sb.Append(c);
                letterCount++;
            }
            else
            {
                if (sb.Length == wordLength)
                {
                    candidates.Add((sb.ToString(), wordStartLetter));
                }
                sb.Clear();
            }
        }
        if (sb.Length == wordLength)
        {
            candidates.Add((sb.ToString(), wordStartLetter));
        }
        return candidates;
    }

    private bool IsLetter(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }

    private char GetDecryptedChar(char c, int kVal)
    {
        var isLower = Char.IsLower(c);
        var cVal = Char.ToUpper(c) - 'A';
        var pVal = (cVal - kVal + 26) % 26;
        var pChar = (char)(pVal + 'A');
        return isLower ? Char.ToLower(pChar) : pChar;
    }
}

