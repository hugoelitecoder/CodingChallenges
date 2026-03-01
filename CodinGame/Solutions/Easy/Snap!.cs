using System;
using System.Collections.Generic;
using System.Diagnostics;

class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var line = Console.ReadLine();
        var m = int.Parse(line);
        var inputP1 = new List<string>(m);
        var deck1Cards = new List<Card>(m);
        for (var i = 0; i < m; i++)
        {
            var cardStr = Console.ReadLine();
            inputP1.Add(cardStr);
            var card = Card.Parse(cardStr);
            deck1Cards.Add(card);
        }
        line = Console.ReadLine();
        var n = int.Parse(line);
        var inputP2 = new List<string>(n);
        var deck2Cards = new List<Card>(n);
        for (var i = 0; i < n; i++)
        {
            var cardStr = Console.ReadLine();
            inputP2.Add(cardStr);
            var card = Card.Parse(cardStr);
            deck2Cards.Add(card);
        }
        Console.Error.WriteLine("[DEBUG] m=" + m + " n=" + n);
        Console.Error.WriteLine("[DEBUG] P1: " + string.Join(" ", inputP1));
        Console.Error.WriteLine("[DEBUG] P2: " + string.Join(" ", inputP2));
        var game = new SnapGame(deck1Cards, deck2Cards);
        game.Play(out var winner, out var cardsLeft);
        sw.Stop();
        Console.Error.WriteLine("[DEBUG] Winner=Player " + winner + " CardsLeft=" + cardsLeft);
        Console.Error.WriteLine("[DEBUG] TimeMs=" + sw.ElapsedMilliseconds);
        Console.WriteLine("Winner: Player " + winner);
        Console.WriteLine(cardsLeft);
    }
}

struct Card
{
    public byte Rank;
    public byte Suit;
    public Card(byte rank, byte suit)
    {
        Rank = rank;
        Suit = suit;
    }
    public static Card Parse(string s)
    {
        var last = s[s.Length - 1];
        byte suit;
        if (last == 'C')
        {
            suit = 0;
        }
        else if (last == 'D')
        {
            suit = 1;
        }
        else if (last == 'H')
        {
            suit = 2;
        }
        else
        {
            suit = 3;
        }
        var rankLen = s.Length - 1;
        byte rank;
        if (rankLen == 1)
        {
            var ch = s[0];
            if (ch >= '2' && ch <= '9')
            {
                rank = (byte)(ch - '2');
            }
            else if (ch == 'J')
            {
                rank = 9;
            }
            else if (ch == 'Q')
            {
                rank = 10;
            }
            else if (ch == 'K')
            {
                rank = 11;
            }
            else
            {
                rank = 12;
            }
        }
        else
        {
            rank = 8;
        }
        return new Card(rank, suit);
    }
}

class SnapGame
{
    private readonly Queue<Card> _deck1;
    private readonly Queue<Card> _deck2;
    private readonly List<Card> _pile;
    public SnapGame(IEnumerable<Card> deck1, IEnumerable<Card> deck2)
    {
        _deck1 = new Queue<Card>();
        _deck2 = new Queue<Card>();
        foreach (var c in deck1)
        {
            _deck1.Enqueue(c);
        }
        foreach (var c in deck2)
        {
            _deck2.Enqueue(c);
        }
        _pile = new List<Card>(52);
    }
    public void Play(out int winner, out int cardsLeft)
    {
        var current = 1;
        while (_deck1.Count > 0 && _deck2.Count > 0)
        {
            var deck = current == 1 ? _deck1 : _deck2;
            var card = deck.Dequeue();
            _pile.Add(card);
            var pileCount = _pile.Count;
            if (pileCount > 1)
            {
                var last = _pile[pileCount - 1];
                var prev = _pile[pileCount - 2];
                if (last.Rank == prev.Rank)
                {
                    var other = current == 1 ? 2 : 1;
                    var snapPlayer = last.Suit > prev.Suit ? current : other;
                    var snapDeck = snapPlayer == 1 ? _deck1 : _deck2;
                    for (var i = 0; i < pileCount; i++)
                    {
                        var c = _pile[i];
                        snapDeck.Enqueue(c);
                    }
                    _pile.Clear();
                    current = snapPlayer;
                    continue;
                }
            }
            current = current == 1 ? 2 : 1;
        }
        if (_deck1.Count > 0)
        {
            winner = 1;
            cardsLeft = _deck1.Count;
        }
        else
        {
            winner = 2;
            cardsLeft = _deck2.Count;
        }
    }
}
