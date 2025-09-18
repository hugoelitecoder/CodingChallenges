using System;
using System.Linq;

class Solution {
    
    static void Main() {
        int N = int.Parse(Console.ReadLine());
        var dims = new long[N + 1];
        for (int i = 0; i < N; i++) {
            var a = Console.ReadLine().Split().Select(long.Parse).ToArray();
            if (i == 0) dims[0] = a[0];
            dims[i + 1] = a[1];
        }

        var dp = new long[N, N];

        for (int len = 2; len <= N; len++) {
            for (int i = 0; i + len <= N; i++) {
                int j = i + len - 1;
                dp[i, j] = Enumerable
                    .Range(i, j - i)
                    .Select(k => dp[i, k] + dp[k + 1, j] + dims[i] * dims[k + 1] * dims[j + 1])
                    .Min();
            }
        }

        Console.WriteLine(dp[0, N - 1]);
    }
}
