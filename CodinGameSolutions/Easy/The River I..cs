using System;

class Solution
{
    static long Next(long n)
    {
        long sum = n;
        while (n > 0)
        {
            sum += n % 10;
            n /= 10;
        }
        return sum;
    }

    static void Main()
    {
        long r1 = long.Parse(Console.ReadLine());
        long r2 = long.Parse(Console.ReadLine());

        while (r1 != r2)
        {
            if (r1 < r2)
                r1 = Next(r1);
            else
                r2 = Next(r2);
        }

        Console.WriteLine(r1);
    }
}
