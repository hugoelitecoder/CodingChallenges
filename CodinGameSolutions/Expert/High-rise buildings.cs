using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var northClues = Array.ConvertAll(Console.ReadLine().Split(' '), int.Parse);
        var westClues = Array.ConvertAll(Console.ReadLine().Split(' '), int.Parse);
        var eastClues = Array.ConvertAll(Console.ReadLine().Split(' '), int.Parse);
        var southClues = Array.ConvertAll(Console.ReadLine().Split(' '), int.Parse);
        
        var grid = new int[n, n];
        for (var i = 0; i < n; i++)
        {
            var rowInputs = Console.ReadLine().Split(' ');
            for (var j = 0; j < n; j++)
            {
                grid[i, j] = int.Parse(rowInputs[j]);
            }
        }
        
        var solver = new SkyscraperSolver(n, northClues, westClues, eastClues, southClues, grid);
        var solvedGrid = solver.Solve();
        
        for (var i = 0; i < n; i++)
        {
            var line = new string[n];
            for (var j = 0; j < n; j++)
            {
                line[j] = solvedGrid[i, j].ToString();
            }
            Console.WriteLine(string.Join(" ", line));
        }
    }
}

public class SkyscraperSolver
{
    private readonly int _n;
    private readonly int[] _northClues;
    private readonly int[] _westClues;
    private readonly int[] _eastClues;
    private readonly int[] _southClues;
    private readonly int[,] _grid;
    private List<PermutationInfo> _allPerms;

    public SkyscraperSolver(int n, int[] north, int[] west, int[] east, int[] south, int[,] grid)
    {
        _n = n;
        _northClues = north;
        _westClues = west;
        _eastClues = east;
        _southClues = south;
        _grid = grid;
        _allPerms = new List<PermutationInfo>();
    }

    public int[,] Solve()
    {
        PrecomputeAllPermutations();
        var rowPossibilities = GetInitialPossibilities(_westClues, _eastClues);
        var colPossibilities = GetInitialPossibilities(_northClues, _southClues);

        var changed = true;
        while (changed)
        {
            changed = false;
            if (FilterPossibilitiesByGrid(rowPossibilities, colPossibilities)) changed = true;
            if (UpdateGridFromUniqueChoices(rowPossibilities, colPossibilities)) changed = true;
            if (CrossFilterPossibilities(rowPossibilities, colPossibilities)) changed = true;
        }
        return _grid;
    }

    private void PrecomputeAllPermutations()
    {
        var p = new int[_n];
        var used = new bool[_n + 1];
        GeneratePermsRecursive(0, p, used);
    }
    
    private void GeneratePermsRecursive(int k, int[] p, bool[] used)
    {
        if (k == _n)
        {
            var values = (int[])p.Clone();
            _allPerms.Add(new PermutationInfo
            {
                Values = values,
                VisibleFront = CalculateVisibility(values, false),
                VisibleBack = CalculateVisibility(values, true)
            });
            return;
        }
        for (var i = 1; i <= _n; i++)
        {
            if (!used[i])
            {
                used[i] = true;
                p[k] = i;
                GeneratePermsRecursive(k + 1, p, used);
                used[i] = false;
            }
        }
    }

    private int CalculateVisibility(int[] line, bool reversed)
    {
        var count = 0;
        var maxH = 0;
        for (var i = 0; i < _n; i++)
        {
            var index = reversed ? _n - 1 - i : i;
            if (line[index] > maxH)
            {
                maxH = line[index];
                count++;
            }
        }
        return count;
    }

    private List<int>[] GetInitialPossibilities(int[] frontClues, int[] backClues)
    {
        var possibilities = new List<int>[_n];
        for (var i = 0; i < _n; i++)
        {
            possibilities[i] = new List<int>();
            var frontClue = frontClues[i];
            var backClue = backClues[i];
            for (var pIdx = 0; pIdx < _allPerms.Count; pIdx++)
            {
                var p = _allPerms[pIdx];
                var matchFront = (frontClue == 0) || (p.VisibleFront == frontClue);
                var matchBack = (backClue == 0) || (p.VisibleBack == backClue);
                if (matchFront && matchBack)
                {
                    possibilities[i].Add(pIdx);
                }
            }
        }
        return possibilities;
    }
    
    private bool FilterPossibilitiesByGrid(List<int>[] rowPoss, List<int>[] colPoss)
    {
        var changed = false;
        for (var r = 0; r < _n; r++)
        {
            for (var c = 0; c < _n; c++)
            {
                if (_grid[r, c] != 0)
                {
                    var val = _grid[r, c];
                    if (rowPoss[r].RemoveAll(pIdx => _allPerms[pIdx].Values[c] != val) > 0) changed = true;
                    if (colPoss[c].RemoveAll(pIdx => _allPerms[pIdx].Values[r] != val) > 0) changed = true;
                }
            }
        }
        return changed;
    }
    
    private bool UpdateGridFromUniqueChoices(List<int>[] rowPoss, List<int>[] colPoss)
    {
        var changed = false;
        for (var i = 0; i < _n; i++)
        {
            for (var j = 0; j < _n; j++)
            {
                if (_grid[i, j] == 0 && rowPoss[i].Count > 0)
                {
                    var firstVal = _allPerms[rowPoss[i][0]].Values[j];
                    if (rowPoss[i].Skip(1).All(pIdx => _allPerms[pIdx].Values[j] == firstVal))
                    {
                        _grid[i, j] = firstVal;
                        changed = true;
                    }
                }
            }
            for (var j = 0; j < _n; j++)
            {
                if (_grid[j, i] == 0 && colPoss[i].Count > 0)
                {
                    var firstVal = _allPerms[colPoss[i][0]].Values[j];
                    if (colPoss[i].Skip(1).All(pIdx => _allPerms[pIdx].Values[j] == firstVal))
                    {
                        _grid[j, i] = firstVal;
                        changed = true;
                    }
                }
            }
        }
        return changed;
    }

    private bool CrossFilterPossibilities(List<int>[] rowPoss, List<int>[] colPoss)
    {
        var changed = false;
        for (var r = 0; r < _n; r++)
        {
            for (var c = 0; c < _n; c++)
            {
                if (_grid[r, c] != 0) continue;
                
                var possibleRowVals = new bool[_n + 1];
                foreach (var pIdx in rowPoss[r])
                {
                    possibleRowVals[_allPerms[pIdx].Values[c]] = true;
                }
                
                var finalPossibleVals = new bool[_n + 1];
                foreach (var pIdx in colPoss[c])
                {
                    var val = _allPerms[pIdx].Values[r];
                    if (possibleRowVals[val])
                    {
                        finalPossibleVals[val] = true;
                    }
                }
                
                if (rowPoss[r].RemoveAll(pIdx => !finalPossibleVals[_allPerms[pIdx].Values[c]]) > 0) changed = true;
                if (colPoss[c].RemoveAll(pIdx => !finalPossibleVals[_allPerms[pIdx].Values[r]]) > 0) changed = true;
            }
        }
        return changed;
    }
}

public class PermutationInfo
{
    public int[] Values { get; set; }
    public int VisibleFront { get; set; }
    public int VisibleBack { get; set; }
}