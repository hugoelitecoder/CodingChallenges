using System;

class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var lines = new string[n];
        for (var i = 0; i < n; i++)
        {
            lines[i] = Console.ReadLine();
        }

        var takuzu = new Takuzu(n, lines);
        takuzu.Solve();

        var result = takuzu.GetResult();
        foreach (var row in result)
        {
            Console.WriteLine(row);
        }
    }
}

class Takuzu
{
    private readonly int _n;
    private readonly int _halfN;
    private readonly int[][] _grid;

    public Takuzu(int n, string[] lines)
    {
        _n = n;
        _halfN = n / 2;
        _grid = new int[n][];
        for (var i = 0; i < n; i++)
        {
            _grid[i] = new int[n];
            for (var j = 0; j < n; j++)
            {
                var ch = lines[i][j];
                _grid[i][j] = ch == '.' ? -1 : ch - '0';
            }
        }
    }

    public void Solve()
    {
        Backtrack();
    }

    public string[] GetResult()
    {
        var result = new string[_n];
        for (var i = 0; i < _n; i++)
        {
            result[i] = string.Join("", _grid[i]);
        }
        return result;
    }

    private bool Backtrack()
    {
        var r = -1;
        var c = -1;
        for (var i = 0; i < _n; i++)
        {
            for (var j = 0; j < _n; j++)
            {
                if (_grid[i][j] == -1)
                {
                    r = i;
                    c = j;
                    break;
                }
            }
            if (r != -1) break;
        }

        if (r == -1)
        {
            return true;
        }
        
        for (var val = 0; val <= 1; val++)
        {
            _grid[r][c] = val;
            if (IsValid(r, c))
            {
                if (Backtrack())
                {
                    return true;
                }
            }
        }

        _grid[r][c] = -1;
        return false;
    }

    private bool IsValid(int r, int c)
    {
        var val = _grid[r][c];

        if (c > 1 && _grid[r][c - 1] == val && _grid[r][c - 2] == val) return false;
        if (c < _n - 2 && _grid[r][c + 1] == val && _grid[r][c + 2] == val) return false;
        if (c > 0 && c < _n - 1 && _grid[r][c - 1] == val && _grid[r][c + 1] == val) return false;

        if (r > 1 && _grid[r - 1][c] == val && _grid[r - 2][c] == val) return false;
        if (r < _n - 2 && _grid[r + 1][c] == val && _grid[r + 2][c] == val) return false;
        if (r > 0 && r < _n - 1 && _grid[r - 1][c] == val && _grid[r + 1][c] == val) return false;

        var rowCount = 0;
        var colCount = 0;
        var rowEmpty = 0;
        var colEmpty = 0;
        for (var i = 0; i < _n; i++)
        {
            if (_grid[r][i] == val) rowCount++;
            if (_grid[r][i] == -1) rowEmpty++;
            if (_grid[i][c] == val) colCount++;
            if (_grid[i][c] == -1) colEmpty++;
        }

        if (rowCount > _halfN || colCount > _halfN) return false;

        if (rowEmpty == 0)
        {
            for (var i = 0; i < _n; i++)
            {
                if (i == r) continue;
                if (AreRowsSame(i, r)) return false;
            }
        }

        if (colEmpty == 0)
        {
            for (var i = 0; i < _n; i++)
            {
                if (i == c) continue;
                if (AreColsSame(i, c)) return false;
            }
        }

        return true;
    }

    private bool AreRowsSame(int r1, int r2)
    {
        for (var j = 0; j < _n; j++)
        {
            if (_grid[r1][j] == -1) return false;
        }
        for (var j = 0; j < _n; j++)
        {
            if (_grid[r1][j] != _grid[r2][j]) return false;
        }
        return true;
    }

    private bool AreColsSame(int c1, int c2)
    {
        for (var i = 0; i < _n; i++)
        {
            if (_grid[i][c1] == -1) return false;
        }
        for (var i = 0; i < _n; i++)
        {
            if (_grid[i][c1] != _grid[i][c2]) return false;
        }
        return true;
    }
}