using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var grid = new char[Sudoku25x25Solver.Size, Sudoku25x25Solver.Size];
        for (var i = 0; i < Sudoku25x25Solver.Size; i++)
        {
            var rowStr = Console.ReadLine();
            for (var j = 0; j < Sudoku25x25Solver.Size; j++)
            {
                grid[i, j] = rowStr[j];
            }
        }

        var solver = new Sudoku25x25Solver();
        var solution = solver.Solve(grid);

        for (var i = 0; i < Sudoku25x25Solver.Size; i++)
        {
            var rowChars = new char[Sudoku25x25Solver.Size];
            for (var j = 0; j < Sudoku25x25Solver.Size; j++)
            {
                rowChars[j] = solution[i, j];
            }
            Console.WriteLine(new string(rowChars));
        }
    }
}

public struct SudokuMove
{
    public readonly int R, C, V;
    public SudokuMove(int r, int c, int v) { R = r; C = c; V = v; }
}

public class Sudoku25x25Solver
{
    public const int N = 5;
    public const int Size = N * N;
    private const int CellsCount = Size * Size;
    
    public char[,] Solve(char[,] grid)
    {
        int numColumns = 4 * CellsCount;
        int maxNodes = Size * CellsCount * 4; 
        var solver = new AlgorithmXSolver<SudokuMove>(numColumns, maxNodes, CellsCount);

        for (int r = 0; r < Size; r++)
        {
            for (int c = 0; c < Size; c++)
            {
                if (grid[r, c] != '.')
                {
                    int val = grid[r, c] - 'A';
                    var move = new SudokuMove(r, c, val);
                    solver.AddRow(GetColumnIndices(move), move);
                }
                else
                {
                    for (int val = 0; val < Size; val++)
                    {
                        var move = new SudokuMove(r, c, val);
                        solver.AddRow(GetColumnIndices(move), move);
                    }
                }
            }
        }
        
        var solutionMoves = solver.Solve();
        return solutionMoves != null ? DecodeSolution(grid, solutionMoves) : null;
    }

    private List<int> GetColumnIndices(SudokuMove move)
    {
        var r = move.R;
        var c = move.C;
        var v = move.V;
        var box = (r / N) * N + (c / N);
        
        var cellConstraint = r * Size + c;
        var rowValConstraint = CellsCount + r * Size + v;
        var colValConstraint = 2 * CellsCount + c * Size + v;
        var boxValConstraint = 3 * CellsCount + box * Size + v;
        
        return new List<int> { cellConstraint, rowValConstraint, colValConstraint, boxValConstraint };
    }

    private char[,] DecodeSolution(char[,] initialGrid, SudokuMove[] moves)
    {
        var solvedGrid = (char[,])initialGrid.Clone();
        foreach (var move in moves)
        {
            solvedGrid[move.R, move.C] = (char)('A' + move.V);
        }
        return solvedGrid;
    }
}


public class AlgorithmXSolver<T>
{
    private struct DlxNode
    {
        public int Left, Right, Up, Down;
        public int ColHeader;
        public T RowPayload;
        public int Size;
    }

    private readonly DlxNode[] _nodes;
    private int _nodeCount;
    private readonly T[] _solution;
    private int _solutionDepth;

    public AlgorithmXSolver(int numColumns, int maxNodes, int maxSolutionDepth)
    {
        var poolSize = numColumns + 1 + maxNodes;
        _nodes = new DlxNode[poolSize];
        _solution = new T[maxSolutionDepth];

        for (int i = 0; i <= numColumns; i++)
        {
            _nodes[i] = new DlxNode { Left = i - 1, Right = i + 1, Up = i, Down = i, ColHeader = i, Size = 0 };
        }
        _nodes[0].Left = numColumns;
        _nodes[numColumns].Right = 0;
        _nodeCount = numColumns + 1;
    }

    public void AddRow(List<int> columns, T rowPayload)
    {
        if (columns.Count == 0) return;
        
        int firstNode = -1;
        foreach (int c_idx in columns)
        {
            int headerIdx = c_idx + 1;
            _nodes[headerIdx].Size++;
            _nodes[_nodeCount].RowPayload = rowPayload;
            _nodes[_nodeCount].ColHeader = headerIdx;
            _nodes[_nodeCount].Up = _nodes[headerIdx].Up;
            _nodes[_nodeCount].Down = headerIdx;
            _nodes[_nodes[headerIdx].Up].Down = _nodeCount;
            _nodes[headerIdx].Up = _nodeCount;
            
            if (firstNode == -1)
            {
                firstNode = _nodeCount;
                _nodes[_nodeCount].Left = _nodeCount;
                _nodes[_nodeCount].Right = _nodeCount;
            }
            else
            {
                _nodes[_nodeCount].Left = _nodes[firstNode].Left;
                _nodes[_nodeCount].Right = firstNode;
                _nodes[_nodes[firstNode].Left].Right = _nodeCount;
                _nodes[firstNode].Left = _nodeCount;
            }
            _nodeCount++;
        }
    }

    public T[] Solve()
    {
        if (Search(0))
        {
            var result = new T[_solutionDepth];
            Array.Copy(_solution, result, _solutionDepth);
            return result;
        }
        return null;
    }
    
    private bool Search(int k)
    {
        if (_nodes[0].Right == 0)
        {
            _solutionDepth = k;
            return true;
        }

        int c = ChooseColumn();
        Cover(c);

        for (int r_node = _nodes[c].Down; r_node != c; r_node = _nodes[r_node].Down)
        {
            _solution[k] = _nodes[r_node].RowPayload;
            for (int j_node = _nodes[r_node].Right; j_node != r_node; j_node = _nodes[j_node].Right)
            {
                Cover(_nodes[j_node].ColHeader);
            }

            if (Search(k + 1)) return true;
            
            for (int j_node = _nodes[r_node].Left; j_node != r_node; j_node = _nodes[j_node].Left)
            {
                Uncover(_nodes[j_node].ColHeader);
            }
        }
        Uncover(c);
        return false;
    }

    private int ChooseColumn()
    {
        int minSize = int.MaxValue;
        int bestCol = 0;
        for (int c_header = _nodes[0].Right; c_header != 0; c_header = _nodes[c_header].Right)
        {
            if (_nodes[c_header].Size < minSize)
            {
                minSize = _nodes[c_header].Size;
                bestCol = c_header;
            }
        }
        return bestCol;
    }

    private void Cover(int c)
    {
        _nodes[_nodes[c].Right].Left = _nodes[c].Left;
        _nodes[_nodes[c].Left].Right = _nodes[c].Right;
        for (int i = _nodes[c].Down; i != c; i = _nodes[i].Down)
        {
            for (int j = _nodes[i].Right; j != i; j = _nodes[j].Right)
            {
                _nodes[_nodes[j].Up].Down = _nodes[j].Down;
                _nodes[_nodes[j].Down].Up = _nodes[j].Up;
                _nodes[_nodes[j].ColHeader].Size--;
            }
        }
    }

    private void Uncover(int c)
    {
        for (int i = _nodes[c].Up; i != c; i = _nodes[i].Up)
        {
            for (int j = _nodes[i].Left; j != i; j = _nodes[j].Left)
            {
                _nodes[_nodes[j].ColHeader].Size++;
                _nodes[_nodes[j].Up].Down = j;
                _nodes[_nodes[j].Down].Up = j;
            }
        }
        _nodes[_nodes[c].Right].Left = c;
        _nodes[_nodes[c].Left].Right = c;
    }
}