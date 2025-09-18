using System;

class Solution
{
    static void Main(string[] args)
    {
        var a = Console.ReadLine();
        var b = Console.ReadLine();
        var solver = new Levenshtein();
        Console.WriteLine(solver.Compute(a, b));
    }

    class Levenshtein
    {
        public int Compute(string a, string b)
        {
            var lenA = a.Length;
            var lenB = b.Length;
            var dp = new int[lenA + 1, lenB + 1];
            for (var i = 0; i <= lenA; i++) dp[i, 0] = i;
            for (var j = 0; j <= lenB; j++) dp[0, j] = j;
            for (var i = 1; i <= lenA; i++)
            {
                for (var j = 1; j <= lenB; j++)
                {
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
                }
            }
            return dp[lenA, lenB];
        }
    }
}
