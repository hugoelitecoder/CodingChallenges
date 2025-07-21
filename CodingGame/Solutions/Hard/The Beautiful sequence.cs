using System;
class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
        var max = 0;
        for (int i = 0; i < n; i++)
        {
            var min = arr[i];
            for (int j = i; j < n; j++)
            {
                if (arr[j] < min) min = arr[j];
                var val = min * (j - i + 1);
                if (val > max) max = val;
            }
        }
        Console.WriteLine(max);
    }
}
