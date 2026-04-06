using System;
using System.Diagnostics;

public class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var line = Console.ReadLine();
        if (line == null) return;
        var parts = line.Split(' ');
        var w = int.Parse(parts[0]);
        var h = int.Parse(parts[1]);
        var k = int.Parse(Console.ReadLine());
        var grid = new string[h];
        for (var i = 0; i < h; i++)
        {
            grid[i] = Console.ReadLine();
            Console.Error.WriteLine("[DEBUG] ROW " + i + ": " + grid[i]);
        }
        var simulator = new SolarRoofSimulator();
        var result = simulator.CalculateTotalPower(w, h, k, grid);
        sw.Stop();
        Console.Error.WriteLine("[DEBUG] RESULT POWER: " + result);
        Console.Error.WriteLine("[DEBUG] CALCULATION TIME: " + sw.ElapsedMilliseconds + "ms");
        Console.WriteLine(result);
    }
}

public class SolarRoofSimulator
{
    public int CalculateTotalPower(int w, int h, int k, string[] grid)
    {
        var total = 0;
        for (var x = 0; x < w; x++)
        {
            var minY = h;
            for (var y = h - 1; y >= 0; y--)
            {
                var c = grid[y][x];
                if (c == 'P')
                {
                    if (y < minY)
                    {
                        total += 100;
                    }
                }
                else if (char.IsDigit(c))
                {
                    var reach = y - ((c - '0') * k);
                    if (reach < minY)
                    {
                        minY = reach;
                    }
                }
            }
        }
        return total;
    }
}