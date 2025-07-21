using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int L = int.Parse(Console.ReadLine() ?? "0");
        string rooms = Console.ReadLine() ?? string.Empty;
        
        var parts = (Console.ReadLine() ?? string.Empty).Split(' ');
        int N = int.Parse(parts[0]);
        int K = int.Parse(parts[1]);

        int Mindist(char c1, char c2)
        {
            int o1 = rooms.IndexOf(c1);
            int o2 = rooms.IndexOf(c2);
            int diff = Math.Abs(o1 - o2);
            return Math.Min(diff, L - diff);
        }

        for (int i = 0; i < N; i++)
        {
            string raw = Console.ReadLine() ?? string.Empty;
            string path = raw.Trim('#');
            if (path.Length < 2)
            {
                Console.WriteLine("NOT SUS");
                continue;
            }

            char hold = path[0];
            int dist = 0;
            bool sus = false;
            for (int j = 1; j < path.Length; j++)
            {
                dist++;
                char c = path[j];
                if (c == '#')
                    continue;
                if (Mindist(c, hold) > dist)
                {
                    Console.WriteLine("SUS");
                    sus = true;
                    break;
                }
                hold = c;
                dist = 0;
            }

            if (!sus)
                Console.WriteLine("NOT SUS");
        }
    }
}