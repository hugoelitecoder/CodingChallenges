using System;
using System.Collections.Generic;
using System.Linq;

class Solution {
    static readonly int[][] Dirs = {
        new[] {+1, +0},
        new[] {+0, +1},
        new[] {-1, +0},
        new[] {+0, -1}
    };
    static readonly string[] DirectionNames = { "SOUTH", "EAST", "NORTH", "WEST" };

    static void Main() {
        var (grid, start, teleports) = ParseGrid();
        var moves = new List<string>();
        var seen = new HashSet<string>();

        int direction = 0;
        bool breaker = false, inverted = false;
        int[] origPrio = { 0, 1, 2, 3 };
        int[] revPrio  = { 3, 2, 1, 0 };
        int[] prio = origPrio;
        var pos = start;

        while (true) {
            string state = GetStateString(grid, pos, direction, breaker, inverted);
            if (!seen.Add(state)) {
                Console.WriteLine("LOOP");
                return;
            }

            char cell = grid[pos.r][pos.c];
            if (cell == '$') break;

            ApplyModifier(cell, ref direction, ref breaker, ref inverted, ref prio, teleports, ref pos);
            var (nr, nc, nd) = DetermineNextMove(grid, pos, direction, breaker, prio);

            pos = (nr, nc);
            direction = nd;
            moves.Add(DirectionNames[direction]);
        }

        Console.WriteLine(string.Join("\n", moves));
    }

    static (List<char[]> grid, (int r, int c) start, List<(int, int)> teleports) ParseGrid() {
        var rc = Console.ReadLine().Split();
        int L = int.Parse(rc[0]), C = int.Parse(rc[1]);
        var grid = new List<char[]>(L);
        var teleports = new List<(int, int)>();
        (int r, int c) start = (0, 0);

        for (int i = 0; i < L; i++) {
            var row = Console.ReadLine().ToCharArray();
            for (int j = 0; j < C; j++) {
                if (row[j] == '@') start = (i, j);
                else if (row[j] == 'T') teleports.Add((i, j));
            }
            grid.Add(row);
        }
        return (grid, start, teleports);
    }

    static string GetStateString(List<char[]> grid, (int r, int c) pos,
                                 int dir, bool breaker, bool inverted) {
        var flat = string.Concat(grid.SelectMany(r => r));
        return $"{pos.r},{pos.c},{dir},{breaker},{inverted};{flat}";
    }

    static void ApplyModifier(char cell, ref int dir, ref bool breaker,
                              ref bool inverted, ref int[] prio,
                              List<(int, int)> teleports,
                              ref (int r, int c) pos) {
        switch (cell) {
            case 'S': dir = 0; return;
            case 'E': dir = 1; return;
            case 'N': dir = 2; return;
            case 'W': dir = 3; return;
            case 'I':
                inverted = !inverted;
                prio = inverted ? new[] {3,2,1,0} : new[] {0,1,2,3};
                return;
            case 'B':
                breaker = !breaker;
                return;
            case 'T':
                pos = pos == teleports[0] ? teleports[1] : teleports[0];
                return;
            default:
                return;
        }
    }

    static (int nr, int nc, int nd) DetermineNextMove(List<char[]> grid,
                                                      (int r, int c) pos,
                                                      int dir, bool breaker,
                                                      int[] prio) {
        int nr = pos.r + Dirs[dir][0];
        int nc = pos.c + Dirs[dir][1];

        bool Blocked(int rr, int cc) {
            char ch = grid[rr][cc];
            return ch == '#' || (ch == 'X' && !breaker);
        }

        if (Blocked(nr, nc)) {
            foreach (int nd in prio) {
                int rr = pos.r + Dirs[nd][0];
                int cc = pos.c + Dirs[nd][1];
                if (!Blocked(rr, cc)) {
                    if (grid[rr][cc] == 'X' && breaker) grid[rr][cc] = ' ';
                    return (rr, cc, nd);
                }
            }
        } else if (grid[nr][nc] == 'X' && breaker) {
            grid[nr][nc] = ' ';
        }

        return (nr, nc, dir);
    }
}
