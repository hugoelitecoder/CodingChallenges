using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
class Solution
{
    static void Main(string[] args)
    {
        var totalSw = Stopwatch.StartNew();
        var sw = Stopwatch.StartNew();
        var size = int.Parse(Console.ReadLine());
        Console.Error.WriteLine($"[DEBUG] Size: {size}");
        var colHints = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
        Console.Error.WriteLine($"[DEBUG] Col Hints: {string.Join(" ", colHints)}");
        var rowHints = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
        Console.Error.WriteLine($"[DEBUG] Row Hints: {string.Join(" ", rowHints)}");
        var shipsCount = int.Parse(Console.ReadLine());
        Console.Error.WriteLine($"[DEBUG] Ship Count: {shipsCount}");
        var fleet = Console.ReadLine().Split(' ').Select(int.Parse).ToList();
        Console.Error.WriteLine($"[DEBUG] Fleet: {string.Join(" ", fleet)}");
        var initialGrid = new char[size][];
        for (var i = 0; i < size; i++)
        {
            var line = Console.ReadLine();
            Console.Error.WriteLine($"[DEBUG] Grid Row {i}: {line}");
            initialGrid[i] = line.ToCharArray();
        }
        sw.Stop();
        var inputTime = sw.ElapsedMilliseconds;
        Console.Error.WriteLine($"[DEBUG] Input Read Time: {inputTime}ms");
        sw.Restart();
        var solver = new BattleshipsSolitaire(size, colHints, rowHints, fleet, initialGrid);
        var solvedGrid = solver.Solve();
        sw.Stop();
        var calcTime = sw.ElapsedMilliseconds;
        if (solvedGrid == null)
        {
            Console.Error.WriteLine("[DEBUG] No solution found!");
            for (var i = 0; i < size; i++)
            {
                Console.WriteLine(new string(BattleshipsSolitaire.WATER, size));
            }
        }
        else
        {
            for (var i = 0; i < size; i++)
            {
                Console.WriteLine(string.Join("", solvedGrid[i].Select(c => 
                    c == BattleshipsSolitaire.UNKNOWN || c == BattleshipsSolitaire.WATER 
                    ? BattleshipsSolitaire.WATER 
                    : BattleshipsSolitaire.SHIP_SEGMENT)));
            }
        }
        totalSw.Stop();
        Console.Error.WriteLine($"[DEBUG] Calculation Time: {calcTime}ms");
        Console.Error.WriteLine($"[DEBUG] Total Execution Time: {totalSw.ElapsedMilliseconds}ms");
    }
}
public class BattleshipsSolitaire
{
    public const char UNKNOWN = '.';
    public const char WATER = 'x';
    public const char SHIP_SEGMENT = 'o';
    public const char CLUE_SHIP_UP = '^';
    public const char CLUE_SHIP_DOWN = 'v';
    public const char CLUE_SHIP_LEFT = '<';
    public const char CLUE_SHIP_RIGHT = '>';
    public const char CLUE_SHIP_MIDDLE = '#';
    private static readonly string ALL_SHIP_SYMBOLS = $"{SHIP_SEGMENT}{CLUE_SHIP_UP}{CLUE_SHIP_DOWN}{CLUE_SHIP_LEFT}{CLUE_SHIP_RIGHT}{CLUE_SHIP_MIDDLE}";
    
