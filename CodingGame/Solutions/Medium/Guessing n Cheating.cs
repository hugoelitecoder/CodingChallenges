using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        var rounds = new List<Round>(n);

        for (int i = 0; i < n; i++)
        {
            var tokens = Console.ReadLine()
                                .Trim()
                                .Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            int guess = int.Parse(tokens[0]);
            string responseToken = tokens[2];
            rounds.Add(new Round(guess, responseToken));
        }

        int cheatRound = new GuessingGame().FindCheatRound(rounds);

        if (cheatRound > 0)
            Console.WriteLine($"Alice cheated in round {cheatRound}");
        else
            Console.WriteLine("No evidence of cheating");
    }
}

class GuessingGame
{
    public int FindCheatRound(List<Round> rounds)
    {
        bool[] possible = InitializePossible();

        for (int i = 0; i < rounds.Count; i++)
        {
            var round = rounds[i];

            if (round.IsTooLow)
            {
                EliminateUpTo(possible, round.Number);
                if (!AnyRemaining(possible, round.Number + 1, 100))
                    return i + 1;
            }
            else if (round.IsTooHigh)
            {
                EliminateFrom(possible, round.Number);
                if (!AnyRemaining(possible, 1, round.Number - 1))
                    return i + 1;
            }
            else
            {
                if (round.Number < 1 || round.Number > 100 || !possible[round.Number])
                    return i + 1;
                else
                    return 0;
            }
        }

        return 0;
    }

    private bool[] InitializePossible()
    {
        var arr = new bool[101];
        for (int i = 1; i <= 100; i++)
            arr[i] = true;
        return arr;
    }

    private void EliminateUpTo(bool[] arr, int max)
    {
        for (int i = 0; i <= max; i++)
            arr[i] = false;
    }

    private void EliminateFrom(bool[] arr, int min)
    {
        for (int i = min; i < arr.Length; i++)
            arr[i] = false;
    }

    private bool AnyRemaining(bool[] arr, int start, int end)
    {
        for (int i = start; i <= end; i++)
            if (i >= 1 && i < arr.Length && arr[i])
                return true;
        return false;
    }
}

class Round
{
    public int Number { get; }
    public bool IsTooLow { get; }
    public bool IsTooHigh { get; }
    public bool IsRightOn => !IsTooLow && !IsTooHigh;

    public Round(int number, string token)
    {
        Number = number;
        IsTooLow = token == "low";
        IsTooHigh = token == "high";
    }
}
