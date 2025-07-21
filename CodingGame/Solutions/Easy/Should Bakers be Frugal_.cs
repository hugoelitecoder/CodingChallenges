using System;

class Solution
{
    static void Main()
    {
        var inputs = Console.ReadLine().Split(' ');
        double side = double.Parse(inputs[0]);
        double diameter = double.Parse(inputs[1]);

        Console.WriteLine(FrugalBonusBiscuits(side, diameter));
    }

    static int FrugalBonusBiscuits(double side, double diameter)
    {
        int WastefulBiscuits(double s) => (int)(s / diameter) * (int)(s / diameter);
        double BiscuitArea() => Math.PI * Math.Pow(diameter / 2, 2);

        int wasteful = WastefulBiscuits(side);
        int frugal = 0;
        double remainingArea = side * side;

        while (true)
        {
            double doughSide = Math.Sqrt(remainingArea);
            int count = WastefulBiscuits(doughSide);
            if (count == 0) break;

            frugal += count;
            remainingArea -= BiscuitArea() * count;
        }

        return frugal - wasteful;
    }
}
