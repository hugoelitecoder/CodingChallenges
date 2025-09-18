using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

public class Solution
{
    public static void Main(string[] args)
    {
        var sizeInput = Console.ReadLine();
        Console.Error.WriteLine($"[DEBUG]: {sizeInput}");
        var size = sizeInput.Split().Select(int.Parse).ToArray();
        var width = size[0];
        var height = size[1];
        var map = new string[height];
        for (var y = 0; y < height; y++)
        {
            map[y] = Console.ReadLine();
            Console.Error.WriteLine($"[DEBUG]: {map[y]}");
        }
        var entityCountInput = Console.ReadLine();
        Console.Error.WriteLine($"[DEBUG]: {entityCountInput}");
        var entityCount = int.Parse(entityCountInput);
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
            var type = parts[3];
            if (type == "CAR")
            {
                carId = id;
                carX = x;
                carY = y;
            }
            else
            {
                platforms.Add(new Platform(id, x, y, type));
            }
        }
        var board = Board.Create(width, height, map, carId, carX, carY, platforms);
        var solver = new BoardSolver();
        var timer = Stopwatch.StartNew();
        var (moves, path) = solver.SolveWithPath(board);
        timer.Stop();

        Console.Error.WriteLine($"[DEBUG] Planning time: {timer.ElapsedMilliseconds} ms");
        Console.Error.WriteLine(moves != null ? $"[DEBUG] Moves planned: {moves.Count}" : "No solution found");
        if (moves != null)
            Console.Error.WriteLine($"[DEBUG] Moves: {string.Join(", ", moves)}");
        else
            Console.Error.WriteLine($"[DEBUG] Moves: None");

        int boardIndex = 0;
        int moveIndex = 0;

        if (moves != null && moves.Count > 0)
        {
            Console.WriteLine(moves[moveIndex++]);
            boardIndex++;
        }
        else
        {
            Console.WriteLine("WAIT");
        }

