using System;
class Solution
{
    public static void Main(string[] args)
    {
        var N = int.Parse(Console.ReadLine());
        var grid = GenerateSpiral(N);
        var maxNum = N * N;
        var primes = Sieve(maxNum);
        for (var i = 0; i < N; i++)
        {
            for (var j = 0; j < N; j++)
            {
                var ch = primes[grid[i, j]] ? '#' : ' ';
                Console.Write(ch);
                if (j < N - 1) Console.Write(' ');
            }
            Console.WriteLine();
        }
    }
    private static int[,] GenerateSpiral(int N)
    {
        var grid = new int[N, N];
        var x = N / 2;
        var y = N / 2;
        var num = 1;
        var len = 1;
        grid[x, y] = num;
        var dx = new int[] { 0, -1, 0, 1 };
        var dy = new int[] { 1, 0, -1, 0 };
        while (num < N * N)
        {
            for (var dir = 0; dir < 4; dir++)
            {
                for (var s = 0; s < len && num < N * N; s++)
                {
                    x += dx[dir];
                    y += dy[dir];
                    num++;
                    grid[x, y] = num;
                }
                if (dir % 2 == 1) len++;
            }
        }
        return grid;
    }
    private static bool[] Sieve(int max)
    {
        var isPrime = new bool[max + 1];
        if (max >= 2)
        {
            for (var i = 2; i <= max; i++) isPrime[i] = true;
            var limit = (int)Math.Sqrt(max);
            for (var p = 2; p <= limit; p++)
            {
                if (!isPrime[p]) continue;
                for (var q = p * p; q <= max; q += p) isPrime[q] = false;
            }
        }
        return isPrime;
    }
}
