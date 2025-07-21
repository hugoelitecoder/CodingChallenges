using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

public class Solution
{
    public static void Main(string[] args)
    {
        int nOriginal = int.Parse(Console.ReadLine());
        var inputSegments = new List<Segment>();
        for (int i = 0; i < nOriginal; i++)
        {
            var parts = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
            var p1 = new Point2D(parts[0], parts[1]);
            var p2 = new Point2D(parts[2], parts[3]);
            inputSegments.Add(new Segment(p1, p2));
        }

        var primalGraph = new PlanarGraph(inputSegments);
        var faces = primalGraph.FindFaces();
        
        if (!faces.Any())
        {
            Console.WriteLine(nOriginal);
            return;
        }

        var dualGraph = new DualGraph(faces);

        var oddVertices = dualGraph.GetOddDegreeVertices();
        if (!oddVertices.Any())
        {
            Console.WriteLine(nOriginal);
            return;
        }

        long extraCrossings = MinWeightPerfectMatcher.Solve(oddVertices.ToImmutableSortedSet(), dualGraph.DistanceMatrix);

        Console.WriteLine(nOriginal + extraCrossings);
    }
}

public class PlanarGraph
{
    private readonly List<Segment> _segments;

    public PlanarGraph(List<Segment> initialSegments)
    {
        var allPoints = new HashSet<Point2D>();
        foreach (var s in initialSegments)
        {
            allPoints.Add(s.P1);
            allPoints.Add(s.P2);
        }

        AddNeighbors(initialSegments);
        var pointDegrees = CalculatePointDegrees(initialSegments, allPoints);
        _segments = RemoveEndPoints(initialSegments, pointDegrees);
    }

    public List<List<Segment>> FindFaces()
    {
        var cycleSet = new HashSet<List<Segment>>(new UnorderedSegmentListComparer());
        foreach (var segment in _segments)
        {
            cycleSet.Add(TraceCycle(segment, segment.P2, true));
            cycleSet.Add(TraceCycle(segment, segment.P2, false));
        }
        return cycleSet.Where(c => c != null).ToList();
    }

    private List<Segment> TraceCycle(Segment startSegment, Point2D startPoint, bool leftTurn)
    {
        var path = new List<Segment> { startSegment };
        var currentSegment = startSegment;
        var commonPoint = startPoint;
        int maxPathLength = 55; 

        for (int i = 0; i < maxPathLength; i++)
        {
            var nextSegment = Turn(currentSegment, commonPoint, leftTurn);
            if (nextSegment == null) return null;

            if (nextSegment == startSegment)
            {
                return !commonPoint.Equals(startPoint) ? path : null;
            }

            if (path.Contains(nextSegment)) return null;

            path.Add(nextSegment);
            commonPoint = nextSegment.OtherPoint(commonPoint);
            currentSegment = nextSegment;
        }
        return null;
    }

    private Segment Turn(Segment current, Point2D commonPoint, bool leftTurn)
    {
        var candidates = current.Neighbors.Where(n => (n.P1.Equals(commonPoint) || n.P2.Equals(commonPoint)) && n != current).ToList();
        if (!candidates.Any()) return null;

        Func<Segment, double> keySelector = s => current.Angle(s, commonPoint);
        return leftTurn ? candidates.OrderBy(keySelector).FirstOrDefault() : candidates.OrderByDescending(keySelector).FirstOrDefault();
    }
    
    private static void AddNeighbors(List<Segment> allSegments)
    {
        for (int i = 0; i < allSegments.Count; i++)
            for (int j = i + 1; j < allSegments.Count; j++)
                if (allSegments[i].IsNeighbor(allSegments[j]))
                {
                    allSegments[i].Neighbors.Add(allSegments[j]);
                    allSegments[j].Neighbors.Add(allSegments[i]);
                }
    }

    private static Dictionary<Point2D, int> CalculatePointDegrees(List<Segment> allSegments, HashSet<Point2D> allPoints)
    {
        var pointDegrees = allPoints.ToDictionary(p => p, p => 0);
        foreach (var s in allSegments) { pointDegrees[s.P1]++; pointDegrees[s.P2]++; }
        return pointDegrees;
    }

    private static List<Segment> RemoveEndPoints(List<Segment> allSegments, Dictionary<Point2D, int> pointDegrees)
    {
        var segments = new List<Segment>(allSegments);
        while (true)
        {
            var toRemove = segments.Where(s => pointDegrees[s.P1] == 1 || pointDegrees[s.P2] == 1).ToList();
            if (!toRemove.Any()) break;
            foreach (var s in toRemove)
            {
                if (segments.Remove(s))
                {
                    pointDegrees[s.P1]--;
                    pointDegrees[s.P2]--;
                    foreach (var neighbor in s.Neighbors) neighbor.Neighbors.Remove(s);
                }
            }
        }
        return segments;
    }
}

public class DualGraph
{
    private readonly int[] _areaDegrees;
    public int[,] DistanceMatrix { get; }

