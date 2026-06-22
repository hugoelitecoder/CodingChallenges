using System;
using System.Diagnostics;
using System.Globalization;

class Solution
{
    public static void Main(string[] args)
    {
        var totalWatch = Stopwatch.StartNew();
        var readWatch = Stopwatch.StartNew();

        var firstGlyph = new string[5];
        var secondGlyph = new string[5];

        for (var i = 0; i < 5; i++)
            firstGlyph[i] = Console.ReadLine() ?? "";

        for (var i = 0; i < 5; i++)
            secondGlyph[i] = Console.ReadLine() ?? "";

        readWatch.Stop();

        var solveWatch = Stopwatch.StartNew();

        int firstNumber;
        int secondNumber;

        var resultGlyph = CistercianSolver.Add(
            firstGlyph,
            secondGlyph,
            out firstNumber,
            out secondNumber
        );

        var sum = firstNumber + secondNumber;

        solveWatch.Stop();

        var outputWatch = Stopwatch.StartNew();

        for (var i = 0; i < 5; i++)
            Console.WriteLine(resultGlyph[i]);

        outputWatch.Stop();
        totalWatch.Stop();

        PrintDebug(
            firstNumber,
            secondNumber,
            sum,
            readWatch.Elapsed,
            solveWatch.Elapsed,
            outputWatch.Elapsed,
            totalWatch.Elapsed
        );
    }

    private static void PrintDebug(
        int firstNumber,
        int secondNumber,
        int sum,
        TimeSpan readTime,
        TimeSpan solveTime,
        TimeSpan outputTime,
        TimeSpan totalTime)
    {
        Console.Error.WriteLine("[DEBUG] ========================================");
        Console.Error.WriteLine("[DEBUG] Cistercian Addition Report");
        Console.Error.WriteLine("[DEBUG] ========================================");

        Console.Error.WriteLine("[DEBUG] Input:");
        Console.Error.WriteLine("[DEBUG]   Read two five-line Cistercian glyphs.");
        Console.Error.WriteLine("[DEBUG]   Reading input took " + FormatTime(readTime) + ".");

        Console.Error.WriteLine("[DEBUG] Decoding:");
        Console.Error.WriteLine("[DEBUG]   The first glyph represents " + firstNumber + ".");
        Console.Error.WriteLine("[DEBUG]   The second glyph represents " + secondNumber + ".");

        Console.Error.WriteLine("[DEBUG] Addition:");
        Console.Error.WriteLine("[DEBUG]   " + firstNumber + " + " + secondNumber + " = " + sum + ".");
        Console.Error.WriteLine("[DEBUG]   Decoding and drawing took " + FormatTime(solveTime) + ".");

        Console.Error.WriteLine("[DEBUG] Output:");
        Console.Error.WriteLine("[DEBUG]   Wrote the five lines of the result glyph.");
        Console.Error.WriteLine("[DEBUG]   Writing output took " + FormatTime(outputTime) + ".");

        Console.Error.WriteLine("[DEBUG] Total execution time: " + FormatTime(totalTime) + ".");
        Console.Error.WriteLine("[DEBUG] ========================================");
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture) + " ms";
    }
}

static class CistercianSolver
{
    private const int TopRight = 0;
    private const int TopLeft = 1;
    private const int BottomRight = 2;
    private const int BottomLeft = 3;
    private const int BaseLine = 1;
    private const int MiddleLine = 2;
    private const int SlashThree = 4;
    private const int SlashFour = 8;
    private const int SideLine = 16;
    private const int FiveLine = 32;
    private static readonly int[] DigitMask =
    {
        0,
        BaseLine,
        MiddleLine,
        SlashThree,
        SlashFour,
        SlashFour | FiveLine,
        SideLine,
        BaseLine | SideLine,
        MiddleLine | SideLine,
        BaseLine | MiddleLine | SideLine
    };
    private static readonly int[] InnerCol = { 3, 1, 3, 1 };
    private static readonly int[] OuterCol = { 4, 0, 4, 0 };
    private static readonly int[] BaseRow = { 0, 0, 3, 3 };
    private static readonly int[] MiddleRow = { 1, 1, 2, 2 };
    private static readonly int[] SlashRow = { 1, 1, 3, 3 };
    private static readonly int[] FiveRow = { 0, 0, 4, 4 };
    private static readonly char[] SlashForThree =
    {
        '\\', '/', '/', '\\'
    };

