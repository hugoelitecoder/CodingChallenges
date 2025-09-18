using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var target = int.Parse(Console.ReadLine());
        var containersCount = int.Parse(Console.ReadLine());
        var capacities = new int[containersCount];
        for (var i = 0; i < containersCount; i++)
        {
            capacities[i] = int.Parse(Console.ReadLine());
        }

        var solver = new WaterJugSolver(target, capacities);
        var result = solver.Solve();

        Console.WriteLine(result);
    }
}

public class WaterJugSolver
{
    private readonly int _target;
    private readonly int[] _capacities;
    private readonly int _containerCount;

    public WaterJugSolver(int target, int[] capacities)
    {
        _target = target;
        _capacities = capacities;
        _containerCount = capacities.Length;
    }

    public int Solve()
    {
        var initialWaterLevels = new int[_containerCount];
        var startState = new State(initialWaterLevels);

        if (startState.WaterLevels.Contains(_target))
        {
            return 0;
        }

        var queue = new Queue<(State state, int moves)>();
        var visited = new HashSet<State>();

        queue.Enqueue((startState, 0));
        visited.Add(startState);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            var currentState = node.state;
            var currentMoves = node.moves;
            var nextMoves = currentMoves + 1;

            // Fill moves
            for (var i = 0; i < _containerCount; i++)
            {
                var currentLevels = currentState.WaterLevels;
                if (currentLevels[i] < _capacities[i])
                {
                    var newLevels = currentLevels.ToArray();
                    newLevels[i] = _capacities[i];
                    var nextState = new State(newLevels);
                    if (visited.Add(nextState))
                    {
                        if (newLevels.Contains(_target)) return nextMoves;
                        queue.Enqueue((nextState, nextMoves));
                    }
                }
            }

            // Empty moves
            for (var i = 0; i < _containerCount; i++)
            {
                var currentLevels = currentState.WaterLevels;
                if (currentLevels[i] > 0)
                {
                    var newLevels = currentLevels.ToArray();
                    newLevels[i] = 0;
                    var nextState = new State(newLevels);
                    if (visited.Add(nextState))
                    {
                        if (newLevels.Contains(_target)) return nextMoves;
                        queue.Enqueue((nextState, nextMoves));
                    }
                }
            }

            // Pour moves
            for (var from = 0; from < _containerCount; from++)
            {
                for (var to = 0; to < _containerCount; to++)
                {
                    if (from == to) continue;

                    var currentLevels = currentState.WaterLevels;
                    if (currentLevels[from] > 0 && currentLevels[to] < _capacities[to])
                    {
                        var newLevels = currentLevels.ToArray();
                        var amount = Math.Min(currentLevels[from], _capacities[to] - currentLevels[to]);
                        newLevels[from] -= amount;
                        newLevels[to] += amount;
                        var nextState = new State(newLevels);
                        if (visited.Add(nextState))
                        {
                            if (newLevels.Contains(_target)) return nextMoves;
                            queue.Enqueue((nextState, nextMoves));
                        }
                    }
                }
            }
        }

        return -1;
    }
}

public class State
{
    public IReadOnlyList<int> WaterLevels => _waterLevels;
    private readonly int[] _waterLevels;
    private readonly int _hashCode;

    public State(int[] waterLevels)
    {
        _waterLevels = waterLevels;
        _hashCode = ComputeHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is not State other) return false;
        if (this._waterLevels.Length != other._waterLevels.Length) return false;
        if (this.GetHashCode() != other.GetHashCode()) return false;
        return _waterLevels.SequenceEqual(other._waterLevels);
    }

    public override int GetHashCode()
    {
        return _hashCode;
    }

    private int ComputeHashCode()
    {
        var hash = 17;
        foreach (var level in _waterLevels)
        {
            unchecked
            {
                hash = hash * 31 + level;
            }
        }
        return hash;
    }
}