using System;
using System.Collections.Generic;
using System.Text;

class Program
{
    static void Main()
    {
        int width = int.Parse(Console.ReadLine());
        int height = int.Parse(Console.ReadLine());
        int depth = int.Parse(Console.ReadLine());

        var drawer = new AsciiBox3D(width, height, depth);
        foreach (var line in drawer.Generate())
        {
            Console.WriteLine(line);
        }
    }
}

class AsciiBox3D
{
    private readonly int width;
    private readonly int height;
    private readonly int depth;

    public AsciiBox3D(int width, int height, int depth)
    {
        this.width = width;
        this.height = height;
        this.depth = depth;
    }

    public IEnumerable<string> Generate()
    {
        var lines = new List<string>
        {
            GenerateTopEdge()
        };

        for (int i = 0; i < depth + height; i++)
        {
            string solidLine = GenerateSolidLine(i);
            string overlay = GenerateOverlay(i);
            string merged = MergeOverlay(solidLine, overlay, i);
            lines.Add(merged);
        }

        return lines;
    }

    private string GenerateTopEdge()
    {
        return new string(' ', depth) + new string('_', width * 2);
    }

    private string GenerateSolidLine(int row)
    {
        var sb = new StringBuilder();
        sb.Append(GetIndent(row));
        sb.Append(GetSideChar(row));
        sb.Append(GetFill(row));
        sb.Append(GetSideChar(row));
        sb.Append(new string(' ', GetSpacer(row)));
        sb.Append(GetEndChar(row));
        return sb.ToString();
    }

    private string GetIndent(int row) =>
        row < depth ? new string(' ', depth - row - 1)
                    : new string(' ', row - depth);

    private char GetSideChar(int row) => row < depth ? '/' : '\\';

    private string GetFill(int row)
    {
        bool isEdge = row == depth - 1 || row == depth + height - 1;
        char fill = isEdge ? '_' : ' ';
        return new string(fill, width * 2 - 1);
    }

    private int GetSpacer(int row)
    {
        if (row < height && row < depth)
            return row * 2;
        if (row < height)
            return depth * 2 - 1;
        if (row < depth)
            return height * 2 - 1;
        return height * 2 - (row - depth) * 2 - 2;
    }

    private char GetEndChar(int row) => row < height ? '\\' : '/';

    private string GenerateOverlay(int row)
    {
        var sb = new StringBuilder();
        int indent = row < height ? depth + row : 2 * height + depth - row - 1;
        sb.Append(new string(' ', indent));
        sb.Append(row < height ? '⠡' : '⠌');
        if (row == height - 1)
            sb.Append(new string('.', width * 2 - 1));
        return sb.ToString();
    }

    private string MergeOverlay(string solid, string overlay, int row)
    {
        var solidArr = solid.ToCharArray();
        var overlayArr = overlay.ToCharArray();
        for (int i = 0; i < overlayArr.Length && i < solidArr.Length; i++)
        {
            char v = overlayArr[i];
            if (ShouldApplyOverlay(v, solidArr, i, row))
                solidArr[i] = v;
        }
        return new string(solidArr);
    }

    private bool ShouldApplyOverlay(char v, char[] solid, int pos, int row)
    {
        if (v == ' ') return false;
        if (!(solid[pos] == ' ' || solid[pos] == '_')) return false;
        if (v == '.' && solid[pos] == '_') return false;
        if (row == 0 && depth == 1) return false;
        if (row == depth + height - 1 && height == 1) return false;
        if (pos > 0 && pos + 1 < solid.Length)
        {
            if (solid[pos - 1] == '/' && solid[pos + 1] == '/') return false;
            if (solid[pos - 1] == '\\' && solid[pos + 1] == '\\') return false;
        }
        return true;
    }
}