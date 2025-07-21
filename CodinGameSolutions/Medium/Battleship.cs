using System;
using System.Collections.Generic;



class Solution
{
    static void Main(string[] args)
    {
        string shot = Console.ReadLine();
        string[] lines = new string[10];
        for (int i = 0; i < 10; i++)
            lines[i] = Console.ReadLine();

        var game = new BattleShips(lines);
        if (!game.ValidateGrid())
        {
            Console.WriteLine("INVALID");
            return;
        }

        int size;
        var result = game.ProcessShot(shot, out size);
        switch (result)
        {
            case ShotResult.Missed:
                Console.WriteLine("MISSED");
                break;
            case ShotResult.Touche:
                Console.WriteLine("TOUCHE");
                break;
            case ShotResult.Sunk:
                Console.WriteLine($"TOUCHE COULE {size}");
                break;
            case ShotResult.SunkAndLose:
                Console.WriteLine($"TOUCHE COULE {size} THEN LOSE");
                break;
        }
    }
}
enum CellState { Empty, Intact, Destroyed }

enum ShotResult { Missed, Touche, Sunk, SunkAndLose }

class BattleShips
{
    private static readonly (int dr, int dc)[] Dirs = new (int, int)[]
    {
        (1, 0), (-1, 0), (0, 1), (0, -1)
    };

    private readonly CellState[,] _grid;
    private readonly List<Ship> _ships;

    public BattleShips(string[] lines)
    {
        _grid = new CellState[10, 10];
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                _grid[r, c] = lines[r][c] == '+' ? CellState.Intact :
                              lines[r][c] == '_' ? CellState.Destroyed :
                              CellState.Empty;

        _ships = ExtractShips();
        foreach (var ship in _ships)
            foreach (var (r, c) in ship.Coordinates)
                if (_grid[r, c] == CellState.Destroyed)
                    ship.MarkHit(r, c);
    }

    public bool ValidateGrid()
    {
        return ShipCountsValid() && ShipsNotTouching();
    }

    public ShotResult ProcessShot(string shot, out int size)
    {
        int col = shot[0] - 'A';
        int row = int.Parse(shot.Substring(1)) - 1;
        if (row < 0 || row >= 10 || col < 0 || col >= 10 || _grid[row, col] != CellState.Intact)
        {
            size = 0;
            return ShotResult.Missed;
        }

        _grid[row, col] = CellState.Destroyed;
        var ship = GetShipAt(row, col);
        ship.MarkHit(row, col);

        if (!ship.IsSunk)
        {
            size = 0;
            return ShotResult.Touche;
        }

        size = ship.Size;
        return AllShipsSunk() ? ShotResult.SunkAndLose : ShotResult.Sunk;
    }

    private List<Ship> ExtractShips()
    {
        var ships = new List<Ship>();
        var visited = new bool[10, 10];
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                if (!visited[r, c] && _grid[r, c] != CellState.Empty)
                    ships.Add(BuildShip(r, c, visited));
        return ships;
    }

    private Ship BuildShip(int r, int c, bool[,] visited)
    {
        var coords = new List<(int, int)>();
        var q = new Queue<(int, int)>();
        q.Enqueue((r, c));
        visited[r, c] = true;
        while (q.Count > 0)
        {
            var (cr, cc) = q.Dequeue();
            coords.Add((cr, cc));
            foreach (var (dr, dc) in Dirs)
            {
                int nr = cr + dr, nc = cc + dc;
                if (nr >= 0 && nr < 10 && nc >= 0 && nc < 10
                    && !visited[nr, nc] && _grid[nr, nc] != CellState.Empty)
                {
                    visited[nr, nc] = true;
                    q.Enqueue((nr, nc));
                }
            }
        }
        return new Ship(coords);
    }

    private bool ShipCountsValid()
    {
        var count = new Dictionary<int, int>();
        foreach (var ship in _ships)
        {
            int s = ship.Size;
            if (!count.ContainsKey(s)) count[s] = 0;
            count[s]++;
        }
        return count.TryGetValue(5, out var a) && a == 1
            && count.TryGetValue(4, out var b) && b == 1
            && count.TryGetValue(3, out var c3) && c3 == 2
            && count.TryGetValue(2, out var d) && d == 1;
    }

    private bool ShipsNotTouching()
    {
        var idMap = new int[10, 10];
        for (int i = 0; i < _ships.Count; i++)
            foreach (var (r, c) in _ships[i].Coordinates)
                idMap[r, c] = i + 1;

        foreach (var ship in _ships)
            foreach (var (r, c) in ship.Coordinates)
                for (int dr = -1; dr <= 1; dr++)
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr == 0 && dc == 0) continue;
                        int nr = r + dr, nc = c + dc;
                        if (nr >= 0 && nr < 10 && nc >= 0 && nc < 10
                            && idMap[nr, nc] != 0
                            && idMap[nr, nc] != idMap[r, c])
                            return false;
                    }

        return true;
    }

    private Ship GetShipAt(int r, int c)
    {
        foreach (var ship in _ships)
            if (ship.Contains(r, c))
                return ship;
        throw new InvalidOperationException();
    }

    private bool AllShipsSunk()
    {
        foreach (var ship in _ships)
            if (!ship.IsSunk)
                return false;
        return true;
    }
}

class Ship
{
    public List<(int r, int c)> Coordinates { get; }
    private readonly HashSet<(int, int)> _hits;
    public int Size => Coordinates.Count;
    public bool IsSunk => _hits.Count == Size;

    public Ship(List<(int, int)> coords)
    {
        Coordinates = coords;
        _hits = new HashSet<(int, int)>();
    }

    public bool Contains(int r, int c) => Coordinates.Contains((r, c));

    public void MarkHit(int r, int c)
    {
        if (Contains(r, c))
            _hits.Add((r, c));
    }
}