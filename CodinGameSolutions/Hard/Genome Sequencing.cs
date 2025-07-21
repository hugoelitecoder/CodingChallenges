using System;
using System.Linq;

class Solution {
    static void Main() {
        int N = int.Parse(Console.ReadLine());
        var seqs = new string[N];
        for (int i = 0; i < N; i++)
            seqs[i] = Console.ReadLine().Trim();
        var idx = Enumerable.Range(0, N).ToArray();
        int best = int.MaxValue;
        do {
            string cur = seqs[idx[0]];
            for (int i = 1; i < N; i++)
                cur = Merge(cur, seqs[idx[i]]);
            if (cur.Length < best) best = cur.Length;
        } while (NextPerm(idx));
        Console.WriteLine(best);
    }

    static string Merge(string a, string b) {
        if (a.Contains(b)) return a;
        if (b.Contains(a)) return b;
        int max = Math.Min(a.Length, b.Length);
        for (int k = max; k > 0; k--) {
            if (a.EndsWith(b.Substring(0, k)))
                return a + b.Substring(k);
        }
        return a + b;
    }

    static bool NextPerm(int[] a) {
        int i = a.Length - 2;
        while (i >= 0 && a[i] >= a[i + 1]) i--;
        if (i < 0) return false;
        int j = a.Length - 1;
        while (a[j] <= a[i]) j--;
        (a[i], a[j]) = (a[j], a[i]);
        Array.Reverse(a, i + 1, a.Length - i - 1);
        return true;
    }
}
