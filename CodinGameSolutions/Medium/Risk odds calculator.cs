using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int attackers = int.Parse(Console.ReadLine().Trim());
        int defenders = int.Parse(Console.ReadLine().Trim());
        var dp = new double[attackers + 1, defenders + 1];
        for (int a = 0; a <= attackers; a++) dp[a, 0] = 1.0;
        for (int d = 1; d <= defenders; d++) dp[0, d] = 0.0;

        var battle = ComputeBattleProbabilities();
        for (int attacker = 1; attacker <= attackers; attacker++)
        {
            for (int defender = 1; defender <= defenders; defender++)
            {
                double probability = 0.0;
                int attackerDice = Math.Min(3, attacker);
                int defenderDice = Math.Min(2, defender);
                foreach (var (loseA, loseD, prob) in battle[(attackerDice, defenderDice)])
                {
                    int na = attacker - loseA;
                    int nd = defender - loseD;
                    probability  += prob * dp[na, nd];
                }
                dp[attacker, defender] = probability;
            }
        }

        Console.WriteLine($"{dp[attackers, defenders] * 100:0.00}%");
    }

    private static Dictionary<(int, int), List<(int, int, double)>> ComputeBattleProbabilities()
    {
        var result = new Dictionary<(int, int), List<(int, int, double)>>();
        for (int atk = 1; atk <= 3; atk++)
        for (int def = 1; def <= 2; def++)
        {
            var counts = new Dictionary<(int, int), int>();
            int total = 0;
            var atkRolls = AllRolls(atk);
            var defRolls = AllRolls(def);

            foreach (var aroll in atkRolls)
            foreach (var droll in defRolls)
            {
                total++;
                Array.Sort(aroll); Array.Reverse(aroll);
                Array.Sort(droll); Array.Reverse(droll);
                int loseA = 0, loseD = 0;
                int comps = Math.Min(atk, def);
                for (int i = 0; i < comps; i++)
                    if (aroll[i] > droll[i]) loseD++;
                    else                      loseA++;
                var key = (loseA, loseD);
                counts[key] = counts.GetValueOrDefault(key, 0) + 1;
            }

            var list = new List<(int, int, double)>();
            foreach (var kv in counts)
                list.Add((kv.Key.Item1, kv.Key.Item2, kv.Value / (double)total));
            result[(atk, def)] = list;
        }
        return result;
    }

    private static List<int[]> AllRolls(int d)
    {
        var res = new List<int[]>();
        Roll(new int[d], 0, d, res);
        return res;
    }
    private static void Roll(int[] cur, int idx, int d, List<int[]> res)
    {
        if (idx == d)
        {
            var copy = new int[d];
            Array.Copy(cur, copy, d);
            res.Add(copy);
            return;
        }
        for (int v = 1; v <= 6; v++)
        {
            cur[idx] = v;
            Roll(cur, idx + 1, d, res);
        }
    }
}
