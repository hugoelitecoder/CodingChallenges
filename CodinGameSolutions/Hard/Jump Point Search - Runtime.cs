using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

class Solution
{
    public static void Main(string[] args)
    {
        var _ = Console.ReadLine();
        var inputs = Console.ReadLine().Split(' ');
        var start = new Point2D(int.Parse(inputs[0]), int.Parse(inputs[1]));
        var goal = new Point2D(int.Parse(inputs[2]), int.Parse(inputs[3]));
        var openCount = int.Parse(Console.ReadLine());
        var jpsData = new Dictionary<Point2D, int[]>();
        for (var i = 0; i < openCount; i++)
        {
            var tokens = Console.ReadLine().Split(' ');
            var key = new Point2D(int.Parse(tokens[0]), int.Parse(tokens[1]));
            var values = new int[8];
            for (var j = 0; j < 8; j++) values[j] = int.Parse(tokens[j + 2]);
            jpsData[key] = values;
        }
        var search = new JPSPlusRuntimeSearch(jpsData, start, goal);
        var found = false;
        foreach (var step in search.Search())
        {
            Console.WriteLine($"{step.Pos.X} {step.Pos.Y} {step.Parent.X} {step.Parent.Y} {step.G.ToString("F2", CultureInfo.InvariantCulture)}");
            if (step.IsGoal) { found = true; break; }
        }
        if (!found) Console.WriteLine("NO PATH");
    }
}

public sealed class Point2D : IEquatable<Point2D>
{
    public int X { get; }
    public int Y { get; }
    public Point2D(int x, int y) { X = x; Y = y; }
    public bool Equals(Point2D other) => other != null && X == other.X && Y == other.Y;
    public override bool Equals(object obj) => obj is Point2D p && Equals(p);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"{X},{Y}";
}

public sealed class SearchStep
{
    public Point2D Pos { get; }
    public Point2D Parent { get; }
    public double G { get; }
    public bool IsGoal { get; }
    public SearchStep(Point2D pos, Point2D parent, double g, bool isGoal)
    {
        Pos = pos;
        Parent = parent;
        G = g;
        IsGoal = isGoal;
    }
}

internal class JPSPlusRuntimeSearch
{
    private enum Dir { N = 0, NE = 1, E = 2, SE = 3, S = 4, SW = 5, W = 6, NW = 7, START = 8 }
    private static readonly (int dx, int dy)[] Dirs = { (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1) };
    private static readonly double SQRT2 = Math.Sqrt(2);
    private static readonly Dictionary<Dir, Dir[]> ValidDirLookupTable = new()
    {
        { Dir.START, new[] { Dir.N, Dir.NE, Dir.E, Dir.SE, Dir.S, Dir.SW, Dir.W, Dir.NW } },
        { Dir.N, new[] { Dir.E, Dir.NE, Dir.N, Dir.NW, Dir.W } },
        { Dir.NE, new[] { Dir.E, Dir.NE, Dir.N } },
        { Dir.E, new[] { Dir.S, Dir.SE, Dir.E, Dir.NE, Dir.N } },
        { Dir.SE, new[] { Dir.S, Dir.SE, Dir.E } },
        { Dir.S, new[] { Dir.W, Dir.SW, Dir.S, Dir.SE, Dir.E } },
        { Dir.SW, new[] { Dir.W, Dir.SW, Dir.S } },
        { Dir.W, new[] { Dir.N, Dir.NW, Dir.W, Dir.SW, Dir.S } },
        { Dir.NW, new[] { Dir.N, Dir.NW, Dir.W } }
    };

    private readonly Dictionary<Point2D, int[]> _jpsData;
    private readonly Point2D _startPos;
    private readonly Point2D _goalPos;
    private readonly List<JPSNode> _openList = new();
    private readonly Dictionary<Point2D, JPSNode> _openListPositions = new();
    private readonly HashSet<Point2D> _closedList = new();

    private class JPSNode : IComparable<JPSNode>
    {
        public Point2D Pos { get; }
        public double G { get; set; }
        public double H { get; set; }
        public Point2D Parent { get; set; }
        public double F => G + H;

        public JPSNode(Point2D pos, double g, double h, Point2D parent)
        {
            Pos = pos; G = g; H = h; Parent = parent;
        }

        public int CompareTo(JPSNode o)
        {
            var fCmp = F.CompareTo(o.F);
            return fCmp != 0 ? fCmp : H.CompareTo(o.H);
        }
    }

    public JPSPlusRuntimeSearch(Dictionary<Point2D, int[]> jpsData, Point2D start, Point2D goal)
    {
        _jpsData = jpsData;
        _startPos = start;
        _goalPos = goal;
    }

