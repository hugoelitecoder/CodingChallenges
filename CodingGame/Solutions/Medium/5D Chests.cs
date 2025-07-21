using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int size = int.Parse(Console.ReadLine());
        int mod  = int.Parse(Console.ReadLine());
        var prime = Sieve(mod);
        var generator = new TerrainGenerator(size, mod, prime);
        var map = generator.Generate();
        var analyzer = new RoomAnalyzer(map);
        analyzer.Analyze();
        var (total, rooms, max) = analyzer.Results();
        Console.WriteLine(total);
        Console.WriteLine(rooms);
        Console.WriteLine(max);
    }

    static bool[] Sieve(int limit)
    {
        var sieve = new bool[limit + 1];
        if (limit < 2) return sieve;
        sieve[2] = true;
        for (int i = 3; i <= limit; i += 2)
            sieve[i] = true;
        int m = (int)Math.Sqrt(limit);
        for (int i = 3; i <= m; i += 2)
            if (sieve[i])
                for (int j = i * i; j <= limit; j += 2 * i)
                    sieve[j] = false;
        return sieve;
    }
}

struct Point5D
{
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
    public int W { get; }
    public int V { get; }

    public Point5D(int x, int y, int z, int w, int v)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
        V = v;
    }

    public Point5D Offset(int dx, int dy, int dz, int dw, int dv) =>
        new Point5D(X + dx, Y + dy, Z + dz, W + dw, V + dv);
}

class TerrainGenerator
{
    private readonly int _size, _mod;
    private readonly bool[] _prime;
    public TerrainGenerator(int size, int mod, bool[] prime)
    {
        _size = size; 
        _mod = mod; 
        _prime = prime;
    }
    public bool[,,,,] Generate()
    {
        var map = new bool[_size, _size, _size, _size, _size];
        for (int x = 0; x < _size; x++)
        for (int y = 0; y < _size; y++)
        for (int z = 0; z < _size; z++)
        for (int w = 0; w < _size; w++)
        for (int v = 0; v < _size; v++)
        {
            long prod = (long)(x + 1) * (y + 1) * (z + 1) * (w + 1) * (v + 1);
            int idx = 1 + (int)(prod % _mod);
            map[x, y, z, w, v] = _prime[idx];
        }
        return map;
    }
}

class RoomAnalyzer
{
    private readonly bool[,,,,] _map;
    private readonly bool[,,,,] _visited;
    private readonly int _size;
    private int total, rooms, max;

    public RoomAnalyzer(bool[,,,,] map)
    {
        _map = map;
        _size = map.GetLength(0);
        _visited = new bool[_size, _size, _size, _size, _size];
    }

    public void Analyze()
    {
        var queue = new Queue<Point5D>();

        for (int x = 0; x < _size; x++)
        for (int y = 0; y < _size; y++)
        for (int z = 0; z < _size; z++)
        for (int w = 0; w < _size; w++)
        for (int v = 0; v < _size; v++)
        {
            if (!_map[x, y, z, w, v] || _visited[x, y, z, w, v]) continue;
            rooms++;
            int count = 0;
            _visited[x, y, z, w, v] = true;
            queue.Enqueue(new Point5D(x, y, z, w, v));

            void AddIfValid(Point5D p)
            {
                if (p.X < 0 || p.X >= _size ||
                    p.Y < 0 || p.Y >= _size ||
                    p.Z < 0 || p.Z >= _size ||
                    p.W < 0 || p.W >= _size ||
                    p.V < 0 || p.V >= _size)
                    return;

                if (_visited[p.X, p.Y, p.Z, p.W, p.V] || !_map[p.X, p.Y, p.Z, p.W, p.V])
                    return;

                _visited[p.X, p.Y, p.Z, p.W, p.V] = true;
                queue.Enqueue(p);
            }

            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                count++;

                AddIfValid(p.Offset(-1,  0,  0,  0,  0));
                AddIfValid(p.Offset( 1,  0,  0,  0,  0));
                AddIfValid(p.Offset( 0, -1,  0,  0,  0));
                AddIfValid(p.Offset( 0,  1,  0,  0,  0));
                AddIfValid(p.Offset( 0,  0, -1,  0,  0));
                AddIfValid(p.Offset( 0,  0,  1,  0,  0));
                AddIfValid(p.Offset( 0,  0,  0, -1,  0));
                AddIfValid(p.Offset( 0,  0,  0,  1,  0));
                AddIfValid(p.Offset( 0,  0,  0,  0, -1));
                AddIfValid(p.Offset( 0,  0,  0,  0,  1));
            }

            total += count;
            if (count > max) max = count;
        }
    }

    public (int total, int rooms, int max) Results() => (total, rooms, max);
}
