using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        var pupae = new List<Pupa>();
        for (int i = 0; i < N; i++)
        {
            var line = Console.ReadLine().Split();
            int idx = int.Parse(line[0]);
            int horn = int.Parse(line[1]);
            int species = int.Parse(line[2]);
            pupae.Add(new Pupa(idx, horn, species));
        }
        int lastSeparator = -1;
        Process(pupae, ref lastSeparator);
        Console.WriteLine(lastSeparator);
    }

    static double Entropy(List<Pupa> group)
    {
        if (group.Count == 0) return 0;
        var total = group.Count;
        var speciesCounts = group.GroupBy(p => p.SpeciesId).Select(g => g.Count()).ToArray();
        if (speciesCounts.Length == 1) return 0;
        double ent = 0.0;
        foreach (var count in speciesCounts)
        {
            double p = (double)count / total;
            ent -= p * Math.Log(p, 2);
        }
        return ent;
    }

    static void Process(List<Pupa> group, ref int lastSeparator)
    {
        if (group.Select(p => p.SpeciesId).Distinct().Count() == 1)
            return;

        double minWeightedEntropy = double.MaxValue;
        int bestSeparatorIdx = -1;
        int bestSeparatorListIdx = -1;

        for (int i = 1; i < group.Count; i++)
        {
            var separatorHorn = group[i].HornSize;
            var left = group.Where(p => p.HornSize < separatorHorn).ToList();
            var right = group.Where(p => p.HornSize >= separatorHorn).ToList();

            if (left.Count == 0 || right.Count == 0) continue;
            double leftEnt = Entropy(left);
            double rightEnt = Entropy(right);
            double weightedEnt = ((double)left.Count / group.Count) * leftEnt +
                                 ((double)right.Count / group.Count) * rightEnt;

            if (weightedEnt < minWeightedEntropy || (Math.Abs(weightedEnt - minWeightedEntropy) < 1e-8 && group[i].Index < bestSeparatorIdx))
            {
                minWeightedEntropy = weightedEnt;
                bestSeparatorIdx = group[i].Index;
                bestSeparatorListIdx = i;
            }
        }

        if (bestSeparatorListIdx == -1)
            return;

        lastSeparator = bestSeparatorIdx;

        var separatorHornFinal = group[bestSeparatorListIdx].HornSize;
        var leftGroup = group.Where(p => p.HornSize < separatorHornFinal).ToList();
        var rightGroup = group.Where(p => p.HornSize >= separatorHornFinal).ToList();

        if (leftGroup.Count > 0 && leftGroup.Count < group.Count)
            Process(leftGroup, ref lastSeparator);
        if (rightGroup.Count > 0 && rightGroup.Count < group.Count)
            Process(rightGroup, ref lastSeparator);
    }
}

class Pupa
{
    public int Index;
    public int HornSize;
    public int SpeciesId;
    public Pupa(int index, int horn, int species)
    {
        Index = index;
        HornSize = horn;
        SpeciesId = species;
    }
}
