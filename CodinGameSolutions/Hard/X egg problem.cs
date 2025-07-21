using System;

class Solution
{
    public static void Main()
    {
        var n = int.Parse(Console.ReadLine());
        var x = int.Parse(Console.ReadLine());
        if (x == 1)
        {
            Console.WriteLine(n);
            return;
        }
        var dp = new int[x + 1];
        var drops = 0;
        while (dp[x] < n)
        {
            drops++;
            for (var eggs = x; eggs >= 1; eggs--)
            {
                dp[eggs] = dp[eggs] + dp[eggs - 1] + 1;
            }
        }
        Console.WriteLine(drops);
    }
}
