using System;

class Solution
{
    static void Main()
    {
        // Read n, m, c
        var parts = Console.ReadLine()!.Split();
        int n = int.Parse(parts[0]);
        int m = int.Parse(parts[1]);
        int c = int.Parse(parts[2]);

        // Read consumption values
        var cons = new int[n];
        parts = Console.ReadLine()!.Split();
        for (int i = 0; i < n; i++)
            cons[i] = int.Parse(parts[i]);

        // Read sequence of clicks
        parts = Console.ReadLine()!.Split();
        var ops = new int[m];
        for (int i = 0; i < m; i++)
            ops[i] = int.Parse(parts[i]) - 1;  // zero‑based

        // Track state and consumption
        var on = new bool[n];
        int current = 0, maxSeen = 0;

        // Process each click
        foreach (var idx in ops)
        {
            if (!on[idx])
            {
                // turn on
                on[idx] = true;
                current += cons[idx];
                if (current > c)
                {
                    Console.WriteLine("Fuse was blown.");
                    return;
                }
                if (current > maxSeen)
                    maxSeen = current;
            }
            else
            {
                // turn off
                on[idx] = false;
                current -= cons[idx];
            }
        }

        // If we reach here, fuse never blew
        Console.WriteLine("Fuse was not blown.");
        Console.WriteLine($"Maximal consumed current was {maxSeen} A.");
    }
}
