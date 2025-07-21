using System;
using System.Collections.Generic;
using System.Linq;

class Solution {
    static void Main() {
        int N = int.Parse(Console.ReadLine());
        var lines = Enumerable.Range(0, N)
                              .Select(_ => Console.ReadLine())
                              .ToList();

        var roots = Node.BuildForest(lines);
        foreach (var root in roots)
            foreach (var path in root.GetPaths())
                Console.WriteLine(string.Join(" > ", path));
    }
}

public class Node {
    public string Name { get; }
    public List<Node> Children { get; } = new List<Node>();

    private Node(string name) {
        Name = name;
    }

    public static List<Node> BuildForest(List<string> lines) {
        var roots = new List<Node>();
        var stack = new List<Node>();

        foreach (var line in lines) {
            int depth = line.TakeWhile(c => c == '.').Count();
            var name = line.Substring(depth);
            var node = new Node(name);

            if (depth == 0) {
                roots.Add(node);
            } else {
                stack[depth - 1].Children.Add(node);
            }

            if (stack.Count > depth) stack[depth] = node;
            else stack.Add(node);

            if (stack.Count > depth + 1)
                stack.RemoveRange(depth + 1, stack.Count - depth - 1);
        }
        return roots;
    }

    public IEnumerable<List<string>> GetPaths() {
        if (Children.Count == 0) {
            yield return new List<string> { Name };
        } else {
            foreach (var child in Children) {
                foreach (var sub in child.GetPaths()) {
                    var path = new List<string> { Name };
                    path.AddRange(sub);
                    yield return path;
                }
            }
        }
    }
}
