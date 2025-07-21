using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var grid = new int[n,n];
        for (int i = 0; i < n; i++)
        {
            var line = Console.ReadLine().Trim();
            for (int j = 0; j < n; j++)
                grid[i,j] = line[j] - '0';
        }

        var colMap = new Dictionary<string, int>();
        var cols = new List<string>();
        for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
        {
            var c = $"C{i},{j}";
            colMap[c] = cols.Count;
            cols.Add(c);
        }
        for (int i = 0; i < n; i++)
        for (int k = 1; k <= n; k++)
        {
            var r = $"R{i},{k}";
            colMap[r] = cols.Count;
            cols.Add(r);
        }
        for (int j = 0; j < n; j++)
        for (int k = 1; k <= n; k++)
        {
            var kstr = $"K{j},{k}";
            colMap[kstr] = cols.Count;
            cols.Add(kstr);
        }

        int numCols = cols.Count;
        int maxRows = n * n * n;
        var solver = new AlgorithmXSolver<string>(numCols, numCols, maxRows * 5, n * n);

        for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
        {
            if (grid[i,j] != 0)
            {
                int k = grid[i,j];
                string key = $"A{i},{j},{k}";
                var constraintIndices = new List<int> {
                    colMap[$"C{i},{j}"],
                    colMap[$"R{i},{k}"],
                    colMap[$"K{j},{k}"]
                };
                solver.AddRow(constraintIndices, key);
            }
            else
            {
                for (int k = 1; k <= n; k++)
                {
                    string key = $"A{i},{j},{k}";
                    var constraintIndices = new List<int> {
                        colMap[$"C{i},{j}"],
                        colMap[$"R{i},{k}"],
                        colMap[$"K{j},{k}"]
                    };
                    solver.AddRow(constraintIndices, key);
                }
            }
        }

        long count = 0;
        foreach (var sol in solver.SolveAll())
            count++;

        Console.WriteLine(count);
    }
}

public class AlgorithmXSolver<T> where T : class
{
    private struct DlxNode
    {
        public int Left, Right, Up, Down;
        public int ColHeader;
        public T RowPayload;
        public int Size;
    }

    private readonly DlxNode[] _nodes;
    private readonly int _header;
    private int _nodeCount;
    private readonly T[] _solution;
    private int _solutionDepth;

    public AlgorithmXSolver(int numPrimaryColumns, int numTotalColumns, int maxNodes, int maxSolutionDepth)
    {
        var poolSize = numTotalColumns + 1 + maxNodes;
        _nodes = new DlxNode[poolSize];
        _solution = new T[maxSolutionDepth];
        _header = 0;

        for (int i = 0; i <= numTotalColumns; i++)
        {
            _nodes[i] = new DlxNode { Left = i, Right = i, Up = i, Down = i, ColHeader = i, Size = 0 };
        }

        _nodes[_header].Right = _header;
        _nodes[_header].Left = _header;
        for (int i = 1; i <= numPrimaryColumns; i++)
        {
            _nodes[i].Right = _nodes[_header].Right;
            _nodes[i].Left = _header;
            _nodes[_nodes[_header].Right].Left = i;
            _nodes[_header].Right = i;
        }
        _nodeCount = numTotalColumns + 1;
    }

    public void AddRow(List<int> columns, T rowPayload)
    {
        if (columns.Count == 0) return;

        int firstNode = -1;
        foreach (int c_idx in columns)
        {
            int colHeaderNodeIndex = c_idx + 1;
            int newNodeIndex = _nodeCount++;

            _nodes[colHeaderNodeIndex].Size++;
            _nodes[newNodeIndex].RowPayload = rowPayload;
            _nodes[newNodeIndex].ColHeader = colHeaderNodeIndex;

            _nodes[newNodeIndex].Up = _nodes[colHeaderNodeIndex].Up;
            _nodes[newNodeIndex].Down = colHeaderNodeIndex;
            _nodes[_nodes[colHeaderNodeIndex].Up].Down = newNodeIndex;
            _nodes[colHeaderNodeIndex].Up = newNodeIndex;

            if (firstNode == -1)
            {
                firstNode = newNodeIndex;
                _nodes[newNodeIndex].Left = newNodeIndex;
                _nodes[newNodeIndex].Right = newNodeIndex;
            }
            else
            {
                _nodes[newNodeIndex].Left = _nodes[firstNode].Left;
                _nodes[newNodeIndex].Right = firstNode;
                _nodes[_nodes[firstNode].Left].Right = newNodeIndex;
                _nodes[firstNode].Left = newNodeIndex;
            }
        }
    }

    public T[] Solve()
    {
        return EnumerateSolutions().FirstOrDefault();
    }

    public IEnumerable<T[]> SolveAll()
    {
        return EnumerateSolutions();
    }

    private IEnumerable<T[]> EnumerateSolutions()
    {
        if (_nodes[_header].Right == _header)
        {
            var result = new T[_solutionDepth];
            Array.Copy(_solution, result, _solutionDepth);
            yield return result;
            yield break;
        }

        int c = ChooseColumn();
        Cover(c);

        for (int r_node = _nodes[c].Down; r_node != c; r_node = _nodes[r_node].Down)
        {
            _solution[_solutionDepth++] = _nodes[r_node].RowPayload;

            for (int j_node = _nodes[r_node].Right; j_node != r_node; j_node = _nodes[j_node].Right)
            {
                Cover(_nodes[j_node].ColHeader);
            }

            foreach (var sol in EnumerateSolutions())
            {
                yield return sol;
            }

            _solutionDepth--;
            for (int j_node = _nodes[r_node].Left; j_node != r_node; j_node = _nodes[j_node].Left)
            {
                Uncover(_nodes[j_node].ColHeader);
            }
        }
        Uncover(c);
    }

    private int ChooseColumn()
    {
        int minSize = int.MaxValue;
        int bestCol = 0;
        for (int c_header = _nodes[_header].Right; c_header != _header; c_header = _nodes[c_header].Right)
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
        _nodes[_nodes[c].Left].Right = _nodes[c].Right;
        _nodes[_nodes[c].Right].Left = _nodes[c].Left;

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
        _nodes[_nodes[c].Left].Right = c;
        _nodes[_nodes[c].Right].Left = c;
    }
} 