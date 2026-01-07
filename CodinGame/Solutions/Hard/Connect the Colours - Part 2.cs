using System;
using System.Collections.Generic;
using System.Diagnostics;
class Solution
{
    static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var line1 = Console.ReadLine();
        if (line1 == null) return;
        var parts = line1.Split(' ');
        var h = int.Parse(parts[0]);
        var w = int.Parse(parts[1]);
        var rows = new string[h];
        for (var i = 0; i < h; i++) rows[i] = Console.ReadLine();
        var readTime = sw.ElapsedMilliseconds;
        sw.Restart();
        var game = new ConnectTheColours(h, w, rows);
        var setupTime = sw.ElapsedMilliseconds;
        sw.Restart();
        var result = game.Solve();
        sw.Stop();
        var solveTime = sw.ElapsedMilliseconds;
        Console.Error.WriteLine($"[DEBUG] Grid: {h}x{w}");
        Console.Error.WriteLine($"[DEBUG] Components: {game.ColorCount} Colors, {game.InitialEmptyCount} Empty Cells");
        Console.Error.WriteLine($"[DEBUG] Result: {result.Count} Segments, Solved: {game.Solved}");
        Console.Error.WriteLine($"[DEBUG] Time Breakdown:");
        Console.Error.WriteLine($"[DEBUG]   - Read Input: {readTime}ms");
        Console.Error.WriteLine($"[DEBUG]   - Setup/Parse: {setupTime}ms");
        Console.Error.WriteLine($"[DEBUG]   - Pathfinding: {solveTime}ms");
        foreach (var segment in result) Console.WriteLine(segment);
    }
}
class ConnectTheColours
{
    public int H, W, Size, InitialEmptyCount, ColorCount;
    public bool Solved { get; private set; }
    int[] _board;
    List<int> _colors;
    Dictionary<int, List<int>> _endpoints;
    int _currentEmpty;
    Stopwatch _sw;
    List<string> _resultSegments;
    int[] _neighbors;
    int[] _bfsQ;
    int[] _visitedToken;
    int _bfsToken;
    public ConnectTheColours(int h, int w, string[] rows)
    {
        H = h; W = w; Size = H * W;
        _board = new int[Size];
        _colors = new List<int>();
        _endpoints = new Dictionary<int, List<int>>();
        InitialEmptyCount = 0;
        for (var r = 0; r < H; r++)
        {
            var row = rows[r];
            for (var c = 0; c < W; c++)
            {
                var idx = r * W + c;
                var ch = row[c];
                if (ch == '.') { _board[idx] = -1; InitialEmptyCount++; }
                else
                {
                    var color = ch - '0';
                    _board[idx] = color;
                    if (!_endpoints.ContainsKey(color)) { _endpoints[color] = new List<int>(); _colors.Add(color); }
                    _endpoints[color].Add(idx);
                }
            }
        }
        _colors.Sort((a, b) => {
            var d1 = Manhattan(_endpoints[a][0], _endpoints[a][1]);
            var d2 = Manhattan(_endpoints[b][0], _endpoints[b][1]);
            return d1.CompareTo(d2);
        });
        ColorCount = _colors.Count;
        _currentEmpty = InitialEmptyCount;
        _sw = new Stopwatch();
        _resultSegments = new List<string>();
        _neighbors = new int[Size * 4];
        _bfsQ = new int[Size];
        _visitedToken = new int[Size];
        InitializeNeighbors();
        Solved = false;
    }
    void InitializeNeighbors()
    {
        for (var i = 0; i < Size; i++)
        {
            int r = i / W, c = i % W, k = 0;
            _neighbors[i * 4 + k++] = r > 0 ? i - W : -1;
            _neighbors[i * 4 + k++] = r < H - 1 ? i + W : -1;
            _neighbors[i * 4 + k++] = c > 0 ? i - 1 : -1;
            _neighbors[i * 4 + k++] = c < W - 1 ? i + 1 : -1;
        }
    }
    public List<string> Solve()
    {
        _sw.Start();
        Solved = SolveColors(0);
        _sw.Stop();
        return _resultSegments;
    }
    bool SolveColors(int idx)
    {
        if (idx == ColorCount) return _currentEmpty == 0;
        if (_sw.ElapsedMilliseconds > 1900) return false;
        var c = _colors[idx];
        var s = _endpoints[c][0];
        var e = _endpoints[c][1];
        if (!CanReach(s, e, c)) return false;
        return FindPath(c, s, e, idx);
    }
    bool FindPath(int c, int curr, int target, int idx)
    {
        if (_sw.ElapsedMilliseconds > 1900) return false;
        if (curr == target)
        {
            if (idx < ColorCount - 1 && !CheckFuture(idx + 1)) return false;
            return SolveColors(idx + 1);
        }
        int m0 = -1, m1 = -1, m2 = -1, m3 = -1;
        int s0 = int.MaxValue, s1 = int.MaxValue, s2 = int.MaxValue, s3 = int.MaxValue;
        int cnt = 0;
        int bIdx = curr * 4;
        for (int i = 0; i < 4; i++)
        {
            int n = _neighbors[bIdx + i];
            if (n != -1 && (n == target || _board[n] == -1))
            {
                int score = (n == target ? 0 : 1) << 24 | (GetDegree(n) & 0xFF) << 16 | Manhattan(n, target);
                if (cnt == 0) { m0 = n; s0 = score; }
                else if (cnt == 1) { m1 = n; s1 = score; }
                else if (cnt == 2) { m2 = n; s2 = score; }
                else { m3 = n; s3 = score; }
                cnt++;
            }
        }
        if (cnt > 1)
        {
            if (s0 > s1) { Swap(ref s0, ref s1); Swap(ref m0, ref m1); }
            if (cnt > 2 && s1 > s2) { Swap(ref s1, ref s2); Swap(ref m1, ref m2); if (s0 > s1) { Swap(ref s0, ref s1); Swap(ref m0, ref m1); } }
            if (cnt > 3 && s2 > s3) { Swap(ref s2, ref s3); Swap(ref m2, ref m3); if (s1 > s2) { Swap(ref s1, ref s2); Swap(ref m1, ref m2); if (s0 > s1) { Swap(ref s0, ref s1); Swap(ref m0, ref m1); } } }
        }
        for (int i = 0; i < cnt; i++)
        {
            int n = (i == 0) ? m0 : (i == 1 ? m1 : (i == 2 ? m2 : m3));
            bool isT = n == target;
            if (!isT)
            {
                _board[n] = c; _currentEmpty--;
                if (IsDeadEnd(n, target, c)) { _board[n] = -1; _currentEmpty++; continue; }
            }
            if (FindPath(c, n, target, idx))
            {
                AddSegment(curr, n, c); return true;
            }
            if (!isT) { _board[n] = -1; _currentEmpty++; }
        }
        return false;
    }
    void Swap(ref int a, ref int b) { int t = a; a = b; b = t; }
    bool IsDeadEnd(int p, int tgt, int c)
    {
        int bIdx = p * 4;
        for (int i = 0; i < 4; i++)
        {
            int n = _neighbors[bIdx + i];
            if (n != -1 && _board[n] == -1)
            {
                int d = 0, nb = n * 4;
                bool ep = _board[n] >= 0; 
                for (int j = 0; j < 4; j++)
                {
                    int nn = _neighbors[nb + j];
                    if (nn != -1)
                    {
                        if (_board[nn] == -1) d++;
                        else if (nn == tgt && _board[nn] == c) d++;
                        else if (_board[nn] >= 0) d++;
                    }
                }
                if (ep) { if (d < 1) return true; }
                else if (d < 2) return true;
            }
        }
        return false;
    }
    int GetDegree(int p) { int d = 0; for (int i = 0; i < 4; i++) { int n = _neighbors[p * 4 + i]; if (n != -1 && _board[n] == -1) d++; } return d; }
    bool CheckFuture(int idx) { for (int k = idx; k < ColorCount; k++) { int c = _colors[k]; if (!CanReach(_endpoints[c][0], _endpoints[c][1], c)) return false; } return true; }
    bool CanReach(int s, int e, int c)
    {
        _bfsToken++; int head = 0, tail = 0; _bfsQ[tail++] = s; _visitedToken[s] = _bfsToken;
        while (head < tail)
        {
            int curr = _bfsQ[head++];
            if (curr == e) return true;
            int b = curr * 4;
            for (int i = 0; i < 4; i++)
            {
                int n = _neighbors[b + i];
                if (n != -1 && _visitedToken[n] != _bfsToken)
                {
                    if (n == e || _board[n] == -1) { _visitedToken[n] = _bfsToken; _bfsQ[tail++] = n; }
                }
            }
        }
        return false;
    }
    int Manhattan(int a, int b) { int r1 = a / W, c1 = a % W, r2 = b / W, c2 = b % W; return (r1 > r2 ? r1 - r2 : r2 - r1) + (c1 > c2 ? c1 - c2 : c2 - c1); }
    void AddSegment(int f, int t, int c) { int r1 = f / W, c1 = f % W, r2 = t / W, c2 = t % W; _resultSegments.Add($"{c1} {r1} {c2} {r2} {c}"); }
}