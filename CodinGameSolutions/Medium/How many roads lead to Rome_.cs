using System;
using System.Collections.Generic;

class Solution
{
    static int target = 100;
    static Dictionary<int, List<int>> graph = new Dictionary<int, List<int>>();
    static bool[] visited = new bool[101];
    static long pathCount = 0;

    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split();
            int A = int.Parse(parts[0]);
            int B = int.Parse(parts[1]);
            if (!graph.ContainsKey(A)) graph[A] = new List<int>();
            if (!graph.ContainsKey(B)) graph[B] = new List<int>();
            graph[A].Add(B);
            graph[B].Add(A);
        }
        if (!graph.ContainsKey(1) || !graph.ContainsKey(target))
        {
            Console.WriteLine(0);
            return;
        }
        visited[1] = true;
        DFS(1);
        Console.WriteLine(pathCount);
    }

    static void DFS(int node)
    {
        if (node == target)
        {
            pathCount++;
            return;
        }
        foreach (var nei in graph[node])
        {
            if (!visited[nei])
            {
                visited[nei] = true;
                DFS(nei);
                visited[nei] = false;
            }
        }
    }
}
