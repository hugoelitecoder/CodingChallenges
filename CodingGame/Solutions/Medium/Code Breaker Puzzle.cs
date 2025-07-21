using System;
using System.Collections.Generic;
using System.Linq;

class Solution {
    static void Main() {
        var alphabet = Console.ReadLine().Trim();
        var message  = Console.ReadLine().Trim();
        var word     = Console.ReadLine().Trim();

        var cipher = new AffineCipherCracker(alphabet);
        string plaintext = cipher.Crack(message, word);
        Console.WriteLine(plaintext);
    }
}

public class AffineCipherCracker {
    private readonly string _alphabet;
    private readonly int _mod;
    private readonly Dictionary<char, int> _map;
    private readonly char[] _invMap;

    public int Modulus => _mod;
    public string Alphabet => _alphabet;

    public AffineCipherCracker(string alphabet) {
        _alphabet = alphabet;
        _mod = alphabet.Length;
        _map = alphabet.Select((ch, i) => new { ch, i })
                       .ToDictionary(x => x.ch, x => x.i);
        _invMap = alphabet.ToCharArray();
    }

    public string Crack(string cipherText, string knownWord) {
        for (int A = 0; A < _mod; A++) {
            for (int B = 1; B < _mod; B++) {
                if (Gcd(B, _mod) != 1) continue;
                string candidate = Decrypt(cipherText, A, B);
                if (candidate.Contains(knownWord)) return candidate;
            }
        }
        throw new InvalidOperationException("No valid keys found.");
    }

    public string Decrypt(string cipherText, int A, int B) {
        int invB = ModInverse(B, _mod);
        var result = cipherText.Select(ch => {
            int e = _map[ch];
            int x = (invB * e) % _mod;
            x = (x - A) % _mod;
            if (x < 0) x += _mod;
            return _invMap[x];
        });
        return new string(result.ToArray());
    }

    public string Encrypt(string plainText, int A, int B) {
        var result = plainText.Select(ch => {
            int x = _map[ch];
            int e = (x + A) % _mod;
            e = (e * B) % _mod;
            return _invMap[e];
        });
        return new string(result.ToArray());
    }

    public IEnumerable<int> ValidMultipliers()
        => Enumerable.Range(1, _mod - 1).Where(b => Gcd(b, _mod) == 1);

    private int Gcd(int a, int b) {
        while (b != 0) {
            int t = b;
            b = a % b;
            a = t;
        }
        return a;
    }

    private int ModInverse(int a, int m) {
        int m0 = m, x0 = 0, x1 = 1;
        if (m == 1) return 0;
        while (a > 1) {
            int q = a / m;
            int t = m;
            m = a % m; a = t;
            t = x0;
            x0 = x1 - q * x0;
            x1 = t;
        }
        if (x1 < 0) x1 += m0;
        return x1;
    }
}