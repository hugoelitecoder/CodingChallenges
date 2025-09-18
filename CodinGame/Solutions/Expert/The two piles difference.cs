using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;

class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var inputs = Console.ReadLine().Split(' ');
        var values = new List<int>();
        for (var i = 0; i < n; i++)
        {
            var value = int.Parse(inputs[i]);
            values.Add(value);
        }

        var solver = new PartitionSolver();
        var result = solver.Solve(n, values);

        Console.WriteLine(result);
    }
}

class PartitionSolver
{
    public BigInteger Solve(int n, List<int> values)
    {
        var counts = new Dictionary<int, int>();
        foreach (var val in values)
        {
            if (!counts.ContainsKey(val))
            {
                counts[val] = 0;
            }
            counts[val]++;
        }

        var items = counts.OrderBy(kvp => kvp.Key)
                          .Select(kvp => (Value: kvp.Key, Count: kvp.Value))
                          .ToList();

        var mid = items.Count / 2;
        var half1Items = items.Take(mid).ToList();
        var half2Items = items.Skip(mid).ToList();

        var map1 = GeneratePartitions(half1Items);
        var map2 = GeneratePartitions(half2Items);

        var minDiff = BigInteger.MinusOne;
        var targetACount = n / 2;

        foreach (var count1 in map1.Keys)
        {
            var count2 = targetACount - count1;
            if (map2.ContainsKey(count2))
            {
                foreach (var state1 in map1[count1])
                {
                    foreach (var state2 in map2[count2])
                    {
                        var totalSumA = state1.sumA + state2.sumA;
                        var totalProdB = state1.prodB * state2.prodB;
                        
                        var sumSquared = new BigInteger(totalSumA);
                        sumSquared *= sumSquared;

                        var diff = BigInteger.Abs(sumSquared - totalProdB);

                        if (minDiff == BigInteger.MinusOne || diff < minDiff)
                        {
                            minDiff = diff;
                        }
                    }
                }
            }
        }
        return minDiff;
    }

    private Dictionary<int, List<(long sumA, BigInteger prodB)>> GeneratePartitions(List<(int Value, int Count)> items)
    {
        var results = new Dictionary<int, List<(long sumA, BigInteger prodB)>>();
        GenerateRecursive(items, 0, 0, 0L, BigInteger.One, results);
        return results;
    }

    private void GenerateRecursive(
        List<(int Value, int Count)> items,
        int itemIdx,
        int currentACount,
        long currentSumA,
        BigInteger currentProdB,
        Dictionary<int, List<(long sumA, BigInteger prodB)>> results)
    {
        if (itemIdx == items.Count)
        {
            if (!results.ContainsKey(currentACount))
            {
                results[currentACount] = new List<(long sumA, BigInteger prodB)>();
            }
            results[currentACount].Add((currentSumA, currentProdB));
            return;
        }

        var item = items[itemIdx];
        var val = item.Value;
        var totalCount = item.Count;
        
        for (var i = 0; i <= totalCount; i++)
        {
            var countForB = totalCount - i;
            var prodBContribution = BigInteger.Pow(val, countForB);
            
            GenerateRecursive(
                items,
                itemIdx + 1,
                currentACount + i,
                currentSumA + (long)i * val,
                currentProdB * prodBContribution,
                results);
        }
    }
}