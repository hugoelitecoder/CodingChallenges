using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var header = Console.ReadLine().Split();
        int totalNodes = int.Parse(header[0]);
        int totalLinks = int.Parse(header[1]);
        int gatewayCount = int.Parse(header[2]);

        var graph = InitializeGraph(totalNodes);

        for (int i = 0; i < totalLinks; i++)
        {
            var link = Console.ReadLine().Split();
            int a = int.Parse(link[0]);
            int b = int.Parse(link[1]);
            AddLink(graph, a, b);
        }

        for (int i = 0; i < gatewayCount; i++)
        {
            int gw = int.Parse(Console.ReadLine());
            MarkGateway(graph, gw);
        }

        while (true)
        {
            var line = Console.ReadLine();
            if (line == null) break;
            int agentPos = int.Parse(line);
            var (cutNode, gateway) = GetLinkToCut(graph, agentPos);
            Console.WriteLine($"{cutNode} {gateway}");
            RemoveLink(graph, cutNode, gateway);
        }
    }

    class Node
    {
        public HashSet<int> Neighbors = new HashSet<int>();
        public bool IsGateway;
        public HashSet<int> GatewayNeighbors = new HashSet<int>();
    }

    private static List<Node> InitializeGraph(int size)
    {
        var list = new List<Node>(size);
        for (int i = 0; i < size; i++)
            list.Add(new Node());
        return list;
    }

    private static void AddLink(List<Node> graph, int x, int y)
    {
        graph[x].Neighbors.Add(y);
        graph[y].Neighbors.Add(x);
    }

    private static void MarkGateway(List<Node> graph, int id)
    {
        graph[id].IsGateway = true;
        foreach (var nb in graph[id].Neighbors)
            graph[nb].GatewayNeighbors.Add(id);
    }

    private static (int node, int gateway) GetLinkToCut(List<Node> graph, int start)
    {
        var visited = new bool[graph.Count];
        var q = new Queue<int>();
        q.Enqueue(start);
        visited[start] = true;
        int choice = -1;

        while (q.Count > 0)
        {
            int id = q.Dequeue();
            var node = graph[id];

            if (node.GatewayNeighbors.Count > 1)
            {
                choice = id;
                break;
            }
            else if (node.GatewayNeighbors.Count == 1)
            {
                if (choice < 0)
                {
                    choice = id;
                    if (id == start) break;
                }
                EnqueueNeighbors(node.Neighbors, visited, q);
            }
            else if (choice < 0)
            {
                EnqueueNeighbors(node.Neighbors, visited, q);
            }

            visited[id] = true;
        }

        int gw = -1;
        if (choice >= 0)
            gw = new List<int>(graph[choice].GatewayNeighbors)[0];
        return (choice, gw);
    }

    private static void EnqueueNeighbors(IEnumerable<int> neighbors, bool[] visited, Queue<int> q)
    {
        foreach (var nb in neighbors)
            if (!visited[nb])
                q.Enqueue(nb);
    }

    private static void RemoveLink(List<Node> graph, int x, int y)
    {
        graph[x].Neighbors.Remove(y);
        graph[y].Neighbors.Remove(x);
        graph[x].GatewayNeighbors.Remove(y);
    }
}
