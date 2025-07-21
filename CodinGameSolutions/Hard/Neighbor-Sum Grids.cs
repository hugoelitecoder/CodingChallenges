using System;
using System.Numerics;

class Solution
{
    static void Main(string[] args)
    {
        var gridSize = 5;
        var initialGrid = new int[gridSize, gridSize];
        for (var i = 0; i < gridSize; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            for (var j = 0; j < gridSize; j++)
            {
                initialGrid[i, j] = int.Parse(inputs[j]);
            }
        }

        var solver = new GridSolver(initialGrid);
        var solution = solver.Solve();

        for (var i = 0; i < gridSize; i++)
        {
            var row = new int[gridSize];
            for (var j = 0; j < gridSize; j++)
            {
                row[j] = solution[i, j];
            }
            Console.WriteLine(string.Join(" ", row));
        }
    }
}

class GridSolver
{
    private const int GridSize = 5;
    private const int MaxValue = GridSize * GridSize;
    
    private readonly int[] _initialPositions;
    private int _availablePositionsMask;
    private readonly int[] _solutionPositions;

    public GridSolver(int[,] initialGrid)
    {
        _initialPositions = new int[MaxValue + 1];
        _solutionPositions = new int[MaxValue + 1];
        _availablePositionsMask = (1 << MaxValue) - 1;
        Array.Fill(_initialPositions, -1);
        
        for (var r = 0; r < GridSize; r++)
        {
            for (var c = 0; c < GridSize; c++)
            {
                var val = initialGrid[r, c];
                if (val > 0)
                {
                    var pos = r * GridSize + c;
                    _initialPositions[val] = pos;
                    _availablePositionsMask &= ~(1 << pos);
                }
            }
        }
    }

    public int[,] Solve()
    {
        if (Backtrack(1))
        {
            return BuildResultGrid();
        }
        return null;
    }

    private bool Backtrack(int numToPlace)
    {
        if (numToPlace > MaxValue)
        {
            return true;
        }
        var initialPos = _initialPositions[numToPlace];
        if (initialPos != -1)
        {
            _solutionPositions[numToPlace] = initialPos;
            if (numToPlace >= 3)
            {
                var isSumValid = false;
                for (var k = 1; k < numToPlace - k; k++)
                {
                    var num1 = k;
                    var num2 = numToPlace - k;
                    var pos1 = _solutionPositions[num1];
                    var pos2 = _solutionPositions[num2];
                    var commonNeighbors = NeighborData.Masks[pos1] & NeighborData.Masks[pos2];
                    if ((commonNeighbors & (1 << initialPos)) != 0)
                    {
                        isSumValid = true;
                        break;
                    }
                }
                if (!isSumValid) return false;
            }
            return Backtrack(numToPlace + 1);
        }
        else
        {
            var candidatePositions = 0;
            if (numToPlace <= 2)
            {
                candidatePositions = _availablePositionsMask;
            }
            else
            {
                var possiblePositions = 0;
                for (var k = 1; k < numToPlace - k; k++)
                {
                    var num1 = k;
                    var num2 = numToPlace - k;
                    var pos1 = _solutionPositions[num1];
                    var pos2 = _solutionPositions[num2];
                    possiblePositions |= NeighborData.Masks[pos1] & NeighborData.Masks[pos2];
                }
                candidatePositions = possiblePositions & _availablePositionsMask;
            }
            var tempCandidates = candidatePositions;
            while (tempCandidates != 0)
            {
                var pos = BitOperations.TrailingZeroCount(tempCandidates);
                tempCandidates &= ~(1 << pos);
                _solutionPositions[numToPlace] = pos;
                _availablePositionsMask &= ~(1 << pos);
                if (Backtrack(numToPlace + 1))
                {
                    return true;
                }
                _availablePositionsMask |= (1 << pos);
            }
        }
        return false;
    }
    
    private int[,] BuildResultGrid()
    {
        var result = new int[GridSize, GridSize];
        for (var val = 1; val <= MaxValue; val++)
        {
            var pos = _solutionPositions[val];
            var r = pos / GridSize;
            var c = pos % GridSize;
            result[r, c] = val;
        }
        return result;
    }
}

internal static class NeighborData
{
    private const int N = 5;
    public static readonly int[] Masks = new int[N * N];

    static NeighborData()
    {
        for (var i = 0; i < N; i++)
        {
            for (var j = 0; j < N; j++)
            {
                var u = N * i + j;
                var mask = 0;
                for (var vi = i - 1; vi <= i + 1; vi++)
                {
                    for (var vj = j - 1; vj <= j + 1; vj++)
                    {
                        if ((vi != i || vj != j) && vi >= 0 && vi < N && vj >= 0 && vj < N)
                        {
                            var v = N * vi + vj;
                            mask |= 1 << v;
                        }
                    }
                }
                Masks[u] = mask;
            }
        }
    }
}