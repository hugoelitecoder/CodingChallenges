using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        var lines = new string[N];
        for (int i = 0; i < N; i++)
            lines[i] = Console.ReadLine();
        var corrector = new WordFixer();
        corrector.Train(lines);
        foreach (var line in lines)
            Console.WriteLine(corrector.CorrectLine(line));
    }
}

class WordFixer
{
    private Dictionary<string,int> _frequency = new Dictionary<string,int>();
    private Dictionary<string,string> _replacement = new Dictionary<string,string>();

    public void Train(IEnumerable<string> lines)
    {
        foreach(var line in lines)
            foreach(var token in Tokenize(line).Where(IsWord))
                _frequency[token] = _frequency.ContainsKey(token) ? _frequency[token] + 1 : 1;

        var words = _frequency.Keys.ToList();
        foreach(var w in words)
        {
            for(int i = 0; i < w.Length; i++)
            {
                var missing = w.Substring(0,i) + w.Substring(i+1);
                if(_frequency.ContainsKey(missing) && _frequency[w] > _frequency[missing])
                    _replacement[missing] = w;
                var duplicate = w.Substring(0,i+1) + w[i] + w.Substring(i+1);
                if(_frequency.ContainsKey(duplicate) && _frequency[w] > _frequency[duplicate])
                    _replacement[duplicate] = w;
            }
        }
    }

    public string CorrectLine(string line)
    {
        var tokens = Tokenize(line);
        for(int i = 0; i < tokens.Count; i++)
        {
            var t = tokens[i];
            if(IsWord(t) && _replacement.ContainsKey(t))
                tokens[i] = _replacement[t];
        }
        return string.Concat(tokens);
    }

    static List<string> Tokenize(string s)
    {
        var parts = new List<string>();
        var sb = new StringBuilder();
        bool? inWord = null;
        foreach(var ch in s)
        {
            bool nowWord = ch >= 'a' && ch <= 'z';
            if(inWord == null || nowWord != inWord.Value)
            {
                if(sb.Length>0){ parts.Add(sb.ToString()); sb.Clear(); }
                inWord = nowWord;
            }
            sb.Append(ch);
        }
        if(sb.Length>0) parts.Add(sb.ToString());
        return parts;
    }

    static bool IsWord(string token)
        => token.All(c => c >= 'a' && c <= 'z');
}