using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int edgeCount = int.Parse(Console.ReadLine());
        var edges = Enumerable.Range(0, edgeCount)
            .Select(_ => Console.ReadLine().Split().Select(int.Parse).ToArray())
            .ToArray();

        int nodeCount = edges.SelectMany(e => e).Append(0).Max() + 1;
        var neighbors = Enumerable.Range(0, nodeCount)
                                  .Select(_ => new List<int>())
                                  .ToArray();

        foreach (var e in edges)
        {
            neighbors[e[0]].Add(e[1]);
            neighbors[e[1]].Add(e[0]);
        }

        int entry = edges[0][0];
        var firstBfs = BFS(entry, neighbors);
        var secondBfs = BFS(firstBfs.farthest, neighbors);

        Console.WriteLine((secondBfs.distances.Max() + 1) / 2);
    }

    private static (int farthest, int[] distances) BFS(int startNode, List<int>[] neighbors)
    {
        int count = neighbors.Length;
        var distances = Enumerable.Repeat(-1, count).ToArray();
        var queue = new Queue<int>();

        distances[startNode] = 0;
        queue.Enqueue(startNode);

        while (queue.Count > 0)
        {
            int curr = queue.Dequeue();
            foreach (int nei in neighbors[curr])
            {
                if (distances[nei] == -1)
                {
                    distances[nei] = distances[curr] + 1;
                    queue.Enqueue(nei);
                }
            }
        }

        int farthest = 0;
        for (int i = 1; i < count; i++)
            if (distances[i] > distances[farthest])
                farthest = i;

        return (farthest, distances);
    }
}