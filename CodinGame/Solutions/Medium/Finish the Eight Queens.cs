using System;
using System.Numerics;

class Program
{
    const int N = 8;
    static int[] initialPos = new int[N];    // -1 or fixed column
    static int[] solution   = new int[N];
    static int columnsMask = 0;
    static int diag1Mask   = 0;  // r - c + (N-1)
    static int diag2Mask   = 0;  // r + c
    static bool solved     = false;

    static void DFS(int row)
    {
        if (solved) return;
        if (row == N)
        {
            PrintSolution();
            solved = true;
            return;
        }

        // If a queen is pre-placed in this row
        if (initialPos[row] != -1)
        {
            int col = initialPos[row];
            int bit = 1 << col;
            int d1  = row - col + (N - 1);
            int d2  = row + col;

            // Conflict check
            if ((columnsMask & bit) != 0 || (diag1Mask & (1 << d1)) != 0 || (diag2Mask & (1 << d2)) != 0)
                return;

            // Place and recurse
            columnsMask |= bit;
            diag1Mask   |= 1 << d1;
            diag2Mask   |= 1 << d2;
            solution[row] = col;

            DFS(row + 1);

            // Backtrack
            columnsMask ^= bit;
            diag1Mask   ^= 1 << d1;
            diag2Mask   ^= 1 << d2;
        }
        else
        {
            // All available columns
            int freeCols = ((1 << N) - 1) & ~columnsMask;
            while (freeCols != 0 && !solved)
            {
                int bit = freeCols & -freeCols;       // lowest set bit
                freeCols &= freeCols - 1;             // clear it
                int col = BitOperations.TrailingZeroCount(bit);
                int d1  = row - col + (N - 1);
                int d2  = row + col;

                if (((diag1Mask >> d1) & 1) != 0 || ((diag2Mask >> d2) & 1) != 0)
                    continue;

                // Place
                columnsMask |= bit;
                diag1Mask   |= 1 << d1;
                diag2Mask   |= 1 << d2;
                solution[row] = col;

                DFS(row + 1);

                // Backtrack
                columnsMask ^= bit;
                diag1Mask   ^= 1 << d1;
                diag2Mask   ^= 1 << d2;
            }
        }
    }

    static void PrintSolution()
    {
        for (int r = 0; r < N; r++)
        {
            for (int c = 0; c < N; c++)
                Console.Write(solution[r] == c ? 'Q' : '.');
            Console.WriteLine();
        }
    }

    static void Main()
    {
        for (int r = 0; r < N; r++)
        {
            string line = Console.ReadLine().Trim();
            initialPos[r] = -1;
            for (int c = 0; c < N; c++)
                if (line[c] == 'Q') initialPos[r] = c;
        }

        DFS(0);
    }
}