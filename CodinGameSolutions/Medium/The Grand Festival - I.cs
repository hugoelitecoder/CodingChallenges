using System;

class Solution
{
    static int N, R;
    static long[] prizes;
    static long?[,] memo;

    static void Main()
    {
        N = int.Parse(Console.ReadLine());
        R = int.Parse(Console.ReadLine());

        prizes = new long[N+1];
        for (int i = 1; i <= N; i++)
            prizes[i] = long.Parse(Console.ReadLine());

        memo = new long?[N+2, R+1];
        long answer = Solve(1, 0);
        Console.WriteLine(answer);
    }

    static long Solve(int day, int streak)
    {
        if (day > N)
            return 0;

        if (memo[day, streak].HasValue)
            return memo[day, streak].Value;

        long best = Solve(day + 1, 0);
        if (streak < R)
        {
            long play = prizes[day] + Solve(day + 1, streak + 1);
            if (play > best) best = play;
        }

        memo[day, streak] = best;
        return best;
    }
}
