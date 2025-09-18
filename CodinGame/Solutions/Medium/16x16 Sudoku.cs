using System;
using System.Collections.Generic;
using System.Numerics;

class Program
{
    const int N     = 4;
    const int SIZE  = N * N;
    const int ALL   = (1 << SIZE) - 1;
    static int[] rowMask   = new int[SIZE];
    static int[] colMask   = new int[SIZE];
    static int[] boxMask   = new int[SIZE];
    static char[,] solution = new char[SIZE, SIZE];
    static List<int> empties = new List<int>(SIZE * SIZE);

    static void Main()
    {
        for (int i = 0; i < SIZE; i++)
        {
            rowMask[i] = ALL;
            colMask[i] = ALL;
            boxMask[i] = ALL;
        }
        empties.Clear();

        for (int i = 0; i < SIZE; i++)
        {
            string line = Console.ReadLine();
            for (int j = 0; j < SIZE; j++)
            {
                char ch = line[j];
                if (ch == '.')
                {
                    solution[i, j] = '.';
                    empties.Add(i * SIZE + j);
                }
                else
                {
                    solution[i, j] = ch;
                    int v   = ch - 'A';
                    int bit = 1 << v;
                    rowMask[i]       &= ~bit;
                    colMask[j]       &= ~bit;
                    int b            = (i / N) * N + (j / N);
                    boxMask[b]       &= ~bit;
                }
            }
        }

        Backtrack();

        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
                Console.Write(solution[i, j]);
            Console.WriteLine();
        }
    }

    static bool Backtrack()
    {
        if (empties.Count == 0)
            return true;

        int bestIndex = -1, bestCount = int.MaxValue, bestMask = 0;
        for (int k = 0; k < empties.Count; k++)
        {
            int p = empties[k];
            int r = p / SIZE, c = p % SIZE;
            int b = (r / N) * N + (c / N);
            int mask = rowMask[r] & colMask[c] & boxMask[b];
            int count = BitOperations.PopCount((uint)mask);
            if (count == 0)  return false;
            if (count < bestCount)
            {
                bestCount = count;
                bestMask  = mask;
                bestIndex = k;
                if (count == 1) break;
            }
        }

        int pos = empties[bestIndex];
        empties.RemoveAt(bestIndex);
        int x = pos / SIZE, y = pos % SIZE;
        int box = (x / N) * N + (y / N);

        int maskIter = bestMask;
        while (maskIter != 0)
        {
            int bit = maskIter & -maskIter;
            maskIter &= ~bit;
            int v = BitOperations.TrailingZeroCount((uint)bit);
            solution[x, y] = (char)('A' + v);
            rowMask[x]   &= ~bit;
            colMask[y]   &= ~bit;
            boxMask[box] &= ~bit;

            if (Backtrack())
                return true;

            rowMask[x]   |= bit;
            colMask[y]   |= bit;
            boxMask[box] |= bit;
            solution[x, y] = '.';
        }

        empties.Insert(bestIndex, pos);
        return false;
    }
}
