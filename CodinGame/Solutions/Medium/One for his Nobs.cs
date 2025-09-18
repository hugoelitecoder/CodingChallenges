using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        while (n-- > 0)
        {
            var parts = Console.ReadLine().Split();
            var hand = new CribbageHand(parts);
            Console.WriteLine(hand.Score());
        }
    }
}

class CribbageHand
{  
    private static readonly string _ranks = "A23456789TJQK";
    private static readonly int[]  _fifteenVal = {1,2,3,4,5,6,7,8,9,10,10,10,10};
    private readonly Card[] _allCards;
    private readonly Card[] _handCards;
    private readonly Card   _starter;

    public CribbageHand(string[] parts)
    {
        _allCards = parts.Select(p => new Card {
            Rank = _ranks.IndexOf(p[0]),
            Val  = _fifteenVal[_ranks.IndexOf(p[0])],
            Suit = p[1]
        }).ToArray();
        _handCards = _allCards.Take(4).ToArray();
        _starter   = _allCards[4];
    }

    public int Score()
    {
        return ScoreFifteens()
             + ScorePairs()
             + ScoreRuns()
             + ScoreFlush()
             + ScoreNobs();
    }

    private int ScoreFifteens()
    {
        int cnt = 0, n = _allCards.Length;
        for (int mask = 1; mask < (1 << n); mask++)
        {
            int sum = 0;
            for (int i = 0; i < n; i++)
                if ((mask & (1 << i)) != 0)
                    sum += _allCards[i].Val;
            if (sum == 15) cnt++;
        }
        return cnt * 2;
    }

    private int ScorePairs()
    {
        var freq = new int[13];
        foreach (var c in _allCards) freq[c.Rank]++;
        int pts = 0;
        foreach (int f in freq)
            if (f > 1) pts += f * (f - 1);
        return pts;
    }

    private int ScoreRuns()
    {
        var freq = new int[13];
        foreach (var c in _allCards) freq[c.Rank]++;
        int pts = 0;
        for (int i = 0; i < 13;)
        {
            if (freq[i] > 0)
            {
                long mult = 1;
                int j = i;
                while (j < 13 && freq[j] > 0)
                {
                    mult *= freq[j++];
                }
                int len = j - i;
                if (len >= 3) pts += (int)(mult * len);
                i = j;
            }
            else i++;
        }
        return pts;
    }

    private int ScoreFlush()
    {
        char s = _handCards[0].Suit;
        if (_handCards.All(c => c.Suit == s))
            return _starter.Suit == s ? 5 : 4;
        return 0;
    }

    private int ScoreNobs()
    {
        return _handCards.Any(c => c.Rank == 10 && c.Suit == _starter.Suit) ? 1 : 0;
    }

    private struct Card
    {
        public int Rank;
        public int Val;
        public char Suit;
    }

  
}
