using System;
using System.Linq;
using System.Text;

class Solution
{
    static void Main()
    {
        string mode = Console.ReadLine();
        int n = int.Parse(Console.ReadLine());
        var lines = new string[n];
        for (int i = 0; i < n; i++)
            lines[i] = Console.ReadLine();

        int width = lines.Max(s => s.Length);
        var aligner = new TextAligner(mode, width);

        foreach (var line in lines)
            Console.WriteLine(aligner.Format(line));
    }
}

class TextAligner
{
    public enum AlignType { LEFT, RIGHT, CENTER, JUSTIFY }
    private readonly AlignType _align;
    private readonly int _width;

    public TextAligner(string mode, int width)
    {
        _align = Enum.Parse<AlignType>(mode, true);
        _width = width;
    }

    public string Format(string line) => _align switch
    {
        AlignType.LEFT   => line,
        AlignType.RIGHT  => new string(' ', _width - line.Length) + line,
        AlignType.CENTER => new string(' ', (_width - line.Length) / 2) + line,
        AlignType.JUSTIFY=> Justify(line),
        _                => line
    };

    private string Justify(string line)
    {
        var words = line.Split(' ');
        if (words.Length < 2) return line;

        int totalChars   = words.Sum(w => w.Length);
        int spacesNeeded = _width - totalChars;
        int gaps         = words.Length - 1;

        var sb   = new StringBuilder();
        int used = 0;

        for (int i = 0; i < gaps; i++)
        {
            int cum   = (int)Math.Floor((double)(i + 1) * spacesNeeded / gaps);
            int count = cum - used;
            used = cum;

            sb.Append(words[i]);
            sb.Append(' ', count);
        }

        sb.Append(words[^1]);
        return sb.ToString();
    }
}
