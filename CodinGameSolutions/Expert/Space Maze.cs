using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

public class Solution
{
    public static void Main(string[] args)
    {
        var inputLine = Console.ReadLine();
        Console.Error.WriteLine($"[DEBUG]: {inputLine}");
        var input = inputLine.Split().Select(int.Parse).ToArray();
        var width = input[0];
        var height = input[1];
        var grid = new string[height];
        for (var y = 0; y < height; y++)
        {
            grid[y] = Console.ReadLine();
            Console.Error.WriteLine($"[DEBUG]: {grid[y]}");
        }
        var entityCountLine = Console.ReadLine();
        Console.Error.WriteLine($"[DEBUG]: {entityCountLine}");
        var entityCount = int.Parse(entityCountLine);
        var carId = -1;
        var carX = -1;
        var carY = -1;
        var platforms = new List<Platform>();
        for (var i = 0; i < entityCount; i++)
        {
            var entityLine = Console.ReadLine();
            Console.Error.WriteLine($"[DEBUG]: {entityLine}");
            var parts = entityLine.Split();
            var id = int.Parse(parts[0]);
            var x = int.Parse(parts[1]);
            var y = int.Parse(parts[2]);
            var dir = parts[3];
            if (dir == "CAR")
            {
                carId = id;
                carX = x;
                carY = y;
            }
            else
            {
                platforms.Add(new Platform(id, x, y, dir));
            }
        }
        var board = Board.Build(width, height, grid, carId, carX, carY, platforms);
        var solver = new BoardSolver();
        var sw = Stopwatch.StartNew();
        var actions = solver.Solve(board);
        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] Planning time: {sw.ElapsedMilliseconds} ms");
        Console.Error.WriteLine(actions != null ? $"[DEBUG] Actions planned: {actions.Count}" : "No solution found");
        Console.Error.WriteLine($"[DEBUG] Actions: {string.Join(", ",actions)}");
        var actionIndex = 0;
        if (actions != null && actions.Count > 0)
        {
            Console.WriteLine(actions[actionIndex++]);
        }
        else
        {
            Console.WriteLine("WAIT");
        }
        while (true)
        {
            var turnEntitiesLine = Console.ReadLine();
            var turnEntities = int.Parse(turnEntitiesLine);
            for (var i = 0; i < turnEntities; i++)
            {
                var ent = Console.ReadLine();
            }
            if (actions != null && actionIndex < actions.Count)
            {
                Console.WriteLine(actions[actionIndex]);
                actionIndex++;
            }
            else
            {
                Console.WriteLine("WAIT");
            }
        }
    }
}

class BoardSolver
{
    public List<string> Solve(Board board)
    {
        var visited = new Dictionary<Board, Board>(1024);
        visited.Add(board, board);
        var bfs = new HashSet<Board>[1000];
        for (var i = 0; i < bfs.Length; i++) bfs[i] = new HashSet<Board>();
        bfs[0].Add(board);
        Board final = null;
        for (var d = 0; final == null && d < bfs.Length && visited.Count < 1_000_000; d++)
        {
            foreach (var b in bfs[d])
            {
                var expanded = b.Expand();
                for (var k = 0; k < expanded.Count; ++k)
                {
                    var next = expanded[k];
                    next.Depth = d + next.Cost();
                    if (!visited.TryGetValue(next, out var old) || old.Depth > next.Depth)
                    {
                        if (old != null)
                        {
                            visited.Remove(old);
                            bfs[old.Depth].Remove(old);
                        }
                        visited[next] = next;
                        bfs[next.Depth].Add(next);
                        if (next.IsWin())
                        {
                            final = next;
                            if (!final.Car.Cell.IsTarget)
                                final = next.Expand()[0];
                        }
                    }
                }
                if (final != null) break;
            }
        }
        if (final == null) return null;
        var actions = new List<string>(32);
        while (final.Parent != null)
        {
            actions.AddRange(final.GetActions());
            final = final.Parent;
        }
        actions.Reverse();
        return actions;
    }
}

