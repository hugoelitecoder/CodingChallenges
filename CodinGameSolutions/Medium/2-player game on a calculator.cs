using System;
using System.Collections.Generic;

class Solution {
    static readonly int[][] Neighbors = new int[][] {
        new int[0],              
        new[] {2,4,5},           
        new[] {1,3,4,5,6},       
        new[] {2,5,6},           
        new[] {1,2,5,7,8},       
        new[] {1,2,3,4,6,7,8,9}, 
        new[] {2,3,5,8,9},       
        new[] {4,5,8},           
        new[] {4,5,6,7,9},       
        new[] {5,6,8}            
    };

    static void Main() {
        int N = int.Parse(Console.ReadLine());
        var win = new bool[N + 1, 10];

        for (int n = 1; n <= N; n++) {
            bool w0 = false;
            for (int d = 1; d <= 9; d++) {
                if (n - d < 0) continue;
                if (!win[n - d, d]) { w0 = true; break; }
            }
            win[n, 0] = w0;
            for (int last = 1; last <= 9; last++) {
                bool w = false;
                foreach (int d in Neighbors[last]) {
                    if (n - d < 0) continue;
                    if (!win[n - d, d]) { w = true; break; }
                }
                win[n, last] = w;
            }
        }

        var res = new List<int>();
        for (int d = 1; d <= 9; d++) {
            if (N - d < 0) continue;
            if (!win[N - d, d]) res.Add(d);
        }

        Console.WriteLine(res.Count > 0 ? string.Join(" ", res) : "");
    }
}
