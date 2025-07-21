using System;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        var parts = Console.ReadLine().Split();
        int n = int.Parse(parts[0]);
        int k = int.Parse(parts[1]);

        int[] sweetness = Console.ReadLine()
                              .Split()
                              .Select(int.Parse)
                              .ToArray();

        int[] values = MapValues(sweetness, k);
        int[] prefix = BuildPrefix(values);
        int best = FindBest(prefix, n);

        Console.WriteLine(best);
    }

    static int[] MapValues(int[] sweetness, int k)
    {
        int[] result = new int[sweetness.Length];
        for (int i = 0; i < sweetness.Length; i++)
            result[i] = sweetness[i] >= k ? 1 : -1;
        return result;
    }

    static int[] BuildPrefix(int[] values)
    {
        int[] prefix = new int[values.Length + 1];
        for (int i = 0; i < values.Length; i++)
            prefix[i + 1] = prefix[i] + values[i];
        return prefix;
    }

    static int FindBest(int[] prefix, int n)
    {
        int best = 0;
        for (int i = 0; i < n; i++)
        {
            for (int j = n - 1; j >= i; j--)
            {
                int len = j - i + 1;
                if (len <= best)
                    break;
                if (prefix[j + 1] - prefix[i] > 0)
                {
                    best = len;
                    break;
                }
            }
        }
        return best;
    }
}