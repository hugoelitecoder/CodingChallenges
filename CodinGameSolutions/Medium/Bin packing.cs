using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        var tests = int.Parse(Console.ReadLine());
        for (int tc = 0; tc < tests; tc++)
        {
            var data = Console.ReadLine().Split().Select(int.Parse).ToArray();
            int bins = data[0];
            var weights = data[2..].OrderByDescending(w => w).ToArray();
            Console.WriteLine(CanPartition(weights, bins) ? "yes" : "no");
        }
    }

    static bool CanPartition(int[] w, int k)
    {
        int total = w.Sum();
        if (total % k != 0 || w[0] > total / k)
            return false;
        int target = total / k;
        var used = new bool[w.Length];

        bool Search(int start, int sum, int remaining)
        {
            if (remaining == 0) return true;
            if (sum == target)
                return Search(0, 0, remaining - 1);

            int prev = -1;
            for (int i = start; i < w.Length; i++)
            {
                if (used[i] || w[i] == prev || sum + w[i] > target) continue;
                used[i] = true;
                if (Search(i + 1, sum + w[i], remaining)) return true;
                used[i] = false;
                prev = w[i];
                if (sum == 0 || sum + w[i] == target) break;
            }
            return false;
        }

        return Search(0, 0, k);
    }
}
