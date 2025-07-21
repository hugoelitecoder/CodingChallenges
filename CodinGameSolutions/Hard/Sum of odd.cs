using System;
class Solution
{
    public static void Main(string[] args)
    {
        var n = long.Parse(Console.ReadLine());
        var count = 0L;
        long longestLen = 0;
        long firstLongest = 0;
        long lastLongest = 0;
        for (long len = 2; ; len++)
        {
            if (n < len * len) break;
            if (n % len != 0) continue;
            var a = n / len - len + 1;
            if (a <= 0) continue;
            if (a % 2 == 1)
            {
                count++;
                if (len > longestLen)
                {
                    longestLen = len;
                    firstLongest = a;
                    lastLongest = a + 2 * (len - 1);
                }
            }
        }
        if (count > 0)
        {
            Console.WriteLine(count);
            Console.WriteLine($"{firstLongest} {lastLongest}");
        }
        else
        {
            Console.WriteLine(0);
        }
    }
}
