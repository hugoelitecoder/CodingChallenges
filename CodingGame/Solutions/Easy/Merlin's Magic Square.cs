using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static readonly Dictionary<int, int[]> buttonMap = new()
    {
        { 1, new[] {1, 2, 4, 5} },
        { 2, new[] {1, 2, 3} },
        { 3, new[] {2, 3, 5, 6} },
        { 4, new[] {1, 4, 7} },
        { 5, new[] {2, 4, 5, 6, 8} },
        { 6, new[] {3, 6, 9} },
        { 7, new[] {4, 5, 7, 8} },
        { 8, new[] {7, 8, 9} },
        { 9, new[] {5, 6, 8, 9} }
    };

    static void Main()
    {
        var board = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            board.AddRange(Console.ReadLine().Split().Select(c => c == "*" ? 1 : 0));
        }

        string presses = Console.ReadLine();
        foreach (char ch in presses)
        {
            board = Press(board, ch - '0');
        }

        var solved = new List<int> { 1, 1, 1, 1, 0, 1, 1, 1, 1 };

        for (int i = 1; i <= 9; i++)
        {
            if (Press(new List<int>(board), i).SequenceEqual(solved))
            {
                Console.WriteLine(i);
                return;
            }
        }
    }

    static List<int> Press(List<int> board, int button)
    {
        foreach (var pos in buttonMap[button])
        {
            board[pos - 1] ^= 1;
        }
        return board;
    }
}
