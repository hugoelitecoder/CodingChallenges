using System;
using System.Collections.Generic;

class Solution
{
    static int DigitSum(int n)
    {
        int sum = 0;
        while (n > 0)
        {
            sum += n % 10;
            n /= 10;
        }
        return sum;
    }

    static void Main()
    {
        int R = int.Parse(Console.ReadLine());
        int C = int.Parse(Console.ReadLine());
        int T = int.Parse(Console.ReadLine());

        var visited = new bool[R, C];
        var queue = new Queue<(int r, int c)>();
        queue.Enqueue((0, 0));
        int count = 0;

        int[] dr = { 1, -1, 0, 0 };
        int[] dc = { 0, 0, 1, -1 };

        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();
            if (r < 0 || r >= R || c < 0 || c >= C || visited[r, c]) continue;
            if (DigitSum(r) + DigitSum(c) > T) continue;

            visited[r, c] = true;
            count++;

            for (int i = 0; i < 4; i++)
                queue.Enqueue((r + dr[i], c + dc[i]));
        }

        Console.WriteLine(count);
    }
}
