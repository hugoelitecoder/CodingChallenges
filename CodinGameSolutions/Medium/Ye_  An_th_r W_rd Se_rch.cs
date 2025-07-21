using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// A single DLX cell, linking in four directions
class DLXCell
{
    public DLXCell PrevX, NextX, PrevY, NextY;
    public DLXCell ColHeader, RowHeader;
    public object Title;
    public int    Size;

    public DLXCell(object title = null)
    {
        PrevX = NextX = this;
        PrevY = NextY = this;
        ColHeader = RowHeader = null;
        Title     = title;
        Size      = 0;
    }

    public void RemoveX()  { PrevX.NextX = NextX; NextX.PrevX = PrevX; }
    public void RemoveY()  { PrevY.NextY = NextY; NextY.PrevY = PrevY; }
    public void RestoreX() { PrevX.NextX = this;  NextX.PrevX = this;  }
    public void RestoreY() { PrevY.NextY = this;  NextY.PrevY = this;  }

    public void AttachHoriz(DLXCell other)
    {
        var n = PrevX;
        other.PrevX = n;  n.NextX = other;
        PrevX      = other;  other.NextX = this;
    }

    public void AttachVert(DLXCell other)
    {
        var n = PrevY;
        other.PrevY = n;  n.NextY = other;
        PrevY      = other;  other.NextY = this;
    }

    public void RemoveColumn()
    {
        RemoveX();
        for (var node = NextY; node != this; node = node.NextY)
            node.RemoveRow();
    }

    public void RestoreColumn()
    {
        for (var node = PrevY; node != this; node = node.PrevY)
            node.RestoreRow();
        RestoreX();
    }

    public void RemoveRow()
    {
        for (var node = NextX; node != this; node = node.NextX)
        {
            node.ColHeader.Size--;
            node.RemoveY();
        }
    }

    public void RestoreRow()
    {
        for (var node = PrevX; node != this; node = node.PrevX)
        {
            node.ColHeader.Size++;
            node.RestoreY();
        }
    }

    public void Select()
    {
        for (var node = this; ; node = node.NextX)
        {
            node.RemoveY();
            node.ColHeader.RemoveColumn();
            if (node.NextX == this) break;
        }
    }

    public void Unselect()
    {
        for (var node = PrevX; node != this; node = node.PrevX)
        {
            node.ColHeader.RestoreColumn();
            node.RestoreY();
        }
        ColHeader.RestoreColumn();
        RestoreY();
    }
}

// Basic Algorithm X solver with DLX
class AlgorithmXSolver
{
    private readonly DLXCell _root;
    private readonly Dictionary<object, DLXCell> _colHeaders;
    private readonly Dictionary<object, DLXCell> _rowHeaders;
    private readonly Dictionary<object, List<object>> _actions;
    private readonly HashSet<object> _optionals;
    protected readonly List<object> solution = new();
    public  int SolutionCount { get; private set; }
    private readonly List<HashSet<object>> _history = new() { new() };
    protected bool solutionIsValid = true;

    public AlgorithmXSolver(
        List<object> requirements,
        Dictionary<object, List<object>> actions,
        List<object> optionals = null
    )
    {
        _actions   = actions;
        _optionals = optionals is null ? new() : new(optionals);

        // create root header
        _root = new DLXCell("root") { Size = int.MaxValue };

        // create column headers
        _colHeaders = new();
        foreach (var r in requirements)
        {
            var ch = new DLXCell(r);
            _colHeaders[r] = ch;
            _root.AttachHoriz(ch);
        }

        // create row headers
        _rowHeaders = new();
        foreach (var a in actions.Keys)
            _rowHeaders[a] = new DLXCell(a);

        // build matrix rows
        foreach (var action in actions.Keys)
        {
            DLXCell prev = null;
            foreach (var req in actions[action])
            {
                var cell = new DLXCell();
                cell.ColHeader = _colHeaders[req];
                cell.RowHeader = _rowHeaders[action];
                cell.ColHeader.AttachVert(cell);
                cell.ColHeader.Size++;
                if (prev != null)
                    prev.AttachHoriz(cell);
                prev = cell;
            }
        }
    }

    // The main recursive solver, yielding each complete solution
    public IEnumerable<List<object>> Solve()
    {
        // 1) Choose column with fewest rows (excluding optionals)
        DLXCell best = _root;
        int    bestSize = int.MaxValue;
        for (var c = _root.NextX; c != _root; c = c.NextX)
        {
            if (!_optionals.Contains(c.Title))
            {
                var sz = RequirementSortCriteria(c);
                if (best == _root || sz < bestSize)
                {
                    best     = c;
                    bestSize = sz;
                }
            }
        }

        if (best == _root)
        {
            // no columns left: record solution
            ProcessSolution();
            if (solutionIsValid)
            {
                SolutionCount++;
                yield return new List<object>(solution);
            }
        }
        else
        {
            // 2) Cover each row in that column
            var rows = new List<DLXCell>();
            for (var r = best.NextY; r != best; r = r.NextY)
                rows.Add(r);

            // push history
            _history.Add(new HashSet<object>(_history[^1]));

            // try each row
            foreach (var row in rows.OrderBy(r => ActionSortCriteria(r.RowHeader)))
            {
                Select(row);
                if (solutionIsValid)
                {
                    foreach (var sol in Solve())
                        yield return sol;
                }
                Deselect(row);
                solutionIsValid = true;
            }

            _history.RemoveAt(_history.Count - 1);
        }
    }

    private void Select(DLXCell rowNode)
    {
        rowNode.Select();
        var act = rowNode.RowHeader.Title;
        solution.Add(act);
        ProcessRowSelection(act);
    }

