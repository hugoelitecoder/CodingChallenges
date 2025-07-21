using System;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());

        for (int i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split();
            int rows = int.Parse(parts[0]);
            int cols = int.Parse(parts[1]);
            int isWhite = int.Parse(parts[2]);

            Console.WriteLine(CountFast(rows, cols, isWhite));
        }
    }

    static long CountFast(int rows, int cols, int isWhite)
    {
        int boardRows = rows - 7;
        int boardCols = cols - 7;
        long totalBoards = (long)boardRows * boardCols;
        long halfBoards = totalBoards / 2;

        if ((boardRows % 2 == 0) || (boardCols % 2 == 0))
            return halfBoards;
        else
            return halfBoards + (isWhite == 1 ? 1 : 0);
    }
}
