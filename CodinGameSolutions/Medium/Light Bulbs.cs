using System;

class Solution {
    
    static int GrayToIndex(string g) {
        int n = g.Length;
        int idx = g[0] - '0';
        int val = idx;
        for (int i = 1; i < n; i++) {
            int bit = g[i] - '0';
            idx ^= bit;
            val = (val << 1) | idx;
        }
        return val;
    }

    static void Main() {
        string s = Console.ReadLine().Trim();
        string t = Console.ReadLine().Trim();
        int a = GrayToIndex(s);
        int b = GrayToIndex(t);
        Console.WriteLine(Math.Abs(a - b));
    }
}