    private static readonly char[] SlashForFour =
    {
        '/', '\\', '\\', '/'
    };

    private static readonly char[] FiveMark =
    {
        '_', '_', '\u203E', '\u203E'
    };

    private static readonly int[] PlainDigit =
    {
        0, 1, 2, 0, 6, 7, 8, 9
    };

    public static string[] Add(
        string[] firstGlyph,
        string[] secondGlyph,
        out int firstNumber,
        out int secondNumber)
    {
        firstNumber = Read(firstGlyph);
        secondNumber = Read(secondGlyph);

        return Draw(firstNumber + secondNumber);
    }

    private static int Read(string[] glyph)
    {
        return ReadDigit(glyph, TopRight)
            + ReadDigit(glyph, TopLeft) * 10
            + ReadDigit(glyph, BottomRight) * 100
            + ReadDigit(glyph, BottomLeft) * 1000;
    }

    private static int ReadDigit(string[] glyph, int quadrant)
    {
        var hasBaseLine = Get(glyph, BaseRow[quadrant], InnerCol[quadrant]) == '_';
        var hasMiddleLine = Get(glyph, MiddleRow[quadrant], InnerCol[quadrant]) == '_';
        var hasSideLine = Get(glyph, SlashRow[quadrant], OuterCol[quadrant]) == '|';
        var slash = Get(glyph, SlashRow[quadrant], InnerCol[quadrant]);

        if (slash == SlashForThree[quadrant])
            return 3;

        if (slash == SlashForFour[quadrant])
        {
            var isFive = quadrant < BottomRight
                ? hasBaseLine
                : Get(glyph, FiveRow[quadrant], InnerCol[quadrant]) == '\u203E';

            return isFive ? 5 : 4;
        }

        var shape = 0;

        if (hasBaseLine)
            shape |= 1;

        if (hasMiddleLine)
            shape |= 2;

        if (hasSideLine)
            shape |= 4;

        return PlainDigit[shape];
    }

    private static string[] Draw(int number)
    {
        var glyph = new char[25];

        for (var i = 0; i < glyph.Length; i++)
            glyph[i] = ' ';

        Set(glyph, 1, 2, '|');
        Set(glyph, 2, 2, '|');
        Set(glyph, 3, 2, '|');

        DrawDigit(glyph, number % 10, TopRight);
        DrawDigit(glyph, number / 10 % 10, TopLeft);
        DrawDigit(glyph, number / 100 % 10, BottomRight);
        DrawDigit(glyph, number / 1000 % 10, BottomLeft);

        var lines = new string[5];

        for (var row = 0; row < 5; row++)
            lines[row] = new string(glyph, row * 5, 5);

        return lines;
    }

    private static void DrawDigit(char[] glyph, int digit, int quadrant)
    {
        var mask = DigitMask[digit];
        var inner = InnerCol[quadrant];
        var outer = OuterCol[quadrant];

        if ((mask & BaseLine) != 0)
            Set(glyph, BaseRow[quadrant], inner, '_');

        if ((mask & MiddleLine) != 0)
            Set(glyph, MiddleRow[quadrant], inner, '_');

        if ((mask & SlashThree) != 0)
            Set(glyph, SlashRow[quadrant], inner, SlashForThree[quadrant]);

        if ((mask & SlashFour) != 0)
            Set(glyph, SlashRow[quadrant], inner, SlashForFour[quadrant]);

        if ((mask & FiveLine) != 0)
            Set(glyph, FiveRow[quadrant], inner, FiveMark[quadrant]);

        if ((mask & SideLine) != 0)
            Set(glyph, SlashRow[quadrant], outer, '|');
    }

    private static char Get(string[] glyph, int row, int col)
    {
        return col < glyph[row].Length ? glyph[row][col] : ' ';
    }

    private static void Set(char[] glyph, int row, int col, char value)
    {
        glyph[row * 5 + col] = value;
    }
}