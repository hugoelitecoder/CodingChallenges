using System;
using System.Globalization;

class Solution
{
    static void Main()
    {
        int K = int.Parse(Console.ReadLine());
        var inputs = Console.ReadLine().Split();
        var results = new int[K];

        for (int i = 0; i < K; i++)
        {
            double A = double.Parse(inputs[i], CultureInfo.InvariantCulture);
            double logA = Math.Log(A);
            double logFact = 0;
            int N = 1;
            while (true)
            {
                logFact += Math.Log(N);
                if (N * logA < logFact)
                {
                    results[i] = N;
                    break;
                }
                N++;
            }
        }

        Console.WriteLine(string.Join(" ", results));
    }
}
