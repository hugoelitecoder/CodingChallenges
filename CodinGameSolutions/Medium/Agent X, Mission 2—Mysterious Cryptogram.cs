using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

class Solution
{
    static string ciphertext;
    static List<string> dict;
    static List<string> cwords;
    static char[] map = new char[26];
    static char[] revMap = new char[26];

    static void Main()
    {
        ciphertext = Console.ReadLine();
        int N = int.Parse(Console.ReadLine());
        dict = new List<string>(N);
        for(int i=0;i<N;i++) dict.Add(Console.ReadLine().ToUpper());

        cwords = Regex.Matches(ciphertext, @"[A-Za-z']+")
                     .Cast<Match>()
                     .Select(m=>m.Value.ToUpper())
                     .Distinct()
                     .OrderByDescending(w=>w.Length)
                     .ToList();

        if(!Backtrack(0))
            throw new Exception("No solution!");

        var plain = new char[ciphertext.Length];
        for(int i=0;i<ciphertext.Length;i++)
        {
            char c = ciphertext[i];
            if(char.IsLetter(c))
            {
                bool lower = char.IsLower(c);
                int ci = char.ToUpper(c) - 'A';
                char p = map[ci];
                plain[i] = lower ? char.ToLower(p) : p;
            }
            else plain[i] = c;
        }
        Console.WriteLine(new string(plain));

        for(int i=0;i<26;i++)
        {
            char orig = (char)('A'+i);
            char sub = revMap[i];
            Console.WriteLine(sub == '\0' ? "Na" : $"{orig} -> {sub.ToString()}");
        }
    }

    static bool Backtrack(int idx)
    {
        if(idx == cwords.Count) return true;
        string cw = cwords[idx];
        int L = cw.Length;

        foreach(var dw in dict)
        {
            if(dw.Length != L) continue;
            bool shapeOk = true;
            for(int i=0;i<L;i++)
                if((cw[i]=='\'') ^ (dw[i]=='\'')){ shapeOk=false; break; }
            if(!shapeOk) continue;

            var assigns = new List<(int c,int p)>();
            bool ok = true;
            for(int i=0;i<L && ok;i++)
            {
                if(cw[i]=='\'') continue;
                int ci = cw[i]-'A', pi = dw[i]-'A';
                if(map[ci]!=default(char) && map[ci]!=dw[i]) ok=false;
                if(revMap[pi]!=default(char) && revMap[pi]!=cw[i]) ok=false;
                if(!ok) break;
                if(map[ci]==default(char))
                {
                    map[ci] = dw[i];
                    revMap[pi] = cw[i];
                    assigns.Add((ci,pi));
                }
            }
            if(!ok)
            {
                foreach(var (c,p) in assigns)
                {
                    map[c]=default(char);
                    revMap[p]=default(char);
                }
                continue;
            }

            if(Backtrack(idx+1)) return true;

            foreach(var (c,p) in assigns)
            {
                map[c]=default(char);
                revMap[p]=default(char);
            }
        }
        return false;
    }
}
