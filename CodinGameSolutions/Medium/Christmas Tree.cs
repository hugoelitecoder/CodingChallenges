using System;
using System.Numerics;

class Solution
{
    static void Main()
    {
        BigInteger n = BigInteger.Parse(Console.ReadLine());
        BigInteger i = BigInteger.Parse(Console.ReadLine());
        BigInteger left = 0, right = n;
        BigInteger target = n - 1;
        BigInteger bestBranch = 0;

        while (left <= right)
        {
            BigInteger mid = (left + right) >> 1;
            BigInteger sum = SumTop(mid, i);
            if (sum <= target)
            {
                bestBranch = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        BigInteger used = SumTop(bestBranch, i);
        BigInteger leftover = n - used;
        BigInteger height = bestBranch + leftover;

        Console.WriteLine(height);
    }

    static BigInteger SumTop(BigInteger h, BigInteger i)
    {
        BigInteger term = h * (h - 1) / 2;
        return h + i * term;
    }
}