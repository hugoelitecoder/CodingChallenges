using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        long n = long.Parse(Console.ReadLine());

        List<long> fibs = new List<long> { 1, 2 };
        while (fibs[^1] <= n)
            fibs.Add(fibs[^1] + fibs[^2]);

        List<long> result = new List<long>();
        for (int i = fibs.Count - 2; i >= 0; i--) 
        {
            if (fibs[i] <= n)
            {
                result.Add(fibs[i]);
                n -= fibs[i];
                i--; 
            }
        }
        Console.WriteLine(string.Join("+", result));
    }
}
