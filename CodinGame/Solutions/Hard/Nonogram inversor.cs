using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var width = int.Parse(inputs[0]);
        var height = int.Parse(inputs[1]);
        var colClues = new List<int>[width];
        for (var i = 0; i < width; i++)
        {
            var line = Console.ReadLine();
            colClues[i] = string.IsNullOrWhiteSpace(line)
                ? new List<int>()
                : line.Split(' ').Select(int.Parse).ToList();
        }
        var rowClues = new List<int>[height];
        for (var i = 0; i < height; i++)
        {
            var line = Console.ReadLine();
            rowClues[i] = string.IsNullOrWhiteSpace(line)
                ? new List<int>()
                : line.Split(' ').Select(int.Parse).ToList();
        }

        var puzzle = new NonogramPuzzle(width, height, colClues, rowClues);
        var (whiteColClues, whiteRowClues) = puzzle.SolveAndGetWhiteClues();

        foreach (var clueLine in whiteColClues)
        {
            Console.WriteLine(clueLine);
        }
        foreach (var clueLine in whiteRowClues)
        {
            Console.WriteLine(clueLine);
        }
    }
}

public enum CellState { Unknown, White, Black }

public class NonogramPuzzle
{
    private readonly int _width;
    private readonly int _height;
    private readonly List<int>[] _colClues;
    private readonly List<int>[] _rowClues;
    private CellState[,] _grid;
    private readonly Dictionary<string, List<bool[]>> _permCache = new Dictionary<string, List<bool[]>>();

    public NonogramPuzzle(int width, int height, List<int>[] colClues, List<int>[] rowClues)
    {
        _width = width;
        _height = height;
        _colClues = colClues;
        _rowClues = rowClues;
        _grid = new CellState[height, width];
    }

    public (List<string> WhiteColClues, List<string> WhiteRowClues) SolveAndGetWhiteClues()
    {
        SolveGrid();
        return GenerateWhiteClues();
    }

    private void SolveGrid()
    {
        var changed = true;
        while (changed)
        {
            changed = false;
            for (var r = 0; r < _height; r++)
            {
                var originalRow = GetRow(r);
                if (!originalRow.Contains(CellState.Unknown)) continue;
                var newRow = SolveLine(originalRow, _rowClues[r]);
                if (UpdateRow(r, newRow))
                {
                    changed = true;
                }
            }
            for (var c = 0; c < _width; c++)
            {
                var originalCol = GetCol(c);
                if (!originalCol.Contains(CellState.Unknown)) continue;
                var newCol = SolveLine(originalCol, _colClues[c]);
                if (UpdateCol(c, newCol))
                {
                    changed = true;
                }
            }
        }
    }
    
    private (List<string>, List<string>) GenerateWhiteClues()
    {
        var whiteColClues = new List<string>();
        for (var c = 0; c < _width; c++)
        {
            var groups = new List<int>();
            var currentCount = 0;
            for (var r = 0; r < _height; r++)
            {
                if (_grid[r, c] == CellState.White)
                {
                    currentCount++;
                }
                else
                {
                    if (currentCount > 0)
                    {
                        groups.Add(currentCount);
                    }
                    currentCount = 0;
                }
            }
            if (currentCount > 0)
            {
                groups.Add(currentCount);
            }
            whiteColClues.Add(groups.Count > 0 ? string.Join(" ", groups) : "0");
        }

        var whiteRowClues = new List<string>();
        for (var r = 0; r < _height; r++)
        {
            var groups = new List<int>();
            var currentCount = 0;
            for (var c = 0; c < _width; c++)
            {
                if (_grid[r, c] == CellState.White)
                {
                    currentCount++;
                }
                else
                {
                    if (currentCount > 0)
                    {
                        groups.Add(currentCount);
                    }
                    currentCount = 0;
                }
            }
            if (currentCount > 0)
            {
                groups.Add(currentCount);
            }
            whiteRowClues.Add(groups.Count > 0 ? string.Join(" ", groups) : "0");
        }
        return (whiteColClues, whiteRowClues);
    }

