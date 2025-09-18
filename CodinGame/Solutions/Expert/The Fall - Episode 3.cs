using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public enum ExitSide { Dead = 0, Left = 1, Right = 2, Bottom = 3 }
public enum EntrySide { Dead = 0, Left = 1, Right = 2, Top = 3 }
public enum Rotation { Left, Right }

public class GameRunner
{
    public static void Main(string[] args)
    {
        var dimensions = (Console.ReadLine() ?? "").Split(' ').Select(int.Parse).ToArray();
        var width = dimensions[0];
        var height = dimensions[1];

        var initialGrid = new List<List<int>>();
        for (int i = 0; i < height; i++)
        {
            initialGrid.Add((Console.ReadLine() ?? "").Split(' ').Select(int.Parse).ToList());
        }

        var exitX = int.Parse(Console.ReadLine() ?? "");

        var pathfinder = new Pathfinder(initialGrid, exitX);

        while (true)
        {
            var indy = new Pathfinder.Actor(Console.ReadLine());
            var rockCount = int.Parse(Console.ReadLine() ?? "0");
            var rocks = Enumerable.Range(0, rockCount)
                                  .Select(_ => new Pathfinder.Actor(Console.ReadLine()))
                                  .ToList();

            Console.WriteLine(pathfinder.CalculateNextAction(indy, rocks));
        }
    }
}

public class Pathfinder
{
    private Grid currentGrid;
    private readonly Point exitCoordinates;

    private static readonly Dictionary<string, EntrySide> EntrySideMap = new()
    {
        { "LEFT", EntrySide.Left },
        { "RIGHT", EntrySide.Right },
        { "TOP", EntrySide.Top }
    };

    private static readonly Dictionary<ExitSide, EntrySide> ExitToEntryMap = new()
    {
        { ExitSide.Left, EntrySide.Right },
        { ExitSide.Right, EntrySide.Left },
        { ExitSide.Bottom, EntrySide.Top },
        { ExitSide.Dead, EntrySide.Dead }
    };

    public Pathfinder(IEnumerable<IEnumerable<int>> grid, int exitX)
    {
        var gridList = grid.ToList();
        currentGrid = new Grid(gridList);
        exitCoordinates = new Point(exitX, gridList.Count - 1);
    }

    public string CalculateNextAction(Actor indy, IEnumerable<Actor> rocks)
    {
        var startState = new Scenario(indy, currentGrid, rocks);
        var winningPath = FindWinningPath(startState);

        if (!winningPath.Actions.Any())
        {
            return "WAIT";
        }

        var nextAction = winningPath.Actions.First();
        currentGrid = nextAction.Apply(currentGrid);
        return nextAction.ToString();
    }

    private Scenario FindWinningPath(Scenario start)
    {
        var queue = new List<Scenario> { start };
        var turn = 0;

        while (true)
        {
            var nextQueue = new List<Scenario>();
            turn++;

            foreach (var scenario in queue)
            {
                var waitScenario = new Scenario(scenario);
                if (!waitScenario.DoesIndyCollideWithRock() && waitScenario.GetExit(waitScenario.Indy) != ExitSide.Dead)
                {
                    nextQueue.Add(waitScenario);
                }

                var remainingActions = turn - scenario.Actions.Count();
                nextQueue.AddRange(scenario.GetPossibleRockRotations(remainingActions).Where(s => !s.DoesIndyCollideWithRock()));

                if (waitScenario.Indy == null || scenario.Grid.IsOutOfBounds(waitScenario.Indy)) continue;

                var roomProxy = new RoomProxy(waitScenario.Grid, waitScenario.Indy.X, waitScenario.Indy.Y);
                if (!roomProxy.CanRotate)
                {
                    continue;
                }

                var possibleRotations = Math.Min(roomProxy.RotationsAllowed, remainingActions);
                if (possibleRotations > 0)
                {
                    var rotateLeftOnce = new Scenario(scenario, new RotateAction(waitScenario.Indy.X, waitScenario.Indy.Y, Rotation.Left));
                    if (rotateLeftOnce.IsViableForIndy(waitScenario.Indy, waitScenario.GetExit(waitScenario.Indy)))
                    {
                        nextQueue.Add(rotateLeftOnce);
                    }
                }
                if (possibleRotations > 1)
                {
                    var rotateLeftTwice = new Scenario(scenario, new RotateAction(waitScenario.Indy.X, waitScenario.Indy.Y, Rotation.Left), new RotateAction(waitScenario.Indy.X, waitScenario.Indy.Y, Rotation.Left));
                    if (rotateLeftTwice.IsViableForIndy(waitScenario.Indy, waitScenario.GetExit(waitScenario.Indy)))
                    {
                        nextQueue.Add(rotateLeftTwice);
                    }
                }
                if (roomProxy.RotationsAllowed >= 2)
                {
                    var rotateRight = new Scenario(scenario, new RotateAction(waitScenario.Indy.X, waitScenario.Indy.Y, Rotation.Right));
                    if (rotateRight.IsViableForIndy(waitScenario.Indy, waitScenario.GetExit(waitScenario.Indy)))
                    {
                        nextQueue.Add(rotateRight);
                    }
                }
            }

            queue = DeduplicateScenarios(nextQueue);
            if (!queue.Any())
            {
                throw new InvalidOperationException("No path found.");
            }
            if (queue.Count > 300)
            {
                throw new InvalidOperationException("Search space too large.");
            }

            var winningScenarios = queue.Where(s => s.Indy != null && s.Indy.Coords.Equals(exitCoordinates)).ToList();
            if (winningScenarios.Any())
            {
                var maxRocks = winningScenarios.Max(s => s.RockCount);
                return winningScenarios.First(s => s.RockCount == maxRocks);
            }
        }
    }

