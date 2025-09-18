using System;

class Solution
{
    static void Main(string[] args)
    {
        var board = new char[3][];
        for (int i = 0; i < 3; i++)
            board[i] = Console.ReadLine().ToCharArray();

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                if (board[y][x] != '.') continue;
                board[y][x] = 'O';
                if (IsWin(board, 'O'))
                {
                    for (int i = 0; i < 3; i++)
                        Console.WriteLine(new string(board[i]));
                    return;
                }
                board[y][x] = '.';
            }
        }

        Console.WriteLine("false");
    }

    static bool IsWin(char[][] b, char p)
    {
        for (int i = 0; i < 3; i++)
        {
            if (b[i][0] == p && b[i][1] == p && b[i][2] == p) return true;
            if (b[0][i] == p && b[1][i] == p && b[2][i] == p) return true;
        }
        if (b[0][0] == p && b[1][1] == p && b[2][2] == p) return true;
        if (b[0][2] == p && b[1][1] == p && b[2][0] == p) return true;
        return false;
    }
}
