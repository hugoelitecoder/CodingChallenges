using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var header     = Console.ReadLine().Split().Select(int.Parse).ToArray();
        var nodeCount  = header[0];
        var linkCount  = header[1];
        var exitCount  = header[2];
        var graph = Enumerable.Range(0, nodeCount)
                              .Select(_ => new List<int>())
                              .ToArray();
        for (int i = 0; i < linkCount; i++)
        {
            var edge = Console.ReadLine().Split().Select(int.Parse).ToArray();
            graph[edge[0]].Add(edge[1]);
            graph[edge[1]].Add(edge[0]);
        }
        var exits = new HashSet<int>(
            Enumerable.Range(0, exitCount)
                      .Select(_ => int.Parse(Console.ReadLine()))
        );

        while (true)
        {
            var start = int.Parse(Console.ReadLine());

            var prevFromStart = new int[nodeCount];
            var prevFromExit  = new int[nodeCount];
            var visFromStart  = new bool[nodeCount];
            var visFromExit   = new bool[nodeCount];

            Array.Fill(prevFromStart, -1);
            Array.Fill(prevFromExit,  -1);

            var queueStart = new Queue<int>();
            var queueExit  = new Queue<int>();

            visFromStart[start] = true;
            queueStart.Enqueue(start);
            foreach (var ex in exits)
            {
                visFromExit[ex] = true;
                queueExit.Enqueue(ex);
            }

            int meetingNode = -1;
            while (meetingNode < 0 && queueStart.Count > 0 && queueExit.Count > 0)
            {
                if (queueStart.Count <= queueExit.Count)
                    meetingNode = ExpandLayer(queueStart, visFromStart, visFromExit, prevFromStart, graph);
                else
                    meetingNode = ExpandLayer(queueExit, visFromExit, visFromStart, prevFromExit,  graph);
            }
            int hop = meetingNode;
            while (prevFromStart[hop] != start)
                hop = prevFromStart[hop];

            graph[start].Remove(hop);
            graph[hop].Remove(start);

            Console.WriteLine($"{start} {hop}");
        }
    }

    static int ExpandLayer(
        Queue<int>   queue,
        bool[]       visThis,
        bool[]       visOther,
        int[]        parent,
        List<int>[]  graph)
    {
        int layerSize = queue.Count;
        while (layerSize-- > 0)
        {
            int u = queue.Dequeue();
            foreach (var v in graph[u])
            {
                if (visThis[v]) continue;
                visThis[v]   = true;
                parent[v]    = u;
                if (visOther[v]) return v;
                queue.Enqueue(v);
            }
        }
        return -1;
    }
}
