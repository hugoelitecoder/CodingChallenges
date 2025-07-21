using System;
using System.Linq;

class Solution
{
    public static void Main(string[] args)
    {
        var surfaceN = int.Parse(Console.ReadLine());
        for (var i = 0; i < surfaceN; i++)
            Console.ReadLine();  // skip surface points

        var prevPower = 0;
        while (true)
        {
            var s = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
            var vSpeed = s[3];
            var desired = vSpeed <= -40 ? 4 : 0;
            var power = desired > prevPower ? prevPower + 1
                      : desired < prevPower ? prevPower - 1
                      : prevPower;
            prevPower = power;

            Console.WriteLine($"0 {power}");
        }
    }
}
