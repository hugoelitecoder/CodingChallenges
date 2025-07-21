using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        for (var i = 0; i < n; i++)
        {
            var tokens = Console.ReadLine().Split();
            var from = tokens[0];
            var to = tokens[1];
            var result = GetIntervalName(from, to);
            Console.WriteLine(result);
        }
    }

    private static string GetIntervalName(string from, string to)
    {
        var noteMap = new Dictionary<char, int> { { 'A', 0 }, { 'B', 1 }, { 'C', 2 }, { 'D', 3 }, { 'E', 4 }, { 'F', 5 }, { 'G', 6 } };
        var semitoneMap = new Dictionary<char, int> { { 'A', 0 }, { 'B', 2 }, { 'C', 3 }, { 'D', 5 }, { 'E', 7 }, { 'F', 8 }, { 'G', 10 } };
        var names = new[] { "Prime", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh" };
        var refSemitones = new Dictionary<string, int> { { "Prime", 0 }, { "Second", 2 }, { "Third", 3 }, { "Fourth", 5 }, { "Fifth", 7 }, { "Sixth", 8 }, { "Seventh", 10 }, { "Octave", 12 } };
        var refType = new Dictionary<string, string> { { "Prime", "Perfect" }, { "Second", "Major" }, { "Third", "Minor" }, { "Fourth", "Perfect" }, { "Fifth", "Perfect" }, { "Sixth", "Minor" }, { "Seventh", "Minor" }, { "Octave", "Perfect" } };
        var qualPerfect = new Dictionary<int, string> { { -1, "diminished" }, { 0, "perfect" }, { 1, "augmented" } };
        var qualMajor = new Dictionary<int, string> { { -2, "diminished" }, { -1, "minor" }, { 0, "major" }, { 1, "augmented" } };
        var qualMinor = new Dictionary<int, string> { { -1, "diminished" }, { 0, "minor" }, { 1, "major" }, { 2, "augmented" } };

        var (fromLetter, fromAcc) = ParsePitch(from);
        var (toLetter, toAcc) = ParsePitch(to);
        var fromVal = noteMap[fromLetter];
        var toVal = noteMap[toLetter];

        var intervalIdx = (toVal - fromVal + 7) % 7;
        var fromSem = semitoneMap[fromLetter] + fromAcc;
        var toSem = semitoneMap[toLetter] + toAcc;
        var semitones = toSem - fromSem;
        string intervalName;
        if (intervalIdx != 0)
        {
            intervalName = names[intervalIdx];
            if (toVal < fromVal) semitones += 12;
        }
        else
        {
            if (from == to)
            {
                intervalName = "Octave";
                semitones = 12;
            }
            else
            {
                if (semitones < 0)
                {
                    semitones += 12;
                    intervalName = "Octave";
                }
                else
                {
                    intervalName = "Prime";
                }
            }
        }

        var refSem = refSemitones[intervalName];
        var refQ = refType[intervalName];
        var diff = semitones - refSem;
        string quality;
        if (refQ == "Perfect")
            quality = qualPerfect[diff];
        else if (refQ == "Major")
            quality = qualMajor[diff];
        else
            quality = qualMinor[diff];

        return quality + " " + intervalName.ToLower();
    }

    private static (char, int) ParsePitch(string pitch)
    {
        var letter = pitch[0];
        var acc = 0;
        if (pitch.Length > 1)
        {
            if (pitch[1] == '+') acc = 1;
            else if (pitch[1] == '-') acc = -1;
        }
        return (letter, acc);
    }
}
