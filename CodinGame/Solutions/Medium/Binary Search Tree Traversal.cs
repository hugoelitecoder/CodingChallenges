using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        var n = int.Parse(Console.ReadLine().Trim());
        var values = Console.ReadLine()
                            .Trim()
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Select(long.Parse);
        
        TreeNode root = null;
        foreach (var v in values)
            root = Insert(root, v);

        var pre  = new List<long>();
        var ino  = new List<long>();
        var post = new List<long>();
        var lvl  = new List<long>();

        PreOrder(root, pre);
        InOrder(root, ino);
        PostOrder(root, post);
        LevelOrder(root, lvl);

        Console.WriteLine(string.Join(' ', pre));
        Console.WriteLine(string.Join(' ', ino));
        Console.WriteLine(string.Join(' ', post));
        Console.WriteLine(string.Join(' ', lvl));
    }

    private static TreeNode Insert(TreeNode node, long value)
    {
        if (node == null)
            return new TreeNode(value);
        if (value < node.Value)
            node.Left = Insert(node.Left, value);
        else
            node.Right = Insert(node.Right, value);
        return node;
    }

    private static void PreOrder(TreeNode node, List<long> outList)
    {
        if (node == null) return;
        outList.Add(node.Value);
        PreOrder(node.Left,  outList);
        PreOrder(node.Right, outList);
    }

    private static void InOrder(TreeNode node, List<long> outList)
    {
        if (node == null) return;
        InOrder(node.Left,  outList);
        outList.Add(node.Value);
        InOrder(node.Right, outList);
    }

    private static void PostOrder(TreeNode node, List<long> outList)
    {
        if (node == null) return;
        PostOrder(node.Left,  outList);
        PostOrder(node.Right, outList);
        outList.Add(node.Value);
    }

    private static void LevelOrder(TreeNode root, List<long> outList)
    {
        if (root == null) return;
        var q = new Queue<TreeNode>();
        q.Enqueue(root);
        while (q.Count > 0)
        {
            var node = q.Dequeue();
            outList.Add(node.Value);
            if (node.Left  != null) q.Enqueue(node.Left);
            if (node.Right != null) q.Enqueue(node.Right);
        }
    }

    private class TreeNode
    {
        public long Value;
        public TreeNode Left, Right;
        public TreeNode(long value) => Value = value;
    }
}
