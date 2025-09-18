using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var parts = Console.ReadLine().Split();
        var n = int.Parse(parts[0]);
        var up = int.Parse(parts[1]);
        var down = int.Parse(parts[2]);
        var start = int.Parse(parts[3]);
        var goal = int.Parse(parts[4]);
        var result = Elevator.Move(n, up, down, start, goal);
        Console.WriteLine(result);
    }

    class Elevator
    {
        public static string Move(int n, int up, int down, int start, int goal)
        {
            if (start == goal) return "0";
            var visited = new bool[n + 1];
            var queue = new Queue<(int floor, int steps)>();
            visited[start] = true;
            queue.Enqueue((start, 0));
            while (queue.Count > 0)
            {
                var (floor, steps) = queue.Dequeue();
                var nextUp = floor + up;
                var nextDown = floor - down;
                if (nextUp <= n && !visited[nextUp])
                {
                    if (nextUp == goal) return (steps + 1).ToString();
                    visited[nextUp] = true;
                    queue.Enqueue((nextUp, steps + 1));
                }
                if (nextDown >= 1 && !visited[nextDown])
                {
                    if (nextDown == goal) return (steps + 1).ToString();
                    visited[nextDown] = true;
                    queue.Enqueue((nextDown, steps + 1));
                }
            }
            return "IMPOSSIBLE";
        }
    }
}
