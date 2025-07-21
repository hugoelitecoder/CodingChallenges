using System.Linq;
using System;
class Solution
{
    static void Main(string[] args)
    {
        var parts = Console.ReadLine()!.Split(' ');
        var serviceCount = int.Parse(parts[0]);
        var metricLines = int.Parse(parts[1]);
        var capacity = Console.ReadLine()!.Split(' ').Select(int.Parse).ToArray();
        var running = new int[serviceCount];

        for (var line = 0; line < metricLines; line++)
        {
            var clients = Console.ReadLine()!.Split(' ').Select(int.Parse).ToArray();
            var deltas = new int[serviceCount];

            for (var i = 0; i < serviceCount; i++)
            {
                var needed = (clients[i] + capacity[i] - 1) / capacity[i];
                deltas[i] = needed - running[i];
                running[i] = needed;
            }

            Console.WriteLine(string.Join(" ", deltas));
        }
    }
}
