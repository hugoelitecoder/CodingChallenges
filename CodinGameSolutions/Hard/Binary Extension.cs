using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

class Player
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var width = int.Parse(inputs[0]);
        var height = int.Parse(inputs[1]);
        var n = int.Parse(inputs[2]);
        var bombsCount = int.Parse(inputs[3]);
        var goalsCount = int.Parse(inputs[4]);

        var bombs = new HashSet<Point2D>();
        for (var i = 0; i < bombsCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var x = int.Parse(inputs[0]);
            var y = int.Parse(inputs[1]);
            bombs.Add(new Point2D(x, y));
        }

        var goals = new List<Point2D>();
        for (var i = 0; i < goalsCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var x = int.Parse(inputs[0]);
            var y = int.Parse(inputs[1]);
            goals.Add(new Point2D(x, y));
        }

        var solver = new BinaryPuzzleSolver(width, height, n, bombs, goals);
        solver.Solve();
    }
}

public record struct Point2D(int X, int Y);

public class TreeNode
{
    public Point2D Pos { get; }
    public TreeNode Left { get; set; }
    public TreeNode Right { get; set; }
    public int SubtreeSize { get; set; }

    public TreeNode(Point2D pos)
    {
        Pos = pos;
        SubtreeSize = 1;
    }
}

public class BinaryPuzzleSolver
{
    private readonly int _width, _height, _n;
    private readonly HashSet<Point2D> _bombs;
    private readonly List<Point2D> _goals;
    private readonly Point2D _rootCoord;
    private string P(Point2D p) => $"({p.X},{p.Y})";

    public BinaryPuzzleSolver(int width, int height, int n, HashSet<Point2D> bombs, List<Point2D> goals)
    {
        _width = width; _height = height; _n = n;
        _bombs = bombs; _goals = goals;
        _rootCoord = new Point2D((width - 1) / 2, 0);
    }
    
    public void Solve()
    {
        if (_goals.Count == 0 || IsInvalid(_rootCoord) || _goals.Any(IsInvalid))
        {
            return;
        }
        var sortedGoals = _goals.Distinct().OrderBy(g => g.Y).ThenBy(g => g.X).ToList();
        var finalParentMap = FindStructureRecursive(sortedGoals, new HashSet<Point2D> { _rootCoord }, new Dictionary<Point2D, Point2D>(), "  ");
        List<int> solutionSequence = null;
        if (finalParentMap != null)
        {
            var (root, valueMap) = AssignValues(finalParentMap);
            if (root != null)
            {
                solutionSequence = new List<int>();
                GenerateSolutionSequence(root, valueMap, solutionSequence);
            }
        }
        if (solutionSequence != null)
        {
            foreach (var value in solutionSequence)
            {
                Console.WriteLine(value);
            }
        }
    }
    
    private bool IsInvalid(Point2D p) => p.X < 0 || p.X >= _width || p.Y < 0 || p.Y >= _height || _bombs.Contains(p);

    private Dictionary<Point2D, Point2D> FindStructureRecursive(
        List<Point2D> remainingGoals,
        HashSet<Point2D> currentTreeNodes, 
        Dictionary<Point2D, Point2D> currentParentMap, string indent)
    {
        if (!remainingGoals.Any()) return currentParentMap;

        var goalToConnect = remainingGoals.First();
        if (currentTreeNodes.Contains(goalToConnect))
        {
            return FindStructureRecursive(remainingGoals.Skip(1).ToList(), currentTreeNodes, currentParentMap, indent);
        }
        
        var pathsToTree = FindPathsFromNodeToSet(goalToConnect, currentTreeNodes, currentParentMap);
        for (int i = 0; i < pathsToTree.Count; i++)
        {
            var path = pathsToTree[i];
            var newTreeNodes = new HashSet<Point2D>(currentTreeNodes);
            foreach (var node in path.Keys) newTreeNodes.Add(node);
            
            if (newTreeNodes.Count > _n)
            {
                continue; 
            }
            
            var newParentMap = new Dictionary<Point2D, Point2D>(currentParentMap);
            foreach (var edge in path) newParentMap.Add(edge.Key, edge.Value);

            var result = FindStructureRecursive(remainingGoals.Skip(1).ToList(), newTreeNodes, newParentMap, indent + "  ");
            if (result != null) return result; 
        }

        return null; 
    }

