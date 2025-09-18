using System;
using System.Collections.Generic;
using System.Linq;

class Solution {

    static void Main() {
        var nPeople = int.Parse(Console.ReadLine());
        var nLinks = int.Parse(Console.ReadLine());
        var uf = new UnionFind(nPeople);

        Enumerable.Range(0, nLinks).ToList().ForEach(_ => {
            var t = Console.ReadLine().Split();
            uf.Union(int.Parse(t[0]), int.Parse(t[1]));
        });

        var distribution = Enumerable.Range(0, nPeople)
            .GroupBy(i => uf.Find(i))
            .Select(g => g.Count())
            .GroupBy(size => size)
            .OrderByDescending(g => g.Key);

        foreach (var grp in distribution)
            Console.WriteLine($"{grp.Key} {grp.Count()}");
    }
}

public class UnionFind {
    private readonly int[] _parent;
    private readonly int[] _rank;

    public UnionFind(int n) {
        _parent = Enumerable.Range(0, n).ToArray();
        _rank = new int[n];
    }

    public int Find(int x) {
        if (_parent[x] != x)
            _parent[x] = Find(_parent[x]);
        return _parent[x];
    }

    public void Union(int x, int y) {
        int rx = Find(x);
        int ry = Find(y);
        if (rx == ry) return;
        if (_rank[rx] < _rank[ry]) {
            _parent[rx] = ry;
        } else if (_rank[ry] < _rank[rx]) {
            _parent[ry] = rx;
        } else {
            _parent[ry] = rx;
            _rank[rx]++;
        }
    }
}
