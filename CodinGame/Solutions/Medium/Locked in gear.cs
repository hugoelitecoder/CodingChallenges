using System;
using System.Collections.Generic;
using System.Linq;

class Solution {
    
    static void Main() {
        int n = int.Parse(Console.ReadLine());
        var x = new int[n];
        var y = new int[n];
        var r = new int[n];
        for (int i = 0; i < n; i++) {
            var p = Console.ReadLine().Split();
            x[i] = int.Parse(p[0]);
            y[i] = int.Parse(p[1]);
            r[i] = int.Parse(p[2]);
        }

        var adj = Enumerable.Range(0, n).Select(_ => new List<int>()).ToArray();
        for (int i = 0; i < n; i++)
        for (int j = i+1; j < n; j++) {
            long dx = x[i]-x[j], dy = y[i]-y[j], sum = r[i]+r[j];
            if (dx*dx + dy*dy == sum*sum) {
                adj[i].Add(j);
                adj[j].Add(i);
            }
        }

        var depth = Enumerable.Repeat(-1, n).ToArray();
        var q = new Queue<int>();
        depth[0] = 0;
        q.Enqueue(0);
        while (q.Count > 0) {
            var u = q.Dequeue();
            foreach (var v in adj[u]) {
                if (depth[v] == -1) {
                    depth[v] = depth[u] + 1;
                    q.Enqueue(v);
                }
            }
        }

        if (depth[n-1] == -1) {
            Console.WriteLine("NOT MOVING");
            return;
        }

        for (int i = 0; i < n; i++) {
            if (depth[i] < 0) continue;
            var nb = adj[i].Where(v => depth[v] >= 0).ToArray();
            if (nb.Length == 2) {
                int a = nb[0], b = nb[1];
                long cross = (long)(x[a]-x[i]) * (y[b]-y[i])
                           - (long)(y[a]-y[i]) * (x[b]-x[i]);
                if (cross != 0) {
                    Console.WriteLine("NOT MOVING");
                    return;
                }
            }
        }

        Console.WriteLine(depth[n-1] % 2 == 0 ? "CW" : "CCW");
    }
}
