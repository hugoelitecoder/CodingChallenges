using System;
using System.Numerics;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var n = long.Parse(inputs[0]);
        var k = int.Parse(inputs[1]);
        var primeInputs = Console.ReadLine().Split(' ');
        var primes = new int[k];
        for (var i = 0; i < k; i++)
        {
            primes[i] = int.Parse(primeInputs[i]);
        }
        var counter = new DivisibilityCounter();
        var answer = counter.CalculateCount(n, primes);
        Console.WriteLine(answer);
    }
}

public class DivisibilityCounter
{
    public long CalculateCount(long n, int[] primes)
    {
        var totalCount = 0L;
        var k = primes.Length;
        var numSubsets = 1 << k;
        for (var i = 1; i < numSubsets; i++)
        {
            var product = 1L;
            for (var j = 0; j < k; j++)
            {
                if ((i & (1 << j)) == 0)
                {
                    continue;
                }
                var prime = primes[j];
                if (product > n / prime)
                {
                    product = n + 1;
                    break;
                }
                product *= prime;
            }
            if (product <= n)
            {
                var subsetSize = BitOperations.PopCount((uint)i);
                var sign = (subsetSize % 2 != 0) ? 1 : -1;
                totalCount += sign * (n / product);
            }
        }
        return totalCount;
    }
}