    public DualGraph(List<List<Segment>> primalFaces)
    {
        int numInternalFaces = primalFaces.Count;
        int numVertices = numInternalFaces + 1;
        _areaDegrees = new int[numVertices];
        
        var adj = Enumerable.Range(0, numVertices).Select(_ => new List<int>()).ToList();
        var segmentToFaces = new Dictionary<Segment, List<int>>();

        for (int i = 0; i < numInternalFaces; i++)
        {
            _areaDegrees[i] = primalFaces[i].Count;
            foreach (var segment in primalFaces[i])
            {
                if (!segmentToFaces.ContainsKey(segment)) segmentToFaces[segment] = new List<int>();
                segmentToFaces[segment].Add(i);
            }
        }

        int outerFaceIndex = numInternalFaces;
        foreach (var faces in segmentToFaces.Values)
        {
            if (faces.Count == 1)
            {
                adj[faces[0]].Add(outerFaceIndex);
                adj[outerFaceIndex].Add(faces[0]);
            }
            else if (faces.Count == 2)
            {
                adj[faces[0]].Add(faces[1]);
                adj[faces[1]].Add(faces[0]);
            }
        }
        _areaDegrees[outerFaceIndex] = segmentToFaces.Values.Count(f => f.Count == 1);

        DistanceMatrix = new int[numVertices, numVertices];
        for (int i = 0; i < numVertices; i++) for (int j = 0; j < numVertices; j++) DistanceMatrix[i, j] = i == j ? 0 : int.MaxValue / 2;
        for (int i = 0; i < numVertices; i++) foreach (var neighbor in adj[i]) DistanceMatrix[i, neighbor] = 1;

        for (int k = 0; k < numVertices; k++)
            for (int i = 0; i < numVertices; i++)
                for (int j = 0; j < numVertices; j++)
                    DistanceMatrix[i, j] = Math.Min(DistanceMatrix[i, j], DistanceMatrix[i, k] + DistanceMatrix[k, j]);
    }

    public List<int> GetOddDegreeVertices()
    {
        return Enumerable.Range(0, _areaDegrees.Length)
                         .Where(i => _areaDegrees[i] % 2 != 0)
                         .ToList();
    }
}

public static class MinWeightPerfectMatcher
{
    public static long Solve(ImmutableSortedSet<int> nodes, int[,] dist)
    {
        return SolveRecursive(nodes, dist, new Dictionary<ImmutableSortedSet<int>, long>());
    }

    private static long SolveRecursive(ImmutableSortedSet<int> nodes, int[,] dist, Dictionary<ImmutableSortedSet<int>, long> memo)
    {
        if (nodes.IsEmpty) return 0;
        if (memo.TryGetValue(nodes, out var cachedResult)) return cachedResult;

        long minCost = long.MaxValue;
        int first = nodes.First();
        var remainingAfterFirst = nodes.Remove(first);

        foreach (var partner in remainingAfterFirst)
        {
            var remainingNodes = remainingAfterFirst.Remove(partner);
            long cost = (long)dist[first, partner] + SolveRecursive(remainingNodes, dist, memo);
            minCost = Math.Min(minCost, cost);
        }
        memo[nodes] = minCost;
        return minCost;
    }
}

public record struct Point2D(int X, int Y) : IComparable<Point2D>
{
    public int CompareTo(Point2D other)
    {
        int xComparison = X.CompareTo(other.X);
        return xComparison != 0 ? xComparison : Y.CompareTo(other.Y);
    }
}

public class Segment : IEquatable<Segment>, IComparable<Segment>
{
    public Point2D P1 { get; }
    public Point2D P2 { get; }
    public List<Segment> Neighbors { get; } = new List<Segment>();

    public Segment(Point2D p1, Point2D p2)
    {
        if (p1.CompareTo(p2) > 0) (p1, p2) = (p2, p1);
        P1 = p1;
        P2 = p2;
    }

    public bool IsNeighbor(Segment other) => P1.Equals(other.P1) || P1.Equals(other.P2) || P2.Equals(other.P1) || P2.Equals(other.P2);
    public Point2D OtherPoint(Point2D p) => p.Equals(P1) ? P2 : P1;
    
    public double Angle(Segment other, Point2D commonPoint)
    {
        var v1 = new Point2D(OtherPoint(commonPoint).X - commonPoint.X, OtherPoint(commonPoint).Y - commonPoint.Y);
        var v2 = new Point2D(other.OtherPoint(commonPoint).X - commonPoint.X, other.OtherPoint(commonPoint).Y - commonPoint.Y);
        double dot = v1.X * v2.X + v1.Y * v2.Y;
        double det = v1.X * v2.Y - v1.Y * v2.X;
        double angle = Math.Atan2(det, dot);
        return angle >= 0 ? angle : angle + 2 * Math.PI;
    }

    public bool Equals(Segment other) => other != null && P1.Equals(other.P1) && P2.Equals(other.P2);
    public override bool Equals(object obj) => Equals(obj as Segment);
    public override int GetHashCode() => HashCode.Combine(P1, P2);
    public int CompareTo(Segment other)
    {
        if (other is null) return 1;
        int p1Comp = P1.CompareTo(other.P1);
        return p1Comp != 0 ? p1Comp : P2.CompareTo(other.P2);
    }
}

public class UnorderedSegmentListComparer : IEqualityComparer<List<Segment>>
{
    public bool Equals(List<Segment> x, List<Segment> y)
    {
        if (x == null || y == null) return x == y;
        if (x.Count != y.Count) return false;
        return x.ToHashSet().SetEquals(y);
    }

    public int GetHashCode(List<Segment> obj)
    {
        if (obj == null) return 0;
        return obj.OrderBy(s => s.GetHashCode()).Aggregate(0, (current, s) => current ^ s.GetHashCode());
    }
}