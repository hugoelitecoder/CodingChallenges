using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
class Solution
{
    public static void Main(string[] args)
    {
        var totalSw = Stopwatch.StartNew();
        var sw = int.Parse(Console.ReadLine());
        var sh = int.Parse(Console.ReadLine());
        var lh = ReadInts();
        var th = ReadInts();
        var rh = ReadInts();
        var bh = ReadInts();
        Console.Error.WriteLine($"[DEBUG] Initialization complete in {totalSw.ElapsedMilliseconds}ms");
        var solver = new MagnetSolver(sw, sh, lh, th, rh, bh);
        var initialized = false;
        while (true)
        {
            var board = ReadBoard(sw, sh);
            if (!initialized)
            {
                var solveSw = Stopwatch.StartNew();
                solver.Initialize(board);
                if (solver.Solve(0))
                {
                    Console.Error.WriteLine($"[DEBUG] Puzzle solved in {solveSw.ElapsedMilliseconds}ms");
                    solver.PrepareOutput(board);
                }
                initialized = true;
            }
            Console.WriteLine(solver.GetNextMove());
        }
    }
    private static int[] ReadInts()
    {
        return Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
    }
    private static char[,] ReadBoard(int w, int h)
    {
        var b = new char[h, w];
        for (var i = 0; i < h; i++)
        {
            var r = Console.ReadLine();
            for (var j = 0; j < w; j++) b[i, j] = r[j];
        }
        return b;
    }
}
class MagnetSolver
{
    private readonly int _w, _h;
    private readonly int[] _lh, _th, _rh, _bh;
    private readonly int[] _rp, _rn, _cp, _cn;
    private char[,] _g;
    private readonly List<Domino> _dms = new List<Domino>();
    private readonly Queue<string> _out = new Queue<string>();
    private struct Pos { public int x, y; }
    private class Domino { public Pos p1, p2; }
    public MagnetSolver(int w, int h, int[] lh, int[] th, int[] rh, int[] bh)
    {
        _w = w; _h = h;
        _lh = lh; _th = th; _rh = rh; _bh = bh;
        _rp = new int[h]; _rn = new int[h];
        _cp = new int[w]; _cn = new int[w];
    }
    public void Initialize(char[,] board)
    {
        _g = (char[,])board.Clone();
        var v = new bool[_h, _w];
        for (var y = 0; y < _h; y++)
        {
            for (var x = 0; x < _w; x++)
            {
                if (v[y, x]) continue;
                var c = _g[y, x];
                v[y, x] = true;
                if (x + 1 < _w && _g[y, x + 1] == c)
                {
                    _dms.Add(new Domino { p1 = new Pos { x = x, y = y }, p2 = new Pos { x = x + 1, y = y } });
                    v[y, x + 1] = true;
                }
                else if (y + 1 < _h && _g[y + 1, x] == c)
                {
                    _dms.Add(new Domino { p1 = new Pos { x = x, y = y }, p2 = new Pos { x = x, y = y + 1 } });
                    v[y + 1, x] = true;
                }
            }
        }
    }
    public bool Solve(int idx)
    {
        if (idx == _dms.Count) return IsFinal();
        if (!Prune(idx)) return false;
        var d = _dms[idx];
        if (TryPlace(d, '+', '-')) { if (Solve(idx + 1)) return true; Backtrack(d, '+', '-'); }
        if (TryPlace(d, '-', '+')) { if (Solve(idx + 1)) return true; Backtrack(d, '-', '+'); }
        if (TryPlace(d, 'x', 'x')) { if (Solve(idx + 1)) return true; Backtrack(d, 'x', 'x'); }
        return false;
    }
    public void PrepareOutput(char[,] cb)
    {
        for (var y = 0; y < _h; y++)
        {
            for (var x = 0; x < _w; x++)
            {
                if (cb[y, x] != '+' && cb[y, x] != '-' && cb[y, x] != 'x')
                {
                    _out.Enqueue($"{x} {y} {_g[y, x]}");
                    cb[y, x] = _g[y, x];
                    var p = GetPartner(x, y);
                    cb[p.y, p.x] = _g[p.y, p.x];
                }
            }
        }
    }
    public string GetNextMove() => _out.Count > 0 ? _out.Dequeue() : "0 0 x";
    private bool TryPlace(Domino d, char s1, char s2)
    {
        if (s1 != 'x')
        {
            if (!IsSafe(d.p1, s1) || !IsSafe(d.p2, s2)) return false;
            if (!CheckHints(d, s1, s2)) return false;
        }
        UpdateState(d, s1, s2, 1);
        _g[d.p1.y, d.p1.x] = s1;
        _g[d.p2.y, d.p2.x] = s2;
        return true;
    }
    private void Backtrack(Domino d, char s1, char s2)
    {
        UpdateState(d, s1, s2, -1);
        _g[d.p1.y, d.p1.x] = '.';
        _g[d.p2.y, d.p2.x] = '.';
    }
    private void UpdateState(Domino d, char s1, char s2, int v)
    {
        if (s1 == '+') { _rp[d.p1.y] += v; _cp[d.p1.x] += v; }
        else if (s1 == '-') { _rn[d.p1.y] += v; _cn[d.p1.x] += v; }
        if (s2 == '+') { _rp[d.p2.y] += v; _cp[d.p2.x] += v; }
        else if (s2 == '-') { _rn[d.p2.y] += v; _cn[d.p2.x] += v; }
    }
    private bool CheckHints(Domino d, char s1, char s2)
    {
        var p1p = s1 == '+' ? 1 : 0;
        var p1n = s1 == '-' ? 1 : 0;
        var p2p = s2 == '+' ? 1 : 0;
        var p2n = s2 == '-' ? 1 : 0;
        if (_lh[d.p1.y] != -1 && _rp[d.p1.y] + p1p + (d.p1.y == d.p2.y ? p2p : 0) > _lh[d.p1.y]) return false;
        if (_rh[d.p1.y] != -1 && _rn[d.p1.y] + p1n + (d.p1.y == d.p2.y ? p2n : 0) > _rh[d.p1.y]) return false;
        if (_th[d.p1.x] != -1 && _cp[d.p1.x] + p1p > _th[d.p1.x]) return false;
        if (_bh[d.p1.x] != -1 && _cn[d.p1.x] + p1n > _bh[d.p1.x]) return false;
        if (_th[d.p2.x] != -1 && _cp[d.p2.x] + p2p > _th[d.p2.x]) return false;
        if (_bh[d.p2.x] != -1 && _cn[d.p2.x] + p2n > _bh[d.p2.x]) return false;
        if (d.p1.y != d.p2.y)
        {
            if (_lh[d.p2.y] != -1 && _rp[d.p2.y] + p2p > _lh[d.p2.y]) return false;
            if (_rh[d.p2.y] != -1 && _rn[d.p2.y] + p2n > _rh[d.p2.y]) return false;
        }
        return true;
    }
    private bool IsSafe(Pos p, char s)
    {
        if (p.y > 0 && _g[p.y - 1, p.x] == s) return false;
        if (p.x > 0 && _g[p.y, p.x - 1] == s) return false;
        if (p.y < _h - 1 && _g[p.y + 1, p.x] == s) return false;
        if (p.x < _w - 1 && _g[p.y, p.x + 1] == s) return false;
        return true;
    }
    private bool Prune(int idx)
    {
        var remR = new int[_h];
        var remC = new int[_w];
        for (var i = idx; i < _dms.Count; i++)
        {
            var d = _dms[i];
            remR[d.p1.y]++; remR[d.p2.y]++;
            remC[d.p1.x]++; remC[d.p2.x]++;
        }
        for (var i = 0; i < _h; i++)
        {
            if (_lh[i] != -1 && (_rp[i] > _lh[i] || _rp[i] + remR[i] < _lh[i])) return false;
            if (_rh[i] != -1 && (_rn[i] > _rh[i] || _rn[i] + remR[i] < _rh[i])) return false;
        }
        for (var i = 0; i < _w; i++)
        {
            if (_th[i] != -1 && (_cp[i] > _th[i] || _cp[i] + remC[i] < _th[i])) return false;
            if (_bh[i] != -1 && (_cn[i] > _bh[i] || _cn[i] + remC[i] < _bh[i])) return false;
        }
        return true;
    }
    private bool IsFinal()
    {
        for (var i = 0; i < _h; i++)
        {
            if (_lh[i] != -1 && _rp[i] != _lh[i]) return false;
            if (_rh[i] != -1 && _rn[i] != _rh[i]) return false;
        }
        for (var i = 0; i < _w; i++)
        {
            if (_th[i] != -1 && _cp[i] != _th[i]) return false;
            if (_bh[i] != -1 && _cn[i] != _bh[i]) return false;
        }
        return true;
    }
    private Pos GetPartner(int x, int y)
    {
        foreach (var d in _dms)
        {
            if (d.p1.x == x && d.p1.y == y) return d.p2;
            if (d.p2.x == x && d.p2.y == y) return d.p1;
        }
        return new Pos { x = x, y = y };
    }
}