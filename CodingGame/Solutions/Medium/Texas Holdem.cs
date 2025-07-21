using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var hole1 = Console.ReadLine().Split();
        var hole2 = Console.ReadLine().Split();
        var board = Console.ReadLine().Split();

        var game = new TexasHoldemGame(hole1, hole2, board);
        Console.WriteLine(game.DetermineWinner());
    }
}

public enum Suit { Clubs, Diamonds, Hearts, Spades }
public enum CardValue { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }
public enum HandType { High_Card, Pair, Two_Pair, Three_Of_A_Kind, Straight, Flush, Full_House, Four_Of_A_Kind, Straight_Flush }

public class Card
{
    public CardValue Value { get; }
    public Suit Suit { get; }

    public Card(CardValue value, Suit suit)
    {
        Value = value;
        Suit = suit;
    }

    public static Card Parse(string repr)
    {
        var valueChar = repr[0];
        var suitChar = repr[1];
        var value = valueChar switch
        {
            '2' => CardValue.Two,
            '3' => CardValue.Three,
            '4' => CardValue.Four,
            '5' => CardValue.Five,
            '6' => CardValue.Six,
            '7' => CardValue.Seven,
            '8' => CardValue.Eight,
            '9' => CardValue.Nine,
            'T' => CardValue.Ten,
            'J' => CardValue.Jack,
            'Q' => CardValue.Queen,
            'K' => CardValue.King,
            'A' => CardValue.Ace,
            _   => throw new ArgumentException("Invalid card value")
        };
        var suit = suitChar switch
        {
            'C' => Suit.Clubs,
            'D' => Suit.Diamonds,
            'H' => Suit.Hearts,
            'S' => Suit.Spades,
            _   => throw new ArgumentException("Invalid card suit")
        };
        return new Card(value, suit);
    }
}

public class HandResult
{
    public HandType Type { get; }
    public IReadOnlyList<int> Tiebreakers { get; }
    public HandResult(HandType type, List<int> tiebreakers)
    {
        Type = type;
        Tiebreakers = tiebreakers;
    }
}

public class PlayerHand
{
    public Card[] HoleCards { get; }
    public PlayerHand(string[] holeCardStrs)
    {
        HoleCards = holeCardStrs.Select(Card.Parse).ToArray();
    }
    public Card[] AllCards(Card[] community) => HoleCards.Concat(community).ToArray();
}

public static class HandEvaluator
{
    public static HandResult Evaluate(Card[] cards)
    {
        HandResult best = null;
        foreach (var combo in CombinationHelper.GetCombinations(cards, 5))
        {
            var result = EvaluateFive(combo);
            if (best == null || Compare(result, best) > 0)
                best = result;
        }
        return best;
    }

    private static HandResult EvaluateFive(Card[] hand)
    {
        var values = hand.Select(c => (int)c.Value).ToList();
        var suits = hand.Select(c => c.Suit).ToList();
        var freq = values.GroupBy(v => v).ToDictionary(g => g.Key, g => g.Count());
        bool isFlush = suits.All(s => s == suits[0]);
        bool isStraight = StraightHelper.IsStraight(values, out int high);

        if (isStraight && isFlush)
            return new HandResult(HandType.Straight_Flush, new List<int> { high });
        if (FourOfAKind.TryCreate(freq, out var fofk))
            return new HandResult(HandType.Four_Of_A_Kind, fofk);
        if (FullHouse.TryCreate(freq, out var fh))
            return new HandResult(HandType.Full_House, fh);
        if (isFlush)
            return new HandResult(HandType.Flush, Kickers.NonPairKickers(freq));
        if (isStraight)
            return new HandResult(HandType.Straight, new List<int> { high });
        if (ThreeOfAKind.TryCreate(freq, out var toak))
            return new HandResult(HandType.Three_Of_A_Kind, toak);
        if (TwoPair.TryCreate(freq, out var tp))
            return new HandResult(HandType.Two_Pair, tp);
        if (Pair.TryCreate(freq, out var pair))
            return new HandResult(HandType.Pair, pair);

        return new HandResult(HandType.High_Card, Kickers.NonPairKickers(freq));
    }

    public static int Compare(HandResult a, HandResult b)
    {
        int cmp = a.Type.CompareTo(b.Type);
        if (cmp != 0) return cmp;
        for (int i = 0; i < a.Tiebreakers.Count; i++)
        {
            cmp = a.Tiebreakers[i].CompareTo(b.Tiebreakers[i]);
            if (cmp != 0) return cmp;
        }
        return 0;
    }

    private static class StraightHelper
    {
        public static bool IsStraight(List<int> values, out int high)
        {
            var distinct = values.Distinct().OrderByDescending(v => v).ToList();
            high = 0;
            if (distinct.Count != 5) return false;
            int max = distinct[0], min = distinct[4];
            if (max - min == 4) { high = max; return true; }
            if (distinct.SequenceEqual(new[] { 14, 5, 4, 3, 2 })) { high = 5; return true; }
            return false;
        }
    }

    private static class CombinationHelper
    {
        public static IEnumerable<Card[]> GetCombinations(Card[] cards, int k)
        {
            return Combine(cards, k);
            static IEnumerable<Card[]> Combine(Card[] arr, int k)
            {
                int n = arr.Length;
                if (k == 0) yield return new Card[0];
                else
                {
                    for (int i = 0; i <= n - k; i++)
                    {
                        foreach (var tail in Combine(arr.Skip(i + 1).ToArray(), k - 1))
                            yield return new[] { arr[i] }.Concat(tail).ToArray();
                    }
                }
            }
        }
    }

