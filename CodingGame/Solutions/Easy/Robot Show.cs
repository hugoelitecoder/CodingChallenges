using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        int L = int.Parse(Console.ReadLine());
        int N = int.Parse(Console.ReadLine());
        var positions = Console.ReadLine().Split().Select(int.Parse);

        int maxTime = positions.Max(b => Math.Max(b, L - b));
        Console.WriteLine(maxTime);
    }
}
