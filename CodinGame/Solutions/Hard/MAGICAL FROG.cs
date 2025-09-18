using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var line = Console.ReadLine().Split();
        long n = long.Parse(line[0]);
        int k = int.Parse(line[1]);
        if (n <= 1)
        {
            Console.WriteLine("1");
            return;
        }
        var matrix = new Matrix(k);
        matrix = MatrixPower(matrix, n - k + 1);

        var initial = new long[k];
        initial[0] = 1;
        long sum = 1;
        for (int i = 1; i < k; i++)
        {
            initial[i] = sum;
            sum = (sum * 2) % MOD;
        }
        long result = 0;
        for (int i = 0, j = k - 1; i < k; i++, j--)
            result = (result + matrix.A[0, i] * initial[j]) % MOD;

        Console.WriteLine(result);
    }

    static readonly long MOD = 1000000007;

    class Matrix
    {
        public long[,] A;
        public int Size;
        public Matrix(int m)
        {
            Size = m;
            A = new long[m, m];
            for (int i = 0; i < m; i++)
                for (int j = 0; j < m; j++)
                    A[i, j] = (i == 0 ? 1 : 0);
            for (int i = 1, j = 0; i < m; i++, j++)
                A[i, j] = 1;
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            int n = m1.Size;
            var ret = new Matrix(n);
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    long sum = 0;
                    for (int k = 0; k < n; k++)
                        sum = (sum + m1.A[i, k] * m2.A[k, j]) % MOD;
                    ret.A[i, j] = sum;
                }
            return ret;
        }
    }

    static Matrix MatrixPower(Matrix m, long p)
    {
        if (p == 1)
            return m;
        var temp = MatrixPower(m, p / 2);
        temp = temp * temp;
        if (p % 2 == 0)
            return temp;
        return temp * m;
    }
}
