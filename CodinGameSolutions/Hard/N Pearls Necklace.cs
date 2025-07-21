using System;
using System.Collections.Generic;
using System.Numerics;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        int C = int.Parse(Console.ReadLine());
        var solver = new NecklaceSolver();
        var answer = solver.Calculate(N, C);
        Console.WriteLine(answer);
    }
}

public class NecklaceSolver
{
    public BigInteger Calculate(int n, int c)
    {
        var count = BigInteger.Zero;
        var sgn = 1;
        for (var i = c; i >= 1; i--)
        {
            var combinations = Combinations(c, i);
            var burnsideCount = DihedralBurnsideCount(n, i);
            count += sgn * combinations * burnsideCount;
            sgn *= -1;
        }
        return count;
    }

    private BigInteger DihedralBurnsideCount(int n, int c)
    {
        var totalFixedPoints = BigInteger.Pow(c, n);
        for (var k = 1; k < n; k++)
        {
            totalFixedPoints += BigInteger.Pow(c, GCD(k, n));
        }
        if (n % 2 == 1)
        {
            totalFixedPoints += (BigInteger)n * BigInteger.Pow(c, (n + 1) / 2);
        }
        else
        {
            totalFixedPoints += (BigInteger)(n / 2) * BigInteger.Pow(c, n / 2);
            totalFixedPoints += (BigInteger)(n / 2) * BigInteger.Pow(c, n / 2 + 1);
        }
        return totalFixedPoints / (2 * n);
    }

    private BigInteger Combinations(int n, int k)
    {
        if (k < 0 || k > n)
        {
            return 0;
        }
        if (k == 0 || k == n)
        {
            return 1;
        }
        if (k > n / 2)
        {
            k = n - k;
        }
        var result = BigInteger.One;
        for (var i = 1; i <= k; i++)
        {
            result = result * (n - i + 1) / i;
        }
        return result;
    }

    private int GCD(int a, int b)
    {
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}
