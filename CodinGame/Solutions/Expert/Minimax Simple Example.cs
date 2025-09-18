using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var n = int.Parse(inputs[0]);
        var q = int.Parse(inputs[1]);
        var letterInputs = Console.ReadLine().Split(' ');
        var pile = letterInputs.Select(s => s[0]).ToList();
        var rawDict = new Dictionary<string, int>();
        for (var i = 0; i < q; i++)
        {
            var wordInputs = Console.ReadLine().Split(' ');
            var word = wordInputs[0];
            var score = int.Parse(wordInputs[1]);
            rawDict[word] = score;
        }

        var game = new LetterGame(rawDict);
        var (choice, result) = game.Solve(pile);

        Console.WriteLine($"{choice} {result.Score1}-{result.Score2}");
    }
}


public struct LetterResult
{
    public int Score1;
    public int Score2;
    public int Difference => Score1 - Score2;
}

class LetterGame
{
    private readonly List<(HashSet<char> Word, int Score)> _processedDict;

    public LetterGame(Dictionary<string, int> dictionary)
    {
        _processedDict = dictionary
            .Select(kvp => (new HashSet<char>(kvp.Key), kvp.Value))
            .ToList();
    }

    public (char Choice, LetterResult Outcome) Solve(List<char> initialPile)
    {
        var p1Hand1 = new HashSet<char>();
        var p2Hand1 = new HashSet<char>();
        var pile1 = new List<char>(initialPile);
        var choice1 = pile1[0];
        p1Hand1.Add(choice1);
        pile1.RemoveAt(0);
        var result1 = Minimax(pile1, p1Hand1, p2Hand1, false, int.MinValue, int.MaxValue);

        var p1Hand2 = new HashSet<char>();
        var p2Hand2 = new HashSet<char>();
        var pile2 = new List<char>(initialPile);
        var choice2 = pile2[1];
        p1Hand2.Add(choice2);
        pile2.RemoveAt(1);
        var result2 = Minimax(pile2, p1Hand2, p2Hand2, false, int.MinValue, int.MaxValue);

        return result1.Difference > result2.Difference
            ? (choice1, result1)
            : (choice2, result2);
    }

    private LetterResult Minimax(List<char> pile, HashSet<char> p1Hand, HashSet<char> p2Hand, bool isMaximizing, int alpha, int beta)
    {
        if (pile.Count == 0)
        {
            return new LetterResult
            {
                Score1 = CalculateScore(p1Hand),
                Score2 = CalculateScore(p2Hand)
            };
        }

        var currentHand = isMaximizing ? p1Hand : p2Hand;
        var bestResult = new LetterResult { Score1 = isMaximizing ? int.MinValue : int.MaxValue };

        for (var i = 0; i < Math.Min(2, pile.Count); i++)
        {
            var letter = pile[i];
            
            currentHand.Add(letter);
            pile.RemoveAt(i);

            var currentResult = Minimax(pile, p1Hand, p2Hand, !isMaximizing, alpha, beta);

            pile.Insert(i, letter);
            currentHand.Remove(letter);

            if (isMaximizing)
            {
                if (currentResult.Difference > bestResult.Difference)
                {
                    bestResult = currentResult;
                }
                alpha = Math.Max(alpha, currentResult.Difference);
            }
            else
            {
                if (currentResult.Difference < bestResult.Difference)
                {
                    bestResult = currentResult;
                }
                beta = Math.Min(beta, currentResult.Difference);
            }

            if (beta <= alpha)
            {
                break;
            }
        }
        return bestResult;
    }

    private int CalculateScore(HashSet<char> hand)
    {
        var score = 0;
        foreach (var (word, wordScore) in _processedDict)
        {
            if (word.IsSubsetOf(hand))
            {
                score += wordScore;
            }
        }
        return score;
    }
}