    private static List<Scenario> DeduplicateScenarios(List<Scenario> scenarios)
    {
        scenarios.ForEach(s => s.SimplifyActions());
        return scenarios.Distinct(new ScenarioComparer()).ToList();
    }

    public class Grid
    {
        internal static readonly int[,] RoomBehaviors =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 1, 1, 3, 3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 3, 3, 2, 1, 0, 1, 1, 0, 1, 1, 0, 0, 0, 0 },
            { 1, 2, 2, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 1, 5, 5, 0, 3, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0 },
            { 1, 4, 4, 3, 0, 2, 0, 1, 0, 1, 0, 1, 0, 1, 1 },
            { 2, 9, 7, 2, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 2, 6, 8, 0, 3, 3, 0, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 2, 7, 9, 3, 3, 0, 1, 1, 0, 1, 1, 0, 0, 0, 0 },
            { 2, 8, 6, 3, 0, 3, 1, 0, 1, 0, 0, 0, 1, 0, 1 },
            { 2, 13, 11, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1 },
            { 2, 10, 12, 0, 0, 2, 0, 0, 1, 0, 0, 1, 1, 1, 1 },
            { 2, 11, 13, 0, 3, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 },
            { 2, 12, 10, 3, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
        };

        private readonly List<List<int>> cells;

        public Grid(IEnumerable<IEnumerable<int>> grid)
        {
            cells = grid.Select(r => new List<int>(r)).ToList();
        }

        private Grid(Grid other)
        {
            cells = other.cells.Select(r => new List<int>(r)).ToList();
        }

        public Grid Clone() => new Grid(this);

        public bool IsOutOfBounds(Actor actor) => actor == null || IsOutOfBounds(actor.Coords);
        public bool IsOutOfBounds(Point p) => p.X < 0 || p.X >= cells[0].Count || p.Y < 0 || p.Y >= cells.Count;

        public int GetRoomType(int x, int y) => cells[y][x];
        private bool IsFixed(int x, int y) => cells[y][x] < 0;

        private int GetAbsoluteRoomType(int x, int y) => Math.Abs(cells[y][x]);

        private int GetRotationCount(int roomType) => RoomBehaviors[Math.Abs(roomType), 0];
        private int GetNextTypeOnLeftRotation(int roomType) => RoomBehaviors[Math.Abs(roomType), 1];
        private int GetNextTypeOnRightRotation(int roomType) => RoomBehaviors[Math.Abs(roomType), 2];

        public bool CanRotate(int x, int y)
        {
            if (IsFixed(x, y)) return false;
            return GetRotationCount(GetAbsoluteRoomType(x, y)) > 0;
        }

        public Grid RotateLeft(int x, int y)
        {
            if (!CanRotate(x, y)) throw new InvalidOperationException();
            var newGrid = this.Clone();
            newGrid.cells[y][x] = GetNextTypeOnLeftRotation(newGrid.cells[y][x]);
            return newGrid;
        }

