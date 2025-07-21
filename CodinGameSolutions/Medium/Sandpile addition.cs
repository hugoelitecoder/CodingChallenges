using System;
using System.Collections.Generic;
using System.Text;

class Solution
{
    static void Main()
    {
        int size = int.Parse(Console.ReadLine());
        const int T = 4;
        int[] g = new int[size * size];
        for (int pass = 0; pass < 2; pass++)
            for (int r = 0; r < size; r++)
            {
                string line = Console.ReadLine();
                for (int c = 0; c < size; c++)
                    g[r * size + c] += line[c] - '0';
            }

        var dirs = new[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
        var q = new Queue<int>();
        for (int i = 0; i < g.Length; i++)
            if (g[i] >= T)
                q.Enqueue(i);

        while (q.Count > 0)
        {
            int i = q.Dequeue();
            while (g[i] >= T)
            {
                g[i] -= T;
                int r = i / size, c = i % size;
                foreach (var (dr, dc) in dirs)
                {
                    int nr = r + dr, nc = c + dc;
                    if (nr >= 0 && nr < size && nc >= 0 && nc < size)
                    {
                        int j = nr * size + nc;
                        g[j]++;
                        if (g[j] == T)
                            q.Enqueue(j);
                    }
                }
            }
        }

        var sb = new StringBuilder();
        for (int r = 0; r < size; r++)
        {
            sb.Clear();
            for (int c = 0; c < size; c++)
                sb.Append(g[r * size + c]);
            Console.WriteLine(sb);
        }
    }
}