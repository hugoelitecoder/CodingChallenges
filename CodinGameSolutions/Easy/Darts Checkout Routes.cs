using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    class Dart
    {
        public string Name;
        public int Value;
        public bool IsDouble;

        public Dart(string name, int value, bool isDouble)
        {
            Name = name;
            Value = value;
            IsDouble = isDouble;
        }

        public override string ToString() => Name;
    }

    static List<Dart> allDarts = new();
    static List<Dart> doubles = new();
    static HashSet<string> uniqueRoutes = new();
    static int targetScore;
    static int maxDarts;

    static void Main()
    {
        targetScore = int.Parse(Console.ReadLine());
        maxDarts = int.Parse(Console.ReadLine());

        GenerateDarts();

        for (int d = 1; d <= maxDarts; d++)
        {
            DFS(new List<Dart>(), 0, d);
        }

        Console.WriteLine(uniqueRoutes.Count);
    }

    static void GenerateDarts()
    {
        for (int i = 1; i <= 20; i++)
        {
            allDarts.Add(new Dart(i.ToString(), i, false));
            allDarts.Add(new Dart($"D{i}", i * 2, true));
            allDarts.Add(new Dart($"T{i}", i * 3, false));
        }
        allDarts.Add(new Dart("25", 25, false));
        allDarts.Add(new Dart("D25", 50, true));

        doubles = allDarts.Where(d => d.IsDouble).ToList();
    }

    static void DFS(List<Dart> path, int total, int dartsLeft)
    {
        if (dartsLeft == 0)
        {
            if (total == targetScore && path.Last().IsDouble)
            {
                var key = string.Join(",", path.Select(d => d.Name));
                uniqueRoutes.Add(key);
            }
            return;
        }

        foreach (var dart in allDarts)
        {
            if (total + dart.Value > targetScore) continue;

            // Final dart must be a double
            if (dartsLeft == 1 && !dart.IsDouble) continue;

            path.Add(dart);
            DFS(path, total + dart.Value, dartsLeft - 1);
            path.RemoveAt(path.Count - 1);
        }
    }
}
