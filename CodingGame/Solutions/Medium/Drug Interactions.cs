using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var drugs = new string[n];
        for (int i = 0; i < n; i++)
            drugs[i] = Console.ReadLine().Trim().ToLower();

        var solver = new DrugInteractionSolver(drugs);
        Console.WriteLine(solver.DFS());
    }
}

class DrugInteractionSolver
{
    private readonly int _size;
    private readonly List<int>[] _compat;
    private int _maxSet;

    public DrugInteractionSolver(string[] drugs)
    {
        _size = drugs.Length;
        var counts = new int[_size][];
        for (int i = 0; i < _size; i++)
        {
            var cnt = new int[26];
            foreach (char c in drugs[i])
                if (c >= 'a' && c <= 'z') cnt[c - 'a']++;
            counts[i] = cnt;
        }

        _compat = new List<int>[_size];
        for (int i = 0; i < _size; i++)
        {
            _compat[i] = new List<int>();
            for (int j = 0; j < _size; j++)
            {
                if (i == j) continue;
                int common = 0;
                for (int k = 0; k < 26 && common < 3; k++)
                    common += Math.Min(counts[i][k], counts[j][k]);
                if (common < 3)
                    _compat[i].Add(j);
            }
        }
    }

    public int DFS(List<int> path = null, List<int> pool = null)
    {
        if (path == null)
        {
            _maxSet = 0;
            for (int i = 0; i < _size; i++)
            {
                var p = new List<int> { i };
                var pl = new List<int>(_compat[i]);
                DFS(p, pl);
            }
            return _maxSet;
        }

        int size = path.Count;
        if (pool.Count == 0 || size + pool.Count <= _maxSet)
        {
            if (size > _maxSet) _maxSet = size;
            return _maxSet;
        }

        var candidates = new List<int>(pool);
        foreach (int v in candidates)
        {
            path.Add(v);
            var next = new List<int>();
            foreach (int u in pool)
                if (_compat[v].Contains(u)) next.Add(u);

            DFS(path, next);
            path.RemoveAt(path.Count - 1);
            pool.Remove(v);
        }

        return _maxSet;
    }
}