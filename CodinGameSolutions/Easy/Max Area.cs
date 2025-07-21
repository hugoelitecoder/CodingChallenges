using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var heights = Console.ReadLine()
                             .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                             .Select(int.Parse)
                             .ToArray();

        int left = 0, right = n - 1;
        int maxArea = 0;

        while (left < right)
        {
            int height = Math.Min(heights[left], heights[right]);
            int width = right - left;
            int area = height * width;
            if (area > maxArea)
                maxArea = area;

            // Move pointer at the shorter line inward
            if (heights[left] < heights[right])
                left++;
            else
                right--;
        }

        Console.WriteLine(maxArea);
    }
}
