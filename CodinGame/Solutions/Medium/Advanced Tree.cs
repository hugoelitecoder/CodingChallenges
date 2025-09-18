using System;
using System.Collections.Generic;

class Solution
{
    private static bool _showAll;
    private static bool _onlyDirs;
    private static int  _maxDepth;
    private static int  _nbNodes;
    private static int  _nbFiles;
    private static Dictionary<string, Node> _nodes;

    public static void Main()
    {
        // Read start path and flags
        var pathInput = Console.ReadLine();
        var flags = new List<string>(
            Console.ReadLine()
                   .Split(',', StringSplitOptions.RemoveEmptyEntries)
        );

        // Parse flags
        _showAll  = flags.Contains("-a");
        _onlyDirs = flags.Contains("-d");
        _maxDepth = int.MaxValue;
        foreach (var f in flags)
        {
            if (f.StartsWith("-L"))
            {
                var parts = f.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && int.TryParse(parts[1], out var d) && d > 0)
                    _maxDepth = d;
            }
        }

        // Build node map
        var n = int.Parse(Console.ReadLine());
        _nodes = new Dictionary<string, Node> { ["."] = new Node(".", ".") };

        for (var i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            var parts = line.Split('/');
            var current = _nodes["."];

            for (var k = 0; k < parts.Length; k++)
            {
                var name = parts[k];
                if (name != "." && name.StartsWith(".") && !_showAll)
                    break;

                var key = string.Join("/", parts, 0, k + 1);
                if (!_nodes.ContainsKey(key))
                {
                    var node = new Node(name, key);
                    _nodes[key] = node;
                    current.Children.Add(node);
                }
                current = _nodes[key];
            }
        }

        // Determine start node
        var startKey = pathInput.StartsWith(".") ? pathInput : "./" + pathInput;
        if (!_nodes.ContainsKey(startKey) || _nodes[startKey].Children.Count == 0)
        {
            Console.WriteLine($"{pathInput} [error opening dir]");
            Console.WriteLine();
            // Correct pluralization for zero
            var dirWord = 0 == 1 ? "directory" : "directories";
            if (_onlyDirs)
                Console.WriteLine($"0 {dirWord}");
            else
                Console.WriteLine($"0 {dirWord}, 0 files");
            return;
        }

        // Print tree
        Console.WriteLine(pathInput);
        _nbNodes = 0;
        _nbFiles = 0;
        Display(_nodes[startKey], pathInput.StartsWith("."), 0, "", true);

        // Print summary
        Console.WriteLine();
        var dirCount = _nbNodes - _nbFiles;
        var dirWordFinal  = dirCount == 1 ? "directory" : "directories";
        if (_onlyDirs)
            Console.WriteLine($"{dirCount} {dirWordFinal}");
        else
            Console.WriteLine($"{dirCount} {dirWordFinal}, {_nbFiles} {(_nbFiles == 1 ? "file" : "files")}");
    }

    private static void Display(
        Node node,
        bool full,
        int depth,
        string filler,
        bool first,
        bool last = false
    )
    {
        if (depth > _maxDepth) return;

        // Print this node (skip root at depth==0)
        if (depth > 0)
        {
            _nbNodes++;
            if (node.Children.Count == 0) _nbFiles++;

            var toPrint = full ? node.Path : node.Name;
            Console.Write(filler);
            Console.Write(last ? "`-- " : "|-- ");
            Console.WriteLine(toPrint);
        }

        // Gather, filter, and sort children
        var list = new List<Node>();
        foreach (var c in node.Children)
        {
            if (!_showAll && c.Name.StartsWith(".")) continue;
            if (_onlyDirs && c.Children.Count == 0) continue;
            list.Add(c);
        }
        list.Sort((a, b) =>
        {
            var ka = a.Name.StartsWith(".") ? a.Name.Substring(1) : a.Name;
            var kb = b.Name.StartsWith(".") ? b.Name.Substring(1) : b.Name;
            return string.Compare(ka, kb, StringComparison.OrdinalIgnoreCase);
        });

        // Recurse into children
        for (var i = 0; i < list.Count; i++)
        {
            var c      = list[i];
            var isLast = i == list.Count - 1;
            Display(
                c,
                false,
                depth + 1,
                filler + (first ? "" : (last ? "    " : "|   ")),
                false,
                isLast
            );
        }
    }

    private class Node
    {
        public string Name;
        public string Path;
        public List<Node> Children = new List<Node>();
        public Node(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}
