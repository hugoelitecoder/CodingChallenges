using System;

class Solution
{
    static void Main()
    {
        var parts = Console.ReadLine().Split(' ');
        int startX = int.Parse(parts[0]);
        int startY = int.Parse(parts[1]);
        int beachY = int.Parse(Console.ReadLine());
        parts = Console.ReadLine().Split(' ');
        int ballX = int.Parse(parts[0]);
        int ballY = int.Parse(parts[1]);
        int speedLand = int.Parse(Console.ReadLine());
        int speedWater = int.Parse(Console.ReadLine());

        int left = Math.Min(startX, ballX);
        int right = Math.Max(startX, ballX);
        int bestX = left;
        double bestTime = TimeFor(bestX, startX, startY, beachY, ballX, ballY, speedLand, speedWater);

        while (right - left > 3)
        {
            int m1 = left + (right - left) / 3;
            int m2 = right - (right - left) / 3;
            double t1 = TimeFor(m1, startX, startY, beachY, ballX, ballY, speedLand, speedWater);
            double t2 = TimeFor(m2, startX, startY, beachY, ballX, ballY, speedLand, speedWater);
            if (t1 < t2)
                right = m2 - 1;
            else
                left = m1 + 1;
        }
        for (int x = left; x <= right; x++)
        {
            double t = TimeFor(x, startX, startY, beachY, ballX, ballY, speedLand, speedWater);
            if (t < bestTime)
            {
                bestTime = t;
                bestX = x;
            }
        }

        Console.WriteLine(bestX);
    }

    static double TimeFor(int beachX,
        int sx, int sy, int by,
        int bx, int by0,
        int vLand, int vWater)
    {
        double landDist = Math.Sqrt((sx - beachX) * (double)(sx - beachX) + (sy - by) * (double)(sy - by));
        double waterDist = Math.Sqrt((bx - beachX) * (double)(bx - beachX) + (by0 - by) * (double)(by0 - by));
        return landDist / vLand + waterDist / vWater;
    }
}