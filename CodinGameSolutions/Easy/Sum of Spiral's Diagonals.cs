using System;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        int[,] spiral = new int[n, n];

        int value = 1;
        int top = 0, bottom = n - 1, left = 0, right = n - 1;

        while (value <= n * n)
        {
            for (int i = left; i <= right; i++) spiral[top, i] = value++;
            top++;
            for (int i = top; i <= bottom; i++) spiral[i, right] = value++;
            right--;
            for (int i = right; i >= left; i--) spiral[bottom, i] = value++;
            bottom--;
            for (int i = bottom; i >= top; i--) spiral[i, left] = value++;
            left++;
        }

        long sum = 0;
        for (int i = 0; i < n; i++)
        {
            sum += spiral[i, i];
            if (i != n - 1 - i)
                sum += spiral[i, n - 1 - i];
        }

        Console.WriteLine(sum);
    }
}
