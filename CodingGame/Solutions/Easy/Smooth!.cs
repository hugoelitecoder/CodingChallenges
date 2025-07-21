using System;
using System.Linq;
using System.Numerics;

class Solution
{
    public static void Main(string[] args)
    {
        var parts = Console.In
            .ReadToEnd()
            .Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        var numbers = parts.Select(BigInteger.Parse).ToArray();
        var count = (int)numbers[0];
        for (var i = 1; i <= count && i < numbers.Length; i++)
        {
            Console.WriteLine(IsVictory(numbers[i]) ? "VICTORY" : "DEFEAT");
        }
    }

    private static bool IsVictory(BigInteger value)
    {
        var rem = value;
        while (rem % 2 == 0) rem /= 2;
        while (rem % 3 == 0) rem /= 3;
        while (rem % 5 == 0) rem /= 5;
        return rem == 1;
    }
}
