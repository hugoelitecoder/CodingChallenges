using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        var a = Console.ReadLine().Split().Select(int.Parse).ToArray();
        var b = Console.ReadLine().Split().Select(int.Parse).ToArray();

        int x = a[0], y = a[1], u = b[0], v = b[1];

        int dx = Math.Min((x - u + 200) % 200, (u - x + 200) % 200);
        int dy = Math.Min((y - v + 150) % 150, (v - y + 150) % 150);

        int diagonal = Math.Min(dx, dy);
        int straightX = dx - diagonal;
        int straightY = dy - diagonal;

        double time = diagonal * 0.5 + straightX * 0.3 + straightY * 0.4;
        Console.WriteLine($"{time:0.0}");
    }
}