class Board : IEquatable<Board>
{
    public static int Width;
    public static int Height;
    public static Cell[,] Grid;
    public Car Car;
    public List<Platform> Platforms = new List<Platform>(8);
    public Board Parent;
    public int Depth;
    static Cell Target;
    static int[,] Dists;
    public static Board Build(int width, int height, string[] gridLines, int carId, int carX, int carY, List<Platform> plats)
    {
        Width = width;
        Height = height;
        Grid = new Cell[width, height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            Grid[x, y] = new Cell(x, y, gridLines[y][x]);
        foreach (var c in Grid) c.InitNeighbors();
        Car.InitZobrist(width, height);
        Platform.InitZobrist(width, height);
        InitDists();
        var car = new Car(carId, Grid[carX, carY]);
        var platforms = new List<Platform>(plats.Count);
        for (var i = 0; i < plats.Count; ++i)
        {
            var p = plats[i];
            platforms.Add(new Platform(p.Id, Board.Grid[p.X, p.Y], string.Join("", p.Dir.Select(d => Car.dirNames[d]))));
        }
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            if (Grid[x, y].IsTarget) Target = Grid[x, y];
        var b = new Board { Car = car, Platforms = platforms };
        return b;
    }
    public Board() { }
    public Board(Board parent) { Parent = parent; }
    static void InitDists()
    {
        var wh = Width * Height;
        Dists = new int[wh, wh];
        for (var i = 0; i < wh; i++)
        for (var j = 0; j < wh; j++)
            Dists[i, j] = -1;
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var start = Grid[x, y];
            var q = new Queue<Cell>();
            q.Enqueue(start);
            Dists[Idx(start), Idx(start)] = 0;
            while (q.Count > 0)
            {
                var c = q.Dequeue();
                for (var dir = 0; dir < 4; dir++)
                {
                    var n = c.Neighbors[dir];
                    if (n == null || Dists[Idx(n), Idx(start)] != -1) continue;
                    Dists[Idx(n), Idx(start)] = Dists[Idx(c), Idx(start)] + 1;
                    if (n.CanEnter) q.Enqueue(n);
                }
            }
        }
    }
    static int Idx(Cell c) => c.Y * Width + c.X;
    public bool IsWin() => Dists[Idx(Car.Cell), Idx(Target)] >= 0;
    public List<string> GetActions()
    {
        var id = Car.Id;
        if (!Platforms[Platforms.Count - 1].Equals(Parent.Platforms[Platforms.Count - 1]))
        {
            var np = Platforms[Platforms.Count - 1];
            var op = Parent.Platforms.Find(x => x.Id == np.Id);
            var dx = Math.Sign(np.X - op.X);
            var dy = Math.Sign(np.Y - op.Y);
            id = np.Id;
            for (var dir = 0; dir < 4; dir++)
                if (Car.dx[dir] == dx && Car.dy[dir] == dy)
                    return new List<string> { id + " " + Car.dirNames[dir] };
        }
        var tgt = Grid[Car.Cell.X, Car.Cell.Y];
        var cur = Grid[Parent.Car.Cell.X, Parent.Car.Cell.Y];
        var res = new List<string>(8);
        while (!cur.Equals(tgt))
        {
            Cell n = null;
            var bestDist = int.MaxValue;
            for (var i = 0; i < 4; ++i)
            {
                var cand = cur.Neighbors[i];
                if (cand != null && (cand.CanEnter || cand == tgt))
                {
                    var d = Dists[Idx(cand), Idx(tgt)];
                    if (d >= 0 && d < bestDist)
                    {
                        n = cand;
                        bestDist = d;
                    }
                }
            }
            var dx = Math.Sign(n.X - cur.X);
            var dy = Math.Sign(n.Y - cur.Y);
            for (var dir = 0; dir < 4; dir++)
                if (Car.dx[dir] == dx && Car.dy[dir] == dy)
                    res.Add(id + " " + Car.dirNames[dir]);
            cur = n;
        }
        res.Reverse();
        return res;
    }
    public List<Board> Expand()
    {
        var nextBoards = new List<Board>(16);
        if (Dists[Idx(Car.Cell), Idx(Target)] > 0)
        {
            var b = new Board(this);
            b.Car = new Car(Car.Id, Target);
            b.Platforms = Platforms;
            nextBoards.Add(b);
        }
        if (Parent == null)
        {
            for (var i = 0; i < Platforms.Count; ++i)
            {
                var p = Platforms[i];
                if (Dists[Idx(Car.Cell), Idx(p)] > 0)
                {
                    var b = new Board(this);
                    b.Car = new Car(Car.Id, p);
                    b.Platforms = Platforms;
                    nextBoards.Add(b);
                }
            }
        }
        else
        {
            if (Car.Cell is Platform)
            {
                var carExp = Car.Expand(this);
                for (var k = 0; k < carExp.Count; ++k)
                {
                    var b = new Board(this);
                    b.Car = carExp[k];
                    b.Platforms = Platforms;
                    nextBoards.Add(b);
                }
                for (var i = 0; i < Platforms.Count; ++i)
                {
                    var p = Platforms[i];
                    if (Dists[Idx(Car.Cell), Idx(p)] > 0)
                    {
                        var b = new Board(this);
                        b.Car = new Car(Car.Id, p);
                        b.Platforms = Platforms;
                        nextBoards.Add(b);
                    }
                }
            }
            else
            {
                var p = Platforms[Platforms.Count - 1];
                if (Dists[Idx(Car.Cell), Idx(p)] > 0)
                {
                    var b = new Board(this);
                    b.Car = new Car(Car.Id, p);
                    b.Platforms = Platforms;
                    nextBoards.Add(b);
                }
            }
        }
        for (var i = 0; i < Platforms.Count; ++i)
        {
            var p = Platforms[i];
            var exp = p.Expand(this);
            for (var j = 0; j < exp.Count; ++j)
            {
                var plats = new List<Platform>(Platforms.Count);
                for (var k = 0; k < Platforms.Count; ++k)
                    if (k != i)
                        plats.Add(Platforms[k]);
                plats.Add(exp[j]);
                var b = new Board(this);
                b.Car = Car;
                if (Car.Cell == p)
                    b.Car = new Car(Car.Id, exp[j]);
                b.Platforms = plats;
                nextBoards.Add(b);
            }
        }
        return nextBoards;
    }
    public int Cost()
    {
        if (!Car.Equals(Parent.Car) && (!(Car.Cell is Platform) || !(Parent.Car.Cell is Platform) ||
            ((Platform)Car.Cell).Id != ((Platform)Parent.Car.Cell).Id))
            return Dists[Idx(Car.Cell), Idx(Parent.Car.Cell)];
        var lastPlat = Platforms[Platforms.Count - 1];
        if (lastPlat.Moved)
        {
            if (Car.Cell == lastPlat) return 1;
            var prev = Parent.Platforms.Find(p => p.Id == lastPlat.Id);
            if (!prev.Moved)
            {
                if (lastPlat.Dir.Count > 1 && Dists[Idx(Car.Cell), Idx(prev)] == -1 && Dists[Idx(Car.Cell), Idx(lastPlat)] != -1)
                    return 2;
                return 20;
            }
        }
        return 1;
    }
    public override int GetHashCode()
    {
        var hash = Car.GetHashCode();
        for (var i = 0; i < Platforms.Count; ++i) hash ^= Platforms[i].GetHashCode();
        return hash;
    }
    public bool Equals(Board other)
    {
        if (!Car.Cell.Equals(other.Car.Cell))
        {
            if (Car.Cell.IsTarget || other.Car.Cell.IsTarget) return false;
            if (!(Car.Cell.CanEnter && other.Car.Cell.CanEnter && Dists[Idx(Car.Cell), Idx(other.Car.Cell)] > 0))
                return false;
        }
        if (Platforms.Count != other.Platforms.Count) return false;
        for (var i = 0; i < Platforms.Count; ++i)
        {
            var p = Platforms[i];
            var found = false;
            for (var j = 0; j < other.Platforms.Count; ++j)
            {
                if (other.Platforms[j].Equals(p)) { found = true; break; }
            }
            if (!found) return false;
        }
        return true;
    }
}

