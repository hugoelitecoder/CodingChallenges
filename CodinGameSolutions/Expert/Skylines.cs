using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var processor = new SkylineProcessor();
        for (var i = 0; i < n; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var h = int.Parse(inputs[0]);
            var x1 = int.Parse(inputs[1]);
            var x2 = int.Parse(inputs[2]);
            processor.AddBuilding(h, x1, x2);
        }

        var lineCount = processor.CalculateContourLines();
        Console.WriteLine(lineCount);
    }
}

class SkylineProcessor
{
    private readonly SortedDictionary<int, List<Action<ActiveBuildingsState>>> _eventMap = new SortedDictionary<int, List<Action<ActiveBuildingsState>>>();

    public void AddBuilding(int h, int x1, int x2)
    {
        if (!_eventMap.TryGetValue(x1, out var startEvents))
        {
            startEvents = new List<Action<ActiveBuildingsState>>();
            _eventMap[x1] = startEvents;
        }
        startEvents.Add(state => state.Add(h));

        if (!_eventMap.TryGetValue(x2, out var endEvents))
        {
            endEvents = new List<Action<ActiveBuildingsState>>();
            _eventMap[x2] = endEvents;
        }
        endEvents.Add(state => state.Remove(h));
    }

    public long CalculateContourLines()
    {
        var state = new ActiveBuildingsState();
        var climber = new Climber();

        foreach (var eventList in _eventMap.Values)
        {
            foreach (var op in eventList)
            {
                op(state);
            }
            var currentMaxHeight = state.GetMaxHeight();
            climber.Step(currentMaxHeight);
        }

        return climber.LineCount();
    }
}

class ActiveBuildingsState
{
    private readonly SortedDictionary<int, int> _heightCounts = new SortedDictionary<int, int>();

    public void Add(int height)
    {
        if (_heightCounts.ContainsKey(height))
        {
            _heightCounts[height]++;
        }
        else
        {
            _heightCounts.Add(height, 1);
        }
    }

    public void Remove(int height)
    {
        if (_heightCounts[height] > 1)
        {
            _heightCounts[height]--;
        }
        else
        {
            _heightCounts.Remove(height);
        }
    }

    public int GetMaxHeight()
    {
        return _heightCounts.Any() ? _heightCounts.Keys.Last() : 0;
    }
}

class Climber
{
    private int _height;
    private int _steps;

    public void Step(int h)
    {
        if (_height != h)
        {
            _height = h;
            _steps++;
        }
    }

    public long LineCount()
    {
        return _steps == 0 ? 0 : (long)_steps * 2 - 1;
    }
}