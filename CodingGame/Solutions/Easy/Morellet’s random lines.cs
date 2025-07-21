using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var parts = Console.ReadLine().Split();
        int xA = int.Parse(parts[0]), yA = int.Parse(parts[1]);
        int xB = int.Parse(parts[2]), yB = int.Parse(parts[3]);

        int n = int.Parse(Console.ReadLine());
        var uniqueLines = new HashSet<(int a, int b, int c)>();
        for (int i = 0; i < n; i++)
        {
            parts = Console.ReadLine().Split();
            int a = int.Parse(parts[0]);
            int b = int.Parse(parts[1]);
            int c = int.Parse(parts[2]);

            int g = Gcd(Gcd(Math.Abs(a), Math.Abs(b)), Math.Abs(c));
            if (g > 0)
            {
                a /= g;
                b /= g;
                c /= g;
            }
            if (a < 0 || (a == 0 && b < 0))
            {
                a = -a;
                b = -b;
                c = -c;
            }

            uniqueLines.Add((a, b, c));
        }

        bool onLine = false;
        int crossings = 0;

        foreach (var (a, b, c) in uniqueLines)
        {
            long vA = (long)a * xA + (long)b * yA + c;
            long vB = (long)a * xB + (long)b * yB + c;
            if (vA == 0 || vB == 0)
            {
                onLine = true;
                break;
            }
            if ((vA < 0 && vB > 0) || (vA > 0 && vB < 0))
                crossings++;
        }

        if (onLine)
        {
            Console.WriteLine("ON A LINE");
        }
        else
        {
            Console.WriteLine(crossings % 2 == 0 ? "YES" : "NO");
        }
    }

    static int Gcd(int x, int y)
    {
        while (y != 0)
        {
            int t = x % y;
            x = y;
            y = t;
        }
        return x;
    }
}
