using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var initialLines = new string[23];
        for (var i = 0; i < 23; i++)
        {
            initialLines[i] = Console.ReadLine();
        }
        var seconds = int.Parse(Console.ReadLine());

        var hourglass = new Hourglass(initialLines, seconds);
        var finalState = hourglass.GetFinalState();
        Console.WriteLine(string.Join("\n", finalState));
    }
}

class Hourglass
{
    private const int GridHeight = 23;
    private const int GridWidth = 23;
    private const int CenterColumn = 11;
    private const int TotalSand = 100;

    private readonly string[] _referenceLines;
    private readonly int _seconds;
    private char[][] _grid;
    private int _grainsToRemoveFromTop;
    private int _grainsToAddToBottom;

    public Hourglass(string[] initialLines, int seconds)
    {
        _referenceLines = initialLines;
        _seconds = seconds;
    }

    public string[] GetFinalState()
    {
        var initialSandCount = _referenceLines.SelectMany(s => s).Count(c => c == 'o');
        if (initialSandCount != TotalSand)
        {
            return new[] { "BROKEN HOURGLASS" };
        }

        CalculateGrainChanges();
        InitializeGrid();
        FillTopCompletely();
        RemoveGrainsFromTop();
        AddGrainsToBottom();

        return _grid.Select(row => new string(row)).ToArray();
    }

    private void CalculateGrainChanges()
    {
        var initialTopGrains = _referenceLines.Skip(1).Take(10).SelectMany(s => s).Count(c => c == 'o');
        var sandToFall = Math.Min(_seconds, initialTopGrains);
        var finalTopGrains = initialTopGrains - sandToFall;

        _grainsToRemoveFromTop = TotalSand - finalTopGrains;
        _grainsToAddToBottom = (TotalSand - initialTopGrains) + sandToFall;
    }

    private void InitializeGrid()
    {
        _grid = new char[GridHeight][];
        for (var i = 0; i < GridHeight; i++)
        {
            _grid[i] = _referenceLines[i].Replace('o', ' ').ToCharArray();
        }
    }

    private void FillTopCompletely()
    {
        for (var row = 1; row <= 10; row++)
        {
            var leftBoundary = _referenceLines[row].IndexOf('\\');
            var rightBoundary = _referenceLines[row].IndexOf('/');
            for (var col = leftBoundary + 1; col < rightBoundary; col++)
            {
                _grid[row][col] = 'o';
            }
        }
    }

    private void RemoveGrainsFromTop()
    {
        if (_grainsToRemoveFromTop <= 0) return;
        
        for (var layer = 0; layer < 10 && _grainsToRemoveFromTop > 0; layer++)
        {
            _grid[1 + layer][CenterColumn] = ' ';
            if (--_grainsToRemoveFromTop == 0) break;

            for (var rowOffset = 0; rowOffset <= layer && _grainsToRemoveFromTop > 0; rowOffset++)
            {
                for (var dist = 1; dist <= 2 && _grainsToRemoveFromTop > 0; dist++)
                {
                    for (var dir = 1; dir >= -1 && _grainsToRemoveFromTop > 0; dir -= 2)
                    {
                        var row = 1 + rowOffset;
                        var col = CenterColumn + (2 * (layer - rowOffset) + dist) * dir;
                        if (col > _referenceLines[row].IndexOf('\\') && col < _referenceLines[row].IndexOf('/'))
                        {
                            _grid[row][col] = ' ';
                            if (--_grainsToRemoveFromTop == 0) break;
                        }
                    }
                }
            }
        }
    }

    private void AddGrainsToBottom()
    {
        if (_grainsToAddToBottom <= 0) return;
        
        for (var i = 0; i < 10 && _grainsToAddToBottom > 0; i++)
        {
            _grid[12 + i][CenterColumn] = 'o';
            _grainsToAddToBottom--;
        }

        if (_grainsToAddToBottom == 0) return;

        for (var layer = 0; layer < 10 && _grainsToAddToBottom > 0; layer++)
        {
            for (var rowOffset = 0; rowOffset <= layer && _grainsToAddToBottom > 0; rowOffset++)
            {
                for (var dir = 1; dir >= -1 && _grainsToAddToBottom > 0; dir -= 2)
                {
                    var row = 21 - rowOffset;
                    var col = CenterColumn + (layer - rowOffset + 1) * dir;
                    if (col > _referenceLines[row].IndexOf('/') && col < _referenceLines[row].IndexOf('\\'))
                    {
                        _grid[row][col] = 'o';
                        if (--_grainsToAddToBottom == 0) break;
                    }
                }
            }
        }
    }
}