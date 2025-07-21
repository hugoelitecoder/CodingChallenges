using System;
using System.Collections.Generic;

class Solution
{
    private static readonly string[] _adjList = {
        "Adaptable","Adventurous","Affectionate","Courageous","Creative",
        "Dependable","Determined","Diplomatic","Giving","Gregarious",
        "Hardworking","Helpful","Hilarious","Honest","Non-judgmental",
        "Observant","Passionate","Sensible","Sensitive","Sincere"
    };
    private static readonly string[] _goodList = {
        "Love","Forgiveness","Friendship","Inspiration",
        "Epic Transformations","Wins"
    };
    private static readonly string[] _badList = {
        "Crime","Disappointment","Disasters",
        "Illness","Injury","Investment Loss"
    };
    private static readonly char[] _vowels = { 'a', 'e', 'i', 'o', 'u', 'y' };
    private static readonly char[] _consonants = {
        'b','c','d','f','g','h','j','k','l','m',
        'n','p','q','r','s','t','v','w','x','z'
    };

    public static void Main()
    {
        var name = Console.ReadLine();
        var uc = new List<char>();
        var uv = new List<char>();
        foreach (var ch in name)
        {
            if (!char.IsLetter(ch)) continue;
            var c = char.ToLower(ch);
            if (Array.IndexOf(_vowels, c) >= 0)
            {
                if (uv.Count < 2) uv.Add(c);
            }
            else
            {
                if (uc.Count < 3 && !uc.Contains(c)) uc.Add(c);
            }
            if (uc.Count == 3 && uv.Count == 2) break;
        }
        if (uc.Count < 3 || uv.Count < 2)
        {
            Console.WriteLine($"Hello {name}.");
            return;
        }
        var c1 = Array.IndexOf(_consonants, uc[0]) + 1;
        var c2 = Array.IndexOf(_consonants, uc[1]) + 1;
        var c3 = Array.IndexOf(_consonants, uc[2]) + 1;
        var v1 = Array.IndexOf(_vowels, uv[0]) + 1;
        var v2 = Array.IndexOf(_vowels, uv[1]) + 1;
        var adj1 = _adjList[c1 - 1].ToLower();
        var adj2 = _adjList[c2 - 1].ToLower();
        var adj3 = _adjList[c3 - 1].ToLower();
        var good = _goodList[v1 - 1].ToLower();
        var bad  = _badList[v2 - 1].ToLower();

        Console.WriteLine($"It's so nice to meet you, my dear {adj1} {name}.");
        Console.WriteLine($"I sense you are both {adj2} and {adj3}.");
        Console.WriteLine($"May our future together have much more {good} than {bad}.");
    }
}