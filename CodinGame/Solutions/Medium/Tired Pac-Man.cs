using System;
using System.Collections.Generic;
using System.Diagnostics;

class Solution
{
    public static void Main(string[] args)
    {
        var inputs = ReadLine().Split(' ');
        var w = int.Parse(inputs[0]);
        var h = int.Parse(inputs[1]);
        var energy = int.Parse(ReadLine());
        var rows = new string[h];
        for (var i = 0; i < h; i++)
        {
            rows[i] = ReadLine();
        }
        var sw = Stopwatch.StartNew();
        var game = new PacManGame(w, h, energy, rows);
        var result = game.Solve();
        sw.Stop();
        Console.WriteLine(result);
        Console.Error.WriteLine($"[DEBUG] Input: w={w}, h={h}, energy={energy}");
        Console.Error.WriteLine($"[DEBUG] Fruit Count: {game.FruitCount}");
        Console.Error.WriteLine($"[DEBUG] Calculation time: {sw.ElapsedMilliseconds}ms");
    }
    
    private static string ReadLine() => Console.ReadLine();
}

internal class Fruit
{
    public int R { get; }
    public int C { get; }
    public int Value { get; }
    public Fruit(int r, int c, int value)
    {
        R = r;
        C = c;
        Value = value;
    }
}

internal class PacManGame
{
    private readonly int _w;
    private readonly int _h;
    private readonly int _initialEnergy;
    private readonly string[] _rows;
    private bool[][] _isSafe;
    private List<Fruit> _fruits;
    private Dictionary<(int, int), int> _fruitCoordsToId;
    private Dictionary<(int r, int c, int e, long m), int> _memo;
    private int _startR;
    private int _startC;

    public int FruitCount => _fruits.Count;

    public PacManGame(int w, int h, int energy, string[] rows)
    {
        _w = w;
        _h = h;
        _initialEnergy = energy;
        _rows = rows;
        ParseInput();
    }

    public int Solve()
    {
        _memo = new Dictionary<(int r, int c, int e, long m), int>();
        long initialMask = 0;
        var initialScore = 0;
        if (_fruitCoordsToId.TryGetValue((_startR, _startC), out var fruitId))
        {
            initialMask = 1L << fruitId;
            initialScore = _fruits[fruitId].Value;
        }
        return initialScore + FindMaxScore(_startR, _startC, _initialEnergy, initialMask);
    }

    private void ParseInput()
    {
        _fruits = new List<Fruit>();
        _fruitCoordsToId = new Dictionary<(int, int), int>();
        var ghosts = new List<(int r, int c)>();
        _startR = -1;
        _startC = -1;
        for (var i = 0; i < _h; i++)
        {
            for (var j = 0; j < _w; j++)
            {
                var cell = _rows[i][j];
                if (cell == 'P')
                {
                    _startR = i;
                    _startC = j;
                }
                else if (cell == 'G')
                {
                    ghosts.Add((i, j));
                }
                var fruitValue = GetFruitValue(cell);
                if (fruitValue > 0)
                {
                    var fruitId = _fruits.Count;
                    _fruits.Add(new Fruit(i, j, fruitValue));
                    _fruitCoordsToId[(i, j)] = fruitId;
                }
            }
        }
        SetupSafetyGrid(ghosts);
    }

    private void SetupSafetyGrid(List<(int r, int c)> ghosts)
    {
        _isSafe = new bool[_h][];
        for (var i = 0; i < _h; i++)
        {
            _isSafe[i] = new bool[_w];
            for (var j = 0; j < _w; j++)
            {
                _isSafe[i][j] = _rows[i][j] != '#';
            }
        }
        var dr = new[] { -1, 1, 0, 0 };
        var dc = new[] { 0, 0, -1, 1 };
        foreach (var ghost in ghosts)
        {
            for (var i = 0; i < 4; i++)
            {
                var nr = (ghost.r + dr[i] + _h) % _h;
                var nc = (ghost.c + dc[i] + _w) % _w;
                _isSafe[nr][nc] = false;
            }
        }
    }

    private int FindMaxScore(int r, int c, int energy, long mask)
    {
        var state = (r, c, energy, mask);
        if (_memo.TryGetValue(state, out var cachedScore))
        {
            return cachedScore;
        }
        var maxBonusScore = 0;
        var moves = GetPossibleMoves(r, c);
        foreach (var move in moves)
        {
            if (energy >= move.cost && _isSafe[move.nr][move.nc])
            {
                var bonus = 0;
                var nextMask = mask;
                if (_fruitCoordsToId.TryGetValue((move.nr, move.nc), out var fruitId))
                {
                    if ((mask & (1L << fruitId)) == 0)
                    {
                        nextMask |= (1L << fruitId);
                        bonus = _fruits[fruitId].Value;
                    }
                }
                var remainingEnergy = energy - move.cost;
                maxBonusScore = Math.Max(maxBonusScore, bonus + FindMaxScore(move.nr, move.nc, remainingEnergy, nextMask));
            }
        }
        _memo[state] = maxBonusScore;
        return maxBonusScore;
    }

    private List<(int nr, int nc, int cost)> GetPossibleMoves(int r, int c)
    {
        var moves = new List<(int nr, int nc, int cost)>();
        if (r > 0) moves.Add((r - 1, c, 1)); else moves.Add((_h - 1, c, 3));
        if (r < _h - 1) moves.Add((r + 1, c, 1)); else moves.Add((0, c, 3));
        if (c > 0) moves.Add((r, c - 1, 1)); else moves.Add((r, _w - 1, 3));
        if (c < _w - 1) moves.Add((r, c + 1, 1)); else moves.Add((r, 0, 3));
        return moves;
    }

    private int GetFruitValue(char c)
    {
        switch (c)
        {
            case '*': return 5;
            case '.': return 1;
            case ')': return 3;
            default: return 0;
        }
    }
}
