using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        int[,] grid = new int[N, N];

        // Read input into grid
        for (int i = 0; i < N; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            for (int j = 0; j < N; j++)
            {
                grid[i, j] = int.Parse(inputs[j]);
            }
        }

        // Start position (middle of the grid)
        int startX = N / 2, startY = N / 2;
        
        // Directions for moving: Up, Down, Left, Right
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        // BFS queue
        Queue<(int, int)> queue = new Queue<(int, int)>();
        queue.Enqueue((startX, startY));

        // Visited set
        bool[,] visited = new bool[N, N];
        visited[startX, startY] = true;

        // BFS Traversal
        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            // Check if we reached a border with elevation 0
            if ((x == 0 || x == N - 1 || y == 0 || y == N - 1) && grid[x, y] == 0)
            {
                Console.WriteLine("yes");
                return;
            }

            // Explore all possible movements
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];

                // Check if within bounds
                if (nx >= 0 && nx < N && ny >= 0 && ny < N && !visited[nx, ny])
                {
                    // Only move if elevation difference is at most 1
                    if (Math.Abs(grid[x, y] - grid[nx, ny]) <= 1)
                    {
                        queue.Enqueue((nx, ny));
                        visited[nx, ny] = true;
                    }
                }
            }
        }

        // If BFS completes and we never reached an ocean, print "no"
        Console.WriteLine("no");
    }
}
