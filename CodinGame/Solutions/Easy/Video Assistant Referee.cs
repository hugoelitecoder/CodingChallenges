using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    const int WIDTH = 51, HEIGHT = 15;
    static void Main()
    {
        var grid = new char[HEIGHT][];
        for (int y = 0; y < HEIGHT; y++)
            grid[y] = Console.ReadLine().ToCharArray();

        int ballX = -1, ballY = -1;
        bool attackA = false;
        for (int y = 0; y < HEIGHT; y++)
        for (int x = 0; x < WIDTH; x++)
        {
            if (grid[y][x] == 'o' || grid[y][x] == 'O')
            {
                ballX = x;
                ballY = y;
                attackA = (grid[y][x] == 'o');
            }
        }

        bool throwIn = (ballY == 0 || ballY == HEIGHT - 1);
        var attackers = new List<(int x,int y,bool active)>();
        var defenders = new List<int>();
        for (int y = 0; y < HEIGHT; y++)
        for (int x = 0; x < WIDTH; x++)
        {
            char c = grid[y][x];
            if (attackA)
            {
                if (c == 'a' || c == 'A')
                    attackers.Add((x,y,c=='A'));
                else if (c == 'b' || c == 'B')
                    defenders.Add(x);
            }
            else
            {
                if (c == 'b' || c == 'B')
                    attackers.Add((x,y,c=='B'));
                else if (c == 'a' || c == 'A')
                    defenders.Add(x);
            }
        }

        Func<int,int> distToGoal; 
        Func<int,bool> inOpponentHalf;
        if (attackA)
        {
            inOpponentHalf = x => x < WIDTH/2;
            distToGoal = x => x;
        }
        else
        {
            inOpponentHalf = x => x > WIDTH/2;
            distToGoal = x => WIDTH - 1 - x;
        }

        int ballDist = distToGoal(ballX);
        int offsideCount = 0;
        bool offence = false;
        if (!throwIn && defenders.Count >= 2)
        {
            var sorted = defenders
                .Select(x => (x, d: distToGoal(x)))
                .OrderBy(t => t.d)
                .ToList();
            int secondLastDist = sorted[1].d;

            foreach (var (x,y,active) in attackers)
            {
                if (!inOpponentHalf(x)) continue;
                int d = distToGoal(x);
                if (d < ballDist && d < secondLastDist)
                {
                    offsideCount++;
                    if (active) offence = true;
                }
            }
        }

        if (offsideCount == 0)
            Console.WriteLine("No player in an offside position.");
        else
            Console.WriteLine($"{offsideCount} player(s) in an offside position.");

        Console.WriteLine(offence
            ? "VAR: OFFSIDE!"
            : "VAR: ONSIDE!");
    }
}