    private CellState[] SolveLine(CellState[] line, List<int> clues)
    {
        var length = line.Length;
        var allPerms = GetPermutations(length, clues);
        var validPerms = new List<bool[]>();

        foreach (var perm in allPerms)
        {
            var isConsistent = true;
            for (var i = 0; i < length; i++)
            {
                var permCellState = perm[i] ? CellState.Black : CellState.White;
                if (line[i] != CellState.Unknown && line[i] != permCellState)
                {
                    isConsistent = false;
                    break;
                }
            }
            if (isConsistent)
            {
                validPerms.Add(perm);
            }
        }
        
        if (validPerms.Count == 0) return line;

        var newLine = (CellState[])line.Clone();
        for (var i = 0; i < length; i++)
        {
            if (newLine[i] != CellState.Unknown) continue;
            var firstState = validPerms[0][i];
            if (validPerms.Skip(1).All(p => p[i] == firstState))
            {
                newLine[i] = firstState ? CellState.Black : CellState.White;
            }
        }
        return newLine;
    }

    private List<bool[]> GetPermutations(int length, List<int> clues)
    {
        var key = $"{length}:{string.Join(",", clues)}";
        if (_permCache.TryGetValue(key, out var cachedPerms))
        {
            return cachedPerms;
        }

        var results = new List<bool[]>();
        GeneratePermsRecursive(0, 0, clues, new bool[length], results, length);
        _permCache[key] = results;
        return results;
    }

    private void GeneratePermsRecursive(int lineIdx, int clueIdx, List<int> clues, bool[] currentLine, List<bool[]> results, int length)
    {
        if (clueIdx == clues.Count)
        {
            var finalLine = (bool[])currentLine.Clone();
            for (var i = lineIdx; i < length; i++) finalLine[i] = false;
            results.Add(finalLine);
            return;
        }

        var clueLength = clues[clueIdx];
        var remainingCluesCount = clues.Count - (clueIdx + 1);
        var remainingSpace = 0;
        if (remainingCluesCount > 0)
        {
            remainingSpace = clues.Skip(clueIdx + 1).Sum() + remainingCluesCount;
        }

        var maxStartPos = length - remainingSpace - clueLength;
        for (var startPos = lineIdx; startPos <= maxStartPos; startPos++)
        {
            var nextLine = (bool[])currentLine.Clone();
            for (var i = lineIdx; i < startPos; i++) nextLine[i] = false;
            for (var i = 0; i < clueLength; i++) nextLine[startPos + i] = true;

            var nextLineIdx = startPos + clueLength;
            if (clueIdx < clues.Count - 1)
            {
                if (nextLineIdx < length)
                {
                    nextLine[nextLineIdx] = false;
                    nextLineIdx++;
                }
                else
                {
                    continue;
                }
            }
            GeneratePermsRecursive(nextLineIdx, clueIdx + 1, clues, nextLine, results, length);
        }
    }

    private CellState[] GetRow(int r)
    {
        var row = new CellState[_width];
        for (var c = 0; c < _width; c++)
        {
            row[c] = _grid[r, c];
        }
        return row;
    }

    private bool UpdateRow(int r, CellState[] newRow)
    {
        var changed = false;
        for (var c = 0; c < _width; c++)
        {
            if (_grid[r, c] != newRow[c])
            {
                _grid[r, c] = newRow[c];
                changed = true;
            }
        }
        return changed;
    }

    private CellState[] GetCol(int c)
    {
        var col = new CellState[_height];
        for (var r = 0; r < _height; r++)
        {
            col[r] = _grid[r, c];
        }
        return col;
    }

    private bool UpdateCol(int c, CellState[] newCol)
    {
        var changed = false;
        for (var r = 0; r < _height; r++)
        {
            if (_grid[r, c] != newCol[r])
            {
                _grid[r, c] = newCol[r];
                changed = true;
            }
        }
        return changed;
    }
}