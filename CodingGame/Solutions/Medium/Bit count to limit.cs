using System;
using System.Numerics;

class Solution
{
    static void Main()
    {
        ulong n = ulong.Parse(Console.ReadLine()!);
        ulong total = 0;

        // Peel off the highest 1-bit each time:
        while (n > 0)
        {
            // k = index of highest set bit in n  (0-based)
            int k = 63 - BitOperations.LeadingZeroCount(n);
            // p = 2^k
            ulong p = 1UL << k;
            // All lower-bit 1's contributed by [0..p-1]: there are k bits, each set exactly p/2 times
            total += (ulong)k * (p >> 1);
            // The k-th bit itself is set in every number from p..n:
            total += (n - p + 1);
            // Remove that block and repeat on the remainder
            n -= p;
        }

        Console.WriteLine(total);
    }
}
