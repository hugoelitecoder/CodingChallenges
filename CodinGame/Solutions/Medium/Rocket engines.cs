using System;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        long a = long.Parse(inputs[0]);
        long b = long.Parse(inputs[1]);
        inputs = Console.ReadLine().Split(' ');
        long c = long.Parse(inputs[0]);
        long d = long.Parse(inputs[1]);

        long diagDiff = Math.Abs((a + d) - (b + c));
        long result   = (diagDiff + 1) / 2;
        Console.WriteLine(result);
    }
}
