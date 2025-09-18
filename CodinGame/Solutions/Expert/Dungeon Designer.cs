using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

public class Solution
{
    public static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var w = int.Parse(inputs[0]);
        var h = int.Parse(inputs[1]);
        inputs = Console.ReadLine().Split(' ');
        var p = int.Parse(inputs[0]);
        var q = int.Parse(inputs[1]);
        var r = int.Parse(inputs[2]);

        var dungeon = new Dungeon(w, h, p, q, r);
        dungeon.Generate();
        dungeon.PlaceTreasureAndInterceptor();
        dungeon.Print();
    }
}

public class Dungeon
{
    private readonly int _w;
    private readonly int _h;
    private readonly BigInteger _r;
    private readonly BigInteger _m;
    private readonly BigInteger _lambda;

    private char[,] _map;
    private readonly Dictionary<int, bool> _prngCache;

    public Dungeon(int w, int h, int p, int q, int r)
    {
        _w = w;
        _h = h;
        _r = r;
        
        var p_bi = new BigInteger(p);
        var q_bi = new BigInteger(q);
        _m = p_bi * q_bi;

        var pMinus1 = p_bi - 1;
        var qMinus1 = q_bi - 1;
        _lambda = (pMinus1 * qMinus1) / BigInteger.GreatestCommonDivisor(pMinus1, qMinus1);

        _prngCache = new Dictionary<int, bool>();
    }

    public void Generate()
    {
        _map = new char[2 * _h + 1, 2 * _w + 1];

        for (var y = 0; y <= 2 * _h; y++)
        {
            for (var x = 0; x <= 2 * _w; x++)
            {
                if (y == 0 || y == 2 * _h || x == 0 || x == 2 * _w)
                {
                    _map[y, x] = '#';
                }
                else if (x % 2 == 0 && y % 2 == 0)
                {
                    _map[y, x] = '#';
                }
                else
                {
                    _map[y, x] = '.';
                }
            }
        }
        
        for (var y = 0; y < _h - 1; y++)
        {
            for (var x = 0; x < _w - 1; x++)
            {
                var cx = 2 * x + 1;
                var cy = 2 * y + 1;
                if (TossCoin(x, y))
                {
                    _map[cy, cx + 1] = '#';
                }
                else 
                {
                    _map[cy + 1, cx] = '#';
                }
            }
        }

        _map[0, 1] = '.';
        _map[2 * _h, 2 * _w - 1] = '.';
    }
    
    public void PlaceTreasureAndInterceptor()
    {
        var startNode = new Point2D(0, 0);
        var barracksNode = new Point2D(_w - 1, _h - 1);

        var distances = new Dictionary<Point2D, int>();
        var cameFrom = new Dictionary<Point2D, Point2D>();
        var queue = new Queue<Point2D>();
        
        distances[startNode] = 0;
        queue.Enqueue(startNode);

        var farthestNode = startNode;
        var maxDist = 0;
        
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (distances[current] > maxDist)
            {
                maxDist = distances[current];
                farthestNode = current;
            }

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = distances[current] + 1;
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        var treasureNode = farthestNode;
        _map[2 * treasureNode.Y + 1, 2 * treasureNode.X + 1] = 'T';

        var pathToTreasure = new HashSet<Point2D>();
        var pathCurrent = treasureNode;
        while (!pathCurrent.Equals(startNode))
        {
            pathToTreasure.Add(pathCurrent);
            pathCurrent = cameFrom[pathCurrent];
        }
        pathToTreasure.Add(startNode);

        var visitedFromBarracks = new HashSet<Point2D>();
        queue.Clear();
        queue.Enqueue(barracksNode);
        visitedFromBarracks.Add(barracksNode);
        
        while(queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (pathToTreasure.Contains(current))
            {
                if (!current.Equals(treasureNode))
                {
                     _map[2 * current.Y + 1, 2 * current.X + 1] = 'X';
                }
                break;
            }

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!visitedFromBarracks.Contains(neighbor))
                {
                    visitedFromBarracks.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    public void Print()
    {
        var sb = new StringBuilder();
        for (var y = 0; y <= 2 * _h; y++)
        {
            for (var x = 0; x <= 2 * _w; x++)
            {
                sb.Append(_map[y, x]);
            }
            if (y < 2 * _h)
            {
                sb.AppendLine();
            }
        }
        Console.WriteLine(sb.ToString());
    }

    private bool TossCoin(int x, int y)
    {
        var key = y * _w + x;
        if (_prngCache.TryGetValue(key, out var cachedResult))
        {
            return cachedResult;
        }

        var exponent_inner = (BigInteger)(x + (long)y * _w + 1);
        var exponent_outer = BigInteger.ModPow(2, exponent_inner, _lambda);
        var randomNumber = BigInteger.ModPow(_r, exponent_outer, _m);
        var result = !randomNumber.IsEven;
        _prngCache[key] = result;
        return result;
    }

    private IEnumerable<Point2D> GetNeighbors(Point2D p)
    {
        if (p.X < _w - 1 && _map[2 * p.Y + 1, 2 * p.X + 2] == '.') yield return new Point2D(p.X + 1, p.Y);
        if (p.X > 0 && _map[2 * p.Y + 1, 2 * p.X] == '.') yield return new Point2D(p.X - 1, p.Y);
        if (p.Y < _h - 1 && _map[2 * p.Y + 2, 2 * p.X + 1] == '.') yield return new Point2D(p.X, p.Y + 1);
        if (p.Y > 0 && _map[2 * p.Y, 2 * p.X + 1] == '.') yield return new Point2D(p.X, p.Y - 1);
    }
}

public readonly struct Point2D : IEquatable<Point2D>
{
    public readonly int X;
    public readonly int Y;

    public Point2D(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(Point2D other) => X == other.X && Y == other.Y;
    public override bool Equals(object obj) => obj is Point2D other && Equals(other);
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = 17;
            hashCode = hashCode * 23 + X.GetHashCode();
            hashCode = hashCode * 23 + Y.GetHashCode();
            return hashCode;
        }
    }
}
