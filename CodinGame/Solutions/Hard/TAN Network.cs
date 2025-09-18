using System;
using System.Collections.Generic;
using System.Globalization;

public class Solution
{
    public static void Main()
    {
        string startId = Console.ReadLine()?.Trim();
        string endId = Console.ReadLine()?.Trim();
        int N = int.Parse(Console.ReadLine()!);

        var graph = new Graph();
        for (int i = 0; i < N; i++)
        {
            var stop = Stop.Parse(Console.ReadLine()!);
            graph.AddStop(stop);
        }

        int M = int.Parse(Console.ReadLine()!);
        for (int i = 0; i < M; i++)
        {
            var line = Console.ReadLine();
            var tokens = line!.Split(' ');
            graph.AddEdge(tokens[0], tokens[1]);
        }

        var dijkstra = new Dijkstra(graph);
        var path = dijkstra.FindShortestPath(startId!, endId!);

        if (path == null)
        {
            Console.WriteLine("IMPOSSIBLE");
        }
        else
        {
            foreach (var stopId in path)
                Console.WriteLine(graph.Stops[stopId].Name);
        }
    }
}

public static class Constants
{
    public const double EarthRadiusKm = 6371.0;
}

public class Stop
{
    public string Id { get; }
    public string Name { get; }
    public double LatitudeRad { get; }
    public double LongitudeRad { get; }

    public Stop(string id, string name, double latitude, double longitude)
    {
        Id = id;
        Name = name;
        LatitudeRad = latitude * Math.PI / 180.0;
        LongitudeRad = longitude * Math.PI / 180.0;
    }

    public static Stop Parse(string line)
    {
        var fields = SplitCsv(line);
        return new Stop(
            fields[0],
            fields[1].Trim('"'),
            double.Parse(fields[3], CultureInfo.InvariantCulture),
            double.Parse(fields[4], CultureInfo.InvariantCulture)
        );
    }

    private static List<string> SplitCsv(string input)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var token = "";
        foreach (var ch in input)
        {
            if (ch == '"')
                inQuotes = !inQuotes;
            else if (ch == ',' && !inQuotes)
            {
                result.Add(token);
                token = "";
            }
            else
                token += ch;
        }
        result.Add(token);
        return result;
    }
}

public class Graph
{
    public Dictionary<string, Stop> Stops { get; } = new();
    public Dictionary<string, List<string>> AdjacencyList { get; } = new();

    public void AddStop(Stop stop)
    {
        Stops[stop.Id] = stop;
        if (!AdjacencyList.ContainsKey(stop.Id))
            AdjacencyList[stop.Id] = new List<string>();
    }

    public void AddEdge(string fromId, string toId)
    {
        if (!AdjacencyList.ContainsKey(fromId))
            AdjacencyList[fromId] = new List<string>();
        AdjacencyList[fromId].Add(toId);
    }

    public double Distance(string fromId, string toId)
    {
        var a = Stops[fromId];
        var b = Stops[toId];
        double dLat = b.LatitudeRad - a.LatitudeRad;
        double dLon = b.LongitudeRad - a.LongitudeRad;
        double h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(a.LatitudeRad) * Math.Cos(b.LatitudeRad) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));
        return Constants.EarthRadiusKm * c;
    }
}

public class Dijkstra
{
    private readonly Graph _graph;

    public Dijkstra(Graph graph)
    {
        _graph = graph;
    }

    public List<string>? FindShortestPath(string startId, string endId)
    {
        var distances = new Dictionary<string, double>();
        var previous = new Dictionary<string, string>();
        var visited = new HashSet<string>();
        var queue = new PriorityQueue<string, double>();

        foreach (var stopId in _graph.Stops.Keys)
            distances[stopId] = double.PositiveInfinity;
        distances[startId] = 0;
        queue.Enqueue(startId, 0);

        while (queue.Count > 0)
        {
            queue.TryDequeue(out var current, out var currentDist);
            if (current == endId) break;
            if (visited.Contains(current)) continue;
            visited.Add(current);

            foreach (var neighbor in _graph.AdjacencyList.GetValueOrDefault(current, new List<string>()))
            {
                if (visited.Contains(neighbor)) continue;
                double alt = distances[current] + _graph.Distance(current, neighbor);
                if (alt < distances[neighbor])
                {
                    distances[neighbor] = alt;
                    previous[neighbor] = current;
                    queue.Enqueue(neighbor, alt);
                }
            }
        }

        if (!previous.ContainsKey(endId) && startId != endId)
            return null;

        // Reconstruct path
        var path = new List<string>();
        var at = endId;
        path.Add(at);
        while (previous.ContainsKey(at))
        {
            at = previous[at];
            path.Add(at);
        }
        if (path[^1] != startId)
            return null;
        path.Reverse();
        return path;
    }
}

