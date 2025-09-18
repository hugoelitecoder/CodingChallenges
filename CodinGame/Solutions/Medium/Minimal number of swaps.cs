using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        Console.ReadLine();
        var tokens = Console.ReadLine().Split();
        var steps = tokens.Take(tokens.Count(t => t == "1")).Count(t => t == "0");
        Console.WriteLine(steps);
    }
}
