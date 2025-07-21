using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var reg = Console.ReadLine()
                         .Split(' ')
                         .Select(int.Parse)
                         .OrderByDescending(x => x)
                         .ToArray();
                         
        int goal = int.Parse(Console.ReadLine());
        if (goal == 0)
        {
            Console.WriteLine(0);
            return;
        }

        int n = reg.Length;
        var visited = new bool[goal + 1];
        var parentAmt = new int[goal + 1];
        var parentCoin = new int[goal + 1];

        var q = new Queue<int>();
        visited[goal] = true;
        parentAmt[goal] = -2;
        q.Enqueue(goal);
        while (q.Count > 0)
        {
            int curr = q.Dequeue();
            if (curr == 0) break;
            for (int i = 0; i < n; i++)
            {
                int next = curr - reg[i];
                if (next < 0 || visited[next]) continue;
                visited[next] = true;
                parentAmt[next] = curr;
                parentCoin[next] = i;
                q.Enqueue(next);
                if (next == 0) break;
            }
        }

        if (!visited[0])
        {
            Console.WriteLine("IMPOSSIBLE");
            return;
        }

        var coins = new List<int>();
        int a = 0;
        while (parentAmt[a] != -2)
        {
            int idx = parentCoin[a];
            coins.Add(reg[idx]);
            a = parentAmt[a];
        }

        coins.Sort((x, y) => y.CompareTo(x));
        Console.WriteLine(string.Join(' ', coins));
    }
}
