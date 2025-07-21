using System;
using System.Collections.Generic;

class Solution
{
    static int N, M;
    static char[,] grid;

    static readonly int[] dx = { -1, 0, 1, 0 };
    static readonly int[] dy = {  0, 1, 0,-1 };

    static void Main()
    {
        var parts = Console.ReadLine()!.Split();
        N = int.Parse(parts[0]);
        M = int.Parse(parts[1]);

        grid = new char[N, M];
        for (int i = 0; i < N; i++)
        {
            var line = Console.ReadLine()!;
            for (int j = 0; j < M; j++)
                grid[i, j] = line[j];
        }

        int maxLen = 0, maxCount = 0;
        while (true)
        {
            var head = FindHead();
            if (head.x < 0) break;
            int length = RemoveSnake(head.x, head.y);
            if (length > maxLen)
            {
                maxLen = length;
                maxCount = 1;
            }
            else if (length == maxLen)
            {
                maxCount++;
            }
        }

        Console.WriteLine(maxLen);
        Console.WriteLine(maxCount);
    }

    // Find next 'o' in the grid
    static (int x,int y) FindHead()
    {
        for (int i = 0; i < N; i++)
            for (int j = 0; j < M; j++)
                if (grid[i, j] == 'o')
                    return (i, j);
        return (-1, -1);
    }

    // Remove the snake starting at head (x,y), return its length
    static int RemoveSnake(int x, int y)
    {
        int size = 1;
        int dir = -1;
        // erase head
        grid[x, y] = '.';

        // determine initial direction by neighbor
        if (In(x-1,y) && grid[x-1,y] == '|') { x--; dir = 0; }
        else if (In(x+1,y) && grid[x+1,y] == '|') { x++; dir = 2; }
        else if (In(x,y-1) && grid[x,y-1] == '-') { y--; dir = 3; }
        else if (In(x,y+1) && grid[x,y+1] == '-') { y++; dir = 1; }
        size++;
        grid[x, y] = '.';

        // follow until we hit a head marker
        while (!"^<>v".Contains(grid[x, y]))
        {
            if (dir == 0 || dir == 2)
            {
                // move vertically
                x += (dir == 2 ? 1 : -1);
                if (grid[x, y] == '*')
                {
                    // turn at curve
                    if (In(x, y+1) && grid[x, y+1] == '-')
                        dir = 1;
                    else
                        dir = 3;
                    grid[x, y] = '.';
                }
                else if (grid[x, y] == '|')
                {
                    grid[x, y] = '.';
                }
            }
            else
            {
                // move horizontally
                y += (dir == 1 ? 1 : -1);
                if (grid[x, y] == '*')
                {
                    // turn at curve
                    if (In(x+1, y) && grid[x+1, y] == '|')
                        dir = 2;
                    else
                        dir = 0;
                    grid[x, y] = '.';
                }
                else if (grid[x, y] == '-')
                {
                    grid[x, y] = '.';
                }
            }
            size++;
        }

        // erase the final head marker (^<>v)
        grid[x, y] = '.';
        return size;
    }

    static bool In(int x, int y)
        => x >= 0 && x < N && y >= 0 && y < M;
}
