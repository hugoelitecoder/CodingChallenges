using System;
using System.Collections.Generic;
using System.Linq;

public class Solution
{
    public static void Main(string[] args)
    {
        var spyNames = new HashSet<string>(Console.ReadLine().Split(' '));
        var allSuspects = new List<Suspect>();
        for (var i = 0; i < 15; i++)
        {
            var parts = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var name = parts[0];
            var attributes = new HashSet<string>(parts.Skip(2));
            allSuspects.Add(new Suspect(i, name, spyNames.Contains(name), attributes));
        }

        var hunter = new SpyHunter(allSuspects);
        var commands = hunter.FindShortestCommandSequence();
        
        foreach (var command in commands)
        {
            Console.WriteLine(command);
        }
    }
}

public class SpyHunter
{
    private readonly List<Suspect> _allSuspects;
    private readonly int _initialSpyMask;

    public SpyHunter(List<Suspect> suspects)
    {
        _allSuspects = suspects;
        _initialSpyMask = 0;
        foreach (var s in suspects)
        {
            if (s.IsSpy)
            {
                _initialSpyMask |= (1 << s.Id);
            }
        }
    }

    public List<string> FindShortestCommandSequence()
    {
        var initialRemainingMask = (1 << _allSuspects.Count) - 1; // All 1s
        var initialState = new State(initialRemainingMask, 0, new List<string>());

        var queue = new Queue<State>();
        queue.Enqueue(initialState);
        var visited = new HashSet<(int, int)>();
        visited.Add((initialState.RemainingMask, initialState.IdentifiedMask));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (IsGoalState(current.RemainingMask, current.IdentifiedMask))
            {
                return current.Path;
            }

            var activeAttributes = GetActiveAttributes(current.RemainingMask);
            
            foreach (var attr in activeAttributes)
            {
                var matchesMask = GetMatchesMask(current.RemainingMask, attr);
                if (IsPureSpyGroup(matchesMask))
                {
                    var nextState = new State(
                        current.RemainingMask & ~matchesMask,
                        current.IdentifiedMask | matchesMask,
                        new List<string>(current.Path) { attr }
                    );
                    if (visited.Add((nextState.RemainingMask, nextState.IdentifiedMask)))
                    {
                        queue.Enqueue(nextState);
                    }
                }
                if (IsPureInnocentGroup(matchesMask))
                {
                    var nextState = new State(
                        current.RemainingMask & ~matchesMask,
                        current.IdentifiedMask,
                        new List<string>(current.Path) { $"NOT {attr}" }
                    );
                     if (visited.Add((nextState.RemainingMask, nextState.IdentifiedMask)))
                    {
                        queue.Enqueue(nextState);
                    }
                }
            }
        }
        return new List<string>();
    }

    private bool IsGoalState(int remainingMask, int identifiedMask)
    {
        if (int.PopCount(identifiedMask) == 6) return true;
        var remainingSpies = remainingMask & _initialSpyMask;
        return remainingMask == remainingSpies && int.PopCount(identifiedMask | remainingSpies) == 6;
    }

    private bool IsPureSpyGroup(int matchesMask) => (matchesMask & ~_initialSpyMask) == 0;
    
    private bool IsPureInnocentGroup(int matchesMask) => (matchesMask & _initialSpyMask) == 0;
    
    private HashSet<string> GetActiveAttributes(int remainingMask)
    {
        var attributes = new HashSet<string>();
        for (var i = 0; i < _allSuspects.Count; i++)
        {
            if ((remainingMask & (1 << i)) != 0)
            {
                attributes.UnionWith(_allSuspects[i].Attributes);
            }
        }
        return attributes;
    }

    private int GetMatchesMask(int remainingMask, string attribute)
    {
        var matches = 0;
        for (var i = 0; i < _allSuspects.Count; i++)
        {
            if ((remainingMask & (1 << i)) != 0 && _allSuspects[i].Attributes.Contains(attribute))
            {
                matches |= (1 << i);
            }
        }
        return matches;
    }

    private record State(int RemainingMask, int IdentifiedMask, List<string> Path);
}

public class Suspect
{
    public readonly int Id;
    public readonly string Name;
    public readonly bool IsSpy;
    public readonly HashSet<string> Attributes;

    public Suspect(int id, string name, bool isSpy, HashSet<string> attributes)
    {
        Id = id;
        Name = name;
        IsSpy = isSpy;
        Attributes = attributes;
    }
}