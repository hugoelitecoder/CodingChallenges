using System;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        var s = Console.ReadLine().Split(' ');
        var n = int.Parse(s[0]);
        var start = int.Parse(s[1]);
        var cluster = new AppleCluster(n);
        for (var i = 0; i < n; i++)
        {
            var d = Console.ReadLine().Split(' ');
            var x = int.Parse(d[0]);
            var y = int.Parse(d[1]);
            var z = int.Parse(d[2]);
            var r = int.Parse(d[3]);
            cluster.Add(i, x, y, z, r);
        }
        cluster.Simulate(start);
        var result = cluster.CountRemaining();
        Console.WriteLine(result);
    }
}

class AppleCluster
{
    class Apple
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public int R { get; }
        public bool Fallen { get; set; }
        public Apple(int x, int y, int z, int r)
        {
            X = x;
            Y = y;
            Z = z;
            R = r;
            Fallen = false;
        }
    }

    private Apple[] _apples;

    public AppleCluster(int n)
    {
        _apples = new Apple[n];
    }

    public void Add(int idx, int x, int y, int z, int r)
    {
        _apples[idx] = new Apple(x, y, z, r);
    }

    public void Simulate(int start)
    {
        var queue = new Queue<int>();
        _apples[start].Fallen = true;
        queue.Enqueue(start);
        var n = _apples.Length;
        while (queue.Count > 0)
        {
            var idx = queue.Dequeue();
            var faller = _apples[idx];
            for (var i = 0; i < n; i++)
            {
                var target = _apples[i];
                if (target.Fallen) continue;
                if (target.Z >= faller.Z) continue;
                var dx = faller.X - target.X;
                var dy = faller.Y - target.Y;
                var dr = faller.R + target.R;
                var distSq = (long)dx * dx + (long)dy * dy;
                if (distSq <= (long)dr * dr)
                {
                    target.Fallen = true;
                    queue.Enqueue(i);
                }
            }
        }
    }

    public int CountRemaining()
    {
        var cnt = 0;
        for (var i = 0; i < _apples.Length; i++)
        {
            if (!_apples[i].Fallen) cnt++;
        }
        return cnt;
    }
}
