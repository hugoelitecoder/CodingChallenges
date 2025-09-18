using System;
using System.Collections.Generic;

public class Solution
{
    public static void Main(string[] args)
    {
        var firstLine = Console.ReadLine().Split(' ');
        var width = int.Parse(firstLine[0]);
        var height = int.Parse(firstLine[1]);
        var tokens = new List<string>();
        var line = "";
        while ((line = Console.ReadLine()) != null && line.Length > 0)
        {
            tokens.AddRange(line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }
        var score = new EncodedScore(width, height, tokens);
        var reader = new TardisScoreReader();
        var result = reader.ReadScore(score);
        Console.WriteLine(result);
    }
}

internal class EncodedScore
{
    public int Width { get; }
    public int Height { get; }
    public IReadOnlyList<string> Tokens { get; }

    public EncodedScore(int width, int height, IReadOnlyList<string> tokens)
    {
        Width = width;
        Height = height;
        Tokens = tokens;
    }
}

internal class TardisScoreReader
{
    private int _width = 0;
    private int _height = 0;
    private bool[,] _grid = new bool[0, 0];
    private int _scanMinX = 0;
    private int _scanMaxX = 0;
    private int _scanMinY = 0;
    private int _ruleHeight = 0;
    private int _spaceHeight = 0;
    private readonly PitchZone[] _zones = new PitchZone[12];

    public string ReadScore(EncodedScore score)
    {
        PopulateGridAndCalibrate(score);
        InitializeZones();
        return ScanForNotes();
    }

    private void PopulateGridAndCalibrate(EncodedScore score)
    {
        _width = score.Width;
        _height = score.Height;
        _grid = new bool[_width, _height];
        var x = 0;
        var y = 0;
        var tokenIndex = 0;
        var calibX = _width;
        var calibMinY = 0;
        var calibMaxY = 0;
        var blackPixelCount = 0;
        while (tokenIndex < score.Tokens.Count)
        {
            var type = score.Tokens[tokenIndex++][0];
            var length = int.Parse(score.Tokens[tokenIndex++]);
            if (type == 'W')
            {
                var current1D = (long)y * _width + x;
                var new1D = current1D + length;
                y = (int)(new1D / _width);
                x = (int)(new1D % _width);
            }
            else
            {
                if (x < calibX)
                {
                    calibMinY = y;
                    calibX = x;
                    blackPixelCount = 0;
                }
                for (var i = 0; i < length; i++)
                {
                    if (x < _width)
                    {
                        _grid[x, y] = true;
                        if (x == calibX)
                        {
                            blackPixelCount++;
                            calibMaxY = y;
                        }
                    }
                    x++;
                }
            }
        }
        _ruleHeight = blackPixelCount / 5;
        _spaceHeight = ((calibMaxY + 1 - calibMinY) - blackPixelCount) / 4;
        _scanMinX = calibX + 1;
        _scanMaxX = _width - 1;
        _scanMinY = calibMinY - _spaceHeight;
    }

    private void InitializeZones()
    {
        var y = _scanMinY;
        var step = _spaceHeight + _ruleHeight;
        var halfStep = step / 2;
        for (var i = 0; i < 12; i += 2)
        {
            _zones[i] = new PitchZone(y, y + halfStep);
            _zones[i + 1] = new PitchZone(y + halfStep, y + step);
            y += step;
        }
    }

    private string ScanForNotes()
    {
        var notes = new List<string>();
        var isNoteInProgress = false;
        var noteStartY = 0;
        var noteType = "";
        for (var x = _scanMinX; x <= _scanMaxX; x++)
        {
            var foundPixelY = -1;
            var pixelFoundInColumn = TryFindPixelInColumn(x, ref foundPixelY);
            if (pixelFoundInColumn)
            {
                if (!isNoteInProgress)
                {
                    isNoteInProgress = true;
                    noteStartY = foundPixelY;
                    noteType = "Q";
                }
                else if (!_grid[x, noteStartY])
                {
                    noteType = "H";
                }
            }
            else if (isNoteInProgress)
            {
                notes.Add(FinalizeNote(noteStartY, noteType));
                isNoteInProgress = false;
            }
        }
        if (isNoteInProgress)
        {
            notes.Add(FinalizeNote(noteStartY, noteType));
        }
        return string.Join(" ", notes);
    }

    private bool TryFindPixelInColumn(int x, ref int foundPixelY)
    {
        var y = _scanMinY;
        for (var i = 0; i < 6; i++)
        {
            for (var k = 0; k < _spaceHeight; k++)
            {
                if (y >= 0 && y < _height && _grid[x, y])
                {
                    foundPixelY = y;
                    return true;
                }
                y++;
            }
            y += _ruleHeight;
        }
        return false;
    }

    private string FinalizeNote(int y, string type)
    {
        var pitch = "";
        var zoneIndex = GetZoneIndex(y);
        switch (zoneIndex)
        {
            case 0: pitch = "G"; break;
            case 1: pitch = "F"; break;
            case 2: pitch = "E"; break;
            case 3: pitch = "D"; break;
            case 4: pitch = "C"; break;
            case 5: pitch = "B"; break;
            case 6: pitch = "A"; break;
            case 7: pitch = "G"; break;
            case 8: pitch = "F"; break;
            case 9: pitch = "E"; break;
            case 10: pitch = "D"; break;
            case 11: pitch = "C"; break;
        }
        return pitch + type;
    }

    private int GetZoneIndex(int y)
    {
        for (var i = 0; i < _zones.Length; i++)
        {
            if (_zones[i].Contains(y)) return i;
        }
        return -1;
    }
}

internal class PitchZone
{
    private readonly int _low;
    private readonly int _high;

    public PitchZone(int low, int high)
    {
        _low = low;
        _high = high;
    }

    public bool Contains(int pos)
    {
        return pos >= _low && pos < _high;
    }
}