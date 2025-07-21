using System;

class Solution
{
    public static void Main(string[] args)
    {
        var solver = new TetrisSolver();
        solver.ReadInput();
        var result = solver.GetBestPlacement();
        Console.WriteLine($"{result[0]} {result[1]}");
        Console.WriteLine(result[2]);
    }
}

class TetrisSolver
{
    int _shapeW;
    int _shapeH;
    int _fieldW;
    int _fieldH;
    string[] _shape;
    string[] _field;

    public void ReadInput()
    {
        var shapeDims = Console.ReadLine().Split();
        _shapeW = int.Parse(shapeDims[0]);
        _shapeH = int.Parse(shapeDims[1]);
        _shape = new string[_shapeH];
        for (var i = 0; i < _shapeH; ++i) _shape[i] = Console.ReadLine();
        var fieldDims = Console.ReadLine().Split();
        _fieldW = int.Parse(fieldDims[0]);
        _fieldH = int.Parse(fieldDims[1]);
        _field = new string[_fieldH];
        for (var i = 0; i < _fieldH; ++i) _field[i] = Console.ReadLine();
    }

    public int[] GetBestPlacement()
    {
        var bestX = 0;
        var bestY = 0;
        var bestLines = -1;
        for (var y = _shapeH - 1; y < _fieldH; ++y)
        {
            var fieldY = _fieldH - 1 - y;
            for (var x = 0; x <= _fieldW - _shapeW; ++x)
            {
                if (!CanFit(x, fieldY)) continue;
                var lines = GetClearedLines(x, fieldY);
                if (lines > bestLines)
                {
                    bestX = x;
                    bestY = y;
                    bestLines = lines;
                }
            }
        }
        return new[] { bestX, bestY, bestLines };
    }

    bool CanFit(int x, int fieldY)
    {
        for (var row = 0; row < _shapeH; ++row)
            for (var col = 0; col < _shapeW; ++col)
                if (_field[fieldY + row][x + col] == '*' && _shape[row][col] == '*')
                    return false;
        return true;
    }

    int GetClearedLines(int x, int fieldY)
    {
        var lines = 0;
        for (var row = 0; row < _shapeH; ++row)
        {
            var buffer = _field[fieldY + row].Substring(0, x);
            for (var col = 0; col < _shapeW; ++col)
            {
                if (_field[fieldY + row][x + col] == '*' || _shape[row][col] == '*')
                    buffer += '*';
                else
                    buffer += '.';
            }
            buffer += _field[fieldY + row].Substring(x + _shapeW);
            if (IsFull(buffer)) ++lines;
        }
        return lines;
    }

    bool IsFull(string s)
    {
        for (var i = 0; i < s.Length; ++i)
            if (s[i] != '*') return false;
        return true;
    }
}
