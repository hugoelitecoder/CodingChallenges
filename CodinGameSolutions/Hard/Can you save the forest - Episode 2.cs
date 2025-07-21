using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public class Player
{
    public const int ForestSize = 10;

    public static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        int truckCount = int.Parse(inputs[0]);
        int maxBurn = int.Parse(inputs[1]);
        int turn = 0;

        while (true)
        {
            turn++;
            var grid = new ForestTile[ForestSize, ForestSize];
            for (int y = 0; y < ForestSize; y++)
            {
                var line = Console.ReadLine();
                for (int x = 0; x < ForestSize; x++)
                    grid[x, y] = new ForestTile(x, y, line[x]);
            }
            var fireTrucks = new List<FireTruck>();
            for (int i = 0; i < truckCount; i++)
            {
                var nums = Console.ReadLine().Split().Select(int.Parse).ToArray();
                fireTrucks.Add(new FireTruck(grid[nums[0], nums[1]]));
            }

            ForestBoard.InitializeNeighbors(grid);
            ForestTile.ComputeDistances(grid);
            var fires = ForestBoard.ReadFires(grid);

            var game = new ForestBoard(grid, fireTrucks, fires, maxBurn);
            game.ComputeScore();
            PrintDebug(game, turn);

            var sw = Stopwatch.StartNew();
            ForestBoard chosen = null;
            if (game.FireTrucks.Count < 3) chosen = BeamSearch(game, 100, 3);
            else if (game.FireTrucks.Count < 4) chosen = BeamSearch(game, 50, 2);
            else chosen = BeamSearch(game, 50, 1);
            sw.Stop();
            Console.Error.WriteLine("[DEBUG] Time: " + sw.ElapsedMilliseconds + " ms");

            foreach (var action in chosen.Actions)
                Console.WriteLine(action);
        }
    }

    public static ForestBoard BeamSearch(ForestBoard game, int width, int maxDepth)
    {
        var games = new List<ForestBoard> { game };
        for (int depth = 0; depth < maxDepth; depth++)
        {
            var next = games.SelectMany(g => g.Expand()).OrderBy(g => g.Score).ToList();
            if (next.Count > width) next = next.Take(width).ToList();
            if (!next.Any()) break;
            games = next;
        }
        var result = games[0];
        while (result.Parent?.Parent != null) result = result.Parent;
        return result;
    }

    public static void PrintDebug(ForestBoard game, int turn)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"---- MAP TURN {turn} ----");
        var currentGrid = new char[ForestSize, ForestSize];
        for (int y = 0; y < ForestSize; y++)
            for (int x = 0; x < ForestSize; x++)
                currentGrid[x, y] = game.Grid[x, y].State switch
                {
                    TileState.EMPTY => '.',
                    TileState.WATER => '~',
                    TileState.FOREST => '^',
                    TileState.BURNT => '*',
                    _ => '?'
                };
        for (int i = 0; i < 3; i++)
            foreach (var fire in game.Fires[i])
                currentGrid[fire.X, fire.Y] = (char)('1' + i);
        foreach (var truck in game.FireTrucks)
            currentGrid[truck.Tile.X, truck.Tile.Y] = 'T';
        for (int y = 0; y < ForestSize; y++)
        {
            for (int x = 0; x < ForestSize; x++)
                sb.Append(currentGrid[x, y]);
            sb.AppendLine();
        }
        sb.AppendLine();
        sb.AppendLine($"Score: {game.Score:F0}");
        sb.AppendLine($"Fires (L1/L2/L3): {game.Fires[0].Count}/{game.Fires[1].Count}/{game.Fires[2].Count}");
        sb.AppendLine($"Burned: {game.Fires[3].Count}");
        sb.AppendLine();
        Console.Error.Write(sb.ToString());
    }
}

public enum TileState { EMPTY, WATER, FOREST, FIRE1, FIRE2, FIRE3, BURNT }

public class ForestTile
{
    public int X, Y;
    public TileState State;
    public List<ForestTile> Neighbors4 = new();
    public List<ForestTile> Neighbors8 = new();
    public List<string> Neighbors4Dirs = new();
    public List<string> Neighbors8Dirs = new();
    private static int[,] _dist;
    public int ID => Player.ForestSize * X + Y;

    public ForestTile(int x, int y, char c)
    {
        X = x; Y = y;
        State = c switch
        {
            '.' => TileState.EMPTY,
            '~' => TileState.WATER,
            '^' => TileState.FOREST,
            '*' => TileState.BURNT,
            _ => (TileState)(TileState.FIRE1 + c - '1')
        };
    }