class Cell : IEquatable<Cell>
{
    public int X;
    public int Y;
    public Cell[] Neighbors = new Cell[4];
    public bool CanEnter;
    public bool IsTarget;
    public char Ch;
    public Cell(int x, int y, char ch)
    {
        X = x;
        Y = y;
        Ch = ch;
        CanEnter = ch != '#';
        IsTarget = ch == '0';
    }
    public bool Equals(Cell other) => X == other.X && Y == other.Y;
    public void InitNeighbors()
    {
        for (var dir = 0; dir < 4; dir++)
        {
            var x = X + Car.dx[dir];
            var y = Y + Car.dy[dir];
            if (x < 0 || x >= Board.Width || y < 0 || y >= Board.Height) continue;
            Neighbors[dir] = Board.Grid[x, y];
        }
    }
    public bool IsNeighbor(Cell cell) => Math.Abs(X - cell.X) + Math.Abs(Y - cell.Y) == 1;
}

class Platform : Cell
{
    public int Id;
    public List<int> Dir = new List<int>(4);
    static int[,,] Zobrist;
    int dirIdx;
    public bool Moved;

    public Platform(int id, int x, int y, string dirStr) : base(x, y, '.')
    {
        Id = id;
        for (var i = 0; i < 4; i++)
            if (dirStr.Contains(Car.dirNames[i]))
            {
                dirIdx |= 1 << i;
                Dir.Add(i);
            }
    }
    public Platform(int id, Cell cell, string dirStr) : base(cell.X, cell.Y, '.')
    {
        Id = id;
        for (var i = 0; i < 4; i++)
            if (dirStr.Contains(Car.dirNames[i]))
            {
                dirIdx |= 1 << i;
                Dir.Add(i);
            }
    }
    public Platform(int id, int x, int y, List<int> dir) : base(x, y, '.')
    {
        Id = id;
        Dir = dir;
        Moved = true;
    }
    public static void InitZobrist(int width, int height)
    {
        Zobrist = new int[width, height, 16];
        var rand = Car.rand;
        for (var i = 0; i < width; i++)
        for (var j = 0; j < height; j++)
        for (var d = 0; d < 16; d++)
            Zobrist[i, j, d] = rand.Next();
    }
    public bool Equals(Platform other) => base.Equals(other) && dirIdx == other.dirIdx;
    public override int GetHashCode() => Zobrist[X, Y, dirIdx];
    public List<Platform> Expand(Board board)
    {
        var result = new List<Platform>(4);
        for (var idx = 0; idx < Dir.Count; ++idx)
        {
            var dir = Dir[idx];
            var x = X;
            var y = Y;
            while (true)
            {
                x += Car.dx[dir];
                y += Car.dy[dir];
                if (x < 0 || x >= Board.Width || y < 0 || y >= Board.Height) break;
                var stop = Board.Grid[x, y].CanEnter;
                for (var i = 0; !stop && i < board.Platforms.Count; i++)
                    if (board.Platforms[i].X == x && board.Platforms[i].Y == y) stop = true;
                if (stop)
                {
                    var oldX = x - Car.dx[dir];
                    var oldY = y - Car.dy[dir];
                    if (X != oldX || Y != oldY)
                        result.Add(new Platform(Id, oldX, oldY, Dir));
                    break;
                }
            }
        }
        return result;
    }
}

