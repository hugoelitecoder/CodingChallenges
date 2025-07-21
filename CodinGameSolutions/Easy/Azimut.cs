using System;
using System.Collections.Generic;

class Solution
{
    private static readonly string[] _dirOrder = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
    private static readonly Dictionary<string,int> _dirToIndex = new Dictionary<string,int>
    {
        { "N", 0 }, { "NE", 1 }, { "E", 2 }, { "SE", 3 },
        { "S", 4 }, { "SW", 5 }, { "W", 6 }, { "NW", 7 }
    };
    private static readonly Dictionary<string,int> _turnOffset = new Dictionary<string,int>
    {
        { "RIGHT", 1 }, { "LEFT", -1 }, { "BACK", 4 }, { "FORWARD", 0 }
    };

    public static void Main(string[] args)
    {
        var startDirection = Console.ReadLine();
        var N = int.Parse(Console.ReadLine());
        var index = _dirToIndex[startDirection];
        for (var i = 0; i < N; i++)
        {
            var turn = Console.ReadLine();
            index = (index + _turnOffset[turn] + 8) % 8;
        }
        Console.WriteLine(_dirOrder[index]);
    }
   
}
