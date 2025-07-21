using System;
using System.Linq;

class Solution {
    
    static void Main() {
        int V = int.Parse(Console.ReadLine()), 
            N = int.Parse(Console.ReadLine());
        var prices = Enumerable.Range(0, N)
                               .Select(_ => {
                                   var parts = Console.ReadLine().Split();
                                   return int.Parse(parts[^1]);
                               })
                               .ToArray();

        //counting
        var dp = new long[V + 1];
        dp[0] = 1;
        foreach (var p in prices) {
            var next = new long[V + 1];
            for (int sum = 0; sum <= V; sum++) {
                if (dp[sum] == 0) continue;
                for (int k = 0; k < 4 && sum + k * p <= V; k++)
                    next[sum + k * p] += dp[sum];
            }
            dp = next;
        }

        Console.WriteLine(dp[V]);
    }
}