    private void Deselect(DLXCell rowNode)
    {
        rowNode.Unselect();
        var act = rowNode.RowHeader.Title;
        solution.RemoveAt(solution.Count - 1);
        ProcessRowDeselection(act);
    }

    // Hooks for subclasses:
    protected virtual void ProcessSolution() {}
    protected virtual void ProcessRowSelection(object action) {}
    protected virtual void ProcessRowDeselection(object action) {}
    protected virtual int RequirementSortCriteria(DLXCell col) => col.Size;
    protected virtual int ActionSortCriteria(DLXCell row)    => 0;
}

// A concrete solver for the "Yet Another Word Search" problem
class YetAnotherWordSearchSolver : AlgorithmXSolver
{
    private readonly int _height, _width;
    private readonly char[][] _grid;
    private readonly Dictionary<(int, int), List<char>> _locationColors;
    public static readonly Dictionary<string, (int dr, int dc)> DELTAS =
        new()
        {
            ["horizontal"] = (0, 1),
            ["vertical"]   = (1, 0),
            ["up_diag"]    = (-1, 1),
            ["down_diag"]  = (1, 1)
        };

    public YetAnotherWordSearchSolver(string[] words, char[][] grid)
        : base(BuildRequirements(words), BuildActions(words, grid))
    {
        _grid = grid;
        _height = grid.Length;
        _width  = grid[0].Length;
        _locationColors = new Dictionary<(int,int), List<char>>();
        for (int r = 0; r < _height; r++)
            for (int c = 0; c < _width; c++)
                _locationColors[(r, c)] = new List<char>();
    }

    // Build the "must-place" requirements
    private static List<object> BuildRequirements(string[] words) =>
        words.Select(w => (object) (ValueTuple.Create("word placed", w)))
             .ToList();

    // Build the actions for every valid placement
    private static Dictionary<object, List<object>> BuildActions(string[] words, char[][] grid)
    {
        int h = grid.Length, w = grid[0].Length;
        var actions = new Dictionary<object, List<object>>();

        foreach (var word in words)
        {
            int len = word.Length;
            foreach (var orientation in DELTAS.Keys)
            {
                var (dr, dc) = DELTAS[orientation];
                for (int row = 0; row < h; row++)
                for (int col = 0; col < w; col++)
                {
                    // does word fit?
                    if (row + dr * (len - 1) < 0
                        || row + dr * (len - 1) >= h
                        || col + dc * (len - 1) < 0
                        || col + dc * (len - 1) >= w)
                        continue;

                    foreach (var fb in new[] { "forward", "backward" })
                    {
                        var wstr = fb == "forward"
                            ? word
                            : new string(word.Reverse().ToArray());
                        int rr = row, cc = col;
                        var ok = true;
                        foreach (var ch in wstr)
                        {
                            if (grid[rr][cc] != '.' && grid[rr][cc] != ch)
                            {
                                ok = false;
                                break;
                            }
                            rr += dr; cc += dc;
                        }
                        if (!ok) continue;

                        var action = ValueTuple.Create("place word", word, row, col, orientation, fb);
                        var req    = ValueTuple.Create("word placed", word);
                        actions[action] = new List<object> { req };
                    }
                }
            }
        }

        return actions;
    }

    protected override void ProcessRowSelection(object actionObj)
    {
        var action = ((string, string, int, int, string, string))actionObj;
        var word   = action.Item2;
        if (action.Item6 == "backward")
            word = new string(word.Reverse().ToArray());

        var (dr, dc) = DELTAS[action.Item5];
        int r = action.Item3, c = action.Item4;
        foreach (var ch in word)
        {
            var lst = _locationColors[(r, c)];
            if (lst.Count > 0 && lst[^1] != ch)
                solutionIsValid = false;
            lst.Add(ch);
            r += dr; c += dc;
        }
    }

    protected override void ProcessRowDeselection(object actionObj)
    {
        var action = ((string, string, int, int, string, string))actionObj;
        var word   = action.Item2;
        if (action.Item6 == "backward")
            word = new string(word.Reverse().ToArray());

        var (dr, dc) = DELTAS[action.Item5];
        int r = action.Item3, c = action.Item4;
        for (int i = 0; i < word.Length; i++)
        {
            var lst = _locationColors[(r, c)];
            lst.RemoveAt(lst.Count - 1);
            r += dr; c += dc;
        }
    }
}

class Program
{
    static void Main()
    {
        var dims = Console.ReadLine()!.Split();
        int height = int.Parse(dims[0]),
            width  = int.Parse(dims[1]);

        var grid = new char[height][];
        for (int i = 0; i < height; i++)
            grid[i] = Console.ReadLine()!.ToCharArray();

        var words = Console.ReadLine()!.Split();

        // prepare output canvas
        var answer = Enumerable.Range(0, height)
            .Select(_ => new string(' ', width).ToCharArray())
            .ToArray();

        var solver = new YetAnotherWordSearchSolver(words, grid);
        var sw     = Stopwatch.StartNew();

        // take first solution
        foreach (var sol in solver.Solve())
        {
            foreach (var actionObj in sol)
            {
                var action = ((string, string, int, int, string, string))actionObj;
                var word   = action.Item2;
                if (action.Item6 == "backward")
                    word = new string(word.Reverse().ToArray());

                var (dr, dc) = YetAnotherWordSearchSolver.DELTAS[action.Item5];
                int r = action.Item3, c = action.Item4;
                foreach (var ch in word)
                {
                    answer[r][c] = ch;
                    r += dr; c += dc;
                }
            }
            break;
        }

        sw.Stop();

        // print final grid
        foreach (var row in answer)
            Console.WriteLine(new string(row));

        // timing to stderr
        Console.Error.WriteLine($"{sw.ElapsedMilliseconds} ms");
    }
}
