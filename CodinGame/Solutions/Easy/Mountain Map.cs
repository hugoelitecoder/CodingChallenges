using System;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine()); // the number of mountains
        int[] heights = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();

        int maxHeight = heights.Max();
        int totalWidth = heights.Sum(h => h * 2);
        char[][] output = new char[maxHeight][];

        for (int i = 0; i < maxHeight; i++)
        {
            output[i] = new char[totalWidth];
            for (int j = 0; j < totalWidth; j++)
            {
                output[i][j] = ' ';
            }
        }

        int currentPos = 0;
        foreach (int height in heights)
        {
            for (int i = 0; i < height; i++)
            {
                output[maxHeight - 1 - i][currentPos + i] = '/';
            }
            for (int i = 0; i < height; i++)
            {
                output[maxHeight - height + i][currentPos + height + i] = '\\';
            }
            currentPos += height * 2;
        }

        foreach (var line in output)
        {
            Console.WriteLine(new string(line).TrimEnd());
        }
    }
}
