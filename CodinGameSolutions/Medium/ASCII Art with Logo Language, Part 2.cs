using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var programLines = new List<string>();
        for (int i = 0; i < n; i++)
            programLines.Add(Console.ReadLine());
        var turtle = new Turtle();
        var output = turtle.Run(programLines);
        Console.Write(output);
    }
}

class Turtle
{
    private readonly Dictionary<(int x,int y), char> _screen = new();
    private char _fill = ' ';
    private int _minX = int.MaxValue, _maxX = int.MinValue;
    private int _minY = int.MaxValue, _maxY = int.MinValue;

    private int _x, _y;
    private int _angle = 90;
    private bool _penDown = true;
    private string _penChars = "#";
    private int _penIndex;

    private List<string> _tokens = null!;
    private int _pos;

    private static readonly Dictionary<int,(int dx,int dy)> _dirs = new()
    {
        [0]   = (1, 0),
        [45]  = (1, 1),
        [90]  = (0, 1),
        [135] = (-1,1),
        [180] = (-1,0),
        [225] = (-1,-1),
        [270] = (0,-1),
        [315] = (1,-1),
    };

    public string Run(List<string> lines)
    {
        ClearScreen(' ');
        _tokens = Preprocess(lines);
        _pos = 0;
        ExecuteBlock();
        return Render();
    }

    private List<string> Preprocess(List<string> lines)
    {
        var all = string.Join(";", lines)
            .Replace(";", " ; ")
            .Replace("[", " [ ")
            .Replace("]", " ] ")
            .Split(new[]{' ','\t','\r','\n'}, StringSplitOptions.RemoveEmptyEntries)
            .ToList();
        return all;
    }

    private string? NextToken()
    {
        if (_pos < _tokens.Count)
            return _tokens[_pos++];
        return null;
    }

    private void ExecuteBlock()
    {
        while (_pos < _tokens.Count)
        {
            var tok = NextToken();
            if (tok == null) break;
            switch (tok.ToUpper())
            {
                case ";":
                    break;
                case "]":
                    return;
                case "CS":
                    CS();
                    break;
                case "FD":
                    FD();
                    break;
                case "PU":
                    _penDown = false;
                    break;
                case "PD":
                    _penDown = true;
                    break;
                case "RT":
                    RotateRight();
                    break;
                case "LT":
                    RotateLeft();
                    break;
                case "SETPC":
                    SetPenChars();
                    break;
                case "RP":
                    Repeat();
                    break;
                default:
                    break;
            }
        }
    }

    private void CS()
    {
        var ch = NextToken()![0];
        ClearScreen(ch);
    }

    private void FD()
    {
        int count = int.Parse(NextToken()!);
        for (int i = 0; i < count; i++)
            Step();
    }

    private void RotateRight()
    {
        int deg = int.Parse(NextToken()!);
        _angle = (_angle - deg) % 360;
        if (_angle < 0) _angle += 360;
    }

    private void RotateLeft()
    {
        int deg = int.Parse(NextToken()!);
        _angle = (_angle + deg) % 360;
    }

    private void SetPenChars()
    {
        _penChars = NextToken()!;
        _penIndex = 0;
    }

    private void Repeat()
    {
        int times = int.Parse(NextToken()!);
        var bracket = NextToken();
        int start = _pos;
        for (int i = 0; i < times; i++)
        {
            _pos = start;
            ExecuteBlock();
        }
    }

    private void Step()
    {
        if (_penDown)
            Draw();
        var a = _angle % 360;
        var (dx, dy) = _dirs[a];
        _x += dx;
        _y += dy;
    }

    private void Draw()
    {
        var ch = _penChars[_penIndex];
        _penIndex = (_penIndex + 1) % _penChars.Length;
        _screen[(_x, _y)] = ch;
        if (_x < _minX) _minX = _x;
        if (_x > _maxX) _maxX = _x;
        if (_y < _minY) _minY = _y;
        if (_y > _maxY) _maxY = _y;
    }

    private void ClearScreen(char fill)
    {
        _screen.Clear();
        _fill = fill;
        _x = _y = 0;
        _angle = 90;
        _penDown = true;
        _penChars = "#";
        _penIndex = 0;
        _minX = _minY = int.MaxValue;
        _maxX = _maxY = int.MinValue;
    }

    private string Render()
    {
        if (_screen.Count == 0)
            return "";
        int w = _maxX - _minX + 1;
        int h = _maxY - _minY + 1;
        var grid = Enumerable.Range(0, h)
            .Select(_ => Enumerable.Repeat(_fill, w).ToArray())
            .ToArray();

        foreach (var kv in _screen)
        {
            var (x,y) = kv.Key;
            int gx = x - _minX;
            int gy = _maxY - y;
            grid[gy][gx] = kv.Value;
        }

        var sb = new StringBuilder();
        for (int r = 0; r < h; r++)
        {
            var line = new string(grid[r]).TrimEnd();
            sb.AppendLine(line);
        }
        return sb.ToString();
    }
}
