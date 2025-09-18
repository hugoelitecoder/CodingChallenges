using System;
using System.Collections.Generic;
using System.Linq;

class Solution {
    static readonly char[] Xcycle  = {'U','B','D','F'};   // x
    static readonly char[] Xpcycle = {'U','F','D','B'};   // x'
    static readonly char[] Ycycle  = {'F','L','B','R'};   // y
    static readonly char[] Ypcycle = {'F','R','B','L'};   // y'
    static readonly char[] Zcycle  = {'U','R','D','L'};   // z
    static readonly char[] Zpcycle = {'U','L','D','R'};   // z'

    static void ApplyRotate(Dictionary<char,char> pos, char[] cycle) {
        var old = pos.ToDictionary(kv => kv.Key, kv => kv.Value);
        foreach (var piece in old.Keys) {
            var dir = old[piece];
            int idx = Array.IndexOf(cycle, dir);
            if (idx >= 0) {
                pos[piece] = cycle[(idx + 1) % cycle.Length];
            }
        }
    }

    public static void Main() {
        var rotations = Console.ReadLine()?
            .Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

        var pos = new Dictionary<char,char> {
            ['U']='U', ['D']='D', ['F']='F',
            ['B']='B', ['L']='L', ['R']='R'
        };

        foreach (var rot in rotations) {
            switch (rot) {
                case "x":  ApplyRotate(pos, Xcycle);  break;
                case "x'": ApplyRotate(pos, Xpcycle); break;
                case "y":  ApplyRotate(pos, Ycycle);  break;
                case "y'": ApplyRotate(pos, Ypcycle); break;
                case "z":  ApplyRotate(pos, Zcycle);  break;
                case "z'": ApplyRotate(pos, Zpcycle); break;
                default:
                    Console.Error.WriteLine($"Unknown rotation '{rot}'");
                    break;
            }
        }

        for (int i = 0; i < 2; i++) {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line)) {
                Console.WriteLine();
                continue;
            }
            char piece = line[0];
            Console.WriteLine(pos[piece]);
        }
    }
}
