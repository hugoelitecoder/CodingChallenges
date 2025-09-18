using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player {
    public int Num;
    public string Sign;
    public List<int> History = new List<int>();

    public Player(int num, string sign) {
        Num = num;
        Sign = sign;
    }
}

class Solution {
    
    static void Main() {
        int n = int.Parse(Console.ReadLine());
        var players = new List<Player>();

        for (int i = 0; i < n; i++) {
            var parts = Console.ReadLine().Split();
            players.Add(new Player(int.Parse(parts[0]), parts[1]));
        }

        var beats = new Dictionary<string, List<string>> {
            ["R"] = new List<string> { "L", "C" },
            ["P"] = new List<string> { "R", "S" },
            ["C"] = new List<string> { "P", "L" },
            ["L"] = new List<string> { "S", "P" },
            ["S"] = new List<string> { "C", "R" }
        };

        while (players.Count > 1) {
            var next = new List<Player>();
            for (int i = 0; i < players.Count; i += 2) {
                var p1 = players[i];
                var p2 = players[i + 1];
                Player winner, loser;

                if (beats[p1.Sign].Contains(p2.Sign)) {
                    winner = p1;
                    loser = p2;
                } else if (beats[p2.Sign].Contains(p1.Sign)) {
                    winner = p2;
                    loser = p1;
                } else {
                    winner = (p1.Num < p2.Num) ? p1 : p2;
                    loser = (p1.Num < p2.Num) ? p2 : p1;
                }

                winner.History.Add(loser.Num);
                next.Add(winner);
            }
            players = next;
        }

        var champ = players[0];
        Console.WriteLine(champ.Num);
        Console.WriteLine(string.Join(" ", champ.History));
    }
}
