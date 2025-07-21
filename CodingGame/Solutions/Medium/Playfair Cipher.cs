using System;
using System.Collections.Generic;
using System.Linq;

class Solution {
    static void Main() {
        var table = new char[5,5];
        Enumerable.Range(0, 5).ToList().ForEach(r =>
            Console.ReadLine()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select((ch, c) => table[r, c] = ch[0])
                .ToList()
        );

        var cipher = new PlayfairCipher(table);
        var action = Console.ReadLine().Trim();
        var N = int.Parse(Console.ReadLine());

        Enumerable.Range(0, N)
            .Select(_ => Console.ReadLine())
            .Select(msg => action == "ENCRYPT" ? cipher.Encrypt(msg) : cipher.Decrypt(msg))
            .ToList()
            .ForEach(Console.WriteLine);
    }
}

public class PlayfairCipher {
    private readonly char[,] _table;
    private readonly Dictionary<char, (int r, int c)> _pos;

    public PlayfairCipher(char[,] keyTable) {
        _table = keyTable;
        _pos = new Dictionary<char, (int, int)>();
        for (int r = 0; r < 5; r++)
            for (int c = 0; c < 5; c++)
                _pos[_table[r, c]] = (r, c);
    }

    public string Encrypt(string message) => Process(message, true);
    public string Decrypt(string message) => Process(message, false);

    private string Process(string message, bool encrypt) {
        var letters = message.ToUpper()
            .Where(char.IsLetter)
            .Where(ch => _pos.ContainsKey(ch))
            .ToList();
        if (letters.Count % 2 != 0) return "DUD";

        var res = new List<char>(letters.Count);
        for (int i = 0; i < letters.Count; i += 2) {
            var a = letters[i];
            var b = letters[i + 1];
            var (r1, c1) = _pos[a];
            var (r2, c2) = _pos[b];

            if (r1 == r2) {
                var shift = encrypt ? 1 : 4;
                res.Add(_table[r1, (c1 + shift) % 5]);
                res.Add(_table[r2, (c2 + shift) % 5]);
            }
            else if (c1 == c2) {
                var shift = encrypt ? 1 : 4;
                res.Add(_table[(r1 + shift) % 5, c1]);
                res.Add(_table[(r2 + shift) % 5, c2]);
            }
            else {
                res.Add(_table[r1, c2]);
                res.Add(_table[r2, c1]);
            }
        }

        return new string(res.ToArray());
    }
}
