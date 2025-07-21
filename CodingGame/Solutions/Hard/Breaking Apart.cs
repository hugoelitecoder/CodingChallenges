using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
   
    public static void Main(string[] args)
    {
        var cols = int.Parse(Console.ReadLine());
        var text = new List<string>();
        while (true)
        {
            var part = Console.ReadLine();
            if (part == null) break;
            text.AddRange(part.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }
        var curLength = 0;
        var words = new List<string>();
        while (text.Count > 0)
        {
            var curWord = text[0];
            text.RemoveAt(0);
            if (curLength + curWord.Length <= cols)
            {
                words.Add(curWord);
                curLength += curWord.Length + 1;
            }
            else
            {
                var splitFront = "";
                var syllables = GetSyllables(curWord);
                while (syllables.Count > 0 && curLength + splitFront.Length + syllables[0].Length + 1 <= cols)
                {
                    splitFront += syllables[0];
                    syllables.RemoveAt(0);
                }
                if (splitFront.Length == 1)
                {
                    syllables.Insert(0, splitFront);
                    splitFront = "";
                }
                if (splitFront != "")
                    words.Add(splitFront + "-");
                if (syllables.Count > 0)
                    text.Insert(0, string.Join("", syllables));
                Console.WriteLine(string.Join(" ", words));
                words = new List<string>();
                curLength = 0;
            }
        }
        if (words.Count > 0)
            Console.WriteLine(string.Join(" ", words));
    }

     static bool IsVowel(char c)
    {
        return "aeiouAEIOU".Contains(char.ToLower(c));
    }

    static List<string> GetSyllables(string word)
    {
        var syllables = new List<string>();
        var cur = "";
        var n = word.Length;
        for (var i = 0; i < n; i++)
        {
            if (IsVowel(word[i]))
            {
                if (i > 0 && !IsVowel(word[i - 1]))
                    cur += word[i - 1];
                cur += word[i];
                if (i < n - 1)
                {
                    if (i < n - 2)
                    {
                        if (!IsVowel(word[i + 1]) && !IsVowel(word[i + 2]))
                            cur += word[i + 1];
                    }
                    else
                    {
                        if (!IsVowel(word[i + 1]))
                            cur += word[i + 1];
                    }
                }
                syllables.Add(cur);
                cur = "";
            }
        }
        return syllables;
    }

}
