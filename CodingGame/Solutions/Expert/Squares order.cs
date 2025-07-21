using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var h = int.Parse(inputs[0]);
        var w = int.Parse(inputs[1]);
        var nb = int.Parse(Console.ReadLine());
        var lines = new string[h];
        for (var i = 0; i < h; i++)
        {
            lines[i] = Console.ReadLine();
        }

        var solver = new SquareSolver(h, w, nb, lines);
        var result = solver.Solve();

        foreach (var square in result)
        {
            Console.WriteLine($"{square.Label} {square.Size}");
        }
    }
}

public class Square
{
    public char Label { get; }
    public int Size { get; }

    public Square(char label, int size)
    {
        Label = label;
        Size = size;
    }
}

public class SquareSolver
{
    private readonly int _h;
    private readonly int _w;
    private readonly int _nb;
    private char[][] _grid;

    private class PeelInfo
    {
        public char Label;
        public int Size;
    }

    public SquareSolver(int h, int w, int nb, string[] lines)
    {
        _h = h;
        _w = w;
        _nb = nb;
        _grid = new char[h][];
        for (var i = 0; i < h; i++)
        {
            _grid[i] = lines[i].ToCharArray();
        }
    }

    public List<Square> Solve()
    {
        var foundSquares = new List<Square>();
        var labelsToFind = new List<char>();
        for (var i = 1; i <= _nb; i++)
        {
            labelsToFind.Add((char)(i + '0'));
        }

        while (labelsToFind.Count > 0)
        {
            PeelInfo foundThisTurn = null;
            foreach (var label in labelsToFind)
            {
                foundThisTurn = FindPeelableSquare(label);
                if (foundThisTurn != null)
                {
                    break;
                }
            }

            if (foundThisTurn != null)
            {
                foundSquares.Add(new Square(foundThisTurn.Label, foundThisTurn.Size));
                labelsToFind.Remove(foundThisTurn.Label);
                GloballyReplace(foundThisTurn.Label, '?');
            }
            else
            {
                break;
            }
        }

        foundSquares.Reverse();
        return foundSquares;
    }

    private int CountAllOccurrences(char label)
    {
        var count = 0;
        for (var r = 0; r < _h; r++)
        {
            for (var c = 0; c < _w; c++)
            {
                if (_grid[r][c] == label)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private PeelInfo FindPeelableSquare(char label)
    {
        var totalLabelCount = CountAllOccurrences(label);
        if (totalLabelCount == 0) return null;

        for (var r = 0; r < _h; r++)
        {
            for (var c = 0; c < _w; c++)
            {
                var maxSize = Math.Min(_h - r, _w - c);
                for (var s = 2; s <= maxSize; s++)
                {
                    var perimeter = GetPerimeter(r, c, s);
                    var perimeterLabelCount = 0;
                    var isClean = true;
                    foreach (var pChar in perimeter)
                    {
                        if (pChar == label)
                        {
                            perimeterLabelCount++;
                        }
                        else if (pChar != '?')
                        {
                            isClean = false;
                            break;
                        }
                    }

                    if (isClean && perimeterLabelCount == totalLabelCount)
                    {
                        return new PeelInfo { Label = label, Size = s };
                    }
                }
            }
        }
        return null;
    }

    private List<char> GetPerimeter(int r, int c, int s)
    {
        var perimeter = new List<char>();
        var rEnd = r + s - 1;
        var cEnd = c + s - 1;

        for (var i = r; i <= rEnd; i++)
        {
            for (var j = c; j <= cEnd; j++)
            {
                if (i == r || i == rEnd || j == c || j == cEnd)
                {
                    perimeter.Add(_grid[i][j]);
                }
            }
        }
        return perimeter;
    }

    private void GloballyReplace(char from, char to)
    {
        for (var r = 0; r < _h; r++)
        {
            for (var c = 0; c < _w; c++)
            {
                if (_grid[r][c] == from)
                {
                    _grid[r][c] = to;
                }
            }
        }
    }
}