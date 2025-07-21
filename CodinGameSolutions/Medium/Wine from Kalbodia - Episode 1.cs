using System;
using System.Collections.Generic;
using System.Text;

class Solution {

    static void Main(string[] args) {

        int N = int.Parse(Console.ReadLine());
        var reqPats = new string[N];
        
        for (int i = 0; i < N; i++)
            reqPats[i] = Pattern(Console.ReadLine());
        var map = new Dictionary<string, Queue<int>>();
        
        for (int j = 0; j < N; j++) {
            string pat = Pattern(Console.ReadLine());
            if (!map.TryGetValue(pat, out var q))
                map[pat] = q = new Queue<int>();
            q.Enqueue(j + 1);
        }
        
        var sb = new StringBuilder();
        for (int i = 0; i < N; i++)
            sb.AppendLine(map[reqPats[i]].Dequeue().ToString());

        Console.Write(sb);
    }

    static string Pattern(string s) {
        var dict = new Dictionary<char, int>();
        var sb = new StringBuilder();
        int nxt = 0;
        foreach (char c in s) {
            if (!dict.TryGetValue(c, out int code))
                dict[c] = code = nxt++;
            sb.Append(code).Append(',');
        }
        return sb.ToString();
    }
}