    public int DistTo(ForestTile target) => _dist[ID, target.ID];

    public static void ComputeDistances(ForestTile[,] grid)
    {
        if (_dist != null) return;
        int size = Player.ForestSize;
        int total = size * size;
        _dist = new int[total, total];
        for (int a = 0; a < total; a++)
            for (int b = 0; b < total; b++)
                _dist[a, b] = 1000;
        foreach (var target in grid)
        {
            if (target.State == TileState.WATER) continue;
            var queue = new Queue<ForestTile>();
            foreach (var n in target.Neighbors8)
            {
                _dist[n.ID, target.ID] = 0;
                queue.Enqueue(n);
            }
            while (queue.Count > 0)
            {
                var q = queue.Dequeue();
                foreach (var n in q.Neighbors4)
                    if (_dist[n.ID, target.ID] == 1000)
                    {
                        _dist[n.ID, target.ID] = 1 + _dist[q.ID, target.ID];
                        queue.Enqueue(n);
                    }
            }
        }
    }
    public override string ToString() => State switch
    {
        TileState.EMPTY => ".",
        TileState.WATER => "~",
        TileState.FOREST => "^",
        TileState.BURNT => "*",
        _ => ((int)(State - TileState.FIRE1 + 1)).ToString()
    };
}

public enum FireTruckActionType { WAIT, MOVE, FIGHT }

public class FireTruckAction
{
    public FireTruckActionType Type;
    public FireTruck Truck;
    public ForestTile Target;
    public string Dir;

    public FireTruckAction(FireTruckActionType type, FireTruck truck, ForestTile target, string dir = null)
    {
        Type = type;
        Truck = truck;
        Target = target;
        Dir = dir;
    }

    public override string ToString() => Type switch
    {
        FireTruckActionType.WAIT => "WAIT",
        FireTruckActionType.MOVE => "MOVE " + Dir,
        FireTruckActionType.FIGHT => "FIGHT " + Dir,
        _ => "WAIT"
    };

    public void Apply(ForestBoard game)
    {
        switch (Type)
        {
            case FireTruckActionType.MOVE: Truck.Tile = Target; break;
            case FireTruckActionType.FIGHT: game.ExtinguishFire(Target); break;
            case FireTruckActionType.WAIT: break;
        }
    }
}

public class FireTruck
{
    public ForestTile Tile;
    public FireTruck(ForestTile tile) => Tile = tile;

    public IEnumerable<FireTruckAction> GetActions(ForestBoard game)
    {
        yield return new FireTruckAction(FireTruckActionType.WAIT, this, Tile);
        for (int dir = 0; dir < Tile.Neighbors4.Count; dir++)
        {
            var to = Tile.Neighbors4[dir];
            if (game.HasActiveFire(to)) continue;
            yield return new FireTruckAction(FireTruckActionType.MOVE, new FireTruck(Tile), to, Tile.Neighbors4Dirs[dir]);
        }
        for (int dir = 0; dir < Tile.Neighbors8.Count; dir++)
        {
            var to = Tile.Neighbors8[dir];
            if (game.HasActiveFire(to))
                yield return new FireTruckAction(FireTruckActionType.FIGHT, this, to, Tile.Neighbors8Dirs[dir]);
        }
    }

    public override string ToString() => $"{Tile.X} {Tile.Y}";
}

public class ForestBoard
{
    public static readonly string[] Directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
    public static readonly int[] Dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
    public static readonly int[] Dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

    public ForestTile[,] Grid;
    public List<ForestTile>[] Fires;
    public List<FireTruck> FireTrucks;
    public ForestBoard Parent;
    public List<FireTruckAction> Actions;
    public double Score;
    private int _maxBurn;

    public ForestBoard(ForestTile[,] grid, List<FireTruck> trucks, List<ForestTile>[] fires, int maxBurn)
    {
        Grid = grid;
        FireTrucks = trucks;
        Fires = fires;
        Parent = null;
        Actions = null;
        _maxBurn = maxBurn;
    }

