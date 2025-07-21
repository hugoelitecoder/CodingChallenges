using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var vampireCount = int.Parse(inputs[0]);
        var zombieCount = int.Parse(inputs[1]);
        var ghostCount = int.Parse(inputs[2]);
        var size = int.Parse(Console.ReadLine());
        var topSights = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
        var bottomSights = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
        var leftSights = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
        var rightSights = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
        var grid = new char[size, size];
        for (var i = 0; i < size; i++)
        {
            var row = Console.ReadLine();
            for (var j = 0; j < size; j++)
            {
                grid[i, j] = row[j];
            }
        }

        var initialManor = new Manor(vampireCount, zombieCount, ghostCount, size, topSights, bottomSights, leftSights, rightSights, grid);
        var solver = new HauntedManorSolver();
        var resultGrid = solver.Solve(initialManor);
        
        for (var r = 0; r < size; r++)
        {
            var rowStr = new StringBuilder();
            for (var c = 0; c < size; c++)
            {
                rowStr.Append(resultGrid[r, c]);
            }
            Console.WriteLine(rowStr.ToString());
        }
    }
}

public class Manor
{
    public readonly int Size;
    public readonly int ReqVamps;
    public readonly int ReqZombies;
    public readonly int ReqGhosts;
    public readonly int[] Top;
    public readonly int[] Bottom;
    public readonly int[] Left;
    public readonly int[] Right;
    public char[,] Grid;
    public int PlacedVamps;
    public int PlacedZombies;
    public int PlacedGhosts;

    public Manor(int v, int z, int g, int s, int[] t, int[] b, int[] l, int[] r, char[,] gr)
    {
        ReqVamps = v;
        ReqZombies = z;
        ReqGhosts = g;
        Size = s;
        Top = t;
        Bottom = b;
        Left = l;
        Right = r;
        Grid = gr;
        PlacedVamps = 0;
        PlacedZombies = 0;
        PlacedGhosts = 0;
    }

    public Manor(Manor other)
    {
        Size = other.Size;
        ReqVamps = other.ReqVamps;
        ReqZombies = other.ReqZombies;
        ReqGhosts = other.ReqGhosts;
        Top = other.Top;
        Bottom = other.Bottom;
        Left = other.Left;
        Right = other.Right;
        Grid = (char[,])other.Grid.Clone();
        PlacedVamps = other.PlacedVamps;
        PlacedZombies = other.PlacedZombies;
        PlacedGhosts = other.PlacedGhosts;
    }
}

public class HauntedManorSolver
{
    private List<(int r, int c)> _emptyCells;
    private Manor _config;

    public char[,] Solve(Manor config)
    {
        _config = config;
        _emptyCells = FindEmptyCells(config.Grid);
        var solutionState = SolveRecursive(config, 0);
        return solutionState?.Grid;
    }

    private Manor SolveRecursive(Manor currentState, int emptyCellIndex)
    {
        if (IsStateImpossible(currentState))
        {
            return null;
        }
        if (emptyCellIndex == _emptyCells.Count)
        {
            return currentState;
        }
        
        var (r, c) = _emptyCells[emptyCellIndex];
        if (currentState.PlacedGhosts < _config.ReqGhosts)
        {
            var nextState = new Manor(currentState);
            nextState.Grid[r, c] = 'G';
            nextState.PlacedGhosts++;
            var result = SolveRecursive(nextState, emptyCellIndex + 1);
            if (result != null) return result;
        }
        if (currentState.PlacedVamps < _config.ReqVamps)
        {
            var nextState = new Manor(currentState);
            nextState.Grid[r, c] = 'V';
            nextState.PlacedVamps++;
            var result = SolveRecursive(nextState, emptyCellIndex + 1);
            if (result != null) return result;
        }
        if (currentState.PlacedZombies < _config.ReqZombies)
        {
            var nextState = new Manor(currentState);
            nextState.Grid[r, c] = 'Z';
            nextState.PlacedZombies++;
            var result = SolveRecursive(nextState, emptyCellIndex + 1);
            if (result != null) return result;
        }
        
        return null;
    }

    private bool IsStateImpossible(Manor state)
    {
        var remainingVamps = _config.ReqVamps - state.PlacedVamps;
        var remainingZombies = _config.ReqZombies - state.PlacedZombies;
        var remainingGhosts = _config.ReqGhosts - state.PlacedGhosts;
        var totalPlaced = state.PlacedVamps + state.PlacedZombies + state.PlacedGhosts;
        var remainingEmpty = _emptyCells.Count - totalPlaced;
        if (remainingVamps < 0 || remainingZombies < 0 || remainingGhosts < 0 || 
            remainingVamps + remainingZombies + remainingGhosts != remainingEmpty)
        {
            return true;
        }
        
        var sights = new[] { _config.Top, _config.Bottom, _config.Left, _config.Right };
        var starts = new Func<int, (int r, int c)>[]
        {
            i => (0, i),
            i => (state.Size - 1, i),
            i => (i, 0),
            i => (i, state.Size - 1)
        };
        var dirs = new (int dr, int dc)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
        for (var d = 0; d < 4; d++)
        {
            for (var i = 0; i < state.Size; i++)
            {
                var (r, c) = starts[d](i);
                var (dr, dc) = dirs[d];
                var (current, potential) = TracePath(state.Grid, r, c, dr, dc);
                if (current > sights[d][i] || current + potential < sights[d][i]) return true;
            }
        }
        
        return false;
    }
    
    private (int current, int potential) TracePath(char[,] grid, int r, int c, int dr, int dc)
    {
        var size = grid.GetLength(0);
        var currentSighting = 0;
        var potentialSighting = 0;
        var inMirror = false;
        
        while (r >= 0 && r < size && c >= 0 && c < size)
        {
            var cell = grid[r, c];
            if (cell == '\\')
            {
                inMirror = true;
                (dr, dc) = (dc, dr);
            }
            else if (cell == '/')
            {
                inMirror = true;
                (dr, dc) = (-dc, -dr);
            }
            else if (cell == '.')
            {
                potentialSighting++;
            }
            else
            {
                var isVisible = (cell == 'Z') || (cell == 'V' && !inMirror) || (cell == 'G' && inMirror);
                if (isVisible)
                {
                    currentSighting++;
                }
            }
            r += dr;
            c += dc;
        }
        return (currentSighting, potentialSighting);
    }

    private List<(int r, int c)> FindEmptyCells(char[,] grid)
    {
        var empty = new List<(int, int)>();
        var size = grid.GetLength(0);
        for (var r = 0; r < size; r++)
        {
            for (var c = 0; c < size; c++)
            {
                if (grid[r, c] == '.')
                {
                    empty.Add((r, c));
                }
            }
        }
        return empty;
    }
}

