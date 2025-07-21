using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var inputs = Console.ReadLine().Split(' ');
        int w = int.Parse(inputs[0]);
        int h = int.Parse(inputs[1]);
        int countX = int.Parse(inputs[2]);
        int countY = int.Parse(inputs[3]);

        var xInput = Array.ConvertAll(Console.ReadLine().Split(' '), int.Parse);
        var yInput = Array.ConvertAll(Console.ReadLine().Split(' '), int.Parse);

        var xList = new List<int> { 0 };
        xList.AddRange(xInput);
        xList.Add(w);

        var yList = new List<int> { 0 };
        yList.AddRange(yInput);
        yList.Add(h);

        var xDiffs = GetSegmentCounts(xList);
        var yDiffs = GetSegmentCounts(yList);

        int totalSquares = 0;
        foreach (var kv in xDiffs)
        {
            if (yDiffs.TryGetValue(kv.Key, out int yCount))
                totalSquares += kv.Value * yCount;
        }

        Console.WriteLine(totalSquares);
    }

    static Dictionary<int, int> GetSegmentCounts(List<int> positions)
    {
        var counts = new Dictionary<int, int>();
        for (int i = 0; i < positions.Count; i++)
        {
            for (int j = i + 1; j < positions.Count; j++)
            {
                int len = positions[j] - positions[i];
                if (counts.ContainsKey(len)) counts[len]++;
                else counts[len] = 1;
            }
        }
        return counts;
    }
}
