using System;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var f = new int[n+1];
        if (n >= 0) f[0] = 0;
        if (n >= 1) f[1] = 1;
        if (n >= 2) f[2] = 1;
        if (n >= 3) f[3] = 2;
        if (n >= 4) f[4] = 2;
        for (int i = 5; i <= n; i++)
        {
            if ((i & 1) == 1)
                f[i] = 2 * f[(i + 1) / 2] - 1;
            else
            {
                int h = i / 2;
                f[i] = f[h] + f[h + 1] - 1;
            }
        }

        int bestK = 0, bestI = 1;
        int limit = (n + 1) / 2;
        for (int i = 1; i <= limit; i++)
        {
            int gn = f[i] + f[n + 1 - i] - 1;
            if (gn > bestK)
            {
                bestK = gn;
                bestI = i;
            }
        }

        Console.WriteLine($"{bestK} {bestI}");
    }
}
