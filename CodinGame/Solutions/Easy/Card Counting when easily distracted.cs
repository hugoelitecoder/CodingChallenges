using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static void Main(string[] args) {
        string stream = Console.ReadLine();
        int bustThreshold = int.Parse(Console.ReadLine());

        var validCards = "A23456789TJQK";
        var cardValues = new Dictionary<char, int> {
            ['A'] = 1, ['2'] = 2, ['3'] = 3, ['4'] = 4, ['5'] = 5,
            ['6'] = 6, ['7'] = 7, ['8'] = 8, ['9'] = 9,
            ['T'] = 10, ['J'] = 10, ['Q'] = 10, ['K'] = 10
        };

        // Full deck: 4 of each card
        var deck = validCards.ToDictionary(c => c, c => 4);

        // Split input into potential card sequences
        var parts = stream.Split('.');
        foreach (var part in parts) {
            if (part.All(c => validCards.Contains(c))) {
                foreach (var c in part) {
                    if (deck[c] > 0) deck[c]--;
                }
            }
        }

        // Now count how many of the remaining cards are < bustThreshold
        int safe = 0, total = 0;
        foreach (var kv in deck) {
            int val = cardValues[kv.Key];
            int count = kv.Value;
            if (val < bustThreshold) safe += count;
            total += count;
        }

        int percentage = total == 0 ? 0 : (int)Math.Round(100.0 * safe / total);
        Console.WriteLine($"{percentage}%");
    }
}
