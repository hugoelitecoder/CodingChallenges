using System;
using System.Collections.Generic;
using System.Text;

class Solution
{
    public static void Main()
    {
        var parts = Console.ReadLine().Split();
        var n = int.Parse(parts[0]);
        var k = int.Parse(parts[1]);
        var signatures = new HashSet<string>();
        for (var i = 0; i < n; i++)
        {
            var values = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
            var root = BuildTree(values);
            var map = new Dictionary<int,int>();
            var order = new List<int>();
            InOrder(root, order);
            for (var j = 0; j < order.Count; j++)
                map[order[j]] = j;
            var sb = new StringBuilder();
            PreOrder(root, map, sb);
            signatures.Add(sb.ToString());
        }
        Console.WriteLine(signatures.Count);
    }

    private class Node
    {
        public int Value;
        public Node Left, Right;
        public Node(int v) => Value = v;
        public void Add(int v)
        {
            if (v < Value)
            {
                if (Left == null) Left = new Node(v);
                else Left.Add(v);
            }
            else
            {
                if (Right == null) Right = new Node(v);
                else Right.Add(v);
            }
        }
    }

    private static Node BuildTree(int[] vals)
    {
        var root = new Node(vals[0]);
        for (var i = 1; i < vals.Length; i++)
            root.Add(vals[i]);
        return root;
    }

    private static void InOrder(Node node, List<int> list)
    {
        if (node == null) return;
        InOrder(node.Left, list);
        list.Add(node.Value);
        InOrder(node.Right, list);
    }

    private static void PreOrder(Node node, Dictionary<int,int> map, StringBuilder sb)
    {
        if (node == null) return;
        sb.Append(map[node.Value]).Append(' ');
        PreOrder(node.Left, map, sb);
        PreOrder(node.Right, map, sb);
    }
}
