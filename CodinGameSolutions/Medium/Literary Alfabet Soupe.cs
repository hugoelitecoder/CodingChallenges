using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    private const int N = 13;
    private static List<string>[] possible;
    private static string[] result;
    private static HashSet<string> used = new HashSet<string>();

    static void Main()
    {
        var excerpts = new string[N];
        for (int i = 0; i < N; i++)
            excerpts[i] = Console.ReadLine() ?? string.Empty;

        var english = new HashSet<char>("abcdefghijklmnopqrstuvwxyz");
        var alphabets = new Dictionary<string, HashSet<char>>()
        {
            ["Danish"]     = new HashSet<char>(english),
            ["English"]    = new HashSet<char>(english),
            ["Estonian"]   = new HashSet<char>(english),
            ["Finnish"]    = new HashSet<char>(english),
            ["French"]     = new HashSet<char>(english),
            ["German"]     = new HashSet<char>(english),
            ["Irish"]      = new HashSet<char>(english),
            ["Italian"]    = new HashSet<char>(english),
            ["Portuguese"] = new HashSet<char>(english),
            ["Spanish"]    = new HashSet<char>(english),
            ["Swedish"]    = new HashSet<char>(english),
            ["Turkish"]    = new HashSet<char>(english),
            ["Welsh"]      = new HashSet<char>(english)
        };
        alphabets["Danish"].ExceptWith("qz");    alphabets["Danish"].UnionWith("æåø");
        alphabets["Estonian"].ExceptWith("cfqwxy"); alphabets["Estonian"].UnionWith("šžõäöü");
        alphabets["Finnish"].ExceptWith("bfqwx");  alphabets["Finnish"].UnionWith("äö");
        alphabets["French"].UnionWith("çœëïüàèùâêîôûé");
        alphabets["German"].UnionWith("ßäöü");
        alphabets["Irish"].ExceptWith("jkqvwxyz"); alphabets["Irish"].UnionWith("áéíóú");
        alphabets["Italian"].ExceptWith("jkwxy");  alphabets["Italian"].UnionWith("àèìòùé");
        alphabets["Portuguese"].ExceptWith("kw");  alphabets["Portuguese"].UnionWith("çãõàâêôáéíóú");
        alphabets["Spanish"].ExceptWith("kw");   alphabets["Spanish"].UnionWith("ñüáéíóú");
        alphabets["Swedish"].ExceptWith("qw");   alphabets["Swedish"].UnionWith("åäö");
        alphabets["Turkish"].ExceptWith("qwx");  alphabets["Turkish"].UnionWith("ğçşıöüİı");
        alphabets["Welsh"].ExceptWith("jkqvxz");   alphabets["Welsh"].UnionWith("ŵŷâêîôû");

        possible = new List<string>[N];
        for (int i = 0; i < N; i++)
        {
            var chars = new HashSet<char>(
                excerpts[i]
                    .Where(char.IsLetter)
                    .Select(char.ToLowerInvariant)
            );
            possible[i] = alphabets
                .Where(kv => chars.IsSubsetOf(kv.Value))
                .Select(kv => kv.Key)
                .ToList();
        }

        result = new string[N];
        DFS(0);

        foreach (var lang in result)
            Console.WriteLine(lang);
    }

    private static bool DFS(int placed)
    {
        if (placed == N)
            return true;

        int next = -1, minCount = int.MaxValue;
        for (int i = 0; i < N; i++)
        {
            if (result[i] == null && possible[i].Count < minCount)
            {
                minCount = possible[i].Count;
                next = i;
            }
        }
        if (next < 0)
            return false;

        foreach (var lang in possible[next])
        {
            if (!used.Add(lang))
                continue;

            result[next] = lang;
            if (DFS(placed + 1))
                return true;

            used.Remove(lang);
            result[next] = null;
        }

        return false;
    }
}