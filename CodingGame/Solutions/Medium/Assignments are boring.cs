using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int seed = int.Parse(Console.ReadLine());
        int papers = int.Parse(Console.ReadLine());
        int power = int.Parse(Console.ReadLine());
        int mod = 1 << power;

        long[] freq = new long[mod];
        int z = seed;
        for (int i = 0; i < papers; i++)
        {
            z = (int)((1664525L * z + 1013904223L) % mod);
            freq[z]++;
        }

        var output = new List<long>();
        long remaining = papers;
        for (int i = 0; i < mod; i++)
        {
            if (freq[i] != 0)
            {
                remaining -= freq[i];
                output.Add(remaining);
            }
        }

        for (int i = 0; i < output.Count; i++)
        {
            if (i > 0) Console.Write(" ");
            Console.Write(output[i]);
        }
    }
}
