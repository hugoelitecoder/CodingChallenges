using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

class Solution
{
    static void Main(string[] args)
    {
        var stopwatch = Stopwatch.StartNew();
        var inputLinesCount = int.Parse(Console.ReadLine());
        var rawLines = new List<string>(inputLinesCount);
        for (var i = 0; i < inputLinesCount; i++)
        {
            rawLines.Add(Console.ReadLine());
        }

        LogRawInput(inputLinesCount, rawLines);

        var grid = new TableGridCalculator(rawLines);
        grid.Solve();

        var output = grid.GetFormattedOutput();
        foreach (var line in output)
        {
            Console.WriteLine(line);
        }

        stopwatch.Stop();
        Console.Error.WriteLine($"[DEBUG] Execution time: {stopwatch.ElapsedMilliseconds}ms");
    }

    private static void LogRawInput(int count, List<string> lines)
    {
        Console.Error.WriteLine($"[DEBUG] Input lines read: {count}");
        Console.Error.WriteLine("[DEBUG] Raw Input:");
        foreach (var line in lines)
        {
            Console.Error.WriteLine($"[DEBUG] > {line}");
        }
    }

    private class TableGridCalculator
    {
        private readonly List<string> _rawLines;
        private readonly char _operator;
        private readonly int?[] _rowHeaders;
        private readonly int?[] _colHeaders;
        private readonly int?[,] _grid;
        private readonly int _numRows;
        private readonly int _numCols;

        public TableGridCalculator(List<string> rawLines)
        {
            _rawLines = rawLines;
            var headerParts = _rawLines[0].Split('|');
            _numCols = headerParts.Length - 1;
            _operator = headerParts[0].Trim()[0];
            _colHeaders = new int?[_numCols];
            for (var j = 0; j < _numCols; j++)
            {
                _colHeaders[j] = ParseCell(headerParts[j + 1]);
            }
            _numRows = (_rawLines.Count - 1) / 2;
            _rowHeaders = new int?[_numRows];
            _grid = new int?[_numRows, _numCols];
            for (var i = 0; i < _numRows; i++)
            {
                var rowParts = _rawLines[2 * i + 2].Split('|');
                _rowHeaders[i] = ParseCell(rowParts[0]);
                for (var j = 0; j < _numCols; j++)
                {
                    _grid[i, j] = ParseCell(rowParts[j + 1]);
                }
            }
            LogParsedState();
        }

        public void Solve()
        {
            var madeProgress = false;
            do
            {
                madeProgress = false;
                for (var i = 0; i < _numRows; i++)
                {
                    for (var j = 0; j < _numCols; j++)
                    {
                        var rowH = _rowHeaders[i];
                        var colH = _colHeaders[j];
                        var cell = _grid[i, j];
                        if (!cell.HasValue && rowH.HasValue && colH.HasValue)
                        {
                            _grid[i, j] = Calculate(colH, rowH);
                            madeProgress = true;
                        }
                        if (!rowH.HasValue && cell.HasValue && colH.HasValue)
                        {
                            var derived = GetRowHeader(cell, colH);
                            if (derived.HasValue)
                            {
                                _rowHeaders[i] = derived;
                                madeProgress = true;
                            }
                        }
                        if (!colH.HasValue && cell.HasValue && rowH.HasValue)
                        {
                            var derived = GetColHeader(cell, rowH);
                            if (derived.HasValue)
                            {
                                _colHeaders[j] = derived;
                                madeProgress = true;
                            }
                        }
                    }
                }
            } while (madeProgress);
        }

        public List<string> GetFormattedOutput()
        {
            var maxLength = 1;
            foreach (var h in _colHeaders) maxLength = Math.Max(maxLength, h.Value.ToString().Length);
            foreach (var h in _rowHeaders) maxLength = Math.Max(maxLength, h.Value.ToString().Length);
            foreach (var cell in _grid) maxLength = Math.Max(maxLength, cell.Value.ToString().Length);

            var result = new List<string>();
            var sb = new StringBuilder();
            sb.Append(_operator.ToString().PadRight(maxLength));
            for (var j = 0; j < _numCols; j++)
            {
                sb.Append("|");
                sb.Append(_colHeaders[j].Value.ToString().PadRight(maxLength));
            }
            result.Add(sb.ToString());

            for (var i = 0; i < _numRows; i++)
            {
                result.Add(_rawLines[2 * i + 1]);
                sb.Clear();
                sb.Append(_rowHeaders[i].Value.ToString().PadRight(maxLength));
                for (var j = 0; j < _numCols; j++)
                {
                    sb.Append("|");
                    sb.Append(_grid[i, j].Value.ToString().PadRight(maxLength));
                }
                result.Add(sb.ToString());
            }
            return result;
        }

        private int? ParseCell(string s) => string.IsNullOrWhiteSpace(s) ? (int?)null : int.Parse(s.Trim());

        private void LogParsedState() => Console.Error.WriteLine($"[DEBUG] Parsed state: {_numRows}x{_numCols} grid, operator '{_operator}'");
        
        private int? Calculate(int? colH, int? rowH) => _operator switch
        {
            '+' => colH + rowH,
            '-' => colH - rowH,
            'x' => colH * rowH,
            _ => throw new InvalidOperationException("Unknown operator")
        };

        private int? GetRowHeader(int? cell, int? colH) => _operator switch
        {
            '+' => cell - colH,
            '-' => colH - cell,
            'x' => colH == 0 ? (int?)null : cell / colH,
            _ => throw new InvalidOperationException("Unknown operator")
        };
        
        private int? GetColHeader(int? cell, int? rowH) => _operator switch
        {
            '+' => cell - rowH,
            '-' => cell + rowH,
            'x' => rowH == 0 ? (int?)null : cell / rowH,
            _ => throw new InvalidOperationException("Unknown operator")
        };
    }
}