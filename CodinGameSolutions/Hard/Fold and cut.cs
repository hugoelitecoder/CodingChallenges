using System;

class Solution
{
    public static void Main(string[] args)
    {
        var input = Console.ReadLine();
        var folding = PaperFolding.Parse(input);
        var result = folding.CountHolesAfterUnfold();
        Console.WriteLine(result);
    }
}

public class PaperFolding
{
    private readonly string _folds;
    private readonly string _cut;
    private double _xLow = 0.0;
    private double _xHigh = 1.0;
    private double _yLow = 0.0;
    private double _yHigh = 1.0;

    private PaperFolding(string folds, string cut)
    {
        _folds = folds;
        _cut = cut;
    }

    public static PaperFolding Parse(string input)
    {
        var sep = input.IndexOf('-');
        var folds = input.Substring(0, sep);
        var cut = input.Substring(sep + 1);
        return new PaperFolding(folds, cut);
    }

    public int CountHolesAfterUnfold()
    {
        ApplyFolds();
        var xCount = CountHolesOnAxis(_xLow, _xHigh, _cut.Contains("l"));
        var yCount = CountHolesOnAxis(_yLow, _yHigh, _cut.Contains("b"));
        return xCount * yCount;
    }

    private void ApplyFolds()
    {
        foreach (var f in _folds)
        {
            switch (f)
            {
                case 'R':
                    _xHigh = (_xLow + _xHigh) / 2.0;
                    break;
                case 'L':
                    _xLow = (_xLow + _xHigh) / 2.0;
                    break;
                case 'T':
                    _yHigh = (_yLow + _yHigh) / 2.0;
                    break;
                case 'B':
                    _yLow = (_yLow + _yHigh) / 2.0;
                    break;
            }
        }
    }

    private int CountHolesOnAxis(double low, double high, bool atLow)
    {
        var scale = (int)Math.Round(1.0 / (high - low));
        if (scale == 1) return 0;
        var cutIndex = (int)Math.Round((atLow ? low : high) * scale);
        if (cutIndex % 2 == 1)
            return scale / 2;
        return Math.Max(scale / 2 - 1, 0);
    }
}