        public Grid RotateRight(int x, int y)
        {
            if (!CanRotate(x, y) || GetRotationCount(GetAbsoluteRoomType(x, y)) < 2) throw new InvalidOperationException();
            var newGrid = this.Clone();
            newGrid.cells[y][x] = GetNextTypeOnRightRotation(newGrid.cells[y][x]);
            return newGrid;
        }

        public ExitSide GetExitSide(int x, int y, EntrySide entry)
        {
            var roomType = GetRoomType(x, y);
            int exitCode;

            switch (entry)
            {
                case EntrySide.Left: exitCode = RoomBehaviors[Math.Abs(roomType), 3]; break;
                case EntrySide.Right: exitCode = RoomBehaviors[Math.Abs(roomType), 4]; break;
                case EntrySide.Top: exitCode = RoomBehaviors[Math.Abs(roomType), 5]; break;
                default: return ExitSide.Dead;
            }
            return (ExitSide)exitCode;
        }

        public bool DoesCollide(Actor actorA, Actor actorB)
        {
            var roomType = GetRoomType(actorA.X, actorA.Y);
            var collisionMatrixIndex = ((int)actorA.Side - 1) * 3 + ((int)actorB.Side - 1);
            return RoomBehaviors[Math.Abs(roomType), 6 + collisionMatrixIndex] == 1;
        }
    }

    public class Actor
    {
        public Point Coords { get; }
        public EntrySide Side { get; }
        public int X => Coords.X;
        public int Y => Coords.Y;

        public Actor(int x, int y, EntrySide side) : this(new Point(x, y), side) { }
        public Actor(Point p, EntrySide side) { Coords = p; Side = side; }
        public Actor(string input)
        {
            var parts = input.Split(' ');
            Coords = new Point(int.Parse(parts[0]), int.Parse(parts[1]));
            Side = EntrySideMap[parts[2]];
        }
    }

    private class RoomProxy
    {
        private readonly Grid grid;
        private readonly int x;
        private readonly int y;

        public RoomProxy(Grid g, int x, int y) { this.grid = g; this.x = x; this.y = y; }
        public int Type => Math.Abs(grid.GetRoomType(x, y));
        public int RotationsAllowed => Grid.RoomBehaviors[Type, 0];
        public bool CanRotate => grid.CanRotate(x, y);
    }

    public interface IAction
    {
        int X { get; }
        int Y { get; }
        int RotationEffect { get; }
        Grid Apply(Grid grid);
    }

    public class RotateAction : IAction
    {
        public int X { get; }
        public int Y { get; }
        public Rotation Direction { get; }

        public int RotationEffect => Direction == Rotation.Left ? 1 : -1;

        public RotateAction(int x, int y, Rotation direction) { X = x; Y = y; Direction = direction; }
        public Grid Apply(Grid grid) => Direction == Rotation.Left ? grid.RotateLeft(X, Y) : grid.RotateRight(X, Y);
        public override string ToString() => $"{X} {Y} {Direction.ToString().ToUpper()}";
    }

    private class Scenario
    {
        public Actor Indy { get; private set; }
        public Grid Grid { get; private set; }
        public List<IAction> Actions { get; private set; }
        private List<Actor> rocks;

        public int RockCount => rocks.Count;

        public Scenario(Actor indy, Grid grid, IEnumerable<Actor> initialRocks)
        {
            Indy = new Actor(indy.Coords, indy.Side);
            Grid = grid.Clone();
            Actions = new List<IAction>();
            rocks = new List<Actor>(initialRocks);
        }

        public Scenario(Scenario previous, params IAction[] newActions)
        {
            Grid = previous.Grid;
            foreach (var action in newActions)
            {
                Grid = action.Apply(Grid);
            }
            Indy = GetNextActorState(previous.Indy);
            Actions = new List<IAction>(previous.Actions);
            Actions.AddRange(newActions);
            rocks = new List<Actor>(previous.rocks);
            MoveRocks();
        }

        public Scenario(Scenario previous) : this(previous, Array.Empty<IAction>()) { }

        private void MoveRocks()
        {
            var nextRocks = new List<Actor>();
            foreach (var rock in rocks)
            {
                if (rock.Side != EntrySide.Dead)
                {
                    var nextRock = GetNextActorState(rock);
                    if (nextRock != null) nextRocks.Add(nextRock);
                }
            }
            var crashedCoords = nextRocks.GroupBy(r => r.Coords).Where(g => g.Count() > 1).Select(g => g.Key);
            rocks = nextRocks.Where(r => !crashedCoords.Contains(r.Coords)).ToList();
        }

        private Actor GetNextActorState(Actor actor)
        {
            if (Grid.IsOutOfBounds(actor)) return null;
            var exitSide = GetExit(actor);
            if (exitSide == ExitSide.Dead) return new Actor(actor.Coords, EntrySide.Dead);

            var dx = exitSide == ExitSide.Left ? -1 : exitSide == ExitSide.Right ? 1 : 0;
            var dy = exitSide == ExitSide.Bottom ? 1 : 0;
            var nextCoords = new Point(actor.X + dx, actor.Y + dy);
            var nextEntry = ExitToEntryMap[exitSide];

            if (Grid.IsOutOfBounds(nextCoords)) return null;
            return new Actor(nextCoords, nextEntry);
        }

        public ExitSide GetExit(Actor actor) => Grid.IsOutOfBounds(actor) ? ExitSide.Dead : Grid.GetExitSide(actor.X, actor.Y, actor.Side);

        public bool DoesIndyCollideWithRock()
        {
            if (Indy == null) return true;
            var dangerousRocks = rocks.Where(r => r.Coords.Equals(Indy.Coords));
            return dangerousRocks.Any(r => Grid.DoesCollide(Indy, r));
        }

        public List<Scenario> GetPossibleRockRotations(int maxActions)
        {
            var options = new List<Scenario>();
            foreach (var rock in this.rocks)
            {
                var nextRockState = GetNextActorState(rock);
                if (nextRockState == null) continue;
                if (Indy.Coords.Equals(nextRockState.Coords)) continue;
                if (rocks.Any(r => r.Coords.Equals(nextRockState.Coords))) continue;

                var room = new RoomProxy(this.Grid, nextRockState.X, nextRockState.Y);
                if (!room.CanRotate) continue;

                var can = Math.Min(room.RotationsAllowed, maxActions);
                if (can > 0) options.Add(new Scenario(this, new RotateAction(nextRockState.X, nextRockState.Y, Rotation.Left)));
                if (can >= 2) options.Add(new Scenario(this, new RotateAction(nextRockState.X, nextRockState.Y, Rotation.Left), new RotateAction(nextRockState.X, nextRockState.Y, Rotation.Left)));
                if (room.RotationsAllowed >= 2) options.Add(new Scenario(this, new RotateAction(nextRockState.X, nextRockState.Y, Rotation.Right)));
            }
            return options;
        }

        private bool IsPathFatal(Actor actor)
        {
            if (actor == null) return true;
            if (GetExit(actor) == ExitSide.Dead) return true;

            var next = GetNextActorState(actor);
            if (next == null || Grid.IsOutOfBounds(next)) return true;

            return Math.Abs(Grid.GetRoomType(next.X, next.Y)) == 0;
        }

        public bool IsViableForIndy(Actor indyFromWait, ExitSide prevExit)
        {
            if (IsPathFatal(indyFromWait)) return false;
            if (DoesIndyCollideWithRock()) return false;
            return GetExit(indyFromWait) != prevExit;
        }

        public void SimplifyActions()
        {
            var simplifiedActions = new List<IAction>();
            var groupedActions = Actions.GroupBy(a => new Point(a.X, a.Y));
            foreach (var group in groupedActions)
            {
                var netRotation = (group.Sum(a => a.RotationEffect) % 4 + 4) % 4;
                switch (netRotation)
                {
                    case 1: simplifiedActions.Add(new RotateAction(group.Key.X, group.Key.Y, Rotation.Left)); break;
                    case 2: simplifiedActions.Add(new RotateAction(group.Key.X, group.Key.Y, Rotation.Left)); simplifiedActions.Add(new RotateAction(group.Key.X, group.Key.Y, Rotation.Left)); break;
                    case 3: simplifiedActions.Add(new RotateAction(group.Key.X, group.Key.Y, Rotation.Right)); break;
                }
            }
            Actions = simplifiedActions;
        }
    }

    private class ScenarioComparer : IEqualityComparer<Scenario>
    {
        public bool Equals(Scenario x, Scenario y)
        {
            if (x == null || y == null) return x == y;
            return string.Join(":", x.Actions.Select(a => a.ToString())) == string.Join(":", y.Actions.Select(a => a.ToString()));
        }

        public int GetHashCode(Scenario obj) => string.Join(":", obj.Actions.Select(a => a.ToString())).GetHashCode();
    }
}