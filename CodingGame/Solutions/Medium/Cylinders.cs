using System;
using System.Linq;

class Solution
{
    static int m;
    static double[] R;
    static bool[] used;
    static double[] centers;
    static double[] placedR;
    static double bestWidth;

    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        for (int tc = 0; tc < N; tc++)
        {
            var parts = Console.ReadLine().Split().Select(int.Parse).ToArray();
            m = parts[0];
            R = parts.Skip(1).Select(r => (double)r).ToArray();

            Array.Sort(R);
            Array.Reverse(R);

            used = new bool[m];
            centers = new double[m];
            placedR = new double[m];
            bestWidth = double.PositiveInfinity;

            Pack(0, 0.0);

            Console.WriteLine(bestWidth.ToString("F3"));
        }
    }

    static void Pack(int depth, double currentWidth)
    {
        if (depth == m)
        {
            bestWidth = Math.Min(bestWidth, currentWidth);
            return;
        }

        for (int i = 0; i < m; i++)
        {
            if (used[i]) continue;
            used[i] = true;
            double r = R[i];
            placedR[depth] = r;
            double c = r;
            for (int j = 0; j < depth; j++)
            {
                double sep = 2.0 * Math.Sqrt(placedR[j] * r);
                c = Math.Max(c, centers[j] + sep);
            }

            centers[depth] = c;
            double newWidth = Math.Max(currentWidth, c + r);
            if (newWidth < bestWidth)
                Pack(depth + 1, newWidth);

            used[i] = false;
        }
    }
}
