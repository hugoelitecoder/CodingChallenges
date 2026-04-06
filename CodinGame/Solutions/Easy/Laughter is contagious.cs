using System;
using System.Text;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var row = Console.ReadLine();
        Console.WriteLine(LaughterUtil.Spread(row));
    }
}

static class LaughterUtil
{
    public static string Spread(string row)
    {
        var seats = ParseSeats(row);
        var afterStep1 = ApplyStep1(seats);
        var result = ApplyStep2(afterStep1);
        return JoinSeats(result);
    }

    private static string[] ParseSeats(string row)
    {
        var seatCount = row.Length / 2;
        var seats = new string[seatCount];
        for (var i = 0; i < seatCount; i++)
        {
            seats[i] = row.Substring(i * 2, 2);
        }
        return seats;
    }

    private static string[] ApplyStep1(string[] seats)
    {
        var laughingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < seats.Length; i++)
        {
            if (IsLaughing(seats[i]))
            {
                laughingKeys.Add(seats[i]);
            }
        }

        var result = new string[seats.Length];
        for (var i = 0; i < seats.Length; i++)
        {
            result[i] = laughingKeys.Contains(seats[i]) ? seats[i].ToUpperInvariant() : seats[i];
        }
        return result;
    }

    private static string[] ApplyStep2(string[] seats)
    {
        var result = new string[seats.Length];
        Array.Copy(seats, result, seats.Length);

        for (var i = 0; i < seats.Length; i++)
        {
            if (!IsMaybe(seats[i]))
            {
                continue;
            }

            if (CountSullenNeighbors(seats, i) > 1)
            {
                continue;
            }

            var leftIndex = FindLaughing(seats, i, -1);
            var rightIndex = FindLaughing(seats, i, 1);

            if (leftIndex == -1 && rightIndex == -1)
            {
                continue;
            }

            var leftDistance = leftIndex == -1 ? int.MaxValue : i - leftIndex;
            var rightDistance = rightIndex == -1 ? int.MaxValue : rightIndex - i;

            if (leftDistance < rightDistance)
            {
                result[i] = seats[leftIndex];
            }
            else if (rightDistance < leftDistance)
            {
                result[i] = seats[rightIndex];
            }
            else
            {
                result[i] = string.Concat(seats[leftIndex][1], seats[rightIndex][0]);
            }
        }

        return result;
    }

    private static int CountSullenNeighbors(string[] seats, int index)
    {
        var count = 0;
        if (index > 0 && IsSullen(seats[index - 1]))
        {
            count++;
        }
        if (index + 1 < seats.Length && IsSullen(seats[index + 1]))
        {
            count++;
        }
        return count;
    }

    private static int FindLaughing(string[] seats, int index, int direction)
    {
        for (var distance = 1; distance <= 3; distance++)
        {
            var next = index + distance * direction;
            if (next < 0 || next >= seats.Length)
            {
                break;
            }
            if (IsLaughing(seats[next]))
            {
                return next;
            }
        }
        return -1;
    }

    private static string JoinSeats(string[] seats)
    {
        var sb = new StringBuilder(seats.Length * 2);
        for (var i = 0; i < seats.Length; i++)
        {
            sb.Append(seats[i]);
        }
        return sb.ToString();
    }

    private static bool IsLaughing(string seat)
    {
        return char.IsUpper(seat[0]) && char.IsUpper(seat[1]);
    }

    private static bool IsMaybe(string seat)
    {
        return char.IsUpper(seat[0]) && char.IsLower(seat[1]);
    }

    private static bool IsSullen(string seat)
    {
        return char.IsLower(seat[0]) && char.IsLower(seat[1]);
    }
}