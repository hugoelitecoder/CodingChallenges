using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        var actions = new List<string>();
        var indexMap = new Dictionary<string, int>();
        for (int i = 0; i < N; i++)
        {
            string action = Console.ReadLine();
            actions.Add(action);
            indexMap[action] = i;
        }

        int nbOrders = int.Parse(Console.ReadLine());
        var orders = new List<(string, string, string)>();
        for (int i = 0; i < nbOrders; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            orders.Add((parts[0], parts[1], parts[2]));
        }

        var orderer = new TaskOrderer(actions, indexMap, orders);
        var ordered = orderer.GetOrderedActions();

        foreach (var act in ordered)
            Console.WriteLine(act);
    }
}

public class TaskOrderer
{
    private readonly List<string> _actions;
    private readonly Dictionary<string, int> _indexMap;
    private readonly List<(string, string, string)> _orders;

    public TaskOrderer(List<string> actions, Dictionary<string, int> indexMap, List<(string, string, string)> orders)
    {
        _actions = actions;
        _indexMap = indexMap;
        _orders = orders;
    }

    public List<string> GetOrderedActions()
    {
        var adj = _actions.ToDictionary(a => a, _ => new List<string>());
        var indegree = _actions.ToDictionary(a => a, _ => 0);

        foreach (var (a1, precedence, a2) in _orders)
        {
            if (precedence == "before")
            {
                adj[a1].Add(a2);
                indegree[a2]++;
            }
            else
            {
                adj[a2].Add(a1);
                indegree[a1]++;
            }
        }

        var result = new List<string>();
        var ready = new List<string>();
        foreach (var act in _actions)
        {
            if (indegree[act] == 0)
                ready.Add(act);
        }
        ready.Sort((a, b) => _indexMap[a].CompareTo(_indexMap[b]));

        while (ready.Count > 0)
        {
            var current = ready[0];
            ready.RemoveAt(0);
            result.Add(current);

            foreach (var next in adj[current])
            {
                indegree[next]--;
                if (indegree[next] == 0)
                    ready.Add(next);
            }
            ready.Sort((a, b) => _indexMap[a].CompareTo(_indexMap[b]));
        }

        return result;
    }
}
