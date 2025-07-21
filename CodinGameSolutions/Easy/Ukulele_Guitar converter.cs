using System;
using System.Collections.Generic;
class Solution
{
    public static void Main(string[] args)
    {
        var mode = Console.ReadLine();
        var n = int.Parse(Console.ReadLine());
        var openG = new[] { 52, 47, 43, 38, 33, 28 };  // E4, B3, G3, D3, A2, E2
        var openU = new[] { 57, 52, 48, 55 };           // A4, E4, C4, G4
        var maxFretG = 21;
        var maxFretU = 15;

        for (var i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split();
            var str = int.Parse(parts[0]);
            var fret = int.Parse(parts[1]);
            var semitone = (mode == "guitar" ? openG[str] : openU[str]) + fret;

            var matches = new List<Tuple<int,int>>();
            if (mode == "guitar")
            {
                for (var s = 0; s < 4; s++)
                {
                    var f = semitone - openU[s];
                    if (f >= 0 && f <= maxFretU) matches.Add(Tuple.Create(s, f));
                }
            }
            else
            {
                for (var s = 0; s < 6; s++)
                {
                    var f = semitone - openG[s];
                    if (f >= 0 && f <= maxFretG) matches.Add(Tuple.Create(s, f));
                }
            }

            if (matches.Count == 0)
            {
                Console.WriteLine("no match");
            }
            else
            {
                matches.Sort((a, b) =>
                {
                    var d = a.Item1 - b.Item1;
                    return d != 0 ? d : a.Item2 - b.Item2;
                });
                for (var j = 0; j < matches.Count; j++)
                {
                    var m = matches[j];
                    Console.Write($"{m.Item1}/{m.Item2}");
                    if (j < matches.Count - 1) Console.Write(" ");
                }
                Console.WriteLine();
            }
        }
    }
}
