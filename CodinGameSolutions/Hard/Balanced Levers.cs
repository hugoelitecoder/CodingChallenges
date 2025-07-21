using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        float lim = float.Parse(Console.ReadLine());
        int n = int.Parse(Console.ReadLine());
        List<int> weights = Console.ReadLine().Split().Select(int.Parse).ToList();

        var builder = new MobileBuilder();
        float result = builder.GetMaxWidth(weights, lim);

        Console.WriteLine(result < 0 ? "-1" : result.ToString("0.0000"));
    }

    class Mobile
    {
        public float Left, Right, TotalWeight;
        public float Width => Left + Right;

        public Mobile(float left, float right, float totalWeight)
        {
            Left = left;
            Right = right;
            TotalWeight = totalWeight;
        }
    }

    class MobileBuilder
    {
        private readonly Dictionary<string, List<Mobile>> memo = new();
        private readonly HashSet<string> visitedCombos = new();

        public float GetMaxWidth(List<int> weights, float limit)
        {
            var mobiles = Generate(weights);
            float maxWidth = -1;
            foreach (var m in mobiles)
            {
                if (m.Width <= limit)
                    maxWidth = Math.Max(maxWidth, m.Width);
            }
            return maxWidth;
        }

        private List<Mobile> Generate(List<int> weights)
        {
            weights.Sort();
            var key = string.Join(",", weights);
            if (memo.ContainsKey(key))
                return memo[key];

            var result = new List<Mobile>();
            if (weights.Count == 1)
            {
                result.Add(new Mobile(0, 0, weights[0]));
            }
            else
            {
                GenerateAllPartitions(weights, (left, right) =>
                {
                    var leftMobiles = Generate(left);
                    var rightMobiles = Generate(right);

                    foreach (var lm in leftMobiles)
                    {
                        foreach (var rm in rightMobiles)
                        {
                            float totalWeight = lm.TotalWeight + rm.TotalWeight;
                            float a = rm.TotalWeight / totalWeight;
                            float b = lm.TotalWeight / totalWeight;

                            float newLeft = Math.Max(lm.Left + a, rm.Left - b);
                            float newRight = Math.Max(rm.Right + b, lm.Right - a);

                            result.Add(new Mobile(newLeft, newRight, totalWeight));
                        }
                    }
                });
            }

            memo[key] = result;
            return result;
        }

        private void GenerateAllPartitions(List<int> weights, Action<List<int>, List<int>> callback)
        {
            int n = weights.Count;
            int max = (1 << n);

            for (int mask = 1; mask < max - 1; mask++)
            {
                List<int> left = new(), right = new();
                for (int i = 0; i < n; i++)
                {
                    if ((mask & (1 << i)) != 0) left.Add(weights[i]);
                    else right.Add(weights[i]);
                }

                string key = string.Join(",", left.OrderBy(x => x)) + "|" + string.Join(",", right.OrderBy(x => x));
                if (!visitedCombos.Contains(key))
                {
                    visitedCombos.Add(key);
                    callback(left, right);
                }
            }
        }
    }
}