class Car
{
    public int Id;
    public int X;
    public int Y;
    public Cell Cell;
    static int[,] Zobrist;
    public static int[] dx = { 0, 1, 0, -1 };
    public static int[] dy = { 1, 0, -1, 0 };
    public static string[] dirNames = { "D", "R", "U", "L" };
    public static Random rand = new Random();
    public Car(int id, Cell cell)
    {
        Id = id;
        X = cell.X;
        Y = cell.Y;
        Cell = cell;
    }
    public Car(int id, int x, int y)
    {
        Id = id;
        X = x;
        Y = y;
        Cell = Board.Grid[x, y];
    }
    public static void InitZobrist(int width, int height)
    {
        Zobrist = new int[width, height];
        var rand = Car.rand;
        for (var i = 0; i < width; i++)
        for (var j = 0; j < height; j++)
            Zobrist[i, j] = rand.Next();
        var visited = new bool[width, height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var c = Board.Grid[x, y];
            if (!c.CanEnter || c.IsTarget) continue;
            var q = new Queue<Cell>();
            q.Enqueue(c);
            while (q.Count > 0)
            {
                var cc = q.Dequeue();
                if (visited[cc.X, cc.Y]) continue;
                visited[cc.X, cc.Y] = true;
                Zobrist[cc.X, cc.Y] = Zobrist[c.X, c.Y];
                for (var dir = 0; dir < 4; dir++)
                {
                    var n = cc.Neighbors[dir];
                    if (n != null && n.CanEnter && !n.IsTarget)
                        q.Enqueue(n);
                }
            }
        }
    }
    public override int GetHashCode() => Zobrist[Cell.X, Cell.Y];
    public List<Car> Expand(Board board)
    {
        var cars = new List<Car>(8);
        for (var i = 0; i < 4; i++)
        {
            var cell = Board.Grid[Cell.X, Cell.Y].Neighbors[i];
            if (cell != null && cell.CanEnter)
                cars.Add(new Car(Id, cell));
        }
        for (var i = 0; i < board.Platforms.Count; i++)
        {
            var platform = board.Platforms[i];
            if (Cell.IsNeighbor(platform))
                cars.Add(new Car(Id, platform));
        }
        return cars;
    }
}
