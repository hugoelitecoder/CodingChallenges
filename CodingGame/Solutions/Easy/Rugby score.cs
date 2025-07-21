using System;
class Solution
{
    public static void Main(string[] args)
    {
        var N = int.Parse(Console.ReadLine());
        for (var tries = 0; tries * 5 <= N; tries++)
        {
            for (var trans = 0; trans <= tries; trans++)
            {
                var rem = N - tries * 5 - trans * 2;
                if (rem < 0) break;
                if (rem % 3 != 0) continue;
                var pens = rem / 3;
                Console.WriteLine($"{tries} {trans} {pens}");
            }
        }
    }
}
