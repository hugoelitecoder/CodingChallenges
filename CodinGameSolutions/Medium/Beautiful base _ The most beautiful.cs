using System;

class Solution
{
    static void Main(string[] args)
    {
        var nStr = Console.ReadLine();
        var n = long.Parse(nStr);

        var finder = new BaseFinder();
        var beautifulBase = finder.FindMostBeautifulBase(n);
        
        if (beautifulBase.HasValue)
        {
            Console.WriteLine(beautifulBase.Value);
        }
        else
        {
            Console.WriteLine("NONE");
        }
    }
}

public class BaseFinder
{
    public long? FindMostBeautifulBase(long n)
    {
        if (n <= 3)
        {
            return null;
        }

        for (var k = 60; k >= 2; k--)
        {
            var low = 2L;
            var high = (long)Math.Pow(n, 1.0 / k) + 2;

            if (high < low)
            {
                continue;
            }

            while (low <= high)
            {
                var mid = low + (high - low) / 2;
                if (mid <= 1)
                {
                    low = 2;
                    continue;
                }

                var check = PowerCheck(mid, k, n);
                if (check == 0)
                {
                    return mid;
                }
                else if (check < 0)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }
        }
        return null;
    }

    private int PowerCheck(long b, int k, long n)
    {
        var val = 1L;
        for (var i = 0; i < k; i++)
        {
            if (val > n / b)
            {
                return 1;
            }
            val *= b;
        }

        if (val == n)
        {
            return 0;
        }
        
        return -1;
    }
}

