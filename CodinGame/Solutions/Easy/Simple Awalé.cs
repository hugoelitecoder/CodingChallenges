using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var op = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
        var my = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
        int num = int.Parse(Console.ReadLine());
        int hand = my[num];
        my[num] = 0;

        var cycle = new List<(bool isMy, int idx)>();
        for (int i = 0; i < 7; i++) cycle.Add((true, i));
        for (int i = 0; i < 6; i++) cycle.Add((false, i));

        int start = cycle.FindIndex(t => t.isMy && t.idx == num);
        int pos = start;
        int lastPos = -1;
        while (hand > 0)
        {
            pos = (pos + 1) % cycle.Count;
            var (isMy, idx) = cycle[pos];
            if (isMy)
                my[idx]++;
            else
                op[idx]++;
            hand--;
            lastPos = pos;
        }

        Console.Write(string.Join(" ", op.Take(6)));
        Console.Write(" [" + op[6] + "]\n");
        Console.Write(string.Join(" ", my.Take(6)));
        Console.Write(" [" + my[6] + "]\n");

        var last = cycle[lastPos];
        if (last.isMy && last.idx == 6)
            Console.WriteLine("REPLAY");
    }
}
