using System;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        string bestTimeStr = null;
        TimeSpan bestTime = TimeSpan.MaxValue;

        for (int i = 0; i < N; i++)
        {
            string t = Console.ReadLine().Trim();
            if (TimeSpan.TryParse(t, out TimeSpan ts))
            {
                if (ts < bestTime)
                {
                    bestTime = ts;
                    bestTimeStr = t;
                }
            }
            else
            {
                var parts = t.Split(':');
                int h = int.Parse(parts[0]);
                int m = int.Parse(parts[1]);
                int s = int.Parse(parts[2]);
                ts = new TimeSpan(h, m, s);
                if (ts < bestTime)
                {
                    bestTime = ts;
                    bestTimeStr = t;
                }
            }
        }

        Console.WriteLine(bestTimeStr);
    }
}