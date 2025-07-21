using System;

class Solution
{
    static void Main()
    {
        var frogs = new (Point2D Pos, int K)[3];
        for (int i = 0; i < 3; i++)
        {
            var p = Console.ReadLine().Split();
            frogs[i] = (
                Pos: new Point2D(int.Parse(p[0]), int.Parse(p[1])),
                K:   int.Parse(p[2])
            );
        }

        bool possible = true;
        for (int i = 0; i < 3 && possible; i++)
        for (int j = i + 1; j < 3; j++)
        {
            var d = frogs[i].Pos - frogs[j].Pos;
            int g = Gcd(frogs[i].K, frogs[j].K);
            if (d.X % g != 0 || d.Y % g != 0)
            {
                possible = false;
                break;
            }
        }

        Console.WriteLine(possible ? "Possible" : "Impossible");
    }

    static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            int t = b;
            b = a % b;
            a = t;
        }
        return Math.Abs(a);
    }

    struct Point2D
    {
        public int X, Y;
        public Point2D(int x, int y) => (X, Y) = (x, y);
        public static Point2D operator -(Point2D a, Point2D b)
            => new Point2D(a.X - b.X, a.Y - b.Y);
    }
}