    private readonly int _size;
    private readonly int[] _colHints;
    private readonly int[] _rowHints;
    private readonly int[] _colCounts;
    private readonly int[] _rowCounts;
    private readonly int[] _colDots;
    private readonly int[] _rowDots;
    private readonly char[][] _grid;
    private readonly char[][] _initialGrid;
    private readonly List<int> _fleet;
    private readonly bool[][] _visited;
    private readonly int[] _drDiag = { -1, -1, 1, 1 };
    private readonly int[] _dcDiag = { -1, 1, -1, 1 };
    private readonly int[] _drAdj = { -1, 1, 0, 0 }; // Up, Down, Left, Right
    private readonly int[] _dcAdj = { 0, 0, -1, 1 }; // Up, Down, Left, Right
    private static readonly char[] _oppositeInitialClue = { CLUE_SHIP_UP, CLUE_SHIP_DOWN, CLUE_SHIP_LEFT, CLUE_SHIP_RIGHT };
    private static readonly char[] _oppositeClue = { CLUE_SHIP_DOWN, CLUE_SHIP_UP, CLUE_SHIP_RIGHT, CLUE_SHIP_LEFT };
    private static readonly Dictionary<char, (int dr, int dc, char val)[]> _clueActions = new()
    {
        [SHIP_SEGMENT] = new[] { (-1, 0, WATER), (1, 0, WATER), (0, -1, WATER), (0, 1, WATER) },
        [CLUE_SHIP_UP] = new[] { (-1, 0, WATER), (1, 0, SHIP_SEGMENT) },
        [CLUE_SHIP_DOWN] = new[] { (1, 0, WATER), (-1, 0, SHIP_SEGMENT) },
        [CLUE_SHIP_LEFT] = new[] { (0, -1, WATER), (0, 1, SHIP_SEGMENT) },
        [CLUE_SHIP_RIGHT] = new[] { (0, 1, WATER), (0, -1, SHIP_SEGMENT) }
    };
    public BattleshipsSolitaire(int size, int[] colHints, int[] rowHints, List<int> fleet, char[][] initialGrid)
    {
        _size = size;
        _colHints = colHints;
        _rowHints = rowHints;
        _fleet = fleet;
        _grid = initialGrid;
        _colCounts = new int[size];
        _rowCounts = new int[size];
        _colDots = new int[size];
        _rowDots = new int[size];
        _visited = new bool[size][];
        _initialGrid = new char[size][];
        for (var i = 0; i < size; i++)
        {
            _visited[i] = new bool[size];
            _initialGrid[i] = (char[])initialGrid[i].Clone();
        }
        _fleet.Sort();
        for (var r = 0; r < size; r++)
        {
            for (var c = 0; c < size; c++)
            {
                if (IsShip(_grid[r][c]))
                {
                    _rowCounts[r]++;
                    _colCounts[c]++;
                }
                else if (_grid[r][c] == UNKNOWN)
                {
                    _rowDots[r]++;
                    _colDots[c]++;
                }
            }
        }
    }
    public char[][] Solve()
    {
        if (!PropagateInitialClues()) return null;
        if (!Propagate()) return null;
        if (RecursiveSolve()) return _grid;
        return null;
    }
    private bool SetCell(int r, int c, char val)
    {
        var dummy = false;
        return SetCell(r, c, val, ref dummy);
    }
    private bool SetCell(int r, int c, char val, ref bool changed)
    {
        if (r < 0 || r >= _size || c < 0 || c >= _size) return true;
        var cur = _grid[r][c];
        var isCurShip = IsShip(cur);
        if (cur == val || (val == SHIP_SEGMENT && isCurShip)) return true;
        if ((val == WATER && isCurShip) || (val == SHIP_SEGMENT && cur == WATER) || cur != UNKNOWN) return false;
        _grid[r][c] = val;
        _rowDots[r]--;
        _colDots[c]--;
        if (val == SHIP_SEGMENT)
        {
            _rowCounts[r]++;
            _colCounts[c]++;
        }
        if (!IsValid(r, c))
        {
            if (val == SHIP_SEGMENT)
            {
                _rowCounts[r]--;
                _colCounts[c]--;
            }
            _rowDots[r]++;
            _colDots[c]++;
            _grid[r][c] = cur;
            return false;
        }
        changed = true;
        return true;
    }
    private bool PropagateInitialClues()
    {
        var changed = false;
        for (var r = 0; r < _size; r++)
        {
            for (var c = 0; c < _size; c++)
            {
                if (_clueActions.TryGetValue(_initialGrid[r][c], out var actions))
                {
                    foreach (var (dr, dc, val) in actions)
                    {
                        if (!SetCell(r + dr, c + dc, val, ref changed)) return false;
                    }
                }
            }
        }
        return true;
    }
    private bool Propagate()
    {
        var changed = true;
        while (changed)
        {
            changed = false;
            for (var i = 0; i < _size; i++)
            {
                if (_rowDots[i] > 0)
                {
                    if (_rowCounts[i] == _rowHints[i])
                    {
                        for (var j = 0; j < _size; j++)
                            if (_grid[i][j] == UNKNOWN)
                                if (!SetCell(i, j, WATER, ref changed)) return false;
                    }
                    else if (_rowHints[i] == _rowCounts[i] + _rowDots[i])
                    {
                        for (var j = 0; j < _size; j++)
                            if (_grid[i][j] == UNKNOWN)
                                if (!SetCell(i, j, SHIP_SEGMENT, ref changed)) return false;
                    }
                }
                if (_colDots[i] > 0)
                {
                    if (_colCounts[i] == _colHints[i])
                    {
                        for (var j = 0; j < _size; j++)
                            if (_grid[j][i] == UNKNOWN)
                                if (!SetCell(j, i, WATER, ref changed)) return false;
                    }
                    else if (_colHints[i] == _colCounts[i] + _colDots[i])
                    {
                        for (var j = 0; j < _size; j++)
                            if (_grid[j][i] == UNKNOWN)
                                if (!SetCell(j, i, SHIP_SEGMENT, ref changed)) return false;
                    }
                }
            }
            for (var r = 0; r < _size; r++)
            {
                for (var c = 0; c < _size; c++)
                {
                    if (IsShip(_grid[r][c]))
                    {
                        for (var i = 0; i < 4; i++)
                            if (!SetCell(r + _drDiag[i], c + _dcDiag[i], WATER, ref changed)) return false;
                    }
                    if (_initialGrid[r][c] == CLUE_SHIP_MIDDLE)
                    {
                        var adjVal = new char[4];
                        for (var i = 0; i < 4; i++) adjVal[i] = Get(r + _drAdj[i], c + _dcAdj[i]);
                        var shipUp = IsShip(adjVal[0]);
                        var shipDown = IsShip(adjVal[1]);
                        var shipLeft = IsShip(adjVal[2]);
                        var shipRight = IsShip(adjVal[3]);
                        var waterUp = adjVal[0] == WATER;
                        var waterDown = adjVal[1] == WATER;
                        var waterLeft = adjVal[2] == WATER;
                        var waterRight = adjVal[3] == WATER;
                        if (shipUp || shipDown || waterLeft || waterRight) // Vertical
                        {
                            if (!SetCell(r + _drAdj[0], c + _dcAdj[0], SHIP_SEGMENT, ref changed) ||
                                !SetCell(r + _drAdj[1], c + _dcAdj[1], SHIP_SEGMENT, ref changed) ||
                                !SetCell(r + _drAdj[2], c + _dcAdj[2], WATER, ref changed) ||
                                !SetCell(r + _drAdj[3], c + _dcAdj[3], WATER, ref changed)) return false;
                        }
                        else if (shipLeft || shipRight || waterUp || waterDown) // Horizontal
                        {
                            if (!SetCell(r + _drAdj[2], c + _dcAdj[2], SHIP_SEGMENT, ref changed) ||
                                !SetCell(r + _drAdj[3], c + _dcAdj[3], SHIP_SEGMENT, ref changed) ||
                                !SetCell(r + _drAdj[0], c + _dcAdj[0], WATER, ref changed) ||
                                !SetCell(r + _drAdj[1], c + _dcAdj[1], WATER, ref changed)) return false;
                        }
                    }
                }
            }
        }
        return true;
    }
    private (int r, int c) FindNextCellToTry()
    {
        var minDots = int.MaxValue;
        var r = -1;
        var c = -1;
        var useRow = false;
        for (var i = 0; i < _size; i++)
        {
            if (_rowDots[i] > 0 && _rowDots[i] < minDots)
            {
                minDots = _rowDots[i];
                r = i;
                useRow = true;
            }
        }
        for (var i = 0; i < _size; i++)
        {
            if (_colDots[i] > 0 && _colDots[i] < minDots)
            {
                minDots = _colDots[i];
                c = i;
                useRow = false;
            }
        }
        if (minDots == int.MaxValue) return (-1, -1);
        if (useRow)
        {
            for (var j = 0; j < _size; j++)
                if (_grid[r][j] == UNKNOWN) return (r, j);
        }
        else
        {
            for (var i = 0; i < _size; i++)
                if (_grid[i][c] == UNKNOWN) return (i, c);
        }
        return (-1, -1);
    }
    private void RestoreState(char[][] g, int[] rc, int[] cc, int[] rd, int[] cd)
    {
        for (var i = 0; i < _size; i++) _grid[i] = (char[])g[i].Clone();
        Array.Copy(rc, _rowCounts, _size);
        Array.Copy(cc, _colCounts, _size);
        Array.Copy(rd, _rowDots, _size);
        Array.Copy(cd, _colDots, _size);
    }
    private bool RecursiveSolve()
    {
        var (r, c) = FindNextCellToTry();
        if (r == -1) return ValidateSolution();
        var gridBackup = new char[_size][];
        for (var i = 0; i < _size; i++) gridBackup[i] = (char[])_grid[i].Clone();
        var rowCountsBackup = (int[])_rowCounts.Clone();
        var colCountsBackup = (int[])_colCounts.Clone();
        var rowDotsBackup = (int[])_rowDots.Clone();
        var colDotsBackup = (int[])_colDots.Clone();
        if (SetCell(r, c, SHIP_SEGMENT) && Propagate() && RecursiveSolve()) return true;
        RestoreState(gridBackup, rowCountsBackup, colCountsBackup, rowDotsBackup, colDotsBackup);
        if (SetCell(r, c, WATER) && Propagate() && RecursiveSolve()) return true;
        RestoreState(gridBackup, rowCountsBackup, colCountsBackup, rowDotsBackup, colDotsBackup);
        return false;
    }
    private bool IsValid(int r, int c)
    {
        if (_rowCounts[r] > _rowHints[r]) return false;
        if (_colCounts[c] > _colHints[c]) return false;
        if (_grid[r][c] == WATER)
        {
            if (_rowCounts[r] + _rowDots[r] < _rowHints[r]) return false;
            if (_colCounts[c] + _colDots[c] < _colHints[c]) return false;
            for (var i = 0; i < 4; i++)
            {
                if (GetInitial(r + _drAdj[i], c + _dcAdj[i]) == _oppositeInitialClue[i]) return false;
            }
            return true;
        }
        for (var i = 0; i < 4; i++)
        {
            if (IsShip(Get(r + _drDiag[i], c + _dcDiag[i]))) return false;
        }
        var adjShip = new bool[4];
        for (var i = 0; i < 4; i++)
        {
            adjShip[i] = IsShip(Get(r + _drAdj[i], c + _dcAdj[i]));
            if (Get(r + _drAdj[i], c + _dcAdj[i]) == _oppositeClue[i]) return false;
        }
        if ((adjShip[0] || adjShip[1]) && (adjShip[2] || adjShip[3])) return false; // (Up/Down) && (Left/Right)
        return true;
    }
    private bool ValidateSolution()
    {
        for (var i = 0; i < _size; i++)
        {
            if (_rowCounts[i] != _rowHints[i]) return false;
            if (_colCounts[i] != _colHints[i]) return false;
        }
        var foundShips = new List<int>();
        for (var r = 0; r < _size; r++) Array.Clear(_visited[r], 0, _size);
        for (var r = 0; r < _size; r++)
        {
            for (var c = 0; c < _size; c++)
            {
                if (IsShip(_grid[r][c]) && !_visited[r][c])
                {
                    var len = FindShipLength(r, c);
                    if (len == 0) return false;
                    foundShips.Add(len);
                }
            }
        }
        foundShips.Sort();
        return _fleet.SequenceEqual(foundShips);
    }
    private int FindShipLength(int r, int c)
    {
        var len = 0;
        var q = new Queue<(int, int)>();
        q.Enqueue((r, c));
        _visited[r][c] = true;
        var minR = r; var maxR = r;
        var minC = c; var maxC = c;
        while (q.Count > 0)
        {
            var (cr, cc) = q.Dequeue();
            len++;
            if (cr < minR) minR = cr;
            if (cr > maxR) maxR = cr;
            if (cc < minC) minC = cc;
            if (cc > maxC) maxC = cc;
            for (var i = 0; i < 4; i++)
            {
                var nr = cr + _drAdj[i];
                var nc = cc + _dcAdj[i];
                if (nr >= 0 && nr < _size && nc >= 0 && nc < _size && IsShip(_grid[nr][nc]) && !_visited[nr][nc])
                {
                    _visited[nr][nc] = true;
                    q.Enqueue((nr, nc));
                }
            }
        }
        var horiz = minR == maxR;
        var vert = minC == maxC;
        if (!horiz && !vert) return 0;
        if (horiz && (maxC - minC + 1) != len) return 0;
        if (vert && (maxR - minR + 1) != len) return 0;
        return len;
    }
    private char Get(int r, int c) => (r < 0 || r >= _size || c < 0 || c >= _size) ? WATER : _grid[r][c];
    private char GetInitial(int r, int c) => (r < 0 || r >= _size || c < 0 || c >= _size) ? WATER : _initialGrid[r][c];
    private bool IsShip(char c)
    {
        return ALL_SHIP_SYMBOLS.Contains(c);
    }
}