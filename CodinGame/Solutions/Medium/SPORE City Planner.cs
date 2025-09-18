using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine().Trim());
        int L = int.Parse(Console.ReadLine().Trim());
        var edges = new List<(int u,int v)>();
        for(int i=0;i<L;i++){
            var parts = Console.ReadLine().Split().Select(int.Parse).ToArray();
            edges.Add((parts[0], parts[1]));
        }
        
        int maxProd = 0;
        int assignments = (int)Math.Pow(3, N);
        for (int mask = 0; mask < assignments; mask++)
        {
            var type = new int[N + 1];
            type[0] = 0;
            int m = mask;
            for (int i = 1; i <= N; i++)
            {
                type[i] = m % 3;
                m /= 3;
            }
            int happiness = type.Skip(1).Sum(t => t switch { 1 => 1, 2 => -1, _ => 0 });
            int prod = 0;
            foreach (var (u, v) in edges)
            {
                var tu = type[u]; var tv = type[v];
                switch ((tu, tv))
                {
                    case (0, 2): case (2, 0): prod++; break;
                    case (0, 1): case (1, 0): happiness++; break;
                    case (2, 1): case (1, 2): happiness--; break;
                }
            }
            if (happiness >= 0 && prod > maxProd)
                maxProd = prod;
        }
        Console.WriteLine(maxProd);
    }
}
