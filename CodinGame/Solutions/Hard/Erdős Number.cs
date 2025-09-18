using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        string scientist = Console.ReadLine();
        int N = int.Parse(Console.ReadLine());
        var titles = new List<string>();
        var authors = new List<string>();
        for (int i = 0; i < N; i++) titles.Add(Console.ReadLine());
        for (int i = 0; i < N; i++) authors.Add(Console.ReadLine());

        var graph = new CollaborationGraph();
        graph.BuildGraph(N, titles, authors);

        if (scientist == "Erdős")
        {
            Console.WriteLine(0);
            return;
        }
        var path = graph.FindCollaborationPath("Erdős", scientist);

        if (path == null)
        {
            Console.WriteLine("infinite");
        }
        else
        {
            var titlesPath = path.Select(e => e.Title).Reverse().ToList();
            Console.WriteLine(titlesPath.Count);
            foreach (var title in titlesPath)
                Console.WriteLine(title);
        }
    }
}

class AuthorNode
{
    public string Name { get; }
    public HashSet<CollaborationEdge> Edges { get; }

    public AuthorNode(string name)
    {
        Name = name;
        Edges = new HashSet<CollaborationEdge>();
    }
}

class CollaborationEdge : IEquatable<CollaborationEdge>
{
    public AuthorNode Coauthor { get; }
    public string Title { get; }
    public CollaborationEdge(AuthorNode coauthor, string title)
    {
        Coauthor = coauthor;
        Title = title;
    }
    public override int GetHashCode() => (Coauthor.Name, Title).GetHashCode();
    public override bool Equals(object obj) => Equals(obj as CollaborationEdge);
    public bool Equals(CollaborationEdge other) => other != null && Coauthor.Name == other.Coauthor.Name && Title == other.Title;
}

class CollaborationGraph
{
    private Dictionary<string, AuthorNode> nodes = new Dictionary<string, AuthorNode>();

    public void BuildGraph(int n, List<string> titles, List<string> authors)
    {
        for (int i = 0; i < n; i++)
        {
            var authList = authors[i].Split(' ');
            foreach (var pair in PermutePairs(authList))
            {
                var from = GetOrCreateNode(pair.Item1);
                var to = GetOrCreateNode(pair.Item2);
                from.Edges.Add(new CollaborationEdge(to, titles[i]));
            }
        }
    }

    public List<CollaborationEdge> FindCollaborationPath(string startName, string targetName)
    {
        if (!nodes.ContainsKey(startName) || !nodes.ContainsKey(targetName))
            return null;
        var start = nodes[startName];
        var target = nodes[targetName];

        var paths = new Queue<List<CollaborationEdge>>();
        var visited = new HashSet<CollaborationEdge>();
        foreach (var edge in start.Edges)
            paths.Enqueue(new List<CollaborationEdge> { edge });

        while (paths.Count > 0)
        {
            var path = paths.Dequeue();
            var lastEdge = path.Last();
            if (lastEdge.Coauthor == target)
                return path;
            foreach (var next in lastEdge.Coauthor.Edges)
            {
                if (!visited.Contains(next))
                {
                    visited.Add(next);
                    var newPath = new List<CollaborationEdge>(path) { next };
                    paths.Enqueue(newPath);
                }
            }
        }
        return null;
    }

    private AuthorNode GetOrCreateNode(string name)
    {
        if (!nodes.ContainsKey(name))
            nodes[name] = new AuthorNode(name);
        return nodes[name];
    }

    private IEnumerable<(string, string)> PermutePairs(string[] items)
    {
        for (int i = 0; i < items.Length; ++i)
            for (int j = 0; j < items.Length; ++j)
                if (i != j)
                    yield return (items[i], items[j]);
    }
}
