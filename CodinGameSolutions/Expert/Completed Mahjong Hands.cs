using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var line = Console.ReadLine();
        var hand = MahjongHand.Parse(line);
        var isWinning = hand.IsWinningHand();
        Console.WriteLine(isWinning ? "TRUE" : "FALSE");
    }
}

public static class TileHelper
{
    public const int TOTAL_TILE_TYPES = 34;
    private static readonly HashSet<int> _orphans = new HashSet<int>
    {
        0, 8, 9, 17, 18, 26, 27, 28, 29, 30, 31, 32, 33
    };

    public static int MapToIndex(char rankChar, char suitChar)
    {
        var rank = rankChar - '1';
        switch (suitChar)
        {
            case 'm': return rank;
            case 'p': return rank + 9;
            case 's': return rank + 18;
            case 'z': return rank + 27;
            default: return -1;
        }
    }

    public static bool IsOrphan(int index)
    {
        return _orphans.Contains(index);
    }

    public static bool CanFormRun(int index)
    {
        return index < 27 && (index % 9) < 7;
    }
}

public class MahjongHand
{
    private readonly int[] _counts;

    private MahjongHand(int[] counts)
    {
        _counts = counts;
    }

    public static MahjongHand Parse(string line)
    {
        var counts = new int[TileHelper.TOTAL_TILE_TYPES];
        var parts = line.Split(' ');
        var handStr = parts[0];
        var drawnTileStr = parts[1];
        char currentSuit = '\0';
        for (int i = handStr.Length - 1; i >= 0; i--)
        {
            var c = handStr[i];
            if (char.IsLetter(c))
            {
                currentSuit = c;
            }
            else
            {
                var tileIndex = TileHelper.MapToIndex(c, currentSuit);
                if (tileIndex != -1)
                {
                    counts[tileIndex]++;
                }
            }
        }
        var drawnRank = drawnTileStr[0];
        var drawnSuit = drawnTileStr[1];
        var drawnTileIndex = TileHelper.MapToIndex(drawnRank, drawnSuit);
        if (drawnTileIndex != -1)
        {
            counts[drawnTileIndex]++;
        }
        return new MahjongHand(counts);
    }

    public bool IsWinningHand()
    {
        if (_counts.Sum() != 14) return false;
        if (IsKokushiMusou()) return true;
        if (IsSevenPairs()) return true;
        if (IsStandardHand()) return true;
        return false;
    }

    private bool IsKokushiMusou()
    {
        var foundOrphans = new HashSet<int>();
        var hasPair = false;
        for (var i = 0; i < TileHelper.TOTAL_TILE_TYPES; i++)
        {
            if (_counts[i] == 0) continue;
            if (!TileHelper.IsOrphan(i)) return false;
            if (_counts[i] == 1)
            {
                foundOrphans.Add(i);
            }
            else if (_counts[i] == 2)
            {
                if (hasPair) return false;
                hasPair = true;
                foundOrphans.Add(i);
            }
            else
            {
                return false;
            }
        }
        return hasPair && foundOrphans.Count == 13;
    }

    private bool IsSevenPairs()
    {
        var pairCount = 0;
        for (var i = 0; i < TileHelper.TOTAL_TILE_TYPES; i++)
        {
            if (_counts[i] == 2)
            {
                pairCount++;
            }
            else if (_counts[i] != 0)
            {
                return false;
            }
        }
        return pairCount == 7;
    }

    private bool IsStandardHand()
    {
        for (var i = 0; i < TileHelper.TOTAL_TILE_TYPES; i++)
        {
            if (_counts[i] >= 2)
            {
                var remainingCounts = (int[])_counts.Clone();
                remainingCounts[i] -= 2;
                if (CanFormSets(remainingCounts))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool CanFormSets(int[] counts)
    {
        if (counts.Sum() == 0) return true;
        var firstTileIndex = Array.FindIndex(counts, c => c > 0);
        if (firstTileIndex == -1) return true;
        if (counts[firstTileIndex] >= 3)
        {
            var nextCounts = (int[])counts.Clone();
            nextCounts[firstTileIndex] -= 3;
            if (CanFormSets(nextCounts))
            {
                return true;
            }
        }
        if (TileHelper.CanFormRun(firstTileIndex) &&
            counts[firstTileIndex + 1] > 0 && counts[firstTileIndex + 2] > 0)
        {
            var nextCounts = (int[])counts.Clone();
            nextCounts[firstTileIndex]--;
            nextCounts[firstTileIndex + 1]--;
            nextCounts[firstTileIndex + 2]--;
            if (CanFormSets(nextCounts))
            {
                return true;
            }
        }
        return false;
    }
}