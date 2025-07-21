using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var height = int.Parse(inputs[0]);
        var width = int.Parse(inputs[1]);
        var lines = new string[height];
        for (var i = 0; i < height; i++)
        {
            lines[i] = Console.ReadLine();
        }

        var solver = new KakuroSolver(height, width, lines);
        solver.Solve();
        
        Console.WriteLine(solver.GetSolutionString());
    }
}

class KakuroSolver
{
    public static readonly Dictionary<(int, int), List<int[]>> Possibilities;
    private readonly int _height;
    private readonly int _width;
    private readonly Cell[,] _grid;
    private readonly List<Cell> _emptyCells;

    static KakuroSolver()
    {
        Possibilities = GeneratePossibilities();
    }
    
    public KakuroSolver(int height, int width, string[] lines)
    {
        _height = height;
        _width = width;
        _grid = new Cell[height, width];
        _emptyCells = new List<Cell>();
        ParseGrid(lines);
        IdentifyBlocks();
        PopulateEmptyCells();
    }

    public bool Solve()
    {
        return SolveRecursive(0);
    }

    public string GetSolutionString()
    {
        var sb = new StringBuilder();
        for (var r = 0; r < _height; r++)
        {
            for (var c = 0; c < _width; c++)
            {
                var cell = _grid[r, c];
                string cellStr;
                switch (cell.Type)
                {
                    case CellType.Value:
                        cellStr = cell.Value.ToString();
                        break;
                    case CellType.Black:
                        cellStr = "X";
                        break;
                    case CellType.Sum:
                        var sumContent = new StringBuilder();
                        if (cell.VerticalSum != -1)
                        {
                            sumContent.Append(cell.VerticalSum);
                        }
                        sumContent.Append('\\');
                        if (cell.HorizontalSum != -1)
                        {
                            sumContent.Append(cell.HorizontalSum);
                        }
                        cellStr = sumContent.ToString();
                        break;
                    default:
                        cellStr = "";
                        break;
                }
                sb.Append(cellStr);

                if (c < _width - 1)
                {
                    sb.Append(',');
                }
            }
            if (r < _height - 1)
            {
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    private void ParseGrid(string[] lines)
    {
        for (var r = 0; r < _height; r++)
        {
            var parts = lines[r].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (var c = 0; c < _width; c++)
            {
                _grid[r, c] = new Cell(r, c, parts[c]);
            }
        }
    }
    
    private void IdentifyBlocks()
    {
        for (var r = 0; r < _height; r++)
        {
            for (var c = 0; c < _width; c++)
            {
                var cell = _grid[r, c];
                if (cell.Type != CellType.Sum) continue;

                if (cell.HorizontalSum != -1)
                {
                    var hBlock = new Block { TargetSum = cell.HorizontalSum };
                    for (var k = c + 1; k < _width && _grid[r, k].Type == CellType.Value; k++)
                    {
                        var valueCell = _grid[r, k];
                        hBlock.Cells.Add(valueCell);
                        valueCell.HorizontalBlock = hBlock;
                    }
                }

                if (cell.VerticalSum != -1)
                {
                    var vBlock = new Block { TargetSum = cell.VerticalSum };
                    for (var k = r + 1; k < _height && _grid[k, c].Type == CellType.Value; k++)
                    {
                        var valueCell = _grid[k, c];
                        vBlock.Cells.Add(valueCell);
                        valueCell.VerticalBlock = vBlock;
                    }
                }
            }
        }
    }
    
    private void PopulateEmptyCells()
    {
        for (var r = 0; r < _height; r++)
        {
            for (var c = 0; c < _width; c++)
            {
                var cell = _grid[r, c];
                if (cell.Type == CellType.Value && !cell.IsPreFilled)
                {
                    _emptyCells.Add(cell);
                }
            }
        }
    }
    
    private bool SolveRecursive(int cellIndex)
    {
        if (cellIndex == _emptyCells.Count)
        {
            return true;
        }

        var cell = _emptyCells[cellIndex];
        var hBlock = cell.HorizontalBlock;
        var vBlock = cell.VerticalBlock;

        for (var digit = 1; digit <= 9; digit++)
        {
            cell.Value = digit;
            
            if ((hBlock == null || hBlock.IsBlockStateValid()) && 
                (vBlock == null || vBlock.IsBlockStateValid()))
            {
                if (SolveRecursive(cellIndex + 1))
                {
                    return true;
                }
            }
        }

        cell.Value = 0;
        return false;
    }

    private static Dictionary<(int, int), List<int[]>> GeneratePossibilities()
    {
        var cache = new Dictionary<(int, int), List<int[]>>();
        
        void Find(int k, int start, int currentSum, List<int> currentCombo)
        {
            if (currentCombo.Count == k)
            {
                if (!cache.ContainsKey((currentSum, k)))
                {
                    cache[(currentSum, k)] = new List<int[]>();
                }
                cache[(currentSum, k)].Add(currentCombo.ToArray());
                return;
            }

            for (var i = start; i <= 9; i++)
            {
                currentCombo.Add(i);
                Find(k, i + 1, currentSum + i, currentCombo);
                currentCombo.RemoveAt(currentCombo.Count - 1);
            }
        }

        for (var k = 1; k <= 9; k++)
        {
            Find(k, 1, 0, new List<int>());
        }
        
        return cache;
    }
}

enum CellType { Black, Sum, Value }

class Cell
{
    public int Row { get; }
    public int Col { get; }
    public CellType Type { get; }
    public int VerticalSum { get; } = -1;
    public int HorizontalSum { get; } = -1;
    public int Value { get; set; }
    public bool IsPreFilled { get; }
    public Block HorizontalBlock { get; set; }
    public Block VerticalBlock { get; set; }

    public Cell(int row, int col, string content)
    {
        Row = row;
        Col = col;
        var trimmedContent = content.Trim();

        if (trimmedContent == "X")
        {
            Type = CellType.Black;
        }
        else if (trimmedContent.Contains('\\'))
        {
            Type = CellType.Sum;
            var parts = trimmedContent.Split('\\');
            if (!string.IsNullOrEmpty(parts[0]))
            {
                VerticalSum = int.Parse(parts[0]);
            }
            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
            {
                HorizontalSum = int.Parse(parts[1]);
            }
        }
        else if (string.IsNullOrWhiteSpace(trimmedContent))
        {
            Type = CellType.Value;
        }
        else
        {
            Type = CellType.Value;
            Value = int.Parse(trimmedContent);
            IsPreFilled = true;
        }
    }
}

class Block
{
    public int TargetSum { get; set; }
    public List<Cell> Cells { get; } = new List<Cell>();

    public bool IsBlockStateValid()
    {
        var sum = 0;
        var usedDigits = new HashSet<int>();
        var filledCount = 0;

        foreach (var cell in Cells)
        {
            if (cell.Value != 0)
            {
                if (!usedDigits.Add(cell.Value)) return false;
                sum += cell.Value;
                filledCount++;
            }
        }
        if (sum > TargetSum) return false;
        
        if (filledCount == Cells.Count)
        {
            return sum == TargetSum;
        }

        var remainingSum = TargetSum - sum;
        var remainingLen = Cells.Count - filledCount;
        
        if (!KakuroSolver.Possibilities.TryGetValue((remainingSum, remainingLen), out var combos))
        {
            return false;
        }
        
        foreach (var combo in combos)
        {
            var isPossible = true;
            foreach (var digit in combo)
            {
                if (usedDigits.Contains(digit))
                {
                    isPossible = false;
                    break;
                }
            }
            if (isPossible)
            {
                return true;
            }
        }

        return false;
    }
}