using System;
using System.Collections.Generic;

class Solution
{
    static int maxRow, maxCol;
    static bool[,] occupied;
    static int personSuccess;
    static int origRow, origCol, origSize;
    static int groupSuccessCount;

    static void Main(string[] args)
    {
        Initialize();
        ProcessGroups();
        Console.WriteLine($"{groupSuccessCount} {personSuccess}");
    }

    static void Initialize()
    {
        var parts = Console.ReadLine().Split();
        maxRow = int.Parse(parts[0]);
        maxCol = int.Parse(parts[1]);
        occupied = new bool[maxRow, maxCol];
        personSuccess = 0;
        groupSuccessCount = 0;
    }

    static void ProcessGroups()
    {
        int n = int.Parse(Console.ReadLine());
        for (int i = 0; i < n; i++)
            HandleGroup();
    }

    static void HandleGroup()
    {
        var tokens = Console.ReadLine().Split();
        int size = int.Parse(tokens[0]);
        origRow = int.Parse(tokens[1]);
        origCol = int.Parse(tokens[2]);
        origSize = size;

        if (TryOriginalPlacement(size))
        {
            groupSuccessCount++;
            personSuccess += size;
        }
        else
        {
            SeatWithSplitting(size);
        }
    }

    static bool TryOriginalPlacement(int size)
    {
        if (origCol + size > maxCol) return false;
        if (!IsBlockFree(origRow, origCol, size)) return false;
        Occupy(origRow, origCol, size);
        return true;
    }

    static void SeatWithSplitting(int size)
    {
        if (size <= 0) return;
        if (FindAndSeat(size)) return;
        if (size == 1) return;
        int first = (size + 1) / 2;
        int second = size - first;
        SeatWithSplitting(first);
        SeatWithSplitting(second);
    }

    static bool FindAndSeat(int size)
    {
        foreach (int rowOffset in GenerateOffsets(maxRow))
        {
            int r = origRow + rowOffset;
            if (r < 0 || r >= maxRow) continue;
            foreach (int seatOffset in GenerateOffsets(maxCol))
            {
                int c = origCol + seatOffset;
                if (c < 0 || c + size > maxCol) continue;
                if (!IsBlockFree(r, c, size)) continue;
                Occupy(r, c, size);
                CountOriginalSpan(r, c, size);
                return true;
            }
        }
        return false;
    }

    static bool IsBlockFree(int row, int col, int size)
    {
        for (int c = col; c < col + size; c++)
            if (occupied[row, c]) return false;
        return true;
    }

    static void Occupy(int row, int col, int size)
    {
        for (int c = col; c < col + size; c++)
            occupied[row, c] = true;
    }

    static void CountOriginalSpan(int row, int col, int size)
    {
        if (row != origRow) return;
        int start = Math.Max(col, origCol);
        int end = Math.Min(col + size, origCol + origSize);
        for (int c = start; c < end; c++)
            personSuccess++;
    }

    static IEnumerable<int> GenerateOffsets(int limit)
    {
        yield return 0;
        for (int d = 1; d < limit; d++)
        {
            yield return -d;
            yield return d;
        }
    }
}