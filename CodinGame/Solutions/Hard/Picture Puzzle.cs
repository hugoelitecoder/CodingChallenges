using System;
using System.Collections.Generic;
using System.Text;


public class Solution
{
    public static void Main(string[] args)
    {
        var line1 = Console.ReadLine().Split(' ');
        var pieceSize = int.Parse(line1[0]);
        var nPieces = int.Parse(line1[1]);

        var line2 = Console.ReadLine().Split(' ');
        var nColumns = int.Parse(line2[0]);
        var nRows = int.Parse(line2[1]);

        var line3 = Console.ReadLine().Split(' ');
        var pictureWidth = int.Parse(line3[0]);
        var pictureHeight = int.Parse(line3[1]);

        var allPieces = new List<Piece>();
        for (var i = 0; i < nPieces; i++)
        {
            var content = new string[pieceSize];
            for (var j = 0; j < pieceSize; j++)
            {
                content[j] = Console.ReadLine();
            }
            allPieces.Add(new Piece(i, pieceSize, content));
        }

        var solver = new PuzzleSolver(pieceSize, nRows, nColumns, pictureWidth, pictureHeight, allPieces);
        var finalPicture = solver.Solve();

        foreach (var row in finalPicture)
        {
            Console.WriteLine(row);
        }
    }
}

public class PuzzleSolver
{
    private readonly int _pieceSize;
    private readonly int _nRows;
    private readonly int _nCols;
    private readonly int _pictureWidth;
    private readonly int _pictureHeight;
    private readonly Dictionary<string, List<(Piece piece, int side)>> _borderToPieces;
    private readonly Piece[,] _grid;
    private string[] _solution;

    public PuzzleSolver(int pieceSize, int nRows, int nCols, int picWidth, int picHeight, List<Piece> pieces)
    {
        _pieceSize = pieceSize;
        _nRows = nRows;
        _nCols = nCols;
        _pictureWidth = picWidth;
        _pictureHeight = picHeight;
        _grid = new Piece[nRows, nCols];
        _borderToPieces = new Dictionary<string, List<(Piece piece, int side)>>();
        
        foreach (var piece in pieces)
        {
            for (var i = 0; i < 4; i++)
            {
                var border = piece.Borders[i];
                if (!_borderToPieces.ContainsKey(border))
                {
                    _borderToPieces[border] = new List<(Piece piece, int side)>();
                }
                _borderToPieces[border].Add((piece, i));
            }
        }
    }

    public string[] Solve()
    {
        Search(0, -1);
        return _solution;
    }
    
    private bool Search(int r, int c)
    {
        c++;
        if (c >= _nCols)
        {
            c = 0;
            r++;
        }

        if (r >= _nRows)
        {
            _solution = AssemblePicture();
            return true;
        }

        var emptyBorder = new string('#', _pieceSize);
        var topBorder = r == 0 ? emptyBorder : _grid[r - 1, c].GetOrientedBorder(2);
        var leftBorder = c == 0 ? emptyBorder : _grid[r, c - 1].GetOrientedBorder(1);

        var key = Piece.Reverse(topBorder);
        if (!_borderToPieces.ContainsKey(key))
        {
            return false;
        }

        foreach (var (piece, side) in _borderToPieces[key])
        {
            if (piece.Orientation != null) continue;

            var pieceOrientation = (4 - side) % 4;
            
            if (piece.Id == 0 && pieceOrientation != 0) continue;

            var originalLeftIdx = (side + 3) % 4;
            if (Piece.Reverse(piece.Borders[originalLeftIdx]) != leftBorder) continue;

            if (c == _nCols - 1)
            {
                var originalRightIdx = (side + 1) % 4;
                if (piece.Borders[originalRightIdx] != emptyBorder) continue;
            }
            if (r == _nRows - 1)
            {
                var originalBottomIdx = (side + 2) % 4;
                if (piece.Borders[originalBottomIdx] != emptyBorder) continue;
            }

            piece.Orientation = pieceOrientation;
            _grid[r, c] = piece;

            if (Search(r, c))
            {
                return true;
            }
            
            _grid[r, c] = null;
            piece.Orientation = null;
        }

        return false;
    }

    private string[] AssemblePicture()
    {
        var picture = new char[_pictureHeight, _pictureWidth];
        for (var r = 0; r < _nRows; r++)
        {
            for (var c = 0; c < _nCols; c++)
            {
                var piece = _grid[r, c];
                var content = piece.GetOrientedContent();
                var startRow = r * (_pieceSize - 1);
                var startCol = c * (_pieceSize - 1);

                for (var r2 = 0; r2 < _pieceSize; r2++)
                {
                    for (var c2 = 0; c2 < _pieceSize; c2++)
                    {
                        picture[startRow + r2, startCol + c2] = content[r2][c2];
                    }
                }
            }
        }
        
        var finalPicture = new string[_pictureHeight];
        for (var i = 0; i < _pictureHeight; i++)
        {
            var row = new StringBuilder(_pictureWidth);
            for (var j = 0; j < _pictureWidth; j++)
            {
                row.Append(picture[i, j]);
            }
            finalPicture[i] = row.ToString();
        }
        return finalPicture;
    }
}

public class Piece
{
    public int Id { get; }
    public string[] Content { get; }
    public string[] Borders { get; }
    public int? Orientation { get; set; }
    private readonly int _size;

    public Piece(int id, int pieceSize, string[] content)
    {
        Id = id;
        _size = pieceSize;
        Content = content;
        Borders = new string[4];
        Orientation = null;

        Borders[0] = Content[0];
        
        var rightBorder = new StringBuilder();
        for (var i = 0; i < _size; i++)
        {
            rightBorder.Append(Content[i][_size - 1]);
        }
        Borders[1] = rightBorder.ToString();

        Borders[2] = Reverse(Content[_size - 1]);
        
        var leftBorder = new StringBuilder();
        for (var i = _size - 1; i >= 0; i--)
        {
            leftBorder.Append(Content[i][0]);
        }
        Borders[3] = leftBorder.ToString();
    }
    
    public string GetOrientedBorder(int side)
    {
        if (Orientation == null) return null;
        var originalSide = (side - Orientation.Value + 4) % 4;
        return Borders[originalSide];
    }
    
    public string[] GetOrientedContent()
    {
        if (Orientation == null || Orientation.Value == 0) return Content;
        
        var o = Orientation.Value;
        var newContent = new char[_size, _size];
        for (var i = 0; i < _size; i++)
        {
            for (var j = 0; j < _size; j++)
            {
                switch (o)
                {
                    case 1: // 90 deg clockwise
                        newContent[i, j] = Content[_size - 1 - j][i];
                        break;
                    case 2: // 180 deg
                        newContent[i, j] = Content[_size - 1 - i][_size - 1 - j];
                        break;
                    case 3: // 270 deg clockwise
                        newContent[i, j] = Content[j][_size - 1 - i];
                        break;
                }
            }
        }
        
        var result = new string[_size];
        for (var i = 0; i < _size; i++)
        {
            var row = new StringBuilder(_size);
            for(var j = 0; j < _size; j++)
            {
                row.Append(newContent[i, j]);
            }
            result[i] = row.ToString();
        }
        return result;
    }

    public static string Reverse(string s)
    {
        var charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}