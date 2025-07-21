using System;
using System.Collections.Generic;

class Solution {
    private const int PADDING = 3;
    private const int JUMP_DISTANCE = 2;
    private const char WALL = '#';
    private const string JUMP_PODS = "^v<>";
    private static readonly (int dr,int dc)[] DIRECTIONS = {(-1,0),(1,0),(0,-1),(0,1)};

    static void Main() {
        int width  = int.Parse(Console.ReadLine());
        int height = int.Parse(Console.ReadLine());
        int H = height + 2 * PADDING;
        int W = width  + 2 * PADDING;

        var grid = new char[H][];
        for (int i = 0; i < H; i++) {
            grid[i] = new char[W];
            for (int j = 0; j < W; j++) grid[i][j] = WALL;
        }

        var teleports = new Dictionary<char,(int r,int c)>();
        int sr=0, sc=0, er=0, ec=0;

        for (int r = 0; r < height; r++) {
            var row = Console.ReadLine().ToCharArray();
            for (int c = 0; c < width; c++) {
                char ch = grid[r+PADDING][c+PADDING] = row[c];
                if (ch == 'S')      (sr,sc) = (r+PADDING, c+PADDING);
                else if (ch == 'E') (er,ec) = (r+PADDING, c+PADDING);
                else if (char.IsUpper(ch))
                    teleports[char.ToLower(ch)] = (r+PADDING, c+PADDING);
            }
        }

        var steps = new int[H,W];
        for (int i = 0; i < H; i++)
            for (int j = 0; j < W; j++)
                steps[i,j] = -1;

        var queue = new Queue<(int r,int c)>();
        queue.Enqueue((sr,sc));
        steps[sr,sc] = 0;

        while (queue.Count > 0) {
            var (r,c) = queue.Dequeue();
            int s = steps[r,c];
            if (r == er && c == ec) {
                Console.WriteLine(s);
                return;
            }

            foreach (var (dr,dc) in DIRECTIONS) {
                int fr = r + dr, fc = c + dc;
                if (grid[fr][fc] == WALL) continue;

                while (JUMP_PODS.Contains(grid[fr][fc])) {
                    var (jr,jc) = grid[fr][fc] switch {
                        '^' => (-JUMP_DISTANCE, 0),
                        'v' => ( JUMP_DISTANCE, 0),
                        '<' => (0, -JUMP_DISTANCE),
                        '>' => (0,  JUMP_DISTANCE),
                        _   => (0,0)
                    };
                    fr += jr; fc += jc;
                    if (grid[fr][fc] == WALL) { fr = -1; break; }
                }
                if (fr < 0) continue;

                char d = grid[fr][fc];
                if (char.IsLower(d) && teleports.TryGetValue(d, out var dest))
                    (fr,fc) = dest;

                if (steps[fr,fc] == -1) {
                    steps[fr,fc] = s + 1;
                    queue.Enqueue((fr,fc));
                }
            }
        }

        Console.WriteLine(-1);
    }
}
