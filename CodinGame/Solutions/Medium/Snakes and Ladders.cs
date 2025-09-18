using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var width = int.Parse(inputs[0]);
        var height = int.Parse(inputs[1]);
        var n = int.Parse(Console.ReadLine());
        inputs = Console.ReadLine().Split(' ');
        var snakeAmount = int.Parse(inputs[0]);
        var ladderAmount = int.Parse(inputs[1]);
        var shortcuts = new Dictionary<int, int>();
        for (var i = 0; i < snakeAmount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var head = int.Parse(inputs[0]);
            var tail = int.Parse(inputs[1]);
            shortcuts[head] = tail;
        }
        for (var i = 0; i < ladderAmount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var top = int.Parse(inputs[0]);
            var bottom = int.Parse(inputs[1]);
            shortcuts[bottom] = top;
        }
        var solver = new SnakeLadderSolver(width, height, n, shortcuts);
        var result = solver.FindMinRolls();
        Console.WriteLine(result);
    }
}

public class SnakeLadderSolver
{
    private readonly int _finalTile;
    private readonly int _dieSides;
    private readonly IReadOnlyDictionary<int, int> _shortcuts;

    public SnakeLadderSolver(int width, int height, int dieSides, IReadOnlyDictionary<int, int> shortcuts)
    {
        _finalTile = width * height;
        _dieSides = dieSides;
        _shortcuts = shortcuts;
    }

    public int FindMinRolls()
    {
        if (_finalTile <= 1)
        {
            return 0;
        }
        var queue = new Queue<(int tile, int rolls)>();
        var visited = new HashSet<int>();
        queue.Enqueue((1, 0));
        visited.Add(1);
        while (queue.Count > 0)
        {
            var state = queue.Dequeue();
            var tile = state.tile;
            var rolls = state.rolls;
            for (var roll = 1; roll <= _dieSides; roll++)
            {
                var nextTile = tile + roll;
                if (nextTile > _finalTile)
                {
                    continue;
                }
                if (_shortcuts.TryGetValue(nextTile, out var dest))
                {
                    nextTile = dest;
                }
                if (nextTile == _finalTile)
                {
                    return rolls + 1;
                }
                if (visited.Add(nextTile))
                {
                    queue.Enqueue((nextTile, rolls + 1));
                }
            }
        }
        return -1;
    }
}