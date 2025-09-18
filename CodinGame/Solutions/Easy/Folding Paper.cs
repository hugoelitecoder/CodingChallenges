using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static void Main() {
        string order = Console.ReadLine();
        char view = Console.ReadLine()[0];

        var count = new Dictionary<char, int> {
            { 'U', 1 }, { 'D', 1 }, { 'L', 1 }, { 'R', 1 }
        };

        var opposite = new Dictionary<char, char> {
            { 'U', 'D' }, { 'D', 'U' }, { 'L', 'R' }, { 'R', 'L' }
        };

        var perpendicular = new Dictionary<char, char[]> {
            { 'U', new[] { 'L', 'R' } },
            { 'D', new[] { 'L', 'R' } },
            { 'L', new[] { 'U', 'D' } },
            { 'R', new[] { 'U', 'D' } }
        };

        foreach (char fold in order) {
            var snapshot = new Dictionary<char, int>(count);
            char from = fold;
            char to = opposite[fold];

            // Step 1: Transfer folded side's layers to opposite side
            count[to] += snapshot[from];

            // Step 2: Reset folded side to 1 (still visible but just one layer)
            count[from] = 1;

            // Step 3: Double only perpendicular sides
            foreach (char side in perpendicular[fold])
                count[side] = snapshot[side] * 2;

            // Debug output
            Console.Error.WriteLine($"Fold: {fold} => U:{count['U']} D:{count['D']} L:{count['L']} R:{count['R']}");
        }

        Console.WriteLine(count[view]);
    }
}








