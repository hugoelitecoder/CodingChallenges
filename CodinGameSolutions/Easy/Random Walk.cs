using System;

class Solution
{
    public static void Main(string[] args)
    {
        var a = int.Parse(Console.ReadLine());
        var b = int.Parse(Console.ReadLine());
        var m = int.Parse(Console.ReadLine());
        var x = 0;
        var y = 0;
        var d = 0;
        for (var step = 1; step <= 500000; step++)
        {
            d = (int)(((long)a * d + b) % m);
            switch (d % 4)
            {
                case 0: y++; break;  // Up
                case 1: y--; break;  // Down
                case 2: x--; break;  // Left
                default: x++; break; // Right
            }
            if (x == 0 && y == 0)
            {
                Console.WriteLine(step);
                return;
            }
        }
        // If no return within limit, though problem guarantees return ≤500k
        Console.WriteLine("-1");
    }
}
