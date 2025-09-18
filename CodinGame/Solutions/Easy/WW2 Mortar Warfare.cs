using System;
using System.Globalization;
using System.Linq;

class Solution
{
    static void Main()
    {
        string line = Console.ReadLine();
        string digitStr = new string(line.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(digitStr))
        {
            Console.WriteLine("OUT OF RANGE");
            return;
        }
        int R = int.Parse(digitStr, CultureInfo.InvariantCulture);
        const double v = 158.0;
        const double g = 9.8;
        const double maxRange = 1800.0;

        if (R > maxRange)
        {
            Console.WriteLine("OUT OF RANGE");
            return;
        }

        double arg = R * g / (v * v);
        if (arg < 0.0 || arg > 1.0)
        {
            Console.WriteLine("OUT OF RANGE");
            return;
        }

        double theta1 = 0.5 * Math.Asin(arg);
        double theta2 = Math.PI / 2 - theta1;
        double deg1 = theta1 * 180.0 / Math.PI;
        double deg2 = theta2 * 180.0 / Math.PI;
        const double minElev = 40.0;
        const double maxElev = 85.0;

        double chosenDeg;
        double chosenRad;
        bool ok1 = deg1 >= minElev && deg1 <= maxElev;
        bool ok2 = deg2 >= minElev && deg2 <= maxElev;
        if (ok1 && (!ok2 || deg1 <= deg2))
        {
            chosenDeg = deg1;
            chosenRad = theta1;
        }
        else if (ok2)
        {
            chosenDeg = deg2;
            chosenRad = theta2;
        }
        else
        {
            Console.WriteLine("OUT OF RANGE");
            return;
        }

        double t = 2 * v * Math.Sin(chosenRad) / g;
        double angleOut = Math.Round(chosenDeg, 1, MidpointRounding.AwayFromZero);
        double timeOut  = Math.Round(t,         1, MidpointRounding.AwayFromZero);

        Console.WriteLine($"{angleOut:F1} degrees");
        Console.WriteLine($"{timeOut:F1} seconds");
    }
}
