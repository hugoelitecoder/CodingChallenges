using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var N = int.Parse(Console.ReadLine());
        var C = int.Parse(Console.ReadLine());
        var numbersList = new List<List<int>>();
        for (var i = 0; i < N; i++)
        {
            var numbers = Console.ReadLine().Split(' ').Select(int.Parse).ToList();
            numbersList.Add(numbers);
        }
        var clicksList = new List<List<string>>();
        for (var i = 0; i < N; i++)
        {
            var clicks = Console.ReadLine().Split(' ').ToList();
            clicksList.Add(clicks);
        }
        var attempts = new List<Attempt>();
        for (var i = 0; i < N; i++)
        {
            attempts.Add(new Attempt(numbersList[i], clicksList[i]));
        }
        var cracker = new SafeCracker(C, attempts);
        var result = cracker.Solve();
        Console.WriteLine(result);
    }
}

public enum SoundMeaning
{
    Correct,
    Adjacent,
    Incorrect
}

public class Attempt
{
    public List<int> Numbers { get; }
    public List<string> Sounds { get; }

    public Attempt(List<int> numbers, List<string> sounds)
    {
        Numbers = numbers;
        Sounds = sounds;
    }
}

public class SafeCracker
{
    private readonly int _combinationLength;
    private readonly List<Attempt> _attempts;
    private static readonly List<Dictionary<string, SoundMeaning>> _meaningPermutations = new List<Dictionary<string, SoundMeaning>>
    {
        new Dictionary<string, SoundMeaning> {{"CLICK", SoundMeaning.Correct}, {"CLACK", SoundMeaning.Adjacent}, {"CLUCK", SoundMeaning.Incorrect}},
        new Dictionary<string, SoundMeaning> {{"CLICK", SoundMeaning.Correct}, {"CLACK", SoundMeaning.Incorrect}, {"CLUCK", SoundMeaning.Adjacent}},
        new Dictionary<string, SoundMeaning> {{"CLICK", SoundMeaning.Adjacent}, {"CLACK", SoundMeaning.Correct}, {"CLUCK", SoundMeaning.Incorrect}},
        new Dictionary<string, SoundMeaning> {{"CLICK", SoundMeaning.Adjacent}, {"CLACK", SoundMeaning.Incorrect}, {"CLUCK", SoundMeaning.Correct}},
        new Dictionary<string, SoundMeaning> {{"CLICK", SoundMeaning.Incorrect}, {"CLACK", SoundMeaning.Correct}, {"CLUCK", SoundMeaning.Adjacent}},
        new Dictionary<string, SoundMeaning> {{"CLICK", SoundMeaning.Incorrect}, {"CLACK", SoundMeaning.Adjacent}, {"CLUCK", SoundMeaning.Correct}}
    };

    public SafeCracker(int combinationLength, List<Attempt> attempts)
    {
        _combinationLength = combinationLength;
        _attempts = attempts;
    }

    public string Solve()
    {
        var allFoundSolutions = new HashSet<string>();
        foreach (var assumption in _meaningPermutations)
        {
            var solutionsForAssumption = TestAssumption(assumption);
            foreach (var solution in solutionsForAssumption)
            {
                allFoundSolutions.Add(solution);
            }
        }
        if (allFoundSolutions.Count == 1)
        {
            return allFoundSolutions.First();
        }
        return "FLEE";
    }

    private HashSet<string> TestAssumption(Dictionary<string, SoundMeaning> assumption)
    {
        var possibleDigitsPerPosition = new List<HashSet<int>>();
        for (var i = 0; i < _combinationLength; i++)
        {
            possibleDigitsPerPosition.Add(new HashSet<int>(Enumerable.Range(0, 10)));
        }
        foreach (var attempt in _attempts)
        {
            for (var i = 0; i < _combinationLength; i++)
            {
                var guessedNum = attempt.Numbers[i];
                var sound = attempt.Sounds[i];
                var meaning = assumption[sound];
                var possibilitiesFromGuess = GetPossibleDigits(guessedNum, meaning);
                possibleDigitsPerPosition[i].IntersectWith(possibilitiesFromGuess);
            }
        }
        if (possibleDigitsPerPosition.Any(p => p.Count == 0))
        {
            return new HashSet<string>();
        }
        return GenerateCombinations(possibleDigitsPerPosition);
    }

    private HashSet<int> GetPossibleDigits(int guessedNum, SoundMeaning meaning)
    {
        switch (meaning)
        {
            case SoundMeaning.Correct:
                return new HashSet<int> { guessedNum };
            case SoundMeaning.Adjacent:
                return new HashSet<int> { (guessedNum + 9) % 10, (guessedNum + 1) % 10 };
            case SoundMeaning.Incorrect:
                var allDigits = new HashSet<int>(Enumerable.Range(0, 10));
                allDigits.Remove(guessedNum);
                allDigits.Remove((guessedNum + 9) % 10);
                allDigits.Remove((guessedNum + 1) % 10);
                return allDigits;
            default:
                return new HashSet<int>();
        }
    }

    private HashSet<string> GenerateCombinations(List<HashSet<int>> possibleDigits)
    {
        var solutions = new HashSet<string>();
        var currentCombination = new List<int>();
        GenerateCombinationsRecursive(0, currentCombination, possibleDigits, solutions);
        return solutions;
    }

    private void GenerateCombinationsRecursive(int position, List<int> currentCombination, List<HashSet<int>> possibleDigits, HashSet<string> solutions)
    {
        if (position == _combinationLength)
        {
            solutions.Add(string.Join(" ", currentCombination));
            return;
        }
        foreach (var digit in possibleDigits[position])
        {
            currentCombination.Add(digit);
            GenerateCombinationsRecursive(position + 1, currentCombination, possibleDigits, solutions);
            currentCombination.RemoveAt(currentCombination.Count - 1);
        }
    }
}

