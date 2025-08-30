using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;

class Solution
{
    static void Main(string[] args)
    {
        var watch = Stopwatch.StartNew();
        int n = int.Parse(Console.ReadLine());
        var values = new List<double>(n);
        var inputStrings = new List<string>(n);

        for (int i = 0; i < n; i++)
        {
            string s = Console.ReadLine();
            inputStrings.Add($"'{s}'");
            values.Add(GetValue(s));
        }
        
        Console.Error.WriteLine($"[DEBUG] n = {n}");
        Console.Error.WriteLine($"[DEBUG] Inputs: [{string.Join(", ", inputStrings)}]");
        Console.Error.WriteLine($"[DEBUG] Values: [{string.Join(", ", values)}]");
        int result = CalculateMinSwaps(values);
        Console.WriteLine(result);

        watch.Stop();
        Console.Error.WriteLine($"[DEBUG] Execution time: {watch.ElapsedMilliseconds} ms");
    }

    public static int CalculateMinSwaps(List<double> values)
    {
        var n = values.Count;
        if (n <= 1)
        {
            return 0;
        }
        var sortedValues = new List<double>(values);
        sortedValues.Sort();
        for (var i = 1; i < n; i++)
        {
            if (sortedValues[i] == sortedValues[i - 1])
            {
                Console.Error.WriteLine($"[DEBUG] Duplicate value found: {sortedValues[i]}. Impossible to sort strictly.");
                return -1;
            }
        }
        var targetIndices = new Dictionary<double, int>(n);
        for (var i = 0; i < n; i++)
        {
            targetIndices[sortedValues[i]] = i;
        }
        var visited = new bool[n];
        var totalSwaps = 0;
        for (var i = 0; i < n; i++)
        {
            if (visited[i])
            {
                continue;
            }

            var cycleSize = 0;
            var j = i;
            while (!visited[j])
            {
                visited[j] = true;
                j = targetIndices[values[j]];
                cycleSize++;
            }
            
            if (cycleSize > 0) 
            {
                totalSwaps += (cycleSize - 1);
            }
        }
        Console.Error.WriteLine($"[DEBUG] Total swaps calculated: {totalSwaps}");
        return totalSwaps;
    }

    private static double GetValue(string s)
    {
        if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double floatValue))
        {
            return floatValue;
        }
        return s.Sum(c => (double)c);
    }
}