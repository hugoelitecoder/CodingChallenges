using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static void Main(string[] args) {
        var input = Console.ReadLine().Split();
        var R = int.Parse(input[0]);
        var S = int.Parse(input[1]);

        var ranks = "23456789TJQKA";
        var suits = "CDHS";
        var deck = new HashSet<string>(
            from r in ranks
            from s in suits
            select $"{r}{s}"
        );

        for (int i = 0; i < R; i++) {
            var line = Console.ReadLine();
            var pattern = ParsePattern(line, ranks, suits);
            foreach (var card in pattern) deck.Remove(card);
        }

        var soughtCards = new HashSet<string>();
        for (int i = 0; i < S; i++) {
            var line = Console.ReadLine();
            var pattern = ParsePattern(line, ranks, suits);
            foreach (var card in pattern)
                if (deck.Contains(card))
                    soughtCards.Add(card);
        }

        int percentage = (int)Math.Round(100.0 * soughtCards.Count / deck.Count);
        Console.WriteLine($"{percentage}%");
    }

    static IEnumerable<string> ParsePattern(string pattern, string allRanks, string allSuits) {
        var ranks = new HashSet<char>(pattern.Where(c => allRanks.Contains(c)));
        var suits = new HashSet<char>(pattern.Where(c => allSuits.Contains(c)));
        if (ranks.Count == 0) ranks = allRanks.ToHashSet();
        if (suits.Count == 0) suits = allSuits.ToHashSet();
        foreach (var r in ranks)
            foreach (var s in suits)
                yield return $"{r}{s}";
    }
}