    private static class Kickers
    {
        public static List<int> NonPairKickers(Dictionary<int, int> freq)
            => freq.Keys.OrderByDescending(v => v).ToList();
    }

    private static class FourOfAKind
    {
        public static bool TryCreate(Dictionary<int, int> freq, out List<int> tb)
        {
            tb = null;
            var quad = freq.FirstOrDefault(kv => kv.Value == 4);
            if (quad.Key == 0) return false;
            int kicker = freq.First(kv => kv.Value == 1).Key;
            tb = new List<int> { quad.Key, kicker };
            return true;
        }
    }

    private static class FullHouse
    {
        public static bool TryCreate(Dictionary<int, int> freq, out List<int> tb)
        {
            tb = null;
            var three = freq.FirstOrDefault(kv => kv.Value == 3);
            var pair = freq.FirstOrDefault(kv => kv.Value == 2);
            if (three.Key == 0 || pair.Key == 0) return false;
            tb = new List<int> { three.Key, pair.Key };
            return true;
        }
    }

    private static class ThreeOfAKind
    {
        public static bool TryCreate(Dictionary<int, int> freq, out List<int> tb)
        {
            tb = null;
            var three = freq.FirstOrDefault(kv => kv.Value == 3);
            if (three.Key == 0) return false;
            var kickers = freq.Where(kv => kv.Value == 1)
                                .Select(kv => kv.Key)
                                .OrderByDescending(v => v)
                                .ToList();
            tb = new List<int> { three.Key }.Concat(kickers).ToList();
            return true;
        }
    }

    private static class TwoPair
    {
        public static bool TryCreate(Dictionary<int, int> freq, out List<int> tb)
        {
            tb = null;
            var pairs = freq.Where(kv => kv.Value == 2)
                            .Select(kv => kv.Key)
                            .OrderByDescending(v => v)
                            .ToList();
            if (pairs.Count != 2) return false;
            int kicker = freq.First(kv => kv.Value == 1).Key;
            tb = pairs.Concat(new[] { kicker }).ToList();
            return true;
        }
    }

    private static class Pair
    {
        public static bool TryCreate(Dictionary<int, int> freq, out List<int> tb)
        {
            tb = null;
            var pair = freq.FirstOrDefault(kv => kv.Value == 2);
            if (pair.Key == 0) return false;
            var kickers = freq.Where(kv => kv.Value == 1)
                                .Select(kv => kv.Key)
                                .OrderByDescending(v => v)
                                .ToList();
            tb = new List<int> { pair.Key }.Concat(kickers).ToList();
            return true;
        }
    }
}

public class TexasHoldemGame
{
    private readonly PlayerHand _player1;
    private readonly PlayerHand _player2;
    private readonly Card[] _community;

    public TexasHoldemGame(string[] hole1, string[] hole2, string[] board)
    {
        _player1 = new PlayerHand(hole1);
        _player2 = new PlayerHand(hole2);
        _community = board.Select(Card.Parse).ToArray();
    }

    public string DetermineWinner()
    {
        var result1 = HandEvaluator.Evaluate(_player1.AllCards(_community));
        var result2 = HandEvaluator.Evaluate(_player2.AllCards(_community));
        int cmp = HandEvaluator.Compare(result1, result2);
        if (cmp > 0) return Format(1, result1);
        if (cmp < 0) return Format(2, result2);
        return "DRAW";
    }

    private string Format(int player, HandResult hr)
        => $"{player} {hr.Type.ToString().ToUpper()} {HandFormatter.Format(hr)}";
}

public static class HandFormatter
{
    public static string Format(HandResult hr)
    {
        var t = hr.Tiebreakers;
        return hr.Type switch
        {
            HandType.Straight_Flush or HandType.Straight => FormatStraight(t[0]),
            HandType.Four_Of_A_Kind => FormatRepeated(t[0], 4) + FormatSingle(t[1]),
            HandType.Full_House => FormatRepeated(t[0], 3) + FormatRepeated(t[1], 2),
            HandType.Flush or HandType.High_Card => string.Concat(t.Select(FormatSingle)),
            HandType.Three_Of_A_Kind => FormatRepeated(t[0], 3) + string.Concat(t.Skip(1).Select(FormatSingle)),
            HandType.Two_Pair => FormatRepeated(t[0], 2) + FormatRepeated(t[1], 2) + FormatSingle(t[2]),
            HandType.Pair => FormatRepeated(t[0], 2) + string.Concat(t.Skip(1).Select(FormatSingle)),
            _ => string.Empty
        };
    }

    private static string FormatStraight(int high)
    {
        if (high == 5) return "5432A";
           return string.Concat(Enumerable.Range(0, 5).Select(i => FormatSingle(high - i)));
    }

    private static string FormatRepeated(int value, int count)
        => new string(FormatSingle(value)[0], count);

    private static string FormatSingle(int value)
        => value switch
        {
            14 or 1 => "A",
            13 => "K",
            12 => "Q",
            11 => "J",
            10 => "T",
            _ => ((char)('0' + value)).ToString()
        };
}

