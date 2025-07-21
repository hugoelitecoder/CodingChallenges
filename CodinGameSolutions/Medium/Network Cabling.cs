using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        List<Point> houses = new List<Point>();
        for (int i = 0; i < n; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int hx = int.Parse(inputs[0]);
            int hy = int.Parse(inputs[1]);
            houses.Add(new Point(hx, hy));
        }
        
        houses.Sort((a, b) => a.Y.CompareTo(b.Y));
        int y = houses[n/2].Y;
        long result = houses.Max(h => h.X) - houses.Min(h => h.X);
        foreach (Point h in houses) result += Math.Abs(h.Y - y);
       
        Console.WriteLine(result);
    }
}