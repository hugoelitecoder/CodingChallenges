using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        int Y = int.Parse(Console.ReadLine());

        var S = new long[N];
        var H = new long[N];

        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            S[i] = long.Parse(parts[0]);
            H[i] = long.Parse(parts[1]);
        }

        for (int year = 1; year <= Y; year++)
        {
            long totalAlive = 0;

            for (int i = 0; i < N; i++)
            {
                long newlySick = Math.Min(H[i], S[i] * 2);
                H[i] -= newlySick;
                S[i] = newlySick;

                totalAlive += H[i] + S[i];
            }

            Console.WriteLine(totalAlive);

            if (totalAlive == 0)
                break;
        }
    }
}
