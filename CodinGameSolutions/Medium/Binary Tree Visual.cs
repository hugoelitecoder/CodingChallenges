using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        var nodes = new Node[N];
        var leftIdx = new int[N];
        var rightIdx = new int[N];

        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            nodes[i] = new Node { sval = parts[0] };
            leftIdx[i] = int.Parse(parts[1]);
            rightIdx[i] = int.Parse(parts[2]);
        }

        for (int i = 0; i < N; i++)
        {
            nodes[i].left = leftIdx[i] != -1 ? nodes[leftIdx[i]] : null;
            nodes[i].right = rightIdx[i] != -1 ? nodes[rightIdx[i]] : null;
        }

        RenderTree(nodes[0]);
    }

    class Node
    {
        public string sval;
        public Node left, right;
        public int level, col;
    }

    static void RenderTree(Node root)
    {
        var allNodes = new List<Node>();
        GatherNodes(root, allNodes);

        int N = allNodes.Count;
        int W = allNodes.Max(n => n.sval.Length) + 1;

        int maxDepth = 0;
        SetLevels(root, 1, ref maxDepth);

        int colCounter = 0;
        InOrder(root, ref colCounter);

        int rows = 4 * maxDepth - 3;
        int cols = N * W;
        var grid = new char[rows, cols];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                grid[r, c] = ' ';

        foreach (var node in allNodes)
        {
            int r0 = (node.level - 1) * 4;
            int cEnd = node.col * W + W - 1;
            string s = node.sval;
            int start = cEnd - s.Length + 1;
            for (int k = 0; k < s.Length; k++)
                grid[r0, start + k] = s[k];

            if (node.left != null || node.right != null)
                grid[r0 + 1, cEnd] = '|';

            if (node.left != null)
            {
                int cChild = node.left.col * W + W - 1;
                DrawHorizontal(grid, r0 + 2, cEnd, cChild);
                grid[r0 + 3, cChild] = '|';
            }

            if (node.right != null)
            {
                int cChild = node.right.col * W + W - 1;
                DrawHorizontal(grid, r0 + 2, cEnd, cChild);
                grid[r0 + 3, cChild] = '|';
            }
        }

        var sb = new StringBuilder();
        for (int r = 0; r < rows; r++)
        {
            sb.Clear();
            for (int c = 0; c < cols; c++)
                sb.Append(grid[r, c]);
            Console.WriteLine(sb.ToString().TrimEnd());
        }
    }

    static void GatherNodes(Node node, List<Node> list)
    {
        if (node == null || list.Contains(node)) return;
        list.Add(node);
        GatherNodes(node.left, list);
        GatherNodes(node.right, list);
    }

    static void SetLevels(Node node, int depth, ref int maxDepth)
    {
        node.level = depth;
        if (depth > maxDepth) maxDepth = depth;
        if (node.left != null) SetLevels(node.left, depth + 1, ref maxDepth);
        if (node.right != null) SetLevels(node.right, depth + 1, ref maxDepth);
    }

    static void InOrder(Node node, ref int counter)
    {
        if (node.left != null) InOrder(node.left, ref counter);
        node.col = counter++;
        if (node.right != null) InOrder(node.right, ref counter);
    }

    static void DrawHorizontal(char[,] grid, int row, int c1, int c2)
    {
        int low = Math.Min(c1, c2);
        int high = Math.Max(c1, c2);
        grid[row, low] = '+';
        grid[row, high] = '+';
        for (int c = low + 1; c < high; c++)
            grid[row, c] = '-';
    }
}