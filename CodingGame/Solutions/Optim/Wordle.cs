using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Solution
{
    public static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();
        var wordCount = int.Parse(Console.ReadLine());
        var wordsLine = Console.ReadLine();
        var allWords = ParseWords(wordCount, wordsLine);
        var solver = new WordleSolver(allWords);
        string lastGuess = "";
        var turnNumber = 1;
        while (true)
        {
            var turnStartMs = timer.ElapsedMilliseconds;
            var timeLimit = turnNumber == 1 ? 980 : 46;
            var turnDeadlineMs = turnStartMs + timeLimit;
            var states = ReadTurnInput();
            if (turnNumber > 1) solver.ProcessFeedback(lastGuess, states);
            var nextGuess = solver.GetNextGuess(timer, turnDeadlineMs, turnNumber);
            Console.WriteLine(nextGuess);
            lastGuess = nextGuess;
            turnNumber++;
        }
    }

    private static List<string> ParseWords(int wordCount, string line)
    {
        var words = new List<string>(wordCount);
        if (string.IsNullOrEmpty(line)) return words;
        var start = 0;
        for (var i = 0; i < line.Length; i++)
        {
            if (line[i] == ' ')
            {
                if (i > start) words.Add(line.Substring(start, i - start).ToUpper());
                start = i + 1;
            }
        }
        if (start < line.Length) words.Add(line.Substring(start).ToUpper());
        return words;
    }

    private static int[] ReadTurnInput()
    {
        var line = Console.ReadLine();
        var result = new int[6];
        if (!string.IsNullOrEmpty(line))
        {
            var inputs = line.Split(' ');
            for (var i = 0; i < 6 && i < inputs.Length; i++)
                int.TryParse(inputs[i], out result[i]);
        }
        return result;
    }
}

class WordleSolver
{
    private readonly int _wordLength;
    private readonly string[] _allWords;
    private List<int> _candidateIndices;
    private const int GREEDY_STRATEGY_THRESHOLD = 20;

    public WordleSolver(List<string> allWords)
    {
        _wordLength = allWords[0].Length;
        _allWords = allWords.Select(w => w.ToUpper()).ToArray();
        _candidateIndices = Enumerable.Range(0, _allWords.Length).ToList();
    }

    public string GetSafeFallback() => _candidateIndices.Count == 0 ? _allWords[0] : _allWords[_candidateIndices[0]];

    public void ProcessFeedback(string lastGuess, int[] states)
    {
        int obs = StatesToInt(states);
        var newCandidates = new List<int>(_candidateIndices.Count);
        foreach (var index in _candidateIndices)
            if (IsConsistent(_allWords[index], lastGuess, obs))
                newCandidates.Add(index);
        _candidateIndices = newCandidates;
    }

    public string GetNextGuess(Stopwatch timer, long turnDeadlineMs, int turnNumber)
    {
        if (turnNumber == 1)
        {
            return "SALETI";
        }

        if (_candidateIndices.Count <= 2)
        {
            return _allWords[_candidateIndices[0]];
        }

        if (_candidateIndices.Count < GREEDY_STRATEGY_THRESHOLD)
        {
            return FindBestGuessByPositionalHeuristic(timer, turnDeadlineMs);
        }
        else
        {
            return FindBestGuessMinimax(timer, turnDeadlineMs);
        }
    }

    private string FindBestGuessByPositionalHeuristic(Stopwatch timer, long turnDeadlineMs)
    {
        var freq = new int[_wordLength, 26];
        foreach (var index in _candidateIndices)
        {
            var word = _allWords[index];
            for (var i = 0; i < _wordLength; i++)
                freq[i, word[i] - 'A']++;
        }

        string bestGuess = "";
        int maxScore = -1;

        foreach (var index in _candidateIndices)
        {
            if (timer.ElapsedMilliseconds >= turnDeadlineMs - 2) break;

            var candidateWord = _allWords[index];
            var currentScore = 0;
            var seen = new bool[26];
            for (var i = 0; i < _wordLength; i++)
            {
                var c = candidateWord[i];
                if (!seen[c - 'A'])
                {
                    currentScore += freq[i, c - 'A'];
                    seen[c - 'A'] = true;
                }
            }
            if (currentScore > maxScore)
            {
                maxScore = currentScore;
                bestGuess = candidateWord;
            }
        }
        return string.IsNullOrEmpty(bestGuess) ? GetSafeFallback() : bestGuess;
    }

    private string FindBestGuessMinimax(Stopwatch timer, long turnDeadlineMs)
    {
        string bestGuess = _allWords[_candidateIndices[0]];
        int minWorstCase = _candidateIndices.Count;
        bool isBestGuessCandidate = true;

        var candidateSet = new HashSet<int>(_candidateIndices);

        for (int i = 0; i < _allWords.Length; i++)
        {
            if (timer.ElapsedMilliseconds >= turnDeadlineMs - 5) break;

            var guess = _allWords[i];
            var feedbackBuckets = new Dictionary<int, int>(256);

            foreach (var secretIndex in _candidateIndices)
            {
                int feedback = EncodeFeedbackSimple(guess, _allWords[secretIndex]);
                if (!feedbackBuckets.TryAdd(feedback, 1))
                    feedbackBuckets[feedback]++;
            }

            int currentWorstCase = 0;
            foreach (var count in feedbackBuckets.Values)
                if (count > currentWorstCase) currentWorstCase = count;

            bool isCurrentGuessCandidate = candidateSet.Contains(i);

            if (currentWorstCase < minWorstCase || (currentWorstCase == minWorstCase && isCurrentGuessCandidate && !isBestGuessCandidate))
            {
                minWorstCase = currentWorstCase;
                bestGuess = guess;
                isBestGuessCandidate = isCurrentGuessCandidate;
            }
        }

        return bestGuess;
    }

    private int EncodeFeedbackSimple(string guess, string secret)
    {
        int res = 0;
        for (int i = 0; i < _wordLength; i++)
        {
            int s;
            if (guess[i] == secret[i]) s = 3;
            else if (secret.Contains(guess[i])) s = 2;
            else s = 1;
            res |= (s << (2 * i));
        }
        return res;
    }

    private int StatesToInt(int[] states)
    {
        int obs = 0;
        for (int i = 0; i < _wordLength; i++)
            obs |= (states[i] << (2 * i));
        return obs;
    }

    private bool IsConsistent(string candidateSecret, string guess, int observedStatesInt)
    {
        int feedback = EncodeFeedbackSimple(guess, candidateSecret);
        return feedback == observedStatesInt;
    }
}