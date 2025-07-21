using System;
class Solution
{
    public static void Main(string[] args)
    {
        var tokens = Console.ReadLine().Split(' ');
        var n = long.Parse(tokens[0]);
        var s = long.Parse(tokens[1]);
        var dir = Console.ReadLine().Trim();
        var josephus = Josephus(n);
        long winner;
        if (dir == "LEFT")
        {
            winner = (s - 1 + josephus) % n + 1;
        }
        else
        {
            winner = (s - 1 - josephus + n) % n + 1;
        }
        Console.WriteLine(winner);
    }
    
    static long Josephus(long n)
    {
        var l = 1L << (63 - LeadingZeroCount(n));
        return 2 * (n - l);
    }
    static int LeadingZeroCount(long n)
    {
        int count = 0;
        for (int i = 63; i >= 0; i--)
            if (((n >> i) & 1) == 0)
                count++;
            else
                break;
        return count;
    }
}
