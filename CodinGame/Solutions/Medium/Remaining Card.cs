using System;
using System.Numerics;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        if (n == 1)
        {
            Console.WriteLine(1);
            return;
        }

        int floorPow2 = 1 << (31 - BitOperations.LeadingZeroCount((uint)n));
        int remainder = n - floorPow2;
        Console.WriteLine(remainder == 0 ? n : 2 * remainder);
    }
}
