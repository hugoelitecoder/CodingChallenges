using System;

class Solution
{

    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        int count = 0;

        int mLimit = (int)Math.Floor(Math.Sqrt(N));
        for (int m = 2; m <= mLimit; m++)
        {
            for (int n = 1; n < m; n++)
            {
                if (((m - n) & 1) == 0)
                    continue;

                if (Gcd(m, n) != 1)
                    continue;

                int c = m * m + n * n;
                if (c > N)
                    continue;

                count++;
            }
        }

        Console.WriteLine(count);
    }

    static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            int t = b;
            b = a % b;
            a = t;
        }
        return a;
    }
}
