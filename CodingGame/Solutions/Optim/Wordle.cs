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
            if (timer.ElapsedMilliseconds > turnDeadlineMs)
                nextGuess = solver.GetSafeFallback();
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
    private int _wordLength;
    private readonly string[] _allWords;
    private List<int> _candidateIndices;
    private readonly Random _random = new Random();
    private readonly double maxEntropy;

    public WordleSolver(List<string> allWords)
    {
        _wordLength = allWords[0].Length;
        _allWords = allWords.Select(w => w.ToUpper()).ToArray();
        _candidateIndices = Enumerable.Range(0, _allWords.Length).ToList();
        maxEntropy = ComputeCandidateEntropy(_allWords, _candidateIndices);
    }

    public int GetPossibleCount() => _candidateIndices.Count;
    public string GetSafeFallback() => _candidateIndices.Count == 0 ? _allWords[0] : _allWords[_candidateIndices[0]];

    public void ProcessFeedback(string lastGuess, int[] states)
    {
        int obs = StatesToInt(states);
        var newCandidates = new List<int>(_candidateIndices.Count);
        foreach (var index in _candidateIndices)
            if (IsConsistent(_allWords[index], lastGuess, obs))
                newCandidates.Add(index);
        _candidateIndices = newCandidates;
        Console.Error.WriteLine($"[DEBUG] ProcessFeedback: {newCandidates.Count} candidates remain");
    }

    public string GetNextGuess(Stopwatch timer, long turnDeadlineMs, int turnNumber)
    {
        if (turnNumber == 1)
            return FindBestGuessByPositionalHeuristic(timer, turnDeadlineMs, Enumerable.Range(0, _allWords.Length).ToList(), true);
        if (_candidateIndices.Count == 0)
            return _allWords[0];
        if (_candidateIndices.Count == 1)
            return _allWords[_candidateIndices[0]];
        var guess = FindBestGuessByEntropy(timer, turnDeadlineMs, _candidateIndices);
        if (!string.IsNullOrEmpty(guess))
            return guess;
        return FindBestGuessByPositionalHeuristic(timer, turnDeadlineMs, _candidateIndices, false);
    }

    private string FindBestGuessByPositionalHeuristic(Stopwatch timer, long turnDeadlineMs, List<int> currentCandidates, bool isFirstGuess)
    {
        if (currentCandidates.Count == 0) return _allWords[0];
        var freq = new int[_wordLength, 26];
        foreach (var index in currentCandidates)
        {
            var word = _allWords[index];
            for (var i = 0; i < _wordLength; i++)
                freq[i, word[i] - 'A']++;
        }
        string bestGuess = "";
        int maxScore = -1;
        var searchSpaceIndices = isFirstGuess ? Enumerable.Range(0, _allWords.Length).ToList() : currentCandidates;
        foreach (var index in searchSpaceIndices)
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
        return string.IsNullOrEmpty(bestGuess) ? _allWords[currentCandidates[0]] : bestGuess;
    }

    private string FindBestGuessByEntropy(Stopwatch timer, long turnDeadlineMs, List<int> candidates)
    {
        double candidateEntropy = ComputeCandidateEntropy(_allWords, candidates);

        int minBatch = Math.Min(16, candidates.Count);
        int batchCount = minBatch;
        var tBatchStart = timer.ElapsedMilliseconds;
        for (int i = 0; i < batchCount; ++i)
        {
            var guessIdx = candidates[i];
            var guess = _allWords[guessIdx];
            var feedbackBuckets = new Dictionary<int, int>(64);
            foreach (var secretIdx in candidates)
            {
                int key = EncodeFeedback(guess, _allWords[secretIdx]);
                if (!feedbackBuckets.TryAdd(key, 1))
                    feedbackBuckets[key]++;
            }
            double entropy = 0;
            foreach (var count in feedbackBuckets.Values)
            {
                double p = count / (double)candidates.Count;
                entropy += -p * Math.Log(p, 2);
            }
        }
        var tBatchEnd = timer.ElapsedMilliseconds;
        long batchTime = tBatchEnd - tBatchStart;
        batchTime = Math.Max(1, batchTime);

        long timeLeft = Math.Max(1, turnDeadlineMs - tBatchEnd);
        int estTotal = (int)Math.Min(candidates.Count, batchCount * timeLeft / batchTime);

        var candidateSample = candidates;
        if (candidates.Count > estTotal)
            candidateSample = candidates.OrderBy(x => _random.Next()).Take(estTotal).ToList();

        string bestWord = null;
        double bestScore = double.NegativeInfinity;
        int tested = 0;
        foreach (var guessIdx in candidateSample)
        {
            if (timer.ElapsedMilliseconds >= turnDeadlineMs - 2) break;
            var guess = _allWords[guessIdx];
            var feedbackBuckets = new Dictionary<int, int>(64);
            foreach (var secretIdx in candidates)
            {
                int key = EncodeFeedback(guess, _allWords[secretIdx]);
                if (!feedbackBuckets.TryAdd(key, 1))
                    feedbackBuckets[key]++;
            }
            double entropy = 0;
            foreach (var count in feedbackBuckets.Values)
            {
                double p = count / (double)candidates.Count;
                entropy += -p * Math.Log(p, 2);
            }
            if (entropy > bestScore)
            {
                bestScore = entropy;
                bestWord = guess;
            }
            tested++;
        }
        Console.Error.WriteLine($"[DEBUG] EntropySearch: tested={tested}, remain={candidates.Count}");
        return bestWord;
    }

    private int EncodeFeedback(string guess, string secret)
    {
        int res = 0;
        for (int i = 0; i < _wordLength; i++)
        {
            int s;
            if (guess[i] == secret[i]) s = 3;
            else if (secret.IndexOf(guess[i]) >= 0) s = 2;
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
        int feedback = EncodeFeedback(guess, candidateSecret);
        return feedback == observedStatesInt;
    }

    private static double ComputeCandidateEntropy(string[] allWords, List<int> candidates)
    {
        int wordLen = allWords[0].Length;
        var letterHist = new int[wordLen, 26];
        foreach (var idx in candidates)
        {
            var word = allWords[idx];
            for (int i = 0; i < wordLen; i++)
                letterHist[i, word[i] - 'A']++;
        }
        double entropy = 0;
        for (int i = 0; i < wordLen; i++)
        {
            int total = candidates.Count;
            for (int j = 0; j < 26; j++)
            {
                if (letterHist[i, j] == 0) continue;
                double p = letterHist[i, j] / (double)total;
                entropy -= p * Math.Log(p, 2);
            }
        }
        return entropy;
    }
}
