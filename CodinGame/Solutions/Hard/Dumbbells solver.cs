using System;
using System.Collections.Generic;
using System.Diagnostics;

class Solution
{
    public static void Main()
    {
        var count = int.Parse(Console.ReadLine());
        var dims = Console.ReadLine().Split(' ');
        var height = int.Parse(dims[0]);
        var width = int.Parse(dims[1]);
        var grid = new char[height, width];
        for (var i = 0; i < height; i++)
        {
            var line = Console.ReadLine();
            for (var j = 0; j < width; j++)
                grid[i, j] = line[j];
        }
        var box = new DumbbellBox(count, height, width, grid);
        box.SolveAndPrint();
    }
}

class DumbbellBox
{
    private int _count;
    private int _height;
    private int _width;
    private char[,] _input;
    private ulong _requiredMask;
    private List<(ulong mask, ulong ends, int type, int i, int j)> _placements;
    private List<int>[] _coverings;
    private int _found;
    private char[,] _solution;
    private Stopwatch _stopwatch;
    private int _recursionCalls;

    public DumbbellBox(int count, int height, int width, char[,] input)
    {
        _count = count;
        _height = height;
        _width = width;
        _input = input;
        _placements = new List<(ulong, ulong, int, int, int)>();
        _coverings = new List<int>[_height * _width];
        for (var idx = 0; idx < _height * _width; ++idx)
            _coverings[idx] = new List<int>();
        _requiredMask = 0;
        for (var i = 0; i < _height; i++)
            for (var j = 0; j < _width; j++)
                if (_input[i, j] == 'o')
                    _requiredMask |= 1UL << (i * _width + j);
        for (var i = 0; i < _height; i++)
            for (var j = 0; j < _width; j++)
            {
                if (j + 2 < _width)
                {
                    var mask = (1UL << (i * _width + j)) | (1UL << (i * _width + (j + 1))) | (1UL << (i * _width + (j + 2)));
                    var ends = (1UL << (i * _width + j)) | (1UL << (i * _width + (j + 2)));
                    _placements.Add((mask, ends, 0, i, j));
                    _coverings[i * _width + j].Add(_placements.Count - 1);
                    _coverings[i * _width + j + 2].Add(_placements.Count - 1);
                }
                if (i + 2 < _height)
                {
                    var mask = (1UL << (i * _width + j)) | (1UL << ((i + 1) * _width + j)) | (1UL << ((i + 2) * _width + j));
                    var ends = (1UL << (i * _width + j)) | (1UL << ((i + 2) * _width + j));
                    _placements.Add((mask, ends, 1, i, j));
                    _coverings[i * _width + j].Add(_placements.Count - 1);
                    _coverings[(i + 2) * _width + j].Add(_placements.Count - 1);
                }
            }
        _found = 0;
        _solution = new char[_height, _width];
        _stopwatch = new Stopwatch();
        _recursionCalls = 0;
    }

    public void SolveAndPrint()
    {
        _stopwatch.Start();
        var picked = new int[_count];
        DFS(0, 0, 0, picked);
        _stopwatch.Stop();
        for (var i = 0; i < _height; i++)
        {
            for (var j = 0; j < _width; j++)
                Console.Write(_solution[i, j]);
            Console.WriteLine();
        }
        Console.Error.WriteLine($"Recursions: {_recursionCalls}");
        Console.Error.WriteLine($"Time elapsed: {_stopwatch.Elapsed.TotalMilliseconds:F2} ms");
    }

    private void DFS(int placed, ulong occupied, ulong ends, int[] picked)
    {
        _recursionCalls++;
        if (_found > 0) return;
        if (placed == _count)
        {
            if ((ends & _requiredMask) == _requiredMask)
            {
                _found++;
                for (var i = 0; i < _height; i++)
                    for (var j = 0; j < _width; j++)
                        _solution[i, j] = '.';
                for (var k = 0; k < _count; k++)
                {
                    var placement = _placements[picked[k]];
                    var type = placement.type;
                    var i = placement.i;
                    var j = placement.j;
                    if (type == 0)
                    {
                        _solution[i, j] = 'o';
                        _solution[i, j + 1] = '-';
                        _solution[i, j + 2] = 'o';
                    }
                    else
                    {
                        _solution[i, j] = 'o';
                        _solution[i + 1, j] = '|';
                        _solution[i + 2, j] = 'o';
                    }
                }
            }
            return;
        }
        var firstUncovered = -1;
        var minWays = int.MaxValue;
        for (var k = 0; k < _height * _width; ++k)
            if (((_requiredMask & (1UL << k)) != 0) && ((ends & (1UL << k)) == 0))
            {
                var ways = 0;
                foreach (var idx in _coverings[k])
                    if ((occupied & _placements[idx].mask) == 0) ways++;
                if (ways == 0) return;
                if (ways < minWays)
                {
                    minWays = ways;
                    firstUncovered = k;
                }
            }
        if (firstUncovered != -1)
        {
            foreach (var idx in _coverings[firstUncovered])
            {
                var mask = _placements[idx].mask;
                var endmask = _placements[idx].ends;
                if ((occupied & mask) != 0) continue;
                picked[placed] = idx;
                DFS(placed + 1, occupied | mask, ends | endmask, picked);
                if (_found > 0) return;
            }
        }
        else
        {
            for (var idx = 0; idx < _placements.Count; ++idx)
            {
                var mask = _placements[idx].mask;
                var endmask = _placements[idx].ends;
                if ((occupied & mask) != 0) continue;
                picked[placed] = idx;
                DFS(placed + 1, occupied | mask, ends | endmask, picked);
                if (_found > 0) return;
            }
        }
    }
}
