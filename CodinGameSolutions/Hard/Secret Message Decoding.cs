using System;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        int hs = int.Parse(Console.ReadLine());
        int ms = int.Parse(Console.ReadLine());

        int[,] header = new int[hs, hs];
        int[,] data = new int[hs, ms];

        for (int i = 0; i < hs; i++)
        {
            string row = Console.ReadLine();
            for (int j = 0; j < hs; j++)
                header[i, j] = row[j] % Decoder.Mod;
            for (int j = 0; j < ms; j++)
                data[i, j] = row[hs + j] % Decoder.Mod;
        }

        int[,] invHeader = Decoder.Invert(header, hs);
        int[,] original = Decoder.Multiply(invHeader, data, hs, ms);

        char[] message = new char[hs * ms];
        int idx = 0;
        for (int i = 0; i < hs; i++)
            for (int j = 0; j < ms; j++)
                message[idx++] = (char)original[i, j];

        Console.WriteLine(new string(message));
    }
}

static class Decoder
{
    public const int Mod = 127;

    public static int ModPow(int a, int b)
    {
        int result = 1;
        a %= Mod;
        while (b > 0)
        {
            if ((b & 1) == 1) result = (result * a) % Mod;
            a = (a * a) % Mod;
            b >>= 1;
        }
        return result;
    }

    public static int Inv(int a)
    {
        return ModPow(a, Mod - 2);
    }

    public static int[,] Invert(int[,] matrix, int size)
    {
        int[,] m = new int[size, size];
        int[,] inv = new int[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
                m[i, j] = matrix[i, j];
            inv[i, i] = 1;
        }

        for (int col = 0; col < size; col++)
        {
            int pivot = col;
            while (pivot < size && m[pivot, col] == 0) pivot++;
            if (pivot != col)
                SwapRows(m, inv, col, pivot, size);

            int factor = Inv(m[col, col]);
            ScaleRow(m, col, factor, size);
            ScaleRow(inv, col, factor, size);

            for (int row = 0; row < size; row++)
            {
                if (row == col) continue;
                int f = m[row, col];
                if (f != 0)
                {
                    SubtractRow(m, row, col, f, size);
                    SubtractRow(inv, row, col, f, size);
                }
            }
        }

        return inv;
    }

    public static int[,] Multiply(int[,] a, int[,] b, int rows, int cols)
    {
        int[,] result = new int[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                for (int k = 0; k < rows; k++)
                    result[i, j] = (result[i, j] + a[i, k] * b[k, j]) % Mod;
        return result;
    }

    static void SwapRows(int[,] m, int[,] inv, int i, int j, int size)
    {
        for (int k = 0; k < size; k++)
        {
            (m[i, k], m[j, k]) = (m[j, k], m[i, k]);
            (inv[i, k], inv[j, k]) = (inv[j, k], inv[i, k]);
        }
    }

    static void ScaleRow(int[,] m, int row, int factor, int size)
    {
        for (int k = 0; k < size; k++)
            m[row, k] = (m[row, k] * factor) % Mod;
    }

    static void SubtractRow(int[,] m, int target, int source, int factor, int size)
    {
        for (int k = 0; k < size; k++)
            m[target, k] = (m[target, k] - factor * m[source, k] % Mod + Mod) % Mod;
    }
}
