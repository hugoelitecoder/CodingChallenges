using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static Dictionary<int, Room> rooms;
    static Dictionary<int, int> memo;

    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        rooms = Enumerable.Range(0, N)
            .Select(_ => Console.ReadLine().Split())
            .ToDictionary(
                tokens => int.Parse(tokens[0]),
                tokens => new Room(
                    int.Parse(tokens[1]),
                    tokens[2],
                    tokens[3]
                )
            );
        memo = new Dictionary<int, int>();
        Console.WriteLine(DFS(0));
    }

    static int DFS(int current) =>
        memo.TryGetValue(current, out var val) ? val :
        memo[current] = rooms[current].Doors
            .Select(door => door == "E" ? 0 : DFS(int.Parse(door)))
            .Max() + rooms[current].Money;

    class Room
    {
        public int Money;
        public string[] Doors;
        public Room(int money, string door1, string door2)
        {
            Money = money;
            Doors = new[] { door1, door2 };
        }
    }
}
