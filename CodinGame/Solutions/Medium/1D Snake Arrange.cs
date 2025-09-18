using System;
using System.Linq;

class Solution
{
    static string pattern;
    static int N;
    static int S;
    static int[] snakes;
    static long[,] memo;
    
    static void Main()
    {
        int nCases = int.Parse(Console.ReadLine());
        while (nCases-- > 0)
        {
            var line = Console.ReadLine();
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            pattern = parts[0];
            N = pattern.Length;
            
            snakes = parts[1]
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray();
            S = snakes.Length;
            
            memo = new long[N+1, S+1];
            for (int i = 0; i <= N; i++)
                for (int j = 0; j <= S; j++)
                    memo[i,j] = -1;
            
            long res = CountWays(0, 0);
            Console.WriteLine(res);
        }
    }
    
    static long CountWays(int pos, int idx)
    {
        if (idx == S)
        {
            for (int i = pos; i < N; i++)
                if (pattern[i] == '#')
                    return 0;
            return 1;
        }
        if (pos >= N) return 0;
        
        long m = memo[pos, idx];
        if (m >= 0) return m;
        long ways = 0;
        
        char c = pattern[pos];
        
        if (c == '.' || c == '?')
        {
            ways += CountWays(pos+1, idx);
        }
        
        if ((c == '#' || c == '?'))
        {
            int L = snakes[idx];
            if (pos + L <= N)
            {
                bool ok = true;
                for (int k = pos; k < pos+L; k++)
                {
                    char pc = pattern[k];
                    if (pc == '.') { ok = false; break; }
                }
                if (ok)
                {
                    int nextIdx = idx + 1;
                    int end = pos + L;
                    if (nextIdx < S)
                    {
                        if (end < N && pattern[end] != '#')
                        {
                            ways += CountWays(end+1, nextIdx);
                        }
                    }
                    else
                    {
                        ways += CountWays(end, nextIdx);
                    }
                }
            }
        }
        
        memo[pos, idx] = ways;
        return ways;
    }
}
