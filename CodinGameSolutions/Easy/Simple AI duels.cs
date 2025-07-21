using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;

class Solution
{
    const int C_REWARD = 2, D_REWARD = 1, T_REWARD = 3, F_REWARD = 0;

    static readonly Dictionary<char, Dictionary<char, (int, int)>> Payoff = new()
    {
        ['C'] = new() { ['C'] = (C_REWARD, C_REWARD), ['D'] = (F_REWARD, T_REWARD) },
        ['D'] = new() { ['C'] = (T_REWARD, F_REWARD), ['D'] = (D_REWARD, D_REWARD) }
    };

    static int lcg = 12;
    static char RandAction()
    {
        lcg = (137 * lcg + 187) & 0xFF;
        return (BitOperations.PopCount((uint)lcg) & 1) == 0 ? 'D' : 'C';
    }

    class AI
    {
        public string Name;
        public List<string[]> Cmds = new();
        public List<char> History = new();
        public int Score = 0;
    }

    static void Main()
    {
        int turns = int.Parse(Console.ReadLine());
        var players = new AI[2];

        for (int i = 0; i < 2; i++)
        {
            var parts = Console.ReadLine().Split();
            int n = int.Parse(parts[0]);
            var ai = new AI { Name = parts[1] };
            for (int j = 0; j < n; j++)
                ai.Cmds.Add(Console.ReadLine().Split());
            players[i] = ai;
        }

        for (int t = 1; t <= turns; t++)
        {
            var moves = new char[2];

            for (int p = 0; p < 2; p++)
            {
                var me = players[p];
                var opp = players[1 - p];
                foreach (var cmd in me.Cmds)
                {
                    bool cond = false;

                    switch (cmd[0])
                    {
                        case "*":
                            cond = true;
                            break;

                        case "START":
                            cond = t == 1;
                            break;

                        case "OPP":
                            if (cmd[1] == "-1" && opp.History.Count > 0)
                                cond = opp.History[^1] == cmd[2][0];
                            else if (cmd[1] == "MAX")
                            {
                                char ch = cmd[2][0];
                                int countCh = opp.History.Count(c => c == ch);
                                int total = opp.History.Count;
                                cond = countCh > total - countCh;
                            }
                            else if (cmd[1] == "LAST")
                            {
                                int N = int.Parse(cmd[2]);
                                char target = cmd[3][0];
                                int k = Math.Min(N, opp.History.Count);
                                var lastK = opp.History.Skip(opp.History.Count - k);
                                int count = lastK.Count(c => c == target);
                                cond = count > k - count;
                            }
                            break;

                        case "ME":
                            if (cmd[1] == "-1" && me.History.Count > 0)
                                cond = me.History[^1] == cmd[2][0];
                            else if (cmd[1] == "MAX")
                            {
                                char ch = cmd[2][0];
                                int countCh = me.History.Count(c => c == ch);
                                int total = me.History.Count;
                                cond = countCh > total - countCh;
                            }
                            else if (cmd[1] == "WIN")
                                cond = me.Score > opp.Score;
                            break;
                    }

                    if (cond)
                    {
                        var action = cmd[^1];
                        moves[p] = action == "RAND" ? RandAction() : action[0];
                        break;
                    }
                }
            }

            var (scoreA, scoreB) = Payoff[moves[0]][moves[1]];
            players[0].Score += scoreA;
            players[1].Score += scoreB;
            players[0].History.Add(moves[0]);
            players[1].History.Add(moves[1]);
        }

        if (players[0].Score > players[1].Score) Console.WriteLine(players[0].Name);
        else if (players[1].Score > players[0].Score) Console.WriteLine(players[1].Name);
        else Console.WriteLine("DRAW");
    }
}
