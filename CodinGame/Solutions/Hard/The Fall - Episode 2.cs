using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        var dims = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int width = dims[0], height = dims[1];
        var rooms = Enumerable.Range(0, height)
            .Select(_ => Console.ReadLine().Split().Select(int.Parse).ToArray())
            .ToArray();
        int exitX = int.Parse(Console.ReadLine());

        var roomGrid = RoomRules.ToRoomGrid(rooms);
        var solver = new Solver(roomGrid, exitX);

        while (true)
        {
            var indyData = Console.ReadLine().Split();
            var indy = new Entity(int.Parse(indyData[0]), int.Parse(indyData[1]), indyData[2][0]);
            int rockCount = int.Parse(Console.ReadLine());
            var rockEntities = Enumerable.Range(0, rockCount)
                .Select(_ => Console.ReadLine().Split())
                .Select(a => new Entity(int.Parse(a[0]), int.Parse(a[1]), a[2][0]))
                .ToArray();

            var rockOptions = rockEntities
                .Select(e => e.Trace(solver.Rooms))
                .Select(route => Solver.GetRockOptions(route, solver.Rooms))
                .Where(list => list.Count > 0)
                .ToList();

            var path = Solver.FindPath(indy, solver.Rooms, solver.ExitX);
            Step toRotate = path?.FirstOrDefault(s => s.Rotation != RotationDirection.None);

            while (rockOptions.Count > 0 && toRotate != null && path.IndexOf(toRotate) > 1)
            {
                var smallest = rockOptions.OrderBy(r => r.Count).First();
                var opt = smallest[0];
                bool atStart = indy.X == opt.X && indy.Y == opt.Y;
                if (atStart || solver.IsBlocked(opt))
                    rockOptions.Remove(smallest);
                else
                {
                    toRotate = new Step(opt.X, opt.Y, opt.Dir, opt.Rotation, null);
                    solver.Block(opt);
                }
            }

            if (toRotate != null)
            {
                solver.ApplyRotation(toRotate.X, toRotate.Y, toRotate.Rotation);
                Console.WriteLine($"{toRotate.X} {toRotate.Y} {toRotate.Rotation.ToString().ToUpper()}");
            }
            else
            {
                Console.WriteLine("WAIT");
            }
        }
    }
}

public enum RotationDirection { None, Left, Right }

public class Entity
{
    public int X { get; }
    public int Y { get; }
    public char Dir { get; }
    public Entity(int x, int y, char dir) => (X, Y, Dir) = (x, y, dir);

    public Entity Move(Room room)
    {
        var move = room.GetExit(Dir);
        if (move == null) return null;
        return new Entity(X + move.Value.dx, Y + move.Value.dy, move.Value.dir);
    }

    public List<Entity> Trace(Room[][] rooms)
    {
        var result = new List<Entity>();
        var current = this;
        while (current != null &&
               current.Y >= 0 && current.Y < rooms.Length &&
               current.X >= 0 && current.X < rooms[0].Length)
        {
            result.Add(current);
            current = current.Move(rooms[current.Y][current.X]);
        }
        return result;
    }
}

public class Step
{
    public int X { get; }
    public int Y { get; }
    public char Dir { get; }
    public RotationDirection Rotation { get; }
    public Entity Next { get; }
    public Step(int x, int y, char dir, RotationDirection rotation, Entity next)
        => (X, Y, Dir, Rotation, Next) = (x, y, dir, rotation, next);
}

public class Room
{
    public int Type { get; private set; }
    public Room(int type) { Type = type; }

    public (int dx, int dy, char dir)? GetExit(char entryDir)
    {
        return RoomRules.GetNextMove(Type, entryDir);
    }

    public void Rotate(RotationDirection rotation)
    {
        int? newType = RoomRules.GetRotation(Type, rotation);
        if (newType.HasValue) Type = newType.Value;
    }
}

public static class RoomRules
{
    public static readonly Dictionary<string, (int dx, int dy, char dir)> DirectionsMap = new()
    {
        {"1T", (0,1,'T')}, {"1L", (0,1,'T')}, {"1R", (0,1,'T')},
        {"2L", (1,0,'L')}, {"2R", (-1,0,'R')},
        {"3T", (0,1,'T')},
        {"4T", (-1,0,'R')}, {"4R", (0,1,'T')},
        {"5T", (1,0,'L')}, {"5L", (0,1,'T')},
        {"6L", (1,0,'L')}, {"6R", (-1,0,'R')},
        {"7T", (0,1,'T')}, {"7R", (0,1,'T')},
        {"8L", (0,1,'T')}, {"8R", (0,1,'T')},
        {"9T", (0,1,'T')}, {"9L", (0,1,'T')},
        {"10T", (-1,0,'R')}, {"11T", (1,0,'L')},
        {"12R", (0,1,'T')}, {"13L", (0,1,'T')}
    };

