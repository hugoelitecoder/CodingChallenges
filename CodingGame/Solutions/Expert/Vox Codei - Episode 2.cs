using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var dim = Console.ReadLine().Split(' ');
        var w = int.Parse(dim[0]);
        var h = int.Parse(dim[1]);
        var game = new VoxCodei2Game(w, h);
        var turn = 1;
        while (true)
        {
            var st = Console.ReadLine().Split(' ');
            var turnsLeft = int.Parse(st[0]);
            var bombsLeft = int.Parse(st[1]);
            var map = new List<string>();
            for (var i = 0; i < h; i++) map.Add(Console.ReadLine());
            var (move, units, bombs) = game.Step(turn, bombsLeft, turnsLeft, map);
            Debug(map, units, bombs, turn, w, h);
            Console.WriteLine(move);
            turn++;
        }
    }

    static void Debug(List<string> map, List<Node> units, List<Bomb> bombs, int turn, int w, int h)
    {
        var grid = map.Select(row => row.ToCharArray()).ToArray();
        foreach (var b in bombs)
        {
            var p = b.Pos;
            if (p.Y >= 0 && p.Y < h && p.X >= 0 && p.X < w) grid[p.Y][p.X] = '*';
        }
        foreach (var u in units)
        {
            var p = u.At(turn);
            if (p.Y >= 0 && p.Y < h && p.X >= 0 && p.X < w) grid[p.Y][p.X] = (char)('0' + u.Id % 10);
        }
        Console.Error.WriteLine($"[DEBUG] Turn {turn}");
        foreach (var row in grid) Console.Error.WriteLine($"[DEBUG] {new string(row)}");
    }
}

public class VoxCodei2Game
{
    // Tweakable performance constants
    public const int PathRounds = 100;
    public const int BlastRadius = 3;
    public const int WaitTurns = 3;
    public const int MaxBranch = 15;

    readonly int _w, _h;
    readonly char[,] _wall;
    readonly List<Node> _units = new();
    readonly List<Bomb> _bombs = new();
    bool _detected;
    readonly List<List<string>> _obs = new();
    public BombPlan _plan;
    static readonly Point[] Dir = { new(0, -1), new(0, 1), new(-1, 0), new(1, 0), new(0, 0) };

    public VoxCodei2Game(int w, int h)
    {
        _w = w; _h = h; _wall = new char[h, w];
    }

    public (string, List<Node>, List<Bomb>) Step(int turn, int bombsLeft, int turnsLeft, List<string> map)
    {
        UpdateBombs(turn);

        if (turn <= WaitTurns)
        {
            _obs.Add(map);
            return ("WAIT", new List<Node>(), _bombs);
        }

        if (!_detected)
        {
            DetectUnits();
            if (!_detected) return ("WAIT", new List<Node>(), _bombs);
        }

        RemoveMissing(map, turn);

        var live = _units.Where(u => !u.Dead).ToList();
        if (!live.Any()) return ("WAIT", live, _bombs);

        if (_plan == null || _plan.Done(turn) || _plan.AllDead())
            _plan = BombPlanFinder.Best(live, _bombs, turn, bombsLeft, turnsLeft, _w, _h, _wall);

        var bomb = _plan.BombAt(turn);
        if (bomb.HasValue)
        {
            _bombs.Add(new Bomb(bomb.Value, turn));
            return ($"{bomb.Value.X} {bomb.Value.Y}", live, _bombs);
        }
        return ("WAIT", live, _bombs);
    }

    void DetectUnits()
    {
        for (var y = 0; y < _h; y++)
            for (var x = 0; x < _w; x++)
                _wall[y, x] = _obs[0][y][x] == '#' ? '#' : '.';

        var s1 = GetUnits(_obs[0]);
        var s2 = GetUnits(_obs[1]).ToHashSet();
        var s3 = GetUnits(_obs[2]).ToHashSet();

        var all = s1.Select(pos1 =>
            Dir.Select(vel => Node.Step(pos1, vel, _w, _h, _wall))
            .Select(t1 =>
            {
                var t2 = Node.Step(t1.pos, t1.vel, _w, _h, _wall);
                return new Path(pos1, t1.pos, t2.pos, t1.vel);
            })
            .Where(p => s2.Contains(p.P2) && s3.Contains(p.P3)).ToList()
        ).ToList();

        var found = new List<Path>();
        if (Find(all, 0, new List<Path>(), found, s2, s3))
        {
            _detected = true;
            _units.AddRange(found.Select((p, i) => new Node(i, p.P1, p.Vel)));
            foreach (var u in _units) u.Cache(PathRounds, _w, _h, _wall);
        }
    }

