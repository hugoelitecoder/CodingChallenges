using System;
using System.Linq;

class Solution
{
    static bool LexLess(string a, string b)
    {
        int n = Math.Min(a.Length, b.Length);
        for (int i = 0; i < n; i++)
        {
            if (a[i] != b[i]) 
                return a[i] < b[i];
        }
        return a.Length < b.Length;
    }

    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var names = new string[N];
        for (int i = 0; i < N; i++)
            names[i] = Console.ReadLine();
        Array.Sort(names, StringComparer.Ordinal);

        string L = names[N/2 - 1],
               R = names[N/2];

        string best = L;

        for (int i = 0; i < L.Length; i++)
        {
            string prefix = L.Substring(0, i);
            for (char c = (char)(L[i] + 1); c <= 'Z'; c++)
            {
                string S = prefix + c;
                if (!LexLess(S, L) && LexLess(S, R))
                {
                    if (S.Length < best.Length 
                        || (S.Length == best.Length && LexLess(S, best)))
                    {
                        best = S;
                    }
                }
            }
        }

        Console.WriteLine(best);
    }
}