        while (true)
        {
            var turnEntitiesInput = Console.ReadLine();
            var turnEntities = int.Parse(turnEntitiesInput);
            for (var i = 0; i < turnEntities; i++)
            {
                var ent = Console.ReadLine();
            }

            if (moves != null && moveIndex < moves.Count)
            {
                Console.WriteLine(moves[moveIndex++]);
                if (boardIndex < path.Count - 1)
                    boardIndex++;
                Console.Error.WriteLine("[DEBUG] Board after action:");
                Console.Error.WriteLine(path[boardIndex].ToString());
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
    public (List<string> moves, List<Board> path) SolveWithPath(Board board)
    {
        var visited = new Dictionary<Board, Board>(4096);
        visited.Add(board, board);
        var layers = new HashSet<Board>[1000];
        for (var i = 0; i < layers.Length; i++) layers[i] = new HashSet<Board>();
        layers[0].Add(board);
        Board finalState = null;

        var timer = Stopwatch.StartNew();
        var maxTimeMs = 950;

        for (var depth = 0; finalState == null && depth < layers.Length && visited.Count < 1_000_000 && timer.ElapsedMilliseconds < maxTimeMs; depth++)
        {
            foreach (var boardState in layers[depth])
            {
                var nextStates = boardState.Expand();
                for (var k = 0; k < nextStates.Count; k++)
                {
                    var next = nextStates[k];
                    next.Depth = depth + next.Cost();
                    if (!visited.TryGetValue(next, out var old) || old.Depth > next.Depth)
                    {
                        if (old != null)
                        {
                            visited.Remove(old);
                            layers[old.Depth].Remove(old);
                        }
                        visited[next] = next;
                        layers[next.Depth].Add(next);
                        if (next.IsWin())
                        {
                            finalState = next;
                            if (!finalState.Car.Cell.IsTarget)
                                finalState = next.Expand()[0];
                        }
                    }
                }
                if (finalState != null) break;
            }
        }
        if (finalState == null) return (null, null);

        var moves = new List<string>(32);
        var path = new List<Board>();
        var current = finalState;
        while (current != null)
        {
            path.Add(current);
            if (current.Parent != null)
                moves.AddRange(current.GetMoves());
            current = current.Parent;
        }
        moves.Reverse();
        path.Reverse();

        return (moves, path);
    }
}

class Board : IEquatable<Board>
{
    public static int Width;
    public static int Height;
    public static Cell[,] Cells;
    public Car Car;
    public List<Platform> Platforms = new List<Platform>(8);
    public Board Parent;
    public int Depth;
    static Cell TargetCell;
    static int[,] Distances;

    public static Board Create(int width, int height, string[] mapLines, int carId, int carX, int carY, List<Platform> plats)
    {
        Width = width;
        Height = height;
        Cells = new Cell[width, height];
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                Cells[x, y] = new Cell(x, y, mapLines[y][x]);
        foreach (var c in Cells) c.InitNeighbors();
        Car.InitZobrist(width, height);
        Platform.InitZobrist(width, height);
        InitDistances();
        var car = new Car(carId, Cells[carX, carY]);
        var platforms = new List<Platform>(plats.Count);
        for (var i = 0; i < plats.Count; i++)
        {
            var p = plats[i];
            platforms.Add(new Platform(p.Id, Cells[p.X, p.Y], string.Join("", p.Directions.Select(d => Car.DirectionNames[d]))));
        }
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                if (Cells[x, y].IsTarget) TargetCell = Cells[x, y];
        var board = new Board { Car = car, Platforms = platforms };
        return board;
    }
    public Board() { }
    public Board(Board parent) { Parent = parent; }

    static void InitDistances()
    {
        var totalCells = Width * Height;
        Distances = new int[totalCells, totalCells];
        for (var i = 0; i < totalCells; i++)
            for (var j = 0; j < totalCells; j++)
                Distances[i, j] = -1;
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            {
                var start = Cells[x, y];
                var queue = new Queue<Cell>();
                queue.Enqueue(start);
                Distances[GetIndex(start), GetIndex(start)] = 0;
                while (queue.Count > 0)
                {
                    var cell = queue.Dequeue();
                    for (var dir = 0; dir < 4; dir++)
                    {
                        var neighbor = cell.Neighbors[dir];
                        if (neighbor == null || Distances[GetIndex(neighbor), GetIndex(start)] != -1) continue;
                        Distances[GetIndex(neighbor), GetIndex(start)] = Distances[GetIndex(cell), GetIndex(start)] + 1;
                        if (neighbor.CanEnter) queue.Enqueue(neighbor);
                    }
                }
            }
    }

    static int GetIndex(Cell c) => c.Y * Width + c.X;

    public bool IsWin() => Distances[GetIndex(Car.Cell), GetIndex(TargetCell)] >= 0;

    public List<string> GetMoves()
    {
        var id = Car.Id;
        if (!Platforms[Platforms.Count - 1].Equals(Parent.Platforms[Platforms.Count - 1]))
        {
            var newPlat = Platforms[Platforms.Count - 1];
            var oldPlat = Parent.Platforms.Find(x => x.Id == newPlat.Id);
            var dx = Math.Sign(newPlat.X - oldPlat.X);
            var dy = Math.Sign(newPlat.Y - oldPlat.Y);
            id = newPlat.Id;
            for (var dir = 0; dir < 4; dir++)
                if (Car.dx[dir] == dx && Car.dy[dir] == dy)
                    return new List<string> { id + " " + Car.DirectionNames[dir] };
        }
        var target = Cells[Car.Cell.X, Car.Cell.Y];
        var current = Cells[Parent.Car.Cell.X, Parent.Car.Cell.Y];
        var result = new List<string>(8);
        while (!current.Equals(target))
        {
            Cell nextCell = null;
            var bestDist = int.MaxValue;
            for (var i = 0; i < 4; ++i)
            {
                var candidate = current.Neighbors[i];
                if (candidate != null && (candidate.CanEnter || candidate == target))
                {
                    var dist = Distances[GetIndex(candidate), GetIndex(target)];
                    if (dist >= 0 && dist < bestDist)
                    {
                        nextCell = candidate;
                        bestDist = dist;
                    }
                }
            }
            var dx = Math.Sign(nextCell.X - current.X);
            var dy = Math.Sign(nextCell.Y - current.Y);
            for (var dir = 0; dir < 4; dir++)
                if (Car.dx[dir] == dx && Car.dy[dir] == dy)
                    result.Add(id + " " + Car.DirectionNames[dir]);
            current = nextCell;
        }
        result.Reverse();
        return result;
    }

    public List<Board> Expand()
    {
        var nextBoards = new List<Board>(16);
        if (Distances[GetIndex(Car.Cell), GetIndex(TargetCell)] > 0)
        {
            var b = new Board(this);
            b.Car = new Car(Car.Id, TargetCell);
            b.Platforms = Platforms;
            nextBoards.Add(b);
        }
        if (Parent == null)
        {
            for (var i = 0; i < Platforms.Count; i++)
            {
                var p = Platforms[i];
                if (Distances[GetIndex(Car.Cell), GetIndex(p)] > 0)
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
                var carMoves = Car.Expand(this);
                for (var k = 0; k < carMoves.Count; k++)
                {
                    var b = new Board(this);
                    b.Car = carMoves[k];
                    b.Platforms = Platforms;
                    nextBoards.Add(b);
                }
                for (var i = 0; i < Platforms.Count; i++)
                {
                    var p = Platforms[i];
                    if (Distances[GetIndex(Car.Cell), GetIndex(p)] > 0)
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
                var lastPlat = Platforms[Platforms.Count - 1];
                if (Distances[GetIndex(Car.Cell), GetIndex(lastPlat)] > 0)
                {
                    var b = new Board(this);
                    b.Car = new Car(Car.Id, lastPlat);
                    b.Platforms = Platforms;
                    nextBoards.Add(b);
                }
            }
        }
        for (var i = 0; i < Platforms.Count; i++)
        {
            var p = Platforms[i];
            var expandedPlats = p.Expand(this);
            for (var j = 0; j < expandedPlats.Count; j++)
            {
                var newPlats = new List<Platform>(Platforms.Count);
                for (var k = 0; k < Platforms.Count; k++)
                    if (k != i)
                        newPlats.Add(Platforms[k]);
                newPlats.Add(expandedPlats[j]);
                var b = new Board(this);
                b.Car = Car;
                if (Car.Cell == p)
                    b.Car = new Car(Car.Id, expandedPlats[j]);
                b.Platforms = newPlats;
                nextBoards.Add(b);
            }
        }
        return nextBoards;
    }

    public int Cost()
    {
        if (!Car.Equals(Parent.Car) && (!(Car.Cell is Platform) || !(Parent.Car.Cell is Platform) ||
            ((Platform)Car.Cell).Id != ((Platform)Parent.Car.Cell).Id))
        {
            var dist = Distances[GetIndex(Car.Cell), GetIndex(Parent.Car.Cell)];
            if (dist == -1) dist = 1;
            return dist;
        }

        var lastPlat = Platforms[Platforms.Count - 1];
        if (lastPlat.Moved)
        {
            if (Car.Cell == lastPlat)
                return 1;

            var prev = Parent.Platforms.Find(p => p.Id == lastPlat.Id);
            if (!prev.Moved)
            {
                var platMoveDist = Distances[GetIndex(lastPlat), GetIndex(prev)];
                if (platMoveDist == -1) platMoveDist = 20;

                bool isOffMap = lastPlat.X < 0 || lastPlat.X >= Board.Width || lastPlat.Y < 0 || lastPlat.Y >= Board.Height;
                bool isOnVoid = !isOffMap && Board.Cells[lastPlat.X, lastPlat.Y].Symbol == '#';

                if (isOffMap || isOnVoid)
                    return 50;

                var oldDist = Distances[GetIndex(Parent.Car.Cell), GetIndex(TargetCell)];
                var newDist = Distances[GetIndex(Car.Cell), GetIndex(TargetCell)];
                bool moveImprovesPath = newDist >= 0 && newDist < oldDist;

                if (moveImprovesPath)
                    return Math.Min(platMoveDist, 3);

                return Math.Min(platMoveDist, 20);
            }
        }

        return 1;
    }

    public override int GetHashCode()
    {
        var hash = Car.GetHashCode();
        for (var i = 0; i < Platforms.Count; i++) hash ^= Platforms[i].GetHashCode();
        return hash;
    }

    public bool Equals(Board other)
    {
        if (!Car.Cell.Equals(other.Car.Cell))
        {
            if (Car.Cell.IsTarget || other.Car.Cell.IsTarget) return false;
            if (!(Car.Cell.CanEnter && other.Car.Cell.CanEnter && Distances[GetIndex(Car.Cell), GetIndex(other.Car.Cell)] > 0))
                return false;
        }
        if (Platforms.Count != other.Platforms.Count) return false;
        for (var i = 0; i < Platforms.Count; i++)
        {
            var p = Platforms[i];
            var found = false;
            for (var j = 0; j < other.Platforms.Count; j++)
            {
                if (other.Platforms[j].Equals(p)) { found = true; break; }
            }
            if (!found) return false;
        }
        return true;
    }

    public override string ToString()
    {
        var display = new char[Width, Height];
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                display[x, y] = Cells[x, y].Symbol;

        foreach (var p in Platforms)
            display[p.X, p.Y] = 'P';

        display[Car.Cell.X, Car.Cell.Y] = 'C';

        var lines = new List<string>(Height);
        for (var y = 0; y < Height; y++)
        {
            var lineChars = new char[Width];
            for (var x = 0; x < Width; x++)
                lineChars[x] = display[x, y];
            lines.Add(new string(lineChars));
        }
        lines.Add("---");
        return string.Join("\n", lines);
    }
}

class Cell : IEquatable<Cell>
{
    public int X;
    public int Y;
    public Cell[] Neighbors = new Cell[4];
    public bool CanEnter;
    public bool IsTarget;
    public char Symbol;

    public Cell(int x, int y, char symbol)
    {
        X = x;
        Y = y;
        Symbol = symbol;
        CanEnter = symbol != '#';
        IsTarget = symbol == '0';
    }
    public bool Equals(Cell other) => X == other.X && Y == other.Y;
    public void InitNeighbors()
    {
        for (var dir = 0; dir < 4; dir++)
        {
            var nx = X + Car.dx[dir];
            var ny = Y + Car.dy[dir];
            if (nx < 0 || nx >= Board.Width || ny < 0 || ny >= Board.Height) continue;
            Neighbors[dir] = Board.Cells[nx, ny];
        }
    }
    public bool IsNeighbor(Cell cell) => Math.Abs(X - cell.X) + Math.Abs(Y - cell.Y) == 1;
}

class Platform : Cell
{
    public int Id;
    public List<int> Directions = new List<int>(4);
    static int[,,] Zobrist;
    int directionMask;
    public bool Moved;

    public Platform(int id, int x, int y, string dirStr) : base(x, y, '.')
    {
        Id = id;
        for (var i = 0; i < 4; i++)
            if (dirStr.Contains(Car.DirectionNames[i]))
            {
                directionMask |= 1 << i;
                Directions.Add(i);
            }
    }
    public Platform(int id, Cell cell, string dirStr) : base(cell.X, cell.Y, '.')
    {
        Id = id;
        for (var i = 0; i < 4; i++)
            if (dirStr.Contains(Car.DirectionNames[i]))
            {
                directionMask |= 1 << i;
                Directions.Add(i);
            }
    }
    public Platform(int id, int x, int y, List<int> dir) : base(x, y, '.')
    {
        Id = id;
        Directions = dir;
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
    public bool Equals(Platform other) => base.Equals(other) && directionMask == other.directionMask;
    public override int GetHashCode() => Zobrist[X, Y, directionMask];
    public List<Platform> Expand(Board board)
    {
        var result = new List<Platform>(4);
        for (var idx = 0; idx < Directions.Count; idx++)
        {
            var dir = Directions[idx];
            var x = X;
            var y = Y;
            while (true)
            {
                x += Car.dx[dir];
                y += Car.dy[dir];
                if (x < 0 || x >= Board.Width || y < 0 || y >= Board.Height) break;
                var canStop = Board.Cells[x, y].CanEnter;
                for (var i = 0; !canStop && i < board.Platforms.Count; i++)
                    if (board.Platforms[i].X == x && board.Platforms[i].Y == y) canStop = true;
                if (canStop)
                {
                    var prevX = x - Car.dx[dir];
                    var prevY = y - Car.dy[dir];
                    if (X != prevX || Y != prevY)
                        result.Add(new Platform(Id, prevX, prevY, Directions));
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
    public static string[] DirectionNames = { "D", "R", "U", "L" };
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
        Cell = Board.Cells[x, y];
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
                var c = Board.Cells[x, y];
                if (!c.CanEnter || c.IsTarget) continue;
                var queue = new Queue<Cell>();
                queue.Enqueue(c);
                while (queue.Count > 0)
                {
                    var curr = queue.Dequeue();
                    if (visited[curr.X, curr.Y]) continue;
                    visited[curr.X, curr.Y] = true;
                    Zobrist[curr.X, curr.Y] = Zobrist[c.X, c.Y];
                    for (var dir = 0; dir < 4; dir++)
                    {
                        var n = curr.Neighbors[dir];
                        if (n != null && n.CanEnter && !n.IsTarget)
                            queue.Enqueue(n);
                    }
                }
            }
    }
    public override int GetHashCode() => Zobrist[Cell.X, Cell.Y];
    public List<Car> Expand(Board board)
    {
        var nextCars = new List<Car>(8);
        for (var i = 0; i < 4; i++)
        {
            var neighbor = Board.Cells[Cell.X, Cell.Y].Neighbors[i];
            if (neighbor != null && neighbor.CanEnter)
                nextCars.Add(new Car(Id, neighbor));
        }
        for (var i = 0; i < board.Platforms.Count; i++)
        {
            var platform = board.Platforms[i];
            if (Cell.IsNeighbor(platform))
                nextCars.Add(new Car(Id, platform));
        }
        return nextCars;
    }
}