    public ForestBoard(ForestBoard parent, List<FireTruckAction> actions)
    {
        Parent = parent;
        Actions = actions;
        Grid = parent.Grid;
        FireTrucks = actions.Select(a => a.Truck).ToList();
        Fires = new List<ForestTile>[4];
        var aboutToBurn = parent.Fires[2].ToList();
        Fires[0] = aboutToBurn;
        Fires[1] = parent.Fires[0].ToList();
        Fires[2] = parent.Fires[1].ToList();
        Fires[3] = parent.Fires[3].ToList();
        foreach (var action in actions) action.Apply(this);
        var ignore = new HashSet<ForestTile>(FireTrucks.Select(t => t.Tile));
        for (int i = 0; i <= 3; i++)
            foreach (var tile in Fires[i]) ignore.Add(tile);
        Fires[0] = new List<ForestTile>();
        foreach (var burning in aboutToBurn)
        {
            Fires[3].Add(burning);
            foreach (var neighbor in burning.Neighbors4)
                if (neighbor.State == TileState.FOREST && !ignore.Contains(neighbor))
                {
                    Fires[0].Add(neighbor);
                    ignore.Add(neighbor);
                }
        }
        _maxBurn = parent._maxBurn;
        ComputeScore();
    }

    public void ComputeScore()
    {
        Score = 0;
        for (int i = 0; i < 4; i++) Score += (i + 1) * Fires[i].Count * 1000;
        var allFires = Fires.Take(3).SelectMany(f => f).ToList();
        foreach (var fire in allFires)
            Score += FireTrucks.Min(t => t.Tile.DistTo(fire));
        if (allFires.Count == 0) return;
        foreach (var truck in FireTrucks)
            Score += allFires.Min(f => truck.Tile.DistTo(f));
    }

    public IEnumerable<ForestBoard> Expand()
    {
        var possibleActions = FireTrucks.Select(t => t.GetActions(this).ToList()).ToArray();
        var actionCombos = new List<List<FireTruckAction>>();
        EnumerateCombos(possibleActions, new(), actionCombos, new(), new());
        foreach (var actions in actionCombos)
            yield return new ForestBoard(this, actions);
    }

    public bool HasActiveFire(ForestTile tile) => Fires[0].Contains(tile) || Fires[1].Contains(tile) || Fires[2].Contains(tile);

    public void ExtinguishFire(ForestTile tile)
    {
        for (int i = 0; i < 3; i++)
            if (Fires[i].Contains(tile)) { Fires[i].Remove(tile); break; }
    }

    public static void InitializeNeighbors(ForestTile[,] grid)
    {
        int size = Player.ForestSize;
        foreach (var tile in grid)
        {
            for (int d = 0; d < Directions.Length; d++)
            {
                int x_ = tile.X + Dx[d], y_ = tile.Y + Dy[d];
                if (x_ < 0 || x_ >= size || y_ < 0 || y_ >= size || grid[x_, y_].State == TileState.WATER) continue;
                var t2 = grid[x_, y_];
                if (Directions[d].Length == 1) { tile.Neighbors4.Add(t2); tile.Neighbors4Dirs.Add(Directions[d]); }
                tile.Neighbors8.Add(t2); tile.Neighbors8Dirs.Add(Directions[d]);
            }
        }
    }

    public static List<ForestTile>[] ReadFires(ForestTile[,] grid)
    {
        var fires = new List<ForestTile>[4];
        for (int i = 0; i < fires.Length; i++) fires[i] = new List<ForestTile>();
        foreach (var tile in grid)
        {
            if (tile.State == TileState.FIRE1) fires[0].Add(tile);
            if (tile.State == TileState.FIRE2) fires[1].Add(tile);
            if (tile.State == TileState.FIRE3) fires[2].Add(tile);
            if (tile.State == TileState.BURNT) fires[3].Add(tile);
        }
        return fires;
    }

    private void EnumerateCombos(List<FireTruckAction>[] options, List<FireTruckAction> acc, List<List<FireTruckAction>> combos, List<ForestTile> truckTiles, List<ForestTile> extinguishTiles)
    {
        if (acc.Count == options.Length)
        {
            combos.Add(acc.ToList());
            return;
        }
        foreach (var action in options[acc.Count])
        {
            if (action.Type == FireTruckActionType.MOVE || action.Type == FireTruckActionType.WAIT)
            {
                if (truckTiles.Contains(action.Target)) continue;
                truckTiles.Add(action.Target); acc.Add(action);
                EnumerateCombos(options, acc, combos, truckTiles, extinguishTiles);
                acc.RemoveAt(acc.Count - 1); truckTiles.RemoveAt(truckTiles.Count - 1);
            }
            else
            {
                if (truckTiles.Contains(action.Truck.Tile) || extinguishTiles.Contains(action.Target)) continue;
                truckTiles.Add(action.Truck.Tile); extinguishTiles.Add(action.Target); acc.Add(action);
                EnumerateCombos(options, acc, combos, truckTiles, extinguishTiles);
                acc.RemoveAt(acc.Count - 1); truckTiles.RemoveAt(truckTiles.Count - 1); extinguishTiles.RemoveAt(extinguishTiles.Count - 1);
            }
        }
    }
}