    public IEnumerable<SearchStep> Search()
    {
        ClearState();
        var startNode = CreateStartNode();
        yield return NodeOutput(startNode, IsGoal(startNode.Pos));
        _closedList.Add(startNode.Pos);
        if (IsGoal(startNode.Pos)) yield break;

        ExpandSuccessors(startNode);

        while (_openList.Count > 0)
        {
            var current = PopNextNode();
            if (_closedList.Contains(current.Pos)) continue;
            var isGoal = IsGoal(current.Pos);
            yield return NodeOutput(current, isGoal);
            if (isGoal) yield break;
            _closedList.Add(current.Pos);
            ExpandSuccessors(current);
        }
    }

    private void ClearState()
    {
        _openList.Clear();
        _openListPositions.Clear();
        _closedList.Clear();
    }

    private JPSNode CreateStartNode()
    {
        var h = OctileDistance(_startPos, _goalPos);
        return new JPSNode(_startPos, 0, h, new Point2D(-1, -1));
    }

    private JPSNode PopNextNode()
    {
        _openList.Sort();
        var current = _openList[0];
        _openList.RemoveAt(0);
        _openListPositions.Remove(current.Pos);
        return current;
    }

    private void ExpandSuccessors(JPSNode node)
    {
        var travelDir = GetTravelDirection(node);
        foreach (var dir in ValidDirLookupTable[travelDir])
        {
            var successor = CreateSuccessor(node, dir, out var newG);
            if (successor != null && !_closedList.Contains(successor.Pos))
            {
                successor.G = newG;
                successor.H = OctileDistance(successor.Pos, _goalPos);
                AddToOpenList(successor);
            }
        }
    }

    private JPSNode CreateSuccessor(JPSNode current, Dir dir, out double newG)
    {
        var (dx, dy) = Dirs[(int)dir];
        var isDiag = dx != 0 && dy != 0;
        var precomp = _jpsData[current.Pos][(int)dir];
        var distToObs = Math.Abs(precomp);
        newG = 0;
        if (!isDiag && precomp != 0)
        {
            var goalDx = _goalPos.X - current.Pos.X;
            var goalDy = _goalPos.Y - current.Pos.Y;
            if (dx == Math.Sign(goalDx) && dy == Math.Sign(goalDy))
            {
                var distToGoal = dx != 0 ? Math.Abs(goalDx) : Math.Abs(goalDy);
                if (distToGoal <= distToObs)
                {
                    newG = current.G + distToGoal;
                    return new JPSNode(_goalPos, 0, 0, current.Pos);
                }
            }
        }
        else if (isDiag && precomp != 0)
        {
            var goalDx = _goalPos.X - current.Pos.X;
            var goalDy = _goalPos.Y - current.Pos.Y;
            if (dx == Math.Sign(goalDx) && dy == Math.Sign(goalDy))
            {
                if (Math.Abs(goalDy) <= distToObs || Math.Abs(goalDx) <= distToObs)
                {
                    var minDiff = Math.Min(Math.Abs(goalDx), Math.Abs(goalDy));
                    var newPos = new Point2D(current.Pos.X + minDiff * dx, current.Pos.Y + minDiff * dy);
                    newG = current.G + SQRT2 * minDiff;
                    return new JPSNode(newPos, 0, 0, current.Pos);
                }
            }
        }
        if (precomp > 0)
        {
            var newPos = new Point2D(current.Pos.X + precomp * dx, current.Pos.Y + precomp * dy);
            var moveCost = isDiag ? SQRT2 : 1.0;
            newG = current.G + precomp * moveCost;
            return new JPSNode(newPos, 0, 0, current.Pos);
        }
        return null;
    }

    private void AddToOpenList(JPSNode node)
    {
        if (_openListPositions.TryGetValue(node.Pos, out var exist))
        {
            if (node.G < exist.G)
            {
                exist.G = node.G;
                exist.Parent = node.Parent;
            }
        }
        else
        {
            _openList.Add(node);
            _openListPositions[node.Pos] = node;
        }
    }

    private Dir GetTravelDirection(JPSNode cur)
    {
        if (cur.Parent.X == -1) return Dir.START;
        var dx = Math.Sign(cur.Pos.X - cur.Parent.X);
        var dy = Math.Sign(cur.Pos.Y - cur.Parent.Y);
        for (var i = 0; i < 8; i++)
            if (Dirs[i].dx == dx && Dirs[i].dy == dy) return (Dir)i;
        return Dir.START;
    }

    private static double OctileDistance(Point2D a, Point2D b)
    {
        var dx = Math.Abs(a.X - b.X);
        var dy = Math.Abs(a.Y - b.Y);
        return (dx + dy) + (SQRT2 - 2) * Math.Min(dx, dy);
    }

    private bool IsGoal(Point2D p) => p.X == _goalPos.X && p.Y == _goalPos.Y;

    private static SearchStep NodeOutput(JPSNode n, bool isGoal) =>
        new SearchStep(n.Pos, n.Parent, n.G, isGoal);
}
