using System;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine()!);
        int[] counts = new int[10];  // counts[1] through counts[9]

        for (int i = 0; i < N; i++)
        {
            string line = Console.ReadLine()!;
            char? first = null;
            foreach (char ch in line)
            {
                if (ch >= '1' && ch <= '9')
                {
                    first = ch;
                    break;
                }
                else if (ch == '0')
                {
                    // integer zero is skipped; continue scanning
                    continue;
                }
            }
            if (first.HasValue)
                counts[first.Value - '0']++;
        }

        // Benford's expected percentages for digits 1..9
        double[] expected = {
            0.0,
            30.1, 17.6, 12.5,  9.7,  7.9,
             6.7,  5.8,  5.1,  4.6
        };

        bool fraudulent = false;
        for (int d = 1; d <= 9; d++)
        {
            double pct = counts[d] * 100.0 / N;
            double low  = expected[d] - 10.0;
            double high = expected[d] + 10.0;
            if (pct < low || pct > high)
            {
                fraudulent = true;
                break;
            }
        }

        Console.WriteLine(fraudulent ? "true" : "false");
    }
}
