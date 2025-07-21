using System;

class Solution
{
    static void Main()
    {
        var n = int.Parse(Console.ReadLine()!);
        var rem = long.Parse(Console.ReadLine()!);
        var parts = Console.ReadLine()!.Split(' ');
        var loads = new long[n];
        for (var i = 0; i < n; i++)
            loads[i] = long.Parse(parts[i]);
        Array.Sort(loads);

        var originalMax = loads[n - 1];
        long curMin = loads[0];
        for (int i = 1; i < n; i++)
        {
            var diff = loads[i] - curMin;
            if (diff == 0) continue;
            var need = diff * (long)i;
            if (rem < need)
            {
                curMin += rem / i;
                Console.WriteLine(originalMax - curMin);
                return;
            }
            rem -= need;
            curMin = loads[i];
        }

        Console.WriteLine((rem % n) > 0 ? 1 : 0);
    }
}
