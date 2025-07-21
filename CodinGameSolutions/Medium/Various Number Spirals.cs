using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        var pos = Console.ReadLine().Split();
        string vertPos = pos[0];
        string horPos = pos[1];
        var od = Console.ReadLine().Split();
        string order = od[0];
        string direction = od[1];

        int[,] spiral = new int[n, n];
        var numbers = GetNumbers(n, order);
        var enumerator = numbers.GetEnumerator();
        spiral[n/2, n/2] = numbers.Last();

        FillSpiral(n, vertPos, horPos, enumerator, direction, spiral);

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (j > 0) Console.Write('\t');
                Console.Write(spiral[i, j]);
            }
            Console.WriteLine();
        }
    }

    static List<int> GetNumbers(int n, string order)
    {
        return order == "+"
            ? Enumerable.Range(1, n * n).ToList()
            : Enumerable.Range(1, n * n).Select(i => n * n - (i - 1)).ToList();
    }

    static int[][] GetDirections(string direction)
    {
        return direction == "c"
            ? new[] { new[] {0, 1}, new[] {1, 0}, new[] {0, -1}, new[] {-1, 0} }
            : new[] { new[] {0, 1}, new[] {-1, 0}, new[] {0, -1}, new[] {1, 0} };
    }

    static string[] GetCorners(string direction)
    {
        var corners = new[] {"tl", "tr", "br", "bl"};
        if (direction == "cc") Array.Reverse(corners);
        return corners;
    }

    static void FillSpiral(int n, string vertPos, string horPos, IEnumerator<int> enumerator, string direction, int[,] spiral)
    {
        int h = horPos == "r" ? n - 1 : 0;
        int v = vertPos == "b" ? n - 1 : 0;
        var dirs = GetDirections(direction);
        var corners = GetCorners(direction);
        int curDir = Array.IndexOf(corners, vertPos + horPos);

        int x = v, y = h;
        for (int layer = 1; layer <= n; layer += 2)
        {
            for (int side = 0; side < 4; side++)
            {
                int steps = n - layer;
                for (int i = 0; i < steps; i++)
                {
                    enumerator.MoveNext();
                    spiral[x, y] = enumerator.Current;
                    x += dirs[curDir % 4][0];
                    y += dirs[curDir % 4][1];
                }
                curDir++;
            }
            x += vertPos == "b" ? -1 : 1;
            y += horPos  == "r" ? -1 : 1;
        }
    }
}