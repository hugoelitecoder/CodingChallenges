using System;

class Solution
{
    static void Main()
    {
        string b = Console.ReadLine();
        int max = 0, prev = 0, curr = 0;
        int count = 0;

        for (int i = 0; i < b.Length; i++)
        {
            if (b[i] == '1')
            {
                curr++;
                count++;
            }
            else
            {
                max = Math.Max(max, prev + 1 + curr);
                prev = curr;
                curr = 0;
            }
        }
        max = Math.Max(max, prev + 1 + curr);
        max = Math.Min(max, b.Length);

        Console.WriteLine(max);
    }
}
