using System;

class Solution
{
    public static void Main(string[] args)
    {
        var line0 = Console.ReadLine();
        var line1 = Console.ReadLine();
        var line2 = Console.ReadLine();
        var up    = line0[1] - '0';
        var left  = line1[0] - '0';
        var front = line1[1] - '0';
        var right = line1[2] - '0';
        var back  = line1[3] - '0';
        var down  = line2[1] - '0';
        if (up + down != 7 || left + right != 7 || front + back != 7)
        {
            Console.WriteLine("degenerate");
            return;
        }

        var normals = new double[7][];
        normals[up]    = new[]{ 0.0,  1.0,  0.0};
        normals[down]  = new[]{ 0.0, -1.0,  0.0};
        normals[left]  = new[]{-1.0,  0.0,  0.0};
        normals[right] = new[]{ 1.0,  0.0,  0.0};
        normals[front] = new[]{ 0.0,  0.0,  1.0};
        normals[back]  = new[]{ 0.0,  0.0, -1.0};

        var n1 = normals[1];
        var n2 = normals[2];
        var n3 = normals[3];
        var v  = new[]{ n1[0] + n2[0] + n3[0],  n1[1] + n2[1] + n3[1],  n1[2] + n2[2] + n3[2] };
        var vv = Dot(v, v);
        var p1 = Subtract(n1, Multiply(v, Dot(n1, v) / vv));
        var p2 = Subtract(n2, Multiply(v, Dot(n2, v) / vv));
        var p3 = Subtract(n3, Multiply(v, Dot(n3, v) / vv));
        var orientation = Dot(v, Cross(Subtract(p2, p1), Subtract(p3, p1)));

        Console.WriteLine(orientation > 0 ? "right-handed" : "left-handed");
    }

    private static double Dot(double[] a, double[] b)
    {
        return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
    }
    private static double[] Cross(double[] a, double[] b)
    {
        return new[]
        {
            a[1] * b[2] - a[2] * b[1],
            a[2] * b[0] - a[0] * b[2],
            a[0] * b[1] - a[1] * b[0]
        };
    }
    private static double[] Subtract(double[] a, double[] b)
    {
        return new[]{ a[0] - b[0], a[1] - b[1], a[2] - b[2] };
    }
    private static double[] Multiply(double[] a, double scalar)
    {
        return new[]{ a[0] * scalar, a[1] * scalar, a[2] * scalar };
    }
}
