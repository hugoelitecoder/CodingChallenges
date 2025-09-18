using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        var partition = new List<int>();
        PrintPartitions(n, n, partition);
    }

    static void PrintPartitions(int remain, int maxNext, List<int> part)
    {
        if (remain == 0)
        {
            Console.WriteLine(string.Join(" ", part));
            return;
        }

        for (int next = Math.Min(remain, maxNext); next >= 1; next--)
        {
            part.Add(next);
            PrintPartitions(remain - next, next, part);
            part.RemoveAt(part.Count - 1);
        }
    }
}
