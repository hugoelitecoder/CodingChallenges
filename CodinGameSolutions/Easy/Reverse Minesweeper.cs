using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int w = int.Parse(Console.ReadLine());
        int h = int.Parse(Console.ReadLine());
        var grid = new char[h, w];

        for (int i = 0; i < h; i++)
        {
            var line = Console.ReadLine();
            for (int j = 0; j < w; j++)
                grid[i, j] = line[j];
        }

        for (int i = 0; i < h; i++)
        {
            string resultLine = "";
            for (int j = 0; j < w; j++)
            {
                if (grid[i, j] == 'x')
                {
                    resultLine += '.';
                    continue;
                }
                int count = 0;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dy == 0 && dx == 0) continue;

                        int ni = i + dy;
                        int nj = j + dx;

                        if (ni >= 0 && ni < h && nj >= 0 && nj < w && grid[ni, nj] == 'x')
                            count++;
                    }
                }

                resultLine += count == 0 ? '.' : count.ToString();
            }
            Console.WriteLine(resultLine);
        }
    }
}
