using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    class Score
    {
        public string name;
        public int total;
        public int nbRound;
    }

    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var scores = new List<Score>();
        for (var i = 0; i < n; i++)
        {
            var player = Console.ReadLine();
            scores.Add(new Score { name = player });
        }
        for (var i = 0; i < n; i++)
        {
            var shoots = Console.ReadLine();
            Console.Error.WriteLine(shoots);
            var total = 0;
            var missCount = 0;
            var roundCount = 0;
            var shotCount = 0;
            var roundStart = 0;
            var tokens = shoots.Split(' ');
            foreach (var token in tokens)
            {
                if (token == "X")
                {
                    missCount++;
                    if (missCount == 1) total -= 20;
                    else if (missCount == 2) total -= 30;
                    else total = 0;
                }
                else if (token.Contains("2*"))
                {
                    missCount = 0;
                    total += 2 * int.Parse(token.Replace("2*", ""));
                }
                else if (token.Contains("3*"))
                {
                    missCount = 0;
                    total += 3 * int.Parse(token.Replace("3*", ""));
                }
                else
                {
                    missCount = 0;
                    total += int.Parse(token);
                }
                shotCount++;
                if (total < 0) total = 0;
                else if (total > 101)
                {
                    missCount = 0;
                    total = roundStart;
                    roundCount++;
                    shotCount = 0;
                    missCount = 0;
                }
                else if (shotCount == 3)
                {
                    roundStart = total;
                    shotCount = 0;
                    roundCount++;
                    missCount = 0;
                }
            }
            scores[i].total = total;
            scores[i].nbRound = roundCount;
        }
        var winner = scores
            .Where(s => s.total == 101)
            .OrderBy(s => s.nbRound)
            .First()
            .name;
        Console.WriteLine(winner);
    }
}
