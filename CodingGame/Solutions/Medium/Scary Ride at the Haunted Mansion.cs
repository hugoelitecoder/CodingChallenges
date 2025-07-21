using System;
using System.Linq;

class Solution
{
    const int Capacity = 10;

    static void Main()
    {
        Console.ReadLine();
        var groups = Console.ReadLine().Split();
        var ride = CreateRide();
        bool foundYou = false;
        int rideNumber = 1;

        foreach (var group in groups)
        {
            ride = PlaceGroup(ride, group, out var departed);
            if (departed != null)
            {
                if (foundYou)
                {
                    Console.WriteLine(rideNumber);
                    PrintRide(departed);
                    return;
                }
                rideNumber++;
            }
            if (group.Contains('x'))
                foundYou = true;
        }

        if (foundYou)
        {
            Console.WriteLine(rideNumber);
            PrintRide(ride);
        }
    }

    static char[][] CreateRide() => new[]
    {
        Enumerable.Repeat('D', Capacity).ToArray(),
        Enumerable.Repeat('D', Capacity).ToArray()
    };

    static void PrintRide(char[][] ride)
    {
        Console.WriteLine("/< |" + string.Concat(ride[0].Select(c => $" {c} |")));
        Console.WriteLine("\\< |" + string.Concat(ride[1].Select(c => $" {c} |")));
    }

    static char[][] PlaceGroup(char[][] ride, string group, out char[][] departed)
    {
        departed = null;
        int adultCount = group.Count(c => c == 'A');
        int kidCount   = group.Count(c => c == 'k' || c == 'x');
        if (adultCount < kidCount || kidCount + (adultCount - kidCount + 1) / 2 > Capacity)
            return ride;

        int startIdx = Array.IndexOf(ride[0], 'D');
        if (startIdx < 0) startIdx = Capacity;
        int needed = kidCount + (adultCount - kidCount + 1) / 2;

        if (Capacity - startIdx < needed)
        {
            PrintRideDebug(ride);
            var fresh = CreateRide();
            var placed = PlaceGroup(fresh, group, out _);
            departed = ride;
            return placed;
        }

        bool hasX = group.Contains('x');
        for (int j = 0; j < kidCount; j++)
        {
            ride[0][startIdx + j] = 'A';
            ride[1][startIdx + j] = (j == 0 && hasX) ? 'x' : 'k';
        }

        int pairs = (adultCount - kidCount + 1) / 2;
        for (int j = 0; j < pairs; j++)
        {
            ride[0][startIdx + kidCount + j] = 'A';
            if (j < pairs - 1 || (adultCount - kidCount) % 2 == 0)
                ride[1][startIdx + kidCount + j] = 'A';
        }

        return ride;
    }

    static void PrintRideDebug(char[][] ride)
    {
        Console.Error.WriteLine("/< |" + string.Concat(ride[0].Select(c => $" {c} |")));
        Console.Error.WriteLine("\\< |" + string.Concat(ride[1].Select(c => $" {c} |")));
    }
}
