using System;
using System.Linq;

class Solution {
    
    static void Main() {
        int N = int.Parse(Console.ReadLine());
        var guesses = Enumerable.Range(0, N)
                                .Select(_ => {
                                    var p = Console.ReadLine().Split();
                                    return (Digits: p[0].ToCharArray(),
                                            Bulls: int.Parse(p[1]),
                                            Cows:  int.Parse(p[2]));
                                })
                                .ToArray();

        for (int num = 0; num < 10000; num++) {
            var cand = num.ToString("D4");
            if (guesses.All(g => {
                int b = cand.Zip(g.Digits, (c, d) => c == d).Count(match => match);
                int common = Enumerable.Range(0, 10)
                                       .Sum(d => Math.Min(
                                           cand.Count(c => c - '0' == d),
                                           g.Digits.Count(c => c - '0' == d)
                                       ));
                int c = common - b;
                return b == g.Bulls && c == g.Cows;
            })) {
                Console.WriteLine(cand);
                return;
            }
        }
    }
}
