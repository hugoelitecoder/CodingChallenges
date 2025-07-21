using System;

class Solution
{
    static void Main()
    {
        int r1 = int.Parse(Console.ReadLine()!);
        int digits = r1.ToString().Length;
        int maxDelta = 9 * digits;
        int start = Math.Max(1, r1 - maxDelta);

        bool meets = false;
        for (int n = start; n < r1; n++)
        {
            int sum = n;
            int t = n;
            while (t > 0)
            {
                sum += t % 10;
                t /= 10;
            }
            if (sum == r1)
            {
                meets = true;
                break;
            }
        }

        Console.WriteLine(meets ? "YES" : "NO");
    }
}
