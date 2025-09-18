using System;
using System.Linq;

public class Solution
{
    public static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var n = int.Parse(inputs[0]);
        var g = int.Parse(inputs[1]);
        var rows = new string[n];
        for (var i = 0; i < n; i++)
        {
            rows[i] = Console.ReadLine();
        }

        var game = new TicTacToeGame(n, g, rows);
        var analysis = game.Analyze();

        var outputBoard = game.Board;
        if (analysis.Path != null)
        {
            outputBoard = CreateFormattedWinningBoard(game.Board, analysis.Path, g);
        }

        var statusMessage = GetStatusMessage(analysis.Status);

        for (var i = 0; i < n; i++)
        {
            Console.WriteLine(new string(outputBoard[i]));
        }
        Console.WriteLine(statusMessage);
    }

    private static string GetStatusMessage(GameStatus status)
    {
        return status switch
        {
            GameStatus.WinnerX => "The winner is X.",
            GameStatus.WinnerO => "The winner is O.",
            GameStatus.Draw => "The game ended in a draw!",
            GameStatus.Ongoing => "The game isn't over yet!",
            _ => ""
        };
    }

    private static char[][] CreateFormattedWinningBoard(char[][] originalBoard, WinInfo path, int g)
    {
        var n = originalBoard.Length;
        var newBoard = new char[n][];
        for (var i = 0; i < n; i++)
        {
            newBoard[i] = (char[])originalBoard[i].Clone();
        }

        var (mark, dr, dc) = path.Direction switch
        {
            WinDirection.Horizontal => ('-', 0, 1),
            WinDirection.Vertical => ('|', 1, 0),
            WinDirection.Diagonal => ('\\', 1, 1),
            WinDirection.AntiDiagonal => ('/', -1, 1),
            _ => throw new InvalidOperationException("Unknown direction.")
        };

        for (var i = 0; i < g; i++)
        {
            newBoard[path.StartRow + i * dr][path.StartCol + i * dc] = mark;
        }

        return newBoard;
    }
}

internal enum GameStatus { Ongoing, Draw, WinnerX, WinnerO }
internal enum WinDirection { Horizontal, Vertical, Diagonal, AntiDiagonal }
internal record WinInfo(char Player, int StartRow, int StartCol, WinDirection Direction);

internal class GameAnalysis
{
    public GameStatus Status { get; }
    public WinInfo Path { get; }

    public GameAnalysis(GameStatus status, WinInfo path = null)
    {
        Status = status;
        Path = path;
    }
}

internal class TicTacToeGame
{
    private readonly int _n;
    private readonly int _g;
    private readonly char[][] _board;
    private readonly bool _hasEmptyCells;

    public char[][] Board => _board;

    public TicTacToeGame(int n, int g, string[] rows)
    {
        _n = n;
        _g = g;
        _board = new char[n][];
        var hasEmpty = false;
        for (var i = 0; i < n; i++)
        {
            _board[i] = rows[i].ToCharArray();
            if (!hasEmpty && Array.IndexOf(_board[i], ' ') != -1)
            {
                hasEmpty = true;
            }
        }
        _hasEmptyCells = hasEmpty;
    }

    public GameAnalysis Analyze()
    {
        var winInfo = FindWinner();
        if (winInfo != null)
        {
            var status = winInfo.Player == 'X' ? GameStatus.WinnerX : GameStatus.WinnerO;
            return new GameAnalysis(status, winInfo);
        }
        var finalStatus = _hasEmptyCells ? GameStatus.Ongoing : GameStatus.Draw;
        return new GameAnalysis(finalStatus);
    }

    private WinInfo FindWinner()
    {
        for (var r = 0; r < _n; r++)
        {
            for (var c = 0; c < _n; c++)
            {
                var player = _board[r][c];
                if (player == ' ') continue;

                if (c + _g <= _n && IsWinningLine(r, c, 0, 1, player))
                    return new WinInfo(player, r, c, WinDirection.Horizontal);

                if (r + _g <= _n && IsWinningLine(r, c, 1, 0, player))
                    return new WinInfo(player, r, c, WinDirection.Vertical);

                if (r + _g <= _n && c + _g <= _n && IsWinningLine(r, c, 1, 1, player))
                    return new WinInfo(player, r, c, WinDirection.Diagonal);

                if (r - _g + 1 >= 0 && c + _g <= _n && IsWinningLine(r, c, -1, 1, player))
                    return new WinInfo(player, r, c, WinDirection.AntiDiagonal);
            }
        }
        return null;
    }

    private bool IsWinningLine(int startR, int startC, int dr, int dc, char player)
    {
        for (var i = 1; i < _g; i++)
        {
            if (_board[startR + i * dr][startC + i * dc] != player)
            {
                return false;
            }
        }
        return true;
    }
}