    public static readonly Dictionary<string, int> RotationMap = new()
    {
        {"2L",3}, {"2R",3}, {"3L",2}, {"3R",2},
        {"4L",5}, {"4R",5}, {"5L",4}, {"5R",4},
        {"6L",9}, {"6R",7}, {"7L",6}, {"7R",8},
        {"8L",7}, {"8R",9}, {"9L",8}, {"9R",6},
        {"10L",13}, {"10R",11}, {"11L",10}, {"11R",12},
        {"12L",11}, {"12R",13}, {"13L",12}, {"13R",10}
    };


    public static (int dx, int dy, char dir)? GetNextMove(int type, char entryDir)
    {
        var key = Math.Abs(type) + entryDir.ToString();
        if (DirectionsMap.TryGetValue(key, out var move))
            return move;
        return null;
    }
    
    public static int? GetRotation(int type, RotationDirection rot)
    {
        char rotChar = rot switch
        {
            RotationDirection.Left => 'L',
            RotationDirection.Right => 'R',
            _ => '?'
        };
        string key = Math.Abs(type).ToString() + rotChar;
        return RotationMap.TryGetValue(key, out var newType) ? newType : (int?)null;
    }

    public static Room[][] ToRoomGrid(int[][] types)
    {
        return types.Select(row => row.Select(t => new Room(t)).ToArray()).ToArray();
    }
}

public class Solver
{
    public Room[][] Rooms { get; }
    public int ExitX { get; }
    private readonly HashSet<(int, int)> _blocked = new();

    public Solver(Room[][] rooms, int exitX)
    {
        Rooms = rooms;
        ExitX = exitX;
    }

    public bool IsBlocked(Step opt) => _blocked.Contains((opt.X, opt.Y));
    public void Block(Step opt) => _blocked.Add((opt.X, opt.Y));
    public void ApplyRotation(int x, int y, RotationDirection rot)
    {
        Rooms[y][x].Rotate(rot);
    }

    public static List<Step> FindPath(Entity start, Room[][] rooms, int exitX)
    {
        return Find(start, rooms, exitX, new List<Step>());
    }

    private static List<Step> Find(Entity e, Room[][] rooms, int exitX, List<Step> path)
    {
        if (e.Y == rooms.Length && e.X == exitX) return path;
        if (e.Y < 0 || e.Y >= rooms.Length || e.X < 0 || e.X >= rooms[0].Length) return null;

        foreach (var s in GetNext(e, rooms))
        {
            var np = new List<Step>(path) { s };
            var res = Find(s.Next, rooms, exitX, np);
            if (res != null) return res;
        }
        return null;
    }

    public static List<Step> GetNext(Entity e, Room[][] rooms)
    {
        var list = new List<Step>();
        var type = rooms[e.Y][e.X].Type;
        string key = Math.Abs(type).ToString();
        string dir = e.Dir.ToString();

        Step MakeStep(int roomType, RotationDirection rot)
        {
            var move = RoomRules.GetNextMove(roomType, e.Dir);
            if (move != null)
                return new Step(e.X, e.Y, e.Dir, rot, new Entity(e.X + move.Value.dx, e.Y + move.Value.dy, move.Value.dir));
            return null;
        }

        var baseStep = MakeStep(type, RotationDirection.None);
        if (baseStep != null) list.Add(baseStep);

        if (type != 0)
        {
            if (RoomRules.RotationMap.TryGetValue(key + "L", out var left1))
            {
                var step = MakeStep(left1, RotationDirection.Left);
                if (step != null) list.Add(step);

                if (RoomRules.RotationMap.TryGetValue(Math.Abs(left1).ToString() + "L", out var left2))
                {
                    var doubleStep = MakeStep(left2, RotationDirection.Left);
                    if (doubleStep != null) list.Add(doubleStep);
                }
            }

            if (RoomRules.RotationMap.TryGetValue(key + "R", out var right1))
            {
                var step = MakeStep(right1, RotationDirection.Right);
                if (step != null) list.Add(step);
            }
        }
        return list;
    }

    public static List<Step> GetRockOptions(List<Entity> route, Room[][] rooms)
    {
        return route.Skip(1)
            .Where(pos => pos.Y >= 0 && pos.Y < rooms.Length && pos.X >= 0 && pos.X < rooms[0].Length && rooms[pos.Y][pos.X].Type > 1)
            .Select(pos =>
            {
                var type = rooms[pos.Y][pos.X].Type;
                string leftKey = Math.Abs(type) + "L";
                var mustLeft = !RoomRules.RotationMap.ContainsKey(leftKey)
                            || !RoomRules.DirectionsMap.ContainsKey(RoomRules.RotationMap[leftKey] + pos.Dir.ToString());
                return new Step(pos.X, pos.Y, pos.Dir, mustLeft ? RotationDirection.Left : RotationDirection.Right, null);
            })
            .Where(s => s != null)
            .ToList();
    }
}
