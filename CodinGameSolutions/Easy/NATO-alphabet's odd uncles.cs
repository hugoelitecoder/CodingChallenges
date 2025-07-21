using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var versions = new List<string[]>
        {
            new[] { "Authority", "Bills", "Capture", "Destroy", "Englishmen", "Fractious", "Galloping", "High", "Invariably", "Juggling", "Knights", "Loose", "Managing", "Never", "Owners", "Play", "Queen", "Remarks", "Support", "The", "Unless", "Vindictive", "When", "Xpeditiously", "Your", "Zigzag" },
            new[] { "Apples", "Butter", "Charlie", "Duff", "Edward", "Freddy", "George", "Harry", "Ink", "Johnnie", "King", "London", "Monkey", "Nuts", "Orange", "Pudding", "Queenie", "Robert", "Sugar", "Tommy", "Uncle", "Vinegar", "Willie", "Xerxes", "Yellow", "Zebra" },
            new[] { "Amsterdam", "Baltimore", "Casablanca", "Denmark", "Edison", "Florida", "Gallipoli", "Havana", "Italia", "Jerusalem", "Kilogramme", "Liverpool", "Madagascar", "New-York", "Oslo", "Paris", "Quebec", "Roma", "Santiago", "Tripoli", "Uppsala", "Valencia", "Washington", "Xanthippe", "Yokohama", "Zurich" },
            new[] { "Alfa", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel", "India", "Juliett", "Kilo", "Lima", "Mike", "November", "Oscar", "Papa", "Quebec", "Romeo", "Sierra", "Tango", "Uniform", "Victor", "Whiskey", "X-ray", "Yankee", "Zulu" }
        };

        var input = Console.ReadLine().Split();

        int matchedVersion = -1;
        for (int v = 0; v < versions.Count; v++)
        {
            var version = versions[v];
            if (input.All(w => version.Contains(w)))
            {
                matchedVersion = v;
                break;
            }
        }

        if (matchedVersion == -1)
        {
            Console.WriteLine("Unknown alphabet version.");
            return;
        }

        var current = versions[matchedVersion];
        var next = matchedVersion < versions.Count - 1 ? versions[matchedVersion + 1] : versions[0];

        var result = input.Select(word =>
        {
            int idx = Array.IndexOf(current, word);
            return next[idx];
        });

        Console.WriteLine(string.Join(" ", result));
    }
}
