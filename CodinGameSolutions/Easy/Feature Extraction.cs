using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        // Read image dimensions
        var rc = Console.ReadLine()!.Split();
        int r = int.Parse(rc[0]), c = int.Parse(rc[1]);

        // Read image
        var image = new long[r][];
        for (int i = 0; i < r; i++)
            image[i] = Console.ReadLine()!
                         .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                         .Select(long.Parse)
                         .ToArray();

        // Read kernel dimensions
        var mn = Console.ReadLine()!.Split();
        int m = int.Parse(mn[0]), n = int.Parse(mn[1]);

        // Read kernel
        var kernel = new long[m][];
        for (int i = 0; i < m; i++)
            kernel[i] = Console.ReadLine()!
                          .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                          .Select(long.Parse)
                          .ToArray();

        // Output dimensions
        int outR = r - m + 1;
        int outC = c - n + 1;

        // Convolution
        var output = new long[outR][];
        for (int i = 0; i < outR; i++)
        {
            output[i] = new long[outC];
            for (int j = 0; j < outC; j++)
            {
                long sum = 0;
                for (int u = 0; u < m; u++)
                    for (int v = 0; v < n; v++)
                        sum += image[i + u][j + v] * kernel[u][v];
                output[i][j] = sum;
            }
        }

        // Print result
        for (int i = 0; i < outR; i++)
            Console.WriteLine(string.Join(" ", output[i]));
    }
}