    static bool Find(List<List<Path>> all, int idx, List<Path> cur, List<Path> fin, ISet<Point> s2, ISet<Point> s3)
    {
        if (idx == all.Count)
        {
            if (cur.Select(p => p.P2).ToHashSet().SetEquals(s2) && cur.Select(p => p.P3).ToHashSet().SetEquals(s3))
            {
                fin.AddRange(cur);
                return true;
            }
            return false;
        }
        foreach (var p in all[idx])
        {
            cur.Add(p);
            if (Find(all, idx + 1, cur, fin, s2, s3)) return true;
            cur.RemoveAt(cur.Count - 1);
        }
        return false;
    }

    void UpdateBombs(int turn)
    {
        var now = _bombs.Where(b => b.Explode == turn).ToList();
        if (!now.Any()) return;
        var done = new HashSet<Bomb>();
        while (now.Any())
        {
            foreach (var b in now) done.Add(b);
            var blast = now.SelectMany(b => BombPlanFinder.Blast(b.Pos, _w, _h, _wall)).ToHashSet();
            now = _bombs.Where(b => !done.Contains(b) && blast.Contains(b.Pos)).ToList();
        }
        var area = done.SelectMany(b => BombPlanFinder.Blast(b.Pos, _w, _h, _wall)).ToHashSet();
        foreach (var u in _units.Where(u => !u.Dead && area.Contains(u.At(turn))))
            u.Dead = true;
        _bombs.RemoveAll(b => done.Contains(b));
    }

    void RemoveMissing(List<string> map, int turn)
    {
        var posSet = GetUnits(map).ToHashSet();
        foreach (var u in _units.Where(u => !u.Dead))
            if (!posSet.Contains(u.At(turn)))
                u.Dead = true;
    }

    List<Point> GetUnits(List<string> map) =>
        Enumerable.Range(0, _h).SelectMany(y =>
            Enumerable.Range(0, _w)
                .Where(x => map[y][x] == '@')
                .Select(x => new Point(x, y))
        ).ToList();
}

public static class BombPlanFinder
{
    static readonly int[] dx = { 0, 0, -1, 1 };
    static readonly int[] dy = { -1, 1, 0, 0 };
    static Dictionary<(HashSet<Node>, int), BombPlan> _cache;
    static int _limit;

    public static BombPlan Best(List<Node> units, List<Bomb> bombs, int turn, int bombsLeft, int turnsLeft, int w, int h, char[,] wall)
    {
        _cache = new Dictionary<(HashSet<Node>, int), BombPlan>(new UnitSetCmp());
        _limit = turn + turnsLeft;
        var doomed = bombs.SelectMany(b => units.Where(u => Blast(b.Pos, w, h, wall).Contains(u.At(b.Explode)))).ToHashSet();
        var toHit = units.Except(doomed).ToHashSet();
        if (!toHit.Any()) return new BombPlan(new List<BombStep>());
        var steps = Steps(toHit, turn, w, h, wall);
        var byUnit = toHit.ToDictionary(u => u, u => steps.Where(s => s.Targets.Contains(u)).ToList());
        return Plan(toHit, bombsLeft, byUnit, turn) ?? new BombPlan(new List<BombStep>());
    }

    static BombPlan Plan(HashSet<Node> hit, int bombs, Dictionary<Node, List<BombStep>> byUnit, int minTurn)
    {
        if (!hit.Any()) return new BombPlan(new List<BombStep>());
        if (bombs == 0) return null;
        var key = (hit, minTurn);
        if (_cache.TryGetValue(key, out var found)) return found;
        Node tgt = null; var min = int.MaxValue;
        foreach (var u in hit)
        {
            var cnt = byUnit[u].Count(s => s.Turn >= minTurn);
            if (cnt < min) { min = cnt; tgt = u; }
        }
        if (tgt == null || !byUnit[tgt].Any(s => s.Turn >= minTurn)) { _cache[key] = null; return null; }
        var candidates = byUnit[tgt].Where(s => s.Turn >= minTurn && s.Explode < _limit)
            .OrderByDescending(s => s.Targets.Count(hit.Contains)).ThenBy(s => s.Explode).Take(VoxCodei2Game.MaxBranch);
        foreach (var s in candidates)
        {
            var left = new HashSet<Node>(hit); left.ExceptWith(s.Targets);
            var sub = Plan(left, bombs - 1, byUnit, s.Turn + 1);
            if (sub != null)
            {
                var plan = new BombPlan(new List<BombStep>(sub.Steps) { s });
                _cache[key] = plan;
                return plan;
            }
        }
        _cache[key] = null;
        return null;
    }

