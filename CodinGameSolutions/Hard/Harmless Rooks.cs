using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var board = new char[n][];
        for (var i = 0; i < n; i++) board[i] = Console.ReadLine().ToCharArray();
        var segmentation = new BoardSegmentation(board);
        var matcher = new BipartiteMatcher(segmentation.RowCount, segmentation.ColCount);
        for (var i = 0; i < n; i++)
        for (var j = 0; j < n; j++)
        {
            if (board[i][j] == '.')
            {
                var r = segmentation.GetRowSegment(i, j);
                var c = segmentation.GetColSegment(i, j);
                matcher.AddEdge(r, c);
            }
        }
        var result = matcher.GetMaximumMatching();
        Console.WriteLine(result);
    }
}

class BipartiteMatcher
{
    private readonly List<int>[] _adjacency;
    private readonly int[] _matchTo;
    private readonly bool[] _visited;
    private readonly int _rowCount;
    private readonly int _colCount;

    public BipartiteMatcher(int rowCount, int colCount)
    {
        _rowCount = rowCount;
        _colCount = colCount;
        _adjacency = new List<int>[rowCount];
        for (var i = 0; i < rowCount; i++) _adjacency[i] = new List<int>();
        _matchTo = new int[colCount];
        for (var i = 0; i < colCount; i++) _matchTo[i] = -1;
        _visited = new bool[rowCount];
    }

    public void AddEdge(int row, int col)
    {
        _adjacency[row].Add(col);
    }

    public int GetMaximumMatching()
    {
        var result = 0;
        for (var u = 0; u < _rowCount; u++)
        {
            for (var i = 0; i < _rowCount; i++) _visited[i] = false;
            if (DFS(u)) result++;
        }
        return result;
    }

    private bool DFS(int u)
    {
        if (_visited[u]) return false;
        _visited[u] = true;
        var list = _adjacency[u];
        for (var i = 0; i < list.Count; i++)
        {
            var v = list[i];
            if (_matchTo[v] == -1 || DFS(_matchTo[v]))
            {
                _matchTo[v] = u;
                return true;
            }
        }
        return false;
    }
}

class BoardSegmentation
{
    private readonly char[][] _board;
    private readonly int[,] _rowSegment;
    private readonly int[,] _colSegment;
    private int _rowCount;
    private int _colCount;
    private readonly int _n;

    public BoardSegmentation(char[][] board)
    {
        _board = board;
        _n = board.Length;
        _rowSegment = new int[_n, _n];
        _colSegment = new int[_n, _n];
        SegmentRows();
        SegmentCols();
    }

    public int RowCount => _rowCount;
    public int ColCount => _colCount;

    public int GetRowSegment(int i, int j) => _rowSegment[i, j];
    public int GetColSegment(int i, int j) => _colSegment[i, j];

    private void SegmentRows()
    {
        _rowCount = 0;
        for (var i = 0; i < _n; i++)
        {
            var current = -1;
            for (var j = 0; j < _n; j++)
            {
                if (_board[i][j] == 'X') current = -1;
                else
                {
                    if (current == -1) current = _rowCount++;
                    _rowSegment[i, j] = current;
                }
            }
        }
    }

    private void SegmentCols()
    {
        _colCount = 0;
        for (var j = 0; j < _n; j++)
        {
            var current = -1;
            for (var i = 0; i < _n; i++)
            {
                if (_board[i][j] == 'X') current = -1;
                else
                {
                    if (current == -1) current = _colCount++;
                    _colSegment[i, j] = current;
                }
            }
        }
    }
}
