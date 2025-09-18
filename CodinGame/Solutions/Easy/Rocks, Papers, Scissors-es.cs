using System;

class Solution
{
    static void Main()
    {
        var n = int.Parse(Console.ReadLine());
        var moves = new int[n];
        for (var i = 0; i < n; i++)
        {
            var c = char.ToUpper(Console.ReadLine()[0]);
            moves[i] = c == 'R' ? 0 : c == 'P' ? 1 : 2;
        }

        int Outcome(int m, int opp)
        {
            if (m == opp) return 0;
            return ((m + 2) % 3 == opp) ? +1 : -1;
        }

        var bestWins = -1;
        var bestStart = 0;
        var bestMove = 0;

        for (var start = 0; start < n; start++)
        {
            for (var m = 0; m < 3; m++)
            {
                if (Outcome(m, moves[start]) != +1) continue;

                var wins = 1;
                for (var step = 1; step < n; step++)
                {
                    var idx = (start + step) % n;
                    var res = Outcome(m, moves[idx]);
                    if (res < 0) break;
                    if (res > 0) wins++;
                }

                if (wins > bestWins || (wins == bestWins && (start < bestStart || (start == bestStart && m < bestMove))))
                {
                    bestWins = wins;
                    bestStart = start;
                    bestMove = m;
                }
            }
        }

        var moveStr = bestMove == 0 ? "Rock" : bestMove == 1 ? "Paper" : "Scissors";
        Console.WriteLine(moveStr);
        Console.WriteLine(bestStart);
    }
}
