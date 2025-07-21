using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var seq = Console.ReadLine().Trim();
        double[] weightSum = new double[26];
        var stack = new Stack<char>();

        int i = 0, n = seq.Length;
        while (i < n)
        {
            if (seq[i] == '-')
            {
                char closing = seq[i + 1];
                stack.Pop();
                i += 2;
            }
            else
            {
                char open = seq[i];
                int depth = stack.Count + 1;
                weightSum[open - 'a'] += 1.0 / depth;
                stack.Push(open);
                i++;
            }
        }

        double best = -1;
        char answer = 'a';
        for (int c = 0; c < 26; c++)
        {
            if (weightSum[c] > best + 1e-12)
            {
                best = weightSum[c];
                answer = (char)('a' + c);
            }
        }

        Console.WriteLine(answer);
    }
}
