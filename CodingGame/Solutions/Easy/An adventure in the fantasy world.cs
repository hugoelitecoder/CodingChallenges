using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var path = Console.ReadLine();
        var n = int.Parse(Console.ReadLine());
        var moneyMap = new Dictionary<(int r, int c), int>();
        var enemyMap = new Dictionary<(int r, int c), string>();
        for (var i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            var r = int.Parse(parts[0]);
            var c = int.Parse(parts[1]);
            if (parts[2] == "money")
            {
                var amt = int.Parse(parts[3].TrimEnd('G'));
                moneyMap[(r, c)] = amt;
            }
            else
            {
                enemyMap[(r, c)] = parts[3];
            }
        }

        var row = 0;
        var col = 0;
        var gold = 50;
        foreach (var step in path)
        {
            switch (step)
            {
                case 'U': row--; break;
                case 'D': row++; break;
                case 'L': col--; break;
                case 'R': col++; break;
            }

            var pos = (row, col);
            if (moneyMap.TryGetValue(pos, out var amt))
            {
                gold += amt;
                moneyMap.Remove(pos);
            }

            if (enemyMap.TryGetValue(pos, out var type))
            {
                if (type != "goblin" || gold < 50)
                {
                    Console.WriteLine($"{row} {col} {gold}G {type}");
                    return;
                }
                gold -= 50;
            }
        }

        Console.WriteLine($"GameClear {row} {col} {gold}G");
    }
}
