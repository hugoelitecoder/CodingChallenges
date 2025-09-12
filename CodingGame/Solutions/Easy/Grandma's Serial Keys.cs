using System;
using System.Diagnostics;

class Solution
{
    static void Main(string[] args)
    {
        var username = Console.ReadLine();
        var sw = Stopwatch.StartNew();

        Console.Error.WriteLine($"[DEBUG] Input username: {username}");

        var keygen = new GameOfCodeKeygen(username);
        var serialKey = keygen.GetFormattedKey();

        Console.Error.WriteLine($"[DEBUG] SEED: {keygen.Seed}");
        Console.Error.WriteLine($"[DEBUG] Segments (dec): {keygen.Segment1}, {keygen.Segment2}, {keygen.Segment3}, {keygen.Segment4}");
        Console.Error.WriteLine($"[DEBUG] Segments (hex): {keygen.HexSegment1}, {keygen.HexSegment2}, {keygen.HexSegment3}, {keygen.HexSegment4}");
        
        sw.Stop();
        
        Console.WriteLine(serialKey);
        Console.Error.WriteLine($"[DEBUG] Calculation time: {sw.Elapsed.TotalMilliseconds}ms");
    }
}

public class GameOfCodeKeygen
{
    public int Seed { get; private set; }
    public int Segment1 { get; private set; }
    public int Segment2 { get; private set; }
    public int Segment3 { get; private set; }
    public int Segment4 { get; private set; }

    public string HexSegment1 => ToHex4(Segment1);
    public string HexSegment2 => ToHex4(Segment2);
    public string HexSegment3 => ToHex4(Segment3);
    public string HexSegment4 => ToHex4(Segment4);

    public GameOfCodeKeygen(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            Seed = 0;
            Segment1 = 0;
            Segment2 = 0;
            Segment3 = 0;
            Segment4 = 0;
        }
        else
        {
            Process(username);
        }
    }

    public string GetFormattedKey()
    {
        return $"{HexSegment1}-{HexSegment2}-{HexSegment3}-{HexSegment4}";
    }
    
    private void Process(string username)
    {
        Seed = GenerateSeed(username);
        Segment1 = CalculateSegment1(Seed);
        Segment2 = CalculateSegment2(Seed);
        Segment3 = CalculateSegment3(username);
        Segment4 = CalculateSegment4(Segment1, Segment2, Segment3);
    }
    
    private int GenerateSeed(string username)
    {
        var asciiSum = 0;
        foreach (char c in username)
        {
            asciiSum += c;
        }
        var len = username.Length;
        var product = asciiSum * len;
        return product ^ 20480;
    }

    private int CalculateSegment1(int seed)
    {
        return seed & 65535;
    }

    private int CalculateSegment2(int seed)
    {
        return seed >> 16;
    }

    private int CalculateSegment3(string username)
    {
        var firstAscii = (int)username[0];
        var lastAscii = (int)username[username.Length - 1];
        var len = username.Length;
        return (firstAscii + lastAscii) * len;
    }

    private int CalculateSegment4(int seg1, int seg2, int seg3)
    {
        var sum = seg1 + seg2 + seg3;
        return sum & 65535;
    }

    private string ToHex4(int value)
    {
        return value.ToString("X4");
    }
}