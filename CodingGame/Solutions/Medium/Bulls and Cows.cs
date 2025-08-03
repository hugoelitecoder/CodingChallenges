using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Solution
{
    public static void Main(string[] args)
    {
        var guessCount = int.Parse(Console.ReadLine());
        var guesses = ParseGuesses(guessCount);

        var sw = Stopwatch.StartNew();

        for (var num = 0; num < 10000; num++)
        {
            var candidateDigits = new int[4];
            candidateDigits[0] = num / 1000;
            candidateDigits[1] = (num / 100) % 10;
            candidateDigits[2] = (num / 10) % 10;
            candidateDigits[3] = num % 10;

            if (IsConsistent(candidateDigits, guesses))
            {
                Console.WriteLine($"{candidateDigits[0]}{candidateDigits[1]}{candidateDigits[2]}{candidateDigits[3]}");
                break;
            }
        }

        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] Solved in {sw.Elapsed.TotalMilliseconds:F4} ms");
    }

    private static List<(int[] Digits, int Bulls, int Cows)> ParseGuesses(int count)
    {
        var parsedGuesses = new List<(int[], int, int)>(count);
        for (var i = 0; i < count; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            var digits = new int[4];
            for (var j = 0; j < 4; j++)
            {
                digits[j] = parts[0][j] - '0';
            }
            parsedGuesses.Add((digits, int.Parse(parts[1]), int.Parse(parts[2])));
        }
        return parsedGuesses;
    }

    private static bool IsConsistent(int[] candidate, List<(int[] Digits, int Bulls, int Cows)> guesses)
    {
        foreach (var guessInfo in guesses)
        {
            var (bulls, cows) = Compare(candidate, guessInfo.Digits);
            if (bulls != guessInfo.Bulls || cows != guessInfo.Cows)
            {
                return false;
            }
        }
        return true;
    }

    private static (int bulls, int cows) Compare(int[] candidate, int[] guess)
    {
        var bulls = 0;
        var candidateCounts = new int[10];
        var guessCounts = new int[10];

        for (var i = 0; i < 4; i++)
        {
            if (candidate[i] == guess[i])
            {
                bulls++;
            }
            candidateCounts[candidate[i]]++;
            guessCounts[guess[i]]++;
        }

        var common = 0;
        for (var i = 0; i < 10; i++)
        {
            common += Math.Min(candidateCounts[i], guessCounts[i]);
        }

        var cows = common - bulls;
        return (bulls, cows);
    }
}