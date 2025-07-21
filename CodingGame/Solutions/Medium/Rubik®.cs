using System;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        Console.WriteLine(VisibleMiniCubes(N));
    }

    static long VisibleMiniCubes(int N)
    {
        if (N <= 1) return 1;
        long total = (long)N * N * N;
        long internalCount = (long)(N - 2) * (N - 2) * (N - 2);
        return total - internalCount;
    }
}
