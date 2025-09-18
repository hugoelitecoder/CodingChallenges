using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);

        int[] heights = new int[width];
        int maxArea = 0;

        for (int i = 0; i < height; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            for (int j = 0; j < width; j++)
            {
                int cell = int.Parse(inputs[j]);
                heights[j] = (cell == 1) ? heights[j] + 1 : 0;
            }

            maxArea = Math.Max(maxArea, LargestRectangleArea(heights));
        }

        Console.WriteLine(maxArea);
    }

    static int LargestRectangleArea(int[] heights)
    {
        int n = heights.Length;
        var stack = new Stack<int>();
        int maxArea = 0;
        for (int i = 0; i <= n; i++)
        {
            int currHeight = (i == n ? 0 : heights[i]);
            while (stack.Count > 0 && currHeight < heights[stack.Peek()])
            {
                int h = heights[stack.Pop()];
                int width = stack.Count == 0 ? i : i - stack.Peek() - 1;
                maxArea = Math.Max(maxArea, h * width);
            }
            stack.Push(i);
        }

        return maxArea;
    }
}
