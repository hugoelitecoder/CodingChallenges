using System;
using System.Text;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var width = int.Parse(inputs[0]);
        var height = int.Parse(inputs[1]);
        var cx = float.Parse(inputs[2]);
        var cy = float.Parse(inputs[3]);
        var ro = float.Parse(inputs[4]);
        var ri = float.Parse(inputs[5]);
        var samples = int.Parse(inputs[6]);

        var renderer = new AsciiArtRenderer(width, height, cx, cy, ro, ri, samples);
        var artLines = renderer.Render();

        foreach (var line in artLines)
        {
            Console.WriteLine(line);
        }
    }
}

class AsciiArtRenderer
{
    private readonly int _width;
    private readonly int _height;
    private readonly double _cx;
    private readonly double _cy;
    private readonly int _samples;
    private readonly double _roSq;
    private readonly double _riSq;
    private readonly double _sampleStep;
    private readonly int _totalSamplesPerChar;

    private static readonly char[] CharMap = { ' ', '.', ':', '-', '=', '+', '*', '#', '%', '@', '@' };

    public AsciiArtRenderer(int width, int height, float cx, float cy, float ro, float ri, int samples)
    {
        _width = width;
        _height = height;
        _cx = cx;
        _cy = cy;
        _samples = samples;
        _roSq = (double)ro * ro;
        _riSq = (double)ri * ri;
        _sampleStep = 1.0 / samples;
        _totalSamplesPerChar = samples * samples;
    }

    public List<string> Render()
    {
        var result = new List<string>();
        result.Add("+" + new string('-', _width) + "+");

        for (var y = 0; y < _height; y++)
        {
            var lineBuilder = new StringBuilder();
            lineBuilder.Append('|');
            for (var x = 0; x < _width; x++)
            {
                var coverage = CalculateCoverage(x, y);
                var charIndex = (int)(coverage * 10);
                lineBuilder.Append(CharMap[charIndex]);
            }
            lineBuilder.Append('|');
            result.Add(lineBuilder.ToString());
        }

        result.Add("+" + new string('-', _width) + "+");
        return result;
    }

    private double CalculateCoverage(int px, int py)
    {
        var insideCount = 0;
        var startOffset = _sampleStep / 2.0;
        for (var j = 0; j < _samples; j++)
        {
            var sy = py + startOffset + j * _sampleStep;
            for (var i = 0; i < _samples; i++)
            {
                var sx = px + startOffset + i * _sampleStep;
                if (IsPointInRing(sx, sy))
                {
                    insideCount++;
                }
            }
        }
        return (double)insideCount / _totalSamplesPerChar;
    }

    private bool IsPointInRing(double x, double y)
    {
        var dx = x - _cx;
        var dy = y - _cy;
        var distSq = (0.5 * dx) * (0.5 * dx) + dy * dy;
        return distSq >= _riSq && distSq <= _roSq;
    }
}