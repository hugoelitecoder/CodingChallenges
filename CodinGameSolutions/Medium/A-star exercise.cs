using System;
using System.Linq;
using System.Collections.Generic;

class Solution {
    class Node : IComparable<Node> {
        public int ID, H, G = int.MaxValue;
        public int F => G + H;
        public bool Visited;
        public List<(Node to, int cost)> Neigh = new List<(Node, int)>();
        public int CompareTo(Node o) => F != o.F ? F - o.F : ID - o.ID;
    }

    static void Main() {
        var hdr = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int N = hdr[0], E = hdr[1], S = hdr[2], G = hdr[3];

        var nodes = Enumerable.Range(0, N)
                              .Select(i => new Node { ID = i })
                              .ToArray();

        var hs = Console.ReadLine().Split().Select(int.Parse).ToArray();
        for (int i = 0; i < N; i++) nodes[i].H = hs[i];

        for (int i = 0; i < E; i++) {
            var e = Console.ReadLine().Split().Select(int.Parse).ToArray();
            var u = nodes[e[0]]; var v = nodes[e[1]]; int c = e[2];
            u.Neigh.Add((v, c));
            v.Neigh.Add((u, c));
        }

        var open = new SortedSet<Node>();
        nodes[S].G = 0;
        open.Add(nodes[S]);

        while (open.Count > 0) {
            var cur = open.Min;
            open.Remove(cur);
            if (cur.Visited) continue;

            Console.WriteLine($"{cur.ID} {cur.F}");
            if (cur.ID == G) break;

            cur.Visited = true;
            foreach (var (nx, cost) in cur.Neigh) {
                if (nx.Visited) continue;
                int ng = cur.G + cost;
                if (ng < nx.G) {
                    if (open.Contains(nx)) open.Remove(nx);
                    nx.G = ng;
                    open.Add(nx);
                }
            }
        }
    }
}