    private List<Dictionary<Point2D, Point2D>> FindPathsFromNodeToSet(
        Point2D startNode,
        HashSet<Point2D> targetNodes,
        Dictionary<Point2D, Point2D> existingParentMap)
    {
        var allPaths = new List<Dictionary<Point2D, Point2D>>();
        var queue = new Queue<(Point2D, Dictionary<Point2D, Point2D>)>();
        queue.Enqueue((startNode, new Dictionary<Point2D, Point2D>()));
        var visited = new HashSet<Point2D> { startNode };

        while (queue.Count > 0)
        {
            var (currentPos, pathToCurrent) = queue.Dequeue();
            var potentialParents = new[]
            {
                new { Parent = new Point2D(currentPos.X + 1, currentPos.Y - 1), IsLeftChild = true },
                new { Parent = new Point2D(currentPos.X - 1, currentPos.Y - 1), IsLeftChild = false }
            };

            foreach (var pInfo in potentialParents)
            {
                if (targetNodes.Contains(pInfo.Parent))
                {
                    bool slotOccupied = existingParentMap.Where(kv => kv.Value == pInfo.Parent)
                        .Any(kv => (kv.Key.X < pInfo.Parent.X) == pInfo.IsLeftChild);
                    if (!slotOccupied)
                        allPaths.Add(new Dictionary<Point2D, Point2D>(pathToCurrent) { [currentPos] = pInfo.Parent });
                    continue;
                }
                if (IsInvalid(pInfo.Parent) || existingParentMap.ContainsKey(pInfo.Parent) || visited.Contains(pInfo.Parent)) continue;
                
                visited.Add(pInfo.Parent);
                queue.Enqueue((pInfo.Parent, new Dictionary<Point2D, Point2D>(pathToCurrent) { [currentPos] = pInfo.Parent }));
            }
        }
        return allPaths;
    }

    private (TreeNode, Dictionary<Point2D, int>) AssignValues(Dictionary<Point2D, Point2D> parentMap)
    {
        var allCoords = new HashSet<Point2D> { _rootCoord };
        allCoords.UnionWith(parentMap.Keys);
        allCoords.UnionWith(parentMap.Values);
        
        var allNodes = allCoords.ToDictionary(c => c, c => new TreeNode(c));
        foreach (var (child, parent) in parentMap)
        {
            if (child.X < parent.X) allNodes[parent].Left = allNodes[child];
            else allNodes[parent].Right = allNodes[child];
        }
        
        var root = allNodes[_rootCoord];
        CalculateSubtreeSizes(root);
        
        if (root.SubtreeSize > _n) return (null, null);

        var valueMap = new Dictionary<Point2D, int>();
        return TryAssign(root, 0, _n + 1, new HashSet<int>(), valueMap) ? (root, valueMap) : (null, null);
    }
    
    private int CalculateSubtreeSizes(TreeNode node)
    {
        if (node == null) return 0;
        node.SubtreeSize = 1 + CalculateSubtreeSizes(node.Left) + CalculateSubtreeSizes(node.Right);
        return node.SubtreeSize;
    }

    private bool TryAssign(TreeNode node, int lowerBound, int upperBound, HashSet<int> used, Dictionary<Point2D, int> valueMap)
    {
        if (node == null) return true;
        if (upperBound - lowerBound - 1 < node.SubtreeSize) return false;

        var leftSize = node.Left?.SubtreeSize ?? 0;
        int rightSize = node.SubtreeSize - 1 - leftSize;
        for (var v = lowerBound + 1 + leftSize; v <= upperBound - 1 - rightSize; v++)
        {
            if (used.Contains(v)) continue;
            valueMap[node.Pos] = v;
            used.Add(v);
            if (TryAssign(node.Left, lowerBound, v, used, valueMap) && 
                TryAssign(node.Right, v, upperBound, used, valueMap))
                return true;
            used.Remove(v);
            valueMap.Remove(node.Pos);
        }
        return false;
    }
    
    private void GenerateSolutionSequence(TreeNode node, Dictionary<Point2D, int> valueMap, List<int> sequence)
    {
        if (node == null) return;
        sequence.Add(valueMap[node.Pos]);
        GenerateSolutionSequence(node.Left, valueMap, sequence);
        GenerateSolutionSequence(node.Right, valueMap, sequence);
    }

}