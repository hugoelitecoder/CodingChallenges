using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Solution
{
    public static void Main(string[] args)
    {
        var engineSide = Console.ReadLine()![0];
        var boardChars = new char[3, 3];
        for (var i = 0; i < 3; i++)
        {
            var line = Console.ReadLine()!;
            for (var j = 0; j < 3; j++) boardChars[i, j] = line[j];
        }
        var stopwatch = Stopwatch.StartNew();
        var game = new TicTacToeEngine(engineSide, boardChars);
        var resultBoard = game.ComputeBestMove();
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++) Console.Write(resultBoard[i, j]);
            Console.WriteLine();
        }
        stopwatch.Stop();
        Console.Error.WriteLine($"[DEBUG] --- TIC-TAC-TOE STATS ---");
        Console.Error.WriteLine($"[DEBUG] Engine Side: {engineSide}");
        Console.Error.WriteLine($"[DEBUG] Execution Time: {stopwatch.ElapsedMilliseconds}ms");
    }
}

public record Move(int R, int C, int Value);
public class TicTacToeEngine
{
    private readonly char _engine;
    private readonly char _opponent;
    private readonly char[,] _board;
    private static readonly int[,] _cellValues = { { 8, 4, 7 }, { 3, 9, 2 }, { 6, 1, 5 } };
    public TicTacToeEngine(char engine, char[,] board)
    {
        _engine = engine;
        _opponent = engine == 'X' ? 'O' : 'X';
        _board = board;
    }
    public char[,] ComputeBestMove()
    {
        if (CheckWin(_board, 'X') || CheckWin(_board, 'O') || IsFull(_board)) return _board;
        var bestScore = int.MinValue;
        var bestMove = new Move(-1, -1, -1);
        for (var r = 0; r < 3; r++)
        {
            for (var c = 0; c < 3; c++)
            {
                if (_board[r, c] == '.')
                {
                    _board[r, c] = _engine;
                    var score = Minimax(_board, 0, false);
                    _board[r, c] = '.';
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = new Move(r, c, _cellValues[r, c]);
                    }
                    else if (score == bestScore && _cellValues[r, c] > bestMove.Value)
                    {
                        bestMove = new Move(r, c, _cellValues[r, c]);
                    }
                }
            }
        }
        if (bestMove.R != -1) _board[bestMove.R, bestMove.C] = _engine;
        return _board;
    }
    private int Minimax(char[,] board, int depth, bool isMaximizing)
    {
        if (CheckWin(board, _engine)) return 100 - depth;
        if (CheckWin(board, _opponent)) return depth - 100;
        if (IsFull(board)) return 0;
        if (isMaximizing)
        {
            var bestScore = int.MinValue;
            for (var r = 0; r < 3; r++)
            {
                for (var c = 0; c < 3; c++)
                {
                    if (board[r, c] == '.')
                    {
                        board[r, c] = _engine;
                        var score = Minimax(board, depth + 1, false);
                        board[r, c] = '.';
                        bestScore = Math.Max(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
        else
        {
            var bestScore = int.MaxValue;
            for (var r = 0; r < 3; r++)
            {
                for (var c = 0; c < 3; c++)
                {
                    if (board[r, c] == '.')
                    {
                        board[r, c] = _opponent;
                        var score = Minimax(board, depth + 1, true);
                        board[r, c] = '.';
                        bestScore = Math.Min(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
    }
    private bool CheckWin(char[,] b, char p)
    {
        for (var i = 0; i < 3; i++)
        {
            if (b[i, 0] == p && b[i, 1] == p && b[i, 2] == p) return true;
            if (b[0, i] == p && b[1, i] == p && b[2, i] == p) return true;
        }
        if (b[0, 0] == p && b[1, 1] == p && b[2, 2] == p) return true;
        if (b[0, 2] == p && b[1, 1] == p && b[2, 0] == p) return true;
        return false;
    }
    private bool IsFull(char[,] b)
    {
        foreach (var cell in b) if (cell == '.') return false;
        return true;
    }
}