using System;
using System.Linq;

class Solution
{
    static int D, B;
    static int[] leaves;
    static long nodeCount;
    static int[] blockSize;

    static void Main()
    {
        var tokens = Console.ReadLine().Split().Select(int.Parse).ToArray();
        D = tokens[0];
        B = tokens[1];
        leaves = Console.ReadLine().Split().Select(int.Parse).ToArray();

        nodeCount = 0;
        blockSize = new int[D];
        for (int d = 0; d < D; d++)
            blockSize[d] = (int)Math.Pow(B, D - d - 1);

        var best = Minimax(0, 0, int.MinValue, int.MaxValue);
        Console.WriteLine($"{best} {nodeCount}");
    }

    static int Minimax(int depth, int leafStart, int alpha, int beta)
    {
        nodeCount++;
        if (depth == D)
            return leaves[leafStart];

        bool maximize = (depth % 2 == 0);
        int value = maximize ? int.MinValue : int.MaxValue;
        for (int i = 0; i < B; i++)
        {
            int childStart = leafStart + i * blockSize[depth];
            int childValue = Minimax(depth + 1, childStart, alpha, beta);
            if (maximize)
            {
                value = Math.Max(value, childValue);
                alpha = Math.Max(alpha, value);
            }
            else
            {
                value = Math.Min(value, childValue);
                beta = Math.Min(beta, value);
            }
            if (alpha >= beta)
                break;
        }
        return value;
    }
}