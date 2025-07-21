using System;

class Solution
{
    static void Main()
    {
        var bits = Console.ReadLine().Trim();
        // Convert to int array
        var b = new int[16];
        for (int i = 0; i < 16; i++)
            b[i] = bits[i] - '0';

        // Compute parity checks
        bool flagA = false; // global parity
        int sumAll = 0;
        for (int i = 0; i < 16; i++) sumAll += b[i];
        flagA = (sumAll % 2) != 0;

        int flagB = (b[1] + b[3] + b[5] + b[7] + b[9] + b[11] + b[13] + b[15]) % 2;
        int flagC = (b[2] + b[3] + b[6] + b[7] + b[10] + b[11] + b[14] + b[15]) % 2;
        int flagE = (b[4] + b[5] + b[6] + b[7] + b[12] + b[13] + b[14] + b[15]) % 2;
        int flagI = (b[8] + b[9] + b[10] + b[11] + b[12] + b[13] + b[14] + b[15]) % 2;

        // Syndrome s from local parity bits
        int s = flagB * 1 + flagC * 2 + flagE * 4 + flagI * 8;

        if (!flagA && s == 0)
        {
            // No errors
            Console.WriteLine(bits);
        }
        else if (flagA && s == 0)
        {
            // Error in global parity bit (bit 0)
            b[0] ^= 1;
            PrintBits(b);
        }
        else if (flagA && s != 0)
        {
            // Single error at position s (0-based)
            b[s] ^= 1;
            PrintBits(b);
        }
        else
        {
            // flagA == false && s != 0 => two errors detected
            Console.WriteLine("TWO ERRORS");
        }
    }

    static void PrintBits(int[] b)
    {
        for (int i = 0; i < 16; i++)
            Console.Write(b[i]);
        Console.WriteLine();
    }
}
