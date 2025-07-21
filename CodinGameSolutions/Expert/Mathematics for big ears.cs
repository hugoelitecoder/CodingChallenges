using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var generatorStrings = new List<string>(n);
        for (var i = 0; i < n; i++)
        {
            generatorStrings.Add(Console.ReadLine());
        }

        var order = PermutationGroup.CalculateOrder(generatorStrings);
        Console.WriteLine(order);
    }
}

public static class PermutationGroup
{
    public static int CalculateOrder(List<string> generatorStrings)
    {
        if (generatorStrings.Count == 0)
        {
            return 1;
        }

        var allNumbers = new HashSet<int>();
        var numRegex = new Regex(@"\d+");
        foreach (var str in generatorStrings)
        {
            foreach (Match m in numRegex.Matches(str))
            {
                allNumbers.Add(int.Parse(m.Value));
            }
        }

        if (allNumbers.Count == 0)
        {
            return 1;
        }

        var sortedNumbers = allNumbers.ToList();
        sortedNumbers.Sort();
        
        var remapper = new Dictionary<int, int>();
        for (var i = 0; i < sortedNumbers.Count; i++)
        {
            remapper[sortedNumbers[i]] = i + 1;
        }

        var maxElement = allNumbers.Count;
        var generators = new List<Permutation>();
        foreach (var str in generatorStrings)
        {
            generators.Add(Permutation.Parse(str, maxElement, remapper));
        }

        var identity = Permutation.Identity(maxElement);
        var found = new HashSet<Permutation> { identity };
        var queue = new Queue<Permutation>();
        queue.Enqueue(identity);
        
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var g in generators)
            {
                var next = Permutation.Compose(g, current);
                if (found.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return found.Count;
    }
}

public sealed class Permutation : IEquatable<Permutation>
{
    private readonly int[] _map;
    private int? _hashCode;

    private Permutation(int[] map)
    {
        _map = map;
        _hashCode = null;
    }

    public static Permutation Identity(int maxElement)
    {
        var map = new int[maxElement + 1];
        for (var i = 0; i <= maxElement; i++)
        {
            map[i] = i;
        }
        return new Permutation(map);
    }

    public static Permutation Parse(string str, int maxElement, Dictionary<int, int> remapper)
    {
        var currentPermutation = Identity(maxElement);
        if (string.IsNullOrWhiteSpace(str))
        {
            return currentPermutation;
        }
        
        var cycleRegex = new Regex(@"\((.*?)\)");
        foreach (Match cycleMatch in cycleRegex.Matches(str))
        {
            var cycleContent = cycleMatch.Groups[1].Value.Trim();
            if (string.IsNullOrEmpty(cycleContent)) continue;
            
            var numbers = cycleContent.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => remapper[int.Parse(s)])
                                      .ToList();
            
            if (numbers.Count > 1)
            {
                var cyclePermutation = CreatePermutationForCycle(numbers, maxElement);
                currentPermutation = Compose(currentPermutation, cyclePermutation);
            }
        }
        return currentPermutation;
    }

    private static Permutation CreatePermutationForCycle(List<int> cycle, int maxElement)
    {
        var map = new int[maxElement + 1];
        for (var i = 0; i <= maxElement; i++)
        {
            map[i] = i;
        }
        for (var i = 0; i < cycle.Count - 1; i++)
        {
            map[cycle[i]] = cycle[i + 1];
        }
        map[cycle[cycle.Count - 1]] = cycle[0];
        return new Permutation(map);
    }
    
    public static Permutation Compose(Permutation p1, Permutation p2)
    {
        var maxElement = p1._map.Length - 1;
        var newMap = new int[maxElement + 1];
        for (var i = 1; i <= maxElement; i++)
        {
            newMap[i] = p1._map[p2._map[i]];
        }
        return new Permutation(newMap);
    }

    public bool Equals(Permutation other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (_map.Length != other._map.Length) return false;
        for (var i = 1; i < _map.Length; i++)
        {
            if (_map[i] != other._map[i]) return false;
        }
        return true;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Permutation);
    }

    public override int GetHashCode()
    {
        if (_hashCode.HasValue)
        {
            return _hashCode.Value;
        }
        var hc = new HashCode();
        for (var i = 1; i < _map.Length; i++)
        {
            hc.Add(_map[i]);
        }
        _hashCode = hc.ToHashCode();
        return _hashCode.Value;
    }
}