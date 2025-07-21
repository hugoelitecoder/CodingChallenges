using System;
using System.Collections.Generic;

class Solution {
    
    static readonly string order = "234567891JQKA";
    static void Main() {
        var d1 = ReadDeck();
        var d2 = ReadDeck();
        int rounds = 0;
        while (d1.Count > 0 && d2.Count > 0) {
            rounds++;
            var spoils1 = new List<string>();
            var spoils2 = new List<string>();
            int r1, r2;
            do {
                if (d1.Count < 1 || d2.Count < 1) { Console.WriteLine("PAT"); return; }
                spoils1.Add(d1.Dequeue());
                spoils2.Add(d2.Dequeue());
                r1 = order.IndexOf(spoils1[^1][0]);
                r2 = order.IndexOf(spoils2[^1][0]);
                if (r1 == r2) {
                    if (d1.Count < 3 || d2.Count < 3) { Console.WriteLine("PAT"); return; }
                    for (int i = 0; i < 3; i++) {
                        spoils1.Add(d1.Dequeue());
                        spoils2.Add(d2.Dequeue());
                    }
                }
            } while (r1 == r2);

            var winnerDeck = r1 > r2 ? d1 : d2;
            foreach (var c in spoils1) winnerDeck.Enqueue(c);
            foreach (var c in spoils2) winnerDeck.Enqueue(c);
        }
        Console.WriteLine($"{(d1.Count > 0 ? 1 : 2)} {rounds}");
    }

    static Queue<string> ReadDeck() {
        int n = int.Parse(Console.ReadLine());
        var q = new Queue<string>();
        while (n-- > 0) q.Enqueue(Console.ReadLine());
        return q;
    }
}
