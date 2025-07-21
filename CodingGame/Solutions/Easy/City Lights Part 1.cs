using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var h = int.Parse(Console.ReadLine());
        var w = int.Parse(Console.ReadLine());
        var grid = new string[h];
        for (var i = 0; i < h; i++)
        {
            grid[i] = Console.ReadLine();
        }

        var sources = FindSources(h, w, grid);
        var brightnessGrid = CalculateBrightnessGrid(h, w, sources);

        for (var y = 0; y < h; y++)
        {
            var rowBuilder = new StringBuilder(w);
            for (var x = 0; x < w; x++)
            {
                var brightness = brightnessGrid[y, x];
                var cellChar = GetCharFromBrightness(brightness);
                rowBuilder.Append(cellChar);
            }
            Console.WriteLine(rowBuilder.ToString());
        }
    }

    private static List<LightSource> FindSources(int h, int w, string[] grid)
    {
        var sources = new List<LightSource>();
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var cellChar = grid[y][x];
                if (cellChar != '.')
                {
                    var radius = GetRadiusFromChar(cellChar);
                    var source = new LightSource(x, y, radius);
                    sources.Add(source);
                }
            }
        }
        return sources;
    }

    private static int[,] CalculateBrightnessGrid(int h, int w, List<LightSource> sources)
    {
        var brightnessGrid = new int[h, w];
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var totalBrightness = 0;
                foreach (var source in sources)
                {
                    var dx = source.X - x;
                    var dy = source.Y - y;
                    var dist = Math.Sqrt(dx * dx + dy * dy);
                    var roundedDist = (int)Math.Round(dist);
                    var brightness = source.Radius - roundedDist;
                    if (brightness > 0)
                    {
                        totalBrightness += brightness;
                    }
                }
                brightnessGrid[y, x] = totalBrightness;
            }
        }
        return brightnessGrid;
    }

    private static int GetRadiusFromChar(char c)
    {
        if (c >= '1' && c <= '9')
        {
            return c - '0';
        }
        if (c >= 'A' && c <= 'Z')
        {
            return c - 'A' + 10;
        }
        return 0;
    }

    private static char GetCharFromBrightness(int brightness)
    {
        if (brightness <= 9)
        {
            return (char)(brightness + '0');
        }
        if (brightness <= 35)
        {
            return (char)(brightness - 10 + 'A');
        }
        return 'Z';
    }
}

public class LightSource
{
    public int X { get; }
    public int Y { get; }
    public int Radius { get; }

    public LightSource(int x, int y, int radius)
    {
        X = x;
        Y = y;
        Radius = radius;
    }
}