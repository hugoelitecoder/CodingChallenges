using System;
using System.Collections.Generic;

public class Solution
{
    public static void Main()
    {
        var N = int.Parse(Console.ReadLine());
        var calibration = 0;
        var cLength = 0;
        var Lcount = 0;
        var Rcount = 0;
        var spots = new List<(int idx, char side)>();

        for (var i = 0; i < N; i++)
        {
            var s = Console.ReadLine();
            var FL = s[0] == '1';
            var FR = s[1] == '1';
            var BR = s[2] == '1';
            var BL = s[3] == '1';

            if (calibration == 0)
            {
                if (!FL)
                {
                    cLength++;
                    calibration = 1;
                }
            }
            else if (calibration == 1)
            {
                cLength++;
                if (!BL)
                    calibration = 2;
            }
            else 
            {
                if (!FL) Lcount++; else Lcount = 0;
                if (!FR) Rcount++; else Rcount = 0;
                if (Lcount >= cLength)
                    spots.Add((i + cLength - 1, 'L'));
                if (Rcount >= cLength)
                    spots.Add((i + cLength - 1, 'R'));
            }
        }

        Console.WriteLine(cLength);
        foreach (var (idx, side) in spots)
            Console.WriteLine($"{idx}{side}");
    }
}