    static List<BombStep> Steps(HashSet<Node> units, int turn, int w, int h, char[,] wall)
    {
        var posByTurn = Enumerable.Range(turn, Math.Max(0, _limit - turn)).ToDictionary(r => r, _ => new HashSet<Point>());
        foreach (var u in units) foreach (var r in posByTurn.Keys) if (r < VoxCodei2Game.PathRounds) posByTurn[r].Add(u.At(r));
        var res = new List<BombStep>();
        for (var y = 0; y < h; y++) for (var x = 0; x < w; x++)
        {
            if (wall[y, x] == '#') continue;
            var p = new Point(x, y);
            var area = Blast(p, w, h, wall);
            foreach (var t in posByTurn.Keys)
            {
                if (t + VoxCodei2Game.BlastRadius >= _limit || posByTurn[t].Contains(p)) continue;
                var exp = t + VoxCodei2Game.BlastRadius;
                var targets = units.Where(u => area.Contains(u.At(exp))).ToHashSet();
                if (targets.Any()) res.Add(new BombStep(p, t, exp, targets));
            }
        }
        return res;
    }

    public static HashSet<Point> Blast(Point p, int w, int h, char[,] wall)
    {
        var area = new HashSet<Point> { p };
        for (int d = 0; d < 4; d++)
        {
            for (int i = 1; i <= VoxCodei2Game.BlastRadius; i++)
            {
                var nx = p.X + dx[d] * i;
                var ny = p.Y + dy[d] * i;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h || wall[ny, nx] == '#') break;
                area.Add(new Point(nx, ny));
            }
        }
        return area;
    }
}

public class Node
{
    public int Id { get; }
    public bool Dead;
    readonly Point _start, _vel;
    Point[] _cache;
    public Node(int id, Point pos, Point vel) { Id = id; _start = pos; _vel = vel; }
    public void Cache(int rounds, int w, int h, char[,] wall)
    {
        _cache = new Point[rounds];
        _cache[0] = new Point(-1, -1);
        var p = _start; var v = _vel;
        for (var i = 1; i < rounds; i++)
        {
            _cache[i] = p;
            (p, v) = Step(p, v, w, h, wall);
        }
    }
    public Point At(int turn) => _cache[turn];
    public static (Point pos, Point vel) Step(Point pos, Point vel, int w, int h, char[,] wall)
    {
        var next = new Point(pos.X + vel.X, pos.Y + vel.Y);
        var nv = vel;
        if (next.X < 0 || next.X >= w || next.Y < 0 || next.Y >= h || wall[next.Y, next.X] == '#')
            nv = new Point(-vel.X, -vel.Y);
        return (new Point(pos.X + nv.X, pos.Y + nv.Y), nv);
    }
    public override int GetHashCode() => Id;
    public override bool Equals(object obj) => obj is Node n && n.Id == Id;
}

public class BombPlan
{
    public List<BombStep> Steps { get; }
    int _last;
    public BombPlan(List<BombStep> steps)
    {
        Steps = steps.OrderBy(s => s.Turn).ToList();
        _last = Steps.Any() ? Steps.Max(s => s.Turn) : -1;
    }
    public bool Done(int turn) => turn > _last;
    public Point? BombAt(int turn) => Steps.FirstOrDefault(s => s.Turn == turn)?.Pos;
    public bool AllDead() => Steps.All(s => s.Targets.All(n => n.Dead));
}

public record Bomb(Point Pos, int Turn)
{
    public int Explode => Turn + VoxCodei2Game.BlastRadius;
}

public record BombStep(Point Pos, int Turn, int Explode, HashSet<Node> Targets);

public readonly record struct Point(int X, int Y)
{
    public override string ToString() => $"{X} {Y}";
}

public record Path(Point P1, Point P2, Point P3, Point Vel);

public class UnitSetCmp : IEqualityComparer<(HashSet<Node> units, int turn)>
{
    public bool Equals((HashSet<Node> units, int turn) x, (HashSet<Node> units, int turn) y) =>
        x.turn == y.turn && x.units.SetEquals(y.units);

    public int GetHashCode((HashSet<Node> units, int turn) obj) =>
        obj.units.OrderBy(n => n.Id).Aggregate(obj.turn, (h, n) => h * 31 + n.GetHashCode());
}
