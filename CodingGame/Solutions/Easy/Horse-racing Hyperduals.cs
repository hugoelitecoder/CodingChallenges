using System;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var V = new int[N];
        var E = new int[N];
        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            V[i] = int.Parse(parts[0]);
            E[i] = int.Parse(parts[1]);
        }

        int best = int.MaxValue;
        for (int i = 0; i < N; i++)
        {
            for (int j = i + 1; j < N; j++)
            {
                int d = Math.Abs(V[i] - V[j]) + Math.Abs(E[i] - E[j]);
                if (d < best)
                {
                    best = d;
                    if (best == 0)
                    {
                        Console.WriteLine(0);
                        return;
                    }
                }
            }
        }

        Console.WriteLine(best);
    }
}
