using System;

class Solution
{
    public static void Main()
    {
        var parts = Console.ReadLine().Split();
        var W = int.Parse(parts[0]);
        var H = int.Parse(parts[1]);
        double area = 0;
        for (var i = 0; i < H; i++)
        {
            var row = Console.ReadLine();
            foreach (var c in row)
            {
                if (c == 'X') area += 1.0;
                else if (c != '.') area += 0.5;
            }
        }
        var ratio = area / W / H;
        Console.WriteLine(ratio > 0.8927
            ? "Rectangle"
            : (ratio > 0.6427 ? "Ellipse" : "Triangle"));
    }
}
