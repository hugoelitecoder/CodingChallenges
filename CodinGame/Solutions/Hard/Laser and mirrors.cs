using System;

class Solution
{
    public static void Main(string[] args)
    {
        var line = Console.ReadLine();
        var parts = line.Split(' ');
        var uIn = int.Parse(parts[0]);
        var vIn = int.Parse(parts[1]);
        var result = CalculateLaserPath(uIn, vIn);
        Console.WriteLine(result.Corner + " " + result.Length);
    }

    private static (char Corner, long Length) CalculateLaserPath(int uVal, int vVal)
    {
        var U = (long)uVal;
        var V = (long)vVal;
        var commonDivisor = GCD(U, V);
        var uRatio = U / commonDivisor;
        var vRatio = V / commonDivisor;
        var length = uRatio * V;
        var isURatioOdd = (uRatio % 2 == 1);
        var isVRatioOdd = (vRatio % 2 == 1);
        char corner;
        if (isVRatioOdd)
        {
            if (isURatioOdd)
            {
                corner = 'B';
            }
            else
            {
                corner = 'A';
            }
        }
        else
        {
            corner = 'C';
        }
        return (corner, length);
    }

    private static long GCD(long a, long b)
    {
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}