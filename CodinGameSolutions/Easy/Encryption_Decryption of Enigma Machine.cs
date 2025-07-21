using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    static void Main(string[] args) {
        var op = Console.ReadLine(); // ENCODE or DECODE
        var shift = int.Parse(Console.ReadLine());
        var rotor1 = Console.ReadLine();
        var rotor2 = Console.ReadLine();
        var rotor3 = Console.ReadLine();
        var msg = Console.ReadLine();

        if (op == "ENCODE") {
            msg = CaesarShift(msg, shift);
            msg = Substitute(msg, alphabet, rotor1);
            msg = Substitute(msg, alphabet, rotor2);
            msg = Substitute(msg, alphabet, rotor3);
        } else {
            msg = Substitute(msg, rotor3, alphabet);
            msg = Substitute(msg, rotor2, alphabet);
            msg = Substitute(msg, rotor1, alphabet);
            msg = CaesarUnshift(msg, shift);
        }

        Console.WriteLine(msg);
    }

    static string CaesarShift(string input, int start) {
        var sb = new StringBuilder();
        for (int i = 0; i < input.Length; i++) {
            int pos = (input[i] - 'A' + start + i) % 26;
            sb.Append(alphabet[pos]);
        }
        return sb.ToString();
    }

    static string CaesarUnshift(string input, int start) {
        var sb = new StringBuilder();
        for (int i = 0; i < input.Length; i++) {
            int shiftVal = (input[i] - 'A' - start - i) % 26;
            if (shiftVal < 0) shiftVal += 26;
            sb.Append(alphabet[shiftVal]);
        }
        return sb.ToString();
    }

    static string Substitute(string input, string from, string to) {
        var sb = new StringBuilder();
        foreach (var c in input) {
            int i = from.IndexOf(c);
            if (i == -1) throw new Exception($"Character '{c}' not found in source alphabet.");
            sb.Append(to[i]);
        }
        return sb.ToString();
    }
}
