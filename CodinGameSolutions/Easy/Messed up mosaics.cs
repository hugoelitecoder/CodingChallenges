using System;
class Solution
{
    public static void Main()
    {
        var N = int.Parse(Console.ReadLine());
        var pattern = Console.ReadLine();
        var L = pattern.Length;
        for (var y = 0; y < N; y++)
        {
            var row = Console.ReadLine();
            var bestShift = 0;
            var bestMismatches = int.MaxValue;
            var bestX = -1;
            for (var shift = 0; shift < L; shift++)
            {
                var mismatches = 0;
                var mx = -1;
                for (var x = 0; x < N; x++)
                {
                    if (row[x] != pattern[(shift + x) % L])
                    {
                        mismatches++;
                        mx = x;
                        if (mismatches > bestMismatches) break;
                    }
                }
                if (mismatches < bestMismatches)
                {
                    bestMismatches = mismatches;
                    bestShift = shift;
                    bestX = mx;
                }
                if (bestMismatches == 0) break;
            }
            if (bestMismatches == 1)
            {
                Console.WriteLine($"({bestX},{y})");
                return;
            }
        }
    }
}
