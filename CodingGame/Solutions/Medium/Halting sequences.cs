using System;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        for (int i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            long a = long.Parse(parts[0]);
            long b = long.Parse(parts[1]);
            Console.WriteLine(Halts(a, b) ? "halts" : "loops");
        }
    }

    static long Gcd(long a, long b)
    {
        while (b != 0)
        {
            long t = b;
            b = a % b;
            a = t;
        }
        return a;
    }

    static bool Halts(long x, long y)
    {
        long g = Gcd(x, y);
        long z = (x + y) / g;
        return (z & (z - 1)) == 0;
    }
}