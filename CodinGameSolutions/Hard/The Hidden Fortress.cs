using System;
using System.Text;

class Solution
{
    static void Main(string[] args)
    {
        var sizeStr = Console.ReadLine();
        var size = int.Parse(sizeStr);
        var rows = new string[size];
        for (var i = 0; i < size; i++)
        {
            rows[i] = Console.ReadLine();
        }
        var fortressGrid = new FortressGrid(size, rows);
        var revealedGrid = fortressGrid.RevealLocations();
        foreach (var row in revealedGrid)
        {
            Console.WriteLine(row);
        }
    }
}

public class FortressGrid
{
    private readonly int _size;
    private readonly int[,] _grid;

    public FortressGrid(int size, string[] rows)
    {
        _size = size;
        _grid = new int[_size, _size];
        ParseGrid(rows);
    }

    public string[] RevealLocations()
    {
        var totalSum = CalculateTotalSum();
        var totalForts = totalSum / (2 * _size - 1);
        var rowForts = CalculateRowForts(totalForts);
        var colForts = CalculateColForts(totalForts);
        return BuildRevealedGrid(rowForts, colForts);
    }

    private void ParseGrid(string[] rows)
    {
        for (var r = 0; r < _size; r++)
        {
            for (var c = 0; c < _size; c++)
            {
                _grid[r, c] = GetValueFromChar(rows[r][c]);
            }
        }
    }

    private int GetValueFromChar(char c)
    {
        if (c >= 'a' && c <= 'z') return c - 'a' + 10;
        if (c >= 'A' && c <= 'Z') return c - 'A' + 36;
        return c - '0';
    }

    private int CalculateTotalSum()
    {
        var totalSum = 0;
        for (var r = 0; r < _size; r++)
        {
            for (var c = 0; c < _size; c++)
            {
                totalSum += _grid[r, c];
            }
        }
        return totalSum;
    }

    private int[] CalculateRowForts(int totalForts)
    {
        var rowForts = new int[_size];
        for (var r = 0; r < _size; r++)
        {
            var rowSum = 0;
            for (var c = 0; c < _size; c++)
            {
                rowSum += _grid[r, c];
            }
            rowForts[r] = (rowSum - totalForts) / (_size - 1);
        }
        return rowForts;
    }

    private int[] CalculateColForts(int totalForts)
    {
        var colForts = new int[_size];
        for (var c = 0; c < _size; c++)
        {
            var colSum = 0;
            for (var r = 0; r < _size; r++)
            {
                colSum += _grid[r, c];
            }
            colForts[c] = (colSum - totalForts) / (_size - 1);
        }
        return colForts;
    }

    private string[] BuildRevealedGrid(int[] rowForts, int[] colForts)
    {
        var result = new string[_size];
        for (var r = 0; r < _size; r++)
        {
            var sb = new StringBuilder(_size);
            for (var c = 0; c < _size; c++)
            {
                var expectedEmptyValue = rowForts[r] + colForts[c];
                if (_grid[r, c] == expectedEmptyValue)
                {
                    sb.Append('.');
                }
                else
                {
                    sb.Append('O');
                }
            }
            result[r] = sb.ToString();
        }
        return result;
    }
}
