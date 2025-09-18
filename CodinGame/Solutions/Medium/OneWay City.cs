using System;
using System.Numerics;

class Solution
{
    static void Main()
    {
        int M = int.Parse(Console.ReadLine());
        int N = int.Parse(Console.ReadLine());
        int n = M + N - 2, k = Math.Min(M - 1, N - 1);
        BigInteger paths = BigInteger.One;
        for (int i = 1; i <= k; i++)
        {
            paths = paths * (n - k + i) / i;
        }
        var s = paths.ToString();
        Console.WriteLine(s.Length <= 1000 ? s : s.Substring(0, 1000));
    }
}
