using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var initialDice = new List<Die>();
        for (var i = 0; i < n; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var x = int.Parse(inputs[0]);
            var y = int.Parse(inputs[1]);
            var faceStr = inputs[2];
            var isIron = faceStr == "IRON";
            var face = isIron ? Face.TOP : Enum.Parse<Face>(faceStr, true);
            initialDice.Add(new Die(i, x, y, face, isIron));
        }

        var initialState = new State(initialDice.ToArray(), 0, new List<string>());
        var solver = new PuzzleSolver(n);
        var solutionPath = solver.Solve(initialState);

        if (solutionPath != null)
        {
            foreach (var move in solutionPath)
            {
                Console.WriteLine(move);
            }
        }
    }
}

public record State(Die[] Dice, int RossiOnDieId, List<string> Path);
public record Die(int Id, int X, int Y, Face Face, bool IsIron);
public enum Face { TOP, BOTTOM, NORTH, SOUTH, EAST, WEST }
public enum Direction { LEFT, RIGHT, UP, DOWN }

public class PuzzleSolver
{
    private readonly int _totalDice;
    private static readonly Direction[] AllDirections = (Direction[])Enum.GetValues(typeof(Direction));
    
    private static readonly Dictionary<Direction, (int dx, int dy)> _vectors = new()
    {
        [Direction.UP] = (0, -1),
        [Direction.DOWN] = (0, 1),
        [Direction.LEFT] = (-1, 0),
        [Direction.RIGHT] = (1, 0)
    };

    private static readonly Dictionary<Direction, Dictionary<Face, Face>> _faceChanges = new()
    {
        [Direction.RIGHT] = new() {
            [Face.TOP] = Face.EAST, [Face.EAST] = Face.BOTTOM, [Face.BOTTOM] = Face.WEST, [Face.WEST] = Face.TOP,
            [Face.NORTH] = Face.NORTH, [Face.SOUTH] = Face.SOUTH
        },
        [Direction.LEFT] = new() {
            [Face.TOP] = Face.WEST, [Face.WEST] = Face.BOTTOM, [Face.BOTTOM] = Face.EAST, [Face.EAST] = Face.TOP,
            [Face.NORTH] = Face.NORTH, [Face.SOUTH] = Face.SOUTH
        },
        [Direction.UP] = new() {
            [Face.TOP] = Face.NORTH, [Face.NORTH] = Face.BOTTOM, [Face.BOTTOM] = Face.SOUTH, [Face.SOUTH] = Face.TOP,
            [Face.WEST] = Face.WEST, [Face.EAST] = Face.EAST
        },
        [Direction.DOWN] = new() {
            [Face.TOP] = Face.SOUTH, [Face.SOUTH] = Face.BOTTOM, [Face.BOTTOM] = Face.NORTH, [Face.NORTH] = Face.TOP,
            [Face.WEST] = Face.WEST, [Face.EAST] = Face.EAST
        }
    };

    public PuzzleSolver(int totalDice)
    {
        _totalDice = totalDice;
    }

    public List<string> Solve(State initialState)
    {
        var queue = new Queue<State>();
        var visited = new HashSet<string>();
        queue.Enqueue(initialState);
        visited.Add(GenerateKey(initialState));
        while (queue.Count > 0)
        {
            var currentState = queue.Dequeue();
            if (IsGoalState(currentState))
            {
                return currentState.Path;
            }
            var rossiComponent = GetConnectedComponent(currentState.Dice, currentState.RossiOnDieId);
            var grid = new Die[4, 4];
            foreach (var d in currentState.Dice)
            {
                grid[d.X, d.Y] = d;
            }
            foreach (var dieId in rossiComponent)
            {
                var dieToMove = currentState.Dice[dieId];
                if (dieToMove.IsIron) continue;
                foreach (var dir in AllDirections)
                {
                    var (dx, dy) = _vectors[dir];
                    var newX = dieToMove.X + dx;
                    var newY = dieToMove.Y + dy;
                    if (newX >= 0 && newX < 4 && newY >= 0 && newY < 4 && grid[newX, newY] == null)
                    {
                        var newDice = (Die[])currentState.Dice.Clone();
                        var newFace = _faceChanges[dir][dieToMove.Face];
                        newDice[dieId] = dieToMove with { X = newX, Y = newY, Face = newFace };
                        var newPath = new List<string>(currentState.Path) { $"{dieId} {dir.ToString().ToUpper()}" };
                        var newState = new State(newDice, dieId, newPath);
                        var key = GenerateKey(newState);
                        if (visited.Add(key))
                        {
                            queue.Enqueue(newState);
                        }
                    }
                }
            }
        }
        return null;
    }

    private bool IsGoalState(State state)
    {
        foreach (var die in state.Dice)
        {
            if (die.Face != Face.TOP) return false;
        }
        var component = GetConnectedComponent(state.Dice, 0);
        return component.Count == _totalDice;
    }

    private HashSet<int> GetConnectedComponent(Die[] dice, int startDieId)
    {
        var componentIds = new HashSet<int>();
        if (dice.Length == 0) return componentIds;
        var grid = new Die[4, 4];
        foreach (var d in dice)
        {
            grid[d.X, d.Y] = d;
        }
        var queue = new Queue<Die>();
        var startDie = dice[startDieId];
        queue.Enqueue(startDie);
        componentIds.Add(startDieId);
        while (queue.Count > 0)
        {
            var currentDie = queue.Dequeue();
            foreach (var dir in AllDirections)
            {
                var (dx, dy) = _vectors[dir];
                var nx = currentDie.X + dx;
                var ny = currentDie.Y + dy;
                if (nx >= 0 && nx < 4 && ny >= 0 && ny < 4 && grid[nx, ny] != null)
                {
                    var neighborDie = grid[nx, ny];
                    if (componentIds.Add(neighborDie.Id))
                    {
                        queue.Enqueue(neighborDie);
                    }
                }
            }
        }
        return componentIds;
    }

    private string GenerateKey(State state)
    {
        var sb = new StringBuilder();
        sb.Append(state.RossiOnDieId);
        sb.Append('|');
        for (var i = 0; i < state.Dice.Length; i++)
        {
            var d = state.Dice[i];
            sb.Append(d.X);
            sb.Append(d.Y);
            sb.Append((int)d.Face);
        }
        return sb.ToString();
    }
}
