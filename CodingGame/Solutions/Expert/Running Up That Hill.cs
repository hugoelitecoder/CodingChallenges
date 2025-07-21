using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var cipher = Console.ReadLine();
        var clear = Console.ReadLine();
        var toDecipher = Console.ReadLine();
        var toCipher = Console.ReadLine();

        var solver = new HillCipherSolver();
        var (key, _) = solver.FindKey(clear, cipher);
        var inverseKey = key.Inverse();

        var decipheredText = solver.ProcessText(toDecipher, inverseKey);
        var cipheredText = solver.ProcessText(toCipher, key);

        Console.WriteLine(decipheredText);
        Console.WriteLine(cipheredText);
    }
}

class HillCipherSolver
{
    public const int Modulus = 45;
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";
    private readonly Dictionary<char, int> _charMap;

    public HillCipherSolver()
    {
        _charMap = new Dictionary<char, int>();
        for (var i = 0; i < Alphabet.Length; i++)
        {
            _charMap[Alphabet[i]] = i;
        }
    }

    public (Matrix Key, int Size) FindKey(string clear, string cipher)
    {
        var len = clear.Length;
        var divisors = GetDivisors(len).OrderBy(d => d);

        foreach (var n in divisors)
        {
            if (n == 1) continue;
            var numBlocks = len / n;
            var indices = Enumerable.Range(0, numBlocks).ToArray();

            foreach (var combo in GetCombinations(indices, n))
            {
                var plainMatrix = BuildMatrixFromBlocks(clear, combo, n);
                var det = plainMatrix.Determinant();
                if (Matrix.Gcd(det, Modulus) != 1) continue;

                var cipherMatrix = BuildMatrixFromBlocks(cipher, combo, n);
                var plainInverse = plainMatrix.Inverse();
                var key = cipherMatrix.Multiply(plainInverse);

                if (key.Determinant() == 1)
                {
                    var predictedCipher = ProcessText(clear, key);
                    if (predictedCipher == cipher)
                    {
                        return (key, n);
                    }
                }
            }
        }
        throw new Exception("Key not found.");
    }

    public string ProcessText(string text, Matrix key)
    {
        var n = key.Rows;
        var result = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i += n)
        {
            var pVector = new Matrix(n, 1);
            for (var j = 0; j < n; j++)
            {
                pVector[j, 0] = _charMap[text[i + j]];
            }
            var cVector = key.Multiply(pVector);
            for (var j = 0; j < n; j++)
            {
                result.Append(Alphabet[cVector[j, 0]]);
            }
        }
        return result.ToString();
    }
    
    private Matrix BuildMatrixFromBlocks(string text, int[] blockIndices, int n)
    {
        var matrix = new Matrix(n, n);
        for (var j = 0; j < n; j++)
        {
            var blockIndex = blockIndices[j];
            var blockStart = blockIndex * n;
            for (var i = 0; i < n; i++)
            {
                matrix[i, j] = _charMap[text[blockStart + i]];
            }
        }
        return matrix;
    }

    private static IEnumerable<int> GetDivisors(int number)
    {
        for (var i = 1; i * i <= number; i++)
        {
            if (number % i == 0)
            {
                yield return i;
                if (i * i != number) yield return number / i;
            }
        }
    }
    
    private static IEnumerable<int[]> GetCombinations(int[] source, int k)
    {
        return GetCombinationsRecursive(source, k, 0);
    }

    private static IEnumerable<int[]> GetCombinationsRecursive(int[] source, int k, int start)
    {
        if (k == 0)
        {
            yield return Array.Empty<int>();
            yield break;
        }
        for (var i = start; i <= source.Length - k; i++)
        {
            foreach (var subCombination in GetCombinationsRecursive(source, k - 1, i + 1))
            {
                var result = new int[k];
                result[0] = source[i];
                Array.Copy(subCombination, 0, result, 1, subCombination.Length);
                yield return result;
            }
        }
    }
}

class Matrix
{
    private readonly int[,] _data;
    public int Rows { get; }
    public int Cols { get; }

    public Matrix(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
        _data = new int[rows, cols];
    }

    public int this[int r, int c]
    {
        get => _data[r, c];
        set => _data[r, c] = value;
    }

    public Matrix Multiply(Matrix other)
    {
        var result = new Matrix(Rows, other.Cols);
        for (var i = 0; i < result.Rows; i++)
        {
            for (var j = 0; j < result.Cols; j++)
            {
                long sum = 0;
                for (var k = 0; k < Cols; k++)
                {
                    sum += (long)this[i, k] * other[k, j];
                }
                result[i, j] = Mod(sum, HillCipherSolver.Modulus);
            }
        }
        return result;
    }

    public int Determinant()
    {
        return CalcDeterminant(_data);
    }
    
    public Matrix Inverse()
    {
        var det = Determinant();
        var detInv = ModInverse(det, HillCipherSolver.Modulus);
        var adj = new Matrix(Rows, Cols);
        for (var i = 0; i < Rows; i++)
        {
            for (var j = 0; j < Cols; j++)
            {
                var minor = GetMinorMatrix(_data, i, j);
                long cofactor = (long)Math.Pow(-1, i + j) * CalcDeterminant(minor);
                adj[j, i] = Mod(cofactor, HillCipherSolver.Modulus);
            }
        }
        for (var i = 0; i < Rows; i++)
        {
            for (var j = 0; j < Cols; j++)
            {
                adj[i, j] = Mod((long)adj[i, j] * detInv, HillCipherSolver.Modulus);
            }
        }
        return adj;
    }

    private static int Mod(long n, int m) => (int)((n % m + m) % m);

    private static int CalcDeterminant(int[,] matrix)
    {
        var size = matrix.GetLength(0);
        if (size == 1) return matrix[0, 0];
        long total = 0;
        for (var j = 0; j < size; j++)
        {
            var sign = (j % 2 == 0) ? 1 : -1;
            var minor = GetMinorMatrix(matrix, 0, j);
            total += sign * (long)matrix[0, j] * CalcDeterminant(minor);
            total = Mod(total, HillCipherSolver.Modulus);
        }
        return (int)total;
    }

    private static int[,] GetMinorMatrix(int[,] matrix, int rowToRemove, int colToRemove)
    {
        var size = matrix.GetLength(0);
        var minor = new int[size - 1, size - 1];
        for (int i = 0, mi = 0; i < size; i++)
        {
            if (i == rowToRemove) continue;
            for (int j = 0, mj = 0; j < size; j++)
            {
                if (j == colToRemove) continue;
                minor[mi, mj] = matrix[i, j];
                mj++;
            }
            mi++;
        }
        return minor;
    }
    
    public static int Gcd(int a, int b) => b == 0 ? Math.Abs(a) : Gcd(b, a % b);

    public static int ModInverse(int a, int m)
    {
        var m0 = m;
        var a0 = Mod(a, m);
        var y = 0;
        var x = 1;
        if (m == 1) return 0;
        while (a0 > 1)
        {
            var q = a0 / m0;
            var t = m0;
            m0 = a0 % m0;
            a0 = t;
            t = y;
            y = x - q * y;
            x = t;
        }
        if (x < 0) x += m;
        return x;
    }
}