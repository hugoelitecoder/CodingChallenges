using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var dominoes = new List<(int a, int b)>();
        for (int i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split();
            dominoes.Add((int.Parse(parts[0]), int.Parse(parts[1])));
        }
        var checker = new DominoPath(dominoes);
        Console.WriteLine(checker.CanMakePath().ToString().ToLower());
    }
}

class DominoPath
{
    private readonly int[] degree = new int[7];
    private readonly List<int>[] adjacency = new List<int>[7];

    public DominoPath(IEnumerable<(int a, int b)> dominoes)
    {
        for (int i = 0; i < adjacency.Length; i++)
            adjacency[i] = new List<int>();
        foreach (var (a, b) in dominoes)
        {
            degree[a]++;
            degree[b]++;
            adjacency[a].Add(b);
            adjacency[b].Add(a);
        }
    }

    public bool CanMakePath()
    {
        int start = Array.FindIndex(degree, d => d > 0);
        if (start < 0) return false;
        return IsConnected(start) && HasValidDegree();
    }

    private bool IsConnected(int start)
    {
        var seen = new bool[7];
        var stack = new Stack<int>();
        seen[start] = true;
        stack.Push(start);
        while (stack.Count > 0)
        {
            int v = stack.Pop();
            foreach (var w in adjacency[v])
                if (!seen[w])
                {
                    seen[w] = true;
                    stack.Push(w);
                }
        }
        for (int i = 0; i < degree.Length; i++)
            if (degree[i] > 0 && !seen[i])
                return false;
        return true;
    }

    private bool HasValidDegree()
    {
        int oddCount = 0;
        foreach (var d in degree)
            if ((d & 1) == 1)
                oddCount++;
        return oddCount == 0 || oddCount == 2;
    }
}
