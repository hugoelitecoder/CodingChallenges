using System;
using System.Diagnostics;

class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var nColors = int.Parse(Console.ReadLine());
        var nColumns = int.Parse(Console.ReadLine());
        var nLines = int.Parse(Console.ReadLine());
        var guesses = new Guess[nLines];
        for (var i = 0; i < nLines; i++)
        {
            var s = Console.ReadLine().Split(' ');
            var code = new int[nColumns];
            for (var j = 0; j < nColumns; j++) code[j] = s[0][j] - '0';
            var black = int.Parse(s[1]);
            var white = int.Parse(s[2]);
            guesses[i] = new Guess { Colors = code, Black = black, White = white };
        }
        var possible = new bool[nColumns, nColors];
        for (var i = 0; i < nColumns; i++)
            for (var c = 0; c < nColors; c++)
                possible[i, c] = true;
        for (var i = 0; i < nLines; i++)
            if (guesses[i].Black == 0)
                for (var j = 0; j < nColumns; j++)
                    possible[j, guesses[i].Colors[j]] = false;
        var sol = new int[nColumns];
        _recursions = 0;
        _backtracks = 0;
        if (!Solve(0, sol, possible, guesses, nColors, nColumns, nLines)) {
            Console.Error.WriteLine("No solution found");
        }
        Console.Error.WriteLine("Recursions: " + _recursions);
        Console.Error.WriteLine("Backtracks: " + _backtracks);
        Console.Error.WriteLine("Time: " + sw.ElapsedMilliseconds + " ms");
    }

    static int _recursions;
    static int _backtracks;

    static bool Solve(int idx, int[] sol, bool[,] possible, Guess[] guesses, int nColors, int nColumns, int nLines)
    {
        _recursions++;
        if (idx == nColumns)
        {
            for (var i = 0; i < nLines; i++)
                if (!Match(sol, guesses[i].Colors, guesses[i].Black, guesses[i].White, nColumns))
                    return false;
            for (var i = 0; i < nColumns; i++) Console.Write(sol[i]);
            Console.WriteLine();
            return true;
        }
        for (var c = 0; c < nColors; c++)
        {
            if (!possible[idx, c]) continue;
            sol[idx] = c;
            if (CanBe(sol, idx, guesses, nLines, nColumns))
            {
                if (Solve(idx + 1, sol, possible, guesses, nColors, nColumns, nLines)) return true;
            }
            else
            {
                _backtracks++;
            }
        }
        return false;
    }

    static bool CanBe(int[] sol, int idx, Guess[] guesses, int nLines, int nColumns)
    {
        for (var g = 0; g < nLines; g++)
        {
            var guess = guesses[g].Colors;
            var wantBlack = guesses[g].Black;
            var wantWhite = guesses[g].White;
            var currBlack = 0;
            var cntSol = new int[10];
            var cntGuess = new int[10];
            for (var i = 0; i <= idx; i++)
                if (sol[i] == guess[i]) currBlack++;
            for (var i = 0; i <= idx; i++)
            {
                cntSol[sol[i]]++;
                cntGuess[guess[i]]++;
            }
            var minColor = 0;
            for (var c = 0; c < 10; c++)
                minColor += Math.Min(cntSol[c], cntGuess[c]);
            var currWhite = minColor - currBlack;
            if (currBlack > wantBlack) return false;
            if (currWhite > wantWhite) return false;
        }
        return true;
    }

    static bool Match(int[] sol, int[] guess, int wantBlack, int wantWhite, int nColumns)
    {
        var usedSol = new bool[nColumns];
        var usedGuess = new bool[nColumns];
        var black = 0;
        for (var i = 0; i < nColumns; i++)
            if (sol[i] == guess[i]) { black++; usedSol[i] = usedGuess[i] = true; }
        if (black != wantBlack) return false;
        var white = 0;
        for (var i = 0; i < nColumns; i++)
        {
            if (usedSol[i]) continue;
            for (var j = 0; j < nColumns; j++)
            {
                if (usedGuess[j]) continue;
                if (sol[i] == guess[j])
                {
                    white++; usedGuess[j] = true; break;
                }
            }
        }
        return white == wantWhite;
    }

    class Guess
    {
        public int[] Colors;
        public int Black;
        public int White;
    }
}
