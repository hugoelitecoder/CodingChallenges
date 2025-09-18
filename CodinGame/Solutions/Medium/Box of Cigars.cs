using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        int[] a = new int[N];
        for (int i = 0; i < N; i++)
            a[i] = int.Parse(Console.ReadLine());

        if (N <= 2)
        {
            Console.WriteLine(N);
            return;
        }

        var dp = new Dictionary<int, int>[N];
        for (int i = 0; i < N; i++)
            dp[i] = new Dictionary<int, int>();

        int result = 2;
        for (int j = 0; j < N; j++)
        {
            for (int i = 0; i < j; i++)
            {
                int diff = a[j] - a[i];
                int len = dp[i].TryGetValue(diff, out int prevLen) ? prevLen + 1 : 2;
                dp[j][diff] = len;
                if (len > result)
                    result = len;
            }
        }

        Console.WriteLine(result);
    }
}
