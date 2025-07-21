using System;
using System.Text;
using System.Collections.Generic;

class Solution {
    
    public static void Main() {
        var ins = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sideSize = int.Parse(ins[0]);
        var start = ins[1];
        var rotationDirection = ins[2];
        var c = ins[3][0];
        var repeat = int.Parse(ins[3].Substring(1));
        var cInc = ins[4][0] - ins[3][0];
        var repInc = int.Parse(ins[4].Substring(1)) - int.Parse(ins[3].Substring(1));
        var spiral = new char[sideSize, sideSize];
        for (var i = 0; i < sideSize; i++)
            for (var j = 0; j < sideSize; j++)
                spiral[i, j] = ' ';
        var y = start.StartsWith("top") ? 0 : sideSize - 1;
        var x = start.EndsWith("Left") ? 0 : sideSize - 1;
        var moving = MoveAtStart(rotationDirection, y, x);
        var delta = _moves[moving];
        var rep = repeat;
        while (true) {
            spiral[y, x] = c;
            if (--rep == 0) {
                c = (char)(c + cInc);
                if (!char.IsUpper(c)) break;
                repeat += repInc;
                rep = repeat;
                if (rep == 0) break;
            }
            var nextY = y + delta[0];
            var nextX = x + delta[1];
            if (nextY < 0 || nextX < 0 || nextY >= sideSize || nextX >= sideSize) {
                moving = rotationDirection == "clockwise" ? TurnCW(moving) : TurnCCW(moving);
                delta = _moves[moving];
                nextY = y + delta[0];
                nextX = x + delta[1];
            }
            var fw2Y = y + 2 * delta[0];
            var fw2X = x + 2 * delta[1];
            if (fw2Y >= 0 && fw2X >= 0 && fw2Y < sideSize && fw2X < sideSize && spiral[fw2Y, fw2X] != ' ') {
                moving = rotationDirection == "clockwise" ? TurnCW(moving) : TurnCCW(moving);
                delta = _moves[moving];
                nextY = y + delta[0];
                nextX = x + delta[1];
                fw2Y = y + 2 * delta[0];
                fw2X = x + 2 * delta[1];
                if (fw2Y >= 0 && fw2X >= 0 && fw2Y < sideSize && fw2X < sideSize && spiral[fw2Y, fw2X] != ' ')
                    break;
            }
            y = nextY;
            x = nextX;
        }
        var outSize = Math.Min(sideSize, 31);
        var sb = new StringBuilder();
        for (var i = 0; i < outSize; i++) {
            sb.Clear();
            for (var j = 0; j < outSize; j++) sb.Append(spiral[i, j]);
            Console.WriteLine(sb);
        }
    }
    private static readonly Dictionary<string, int[]> _moves = new() {
        ["UP"] = new[]{-1,0}, ["DOWN"] = new[]{1,0},
        ["LEFT"] = new[]{0,-1}, ["RIGHT"] = new[]{0,1}
    };
    private static string MoveAtStart(string dir, int y, int x) {
        if (y == 0 && x == 0) return dir == "clockwise" ? "RIGHT" : "DOWN";
        if (y != 0 && x != 0) return dir == "clockwise" ? "LEFT" : "UP";
        if (y == 0) return dir == "clockwise" ? "DOWN" : "LEFT";
        return dir == "clockwise" ? "UP" : "RIGHT";
    }
    private static string TurnCW(string d) => d switch {
        "UP" => "RIGHT", "RIGHT" => "DOWN", "DOWN" => "LEFT", "LEFT" => "UP", _ => d
    };
    private static string TurnCCW(string d) => d switch {
        "UP" => "LEFT", "LEFT" => "DOWN", "DOWN" => "RIGHT", "RIGHT" => "UP", _ => d
    };
}
