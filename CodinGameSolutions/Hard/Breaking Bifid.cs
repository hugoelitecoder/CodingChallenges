using System;
using System.Text;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        var plainText1 = Console.ReadLine();
        var cipherText1 = Console.ReadLine();
        var cipherText2 = Console.ReadLine();
        var solver = new BifidSolver();
        var normPlain = Normalize(plainText1);
        var normCipher1 = Normalize(cipherText1);
        var normCipher2 = Normalize(cipherText2);
        var mat = solver.BuildMatrix(normPlain, normCipher1);
        solver.FindGroups(mat);
        var output = solver.Decrypt(mat, normCipher2);
        Console.WriteLine(output);
    }

    static string Normalize(string s)
    {
        var sb = new StringBuilder();
        foreach (var ch in s)
        {
            if (ch == ' ') continue;
            if (ch == 'J' || ch == 'j') sb.Append('I');
            else if (ch >= 'a' && ch <= 'z') sb.Append((char)(ch - 32));
            else sb.Append(ch);
        }
        return sb.ToString();
    }
}

public enum Same { UNKNOWN, TRUE, FALSE }
public struct Matrix
{
    public Same[,] data;
    public Matrix(int n)
    {
        data = new Same[n, n];
    }
    public Same this[int i, int j]
    {
        get => data[i, j];
        set => data[i, j] = value;
    }
}

class BifidSolver
{
    private const int NR_LETTERS = 25;
    private const int NR_VARIABLES = 50;

    private int CharToIdx(char c)
    {
        var v = c - 'A';
        if (c > 'I') v--;
        return v;
    }

    private char IdxToChar(int i)
    {
        var c = (char)(i + 'A');
        if (c >= 'J') c++;
        return c;
    }

    private void RowEqualsRow(ref Matrix mat, int a, int b)
    {
        mat[a, b] = Same.TRUE;
        mat[b, a] = Same.TRUE;
    }

    private void RowEqualsCol(ref Matrix mat, int a, int b)
    {
        mat[a, b + NR_LETTERS] = Same.TRUE;
        mat[b + NR_LETTERS, a] = Same.TRUE;
    }

    private void ColEqualsRow(ref Matrix mat, int a, int b)
    {
        mat[a + NR_LETTERS, b] = Same.TRUE;
        mat[b, a + NR_LETTERS] = Same.TRUE;
    }

    private void ColEqualsCol(ref Matrix mat, int a, int b)
    {
        mat[a + NR_LETTERS, b + NR_LETTERS] = Same.TRUE;
        mat[b + NR_LETTERS, a + NR_LETTERS] = Same.TRUE;
    }

    public Matrix BuildMatrix(string clear, string enc)
    {
        var mat = new Matrix(NR_VARIABLES);
        var eIdx = 0;
        var eType = 0;
        for (var c = 0; c < clear.Length; c++)
        {
            var cl = CharToIdx(clear[c]);
            var el = CharToIdx(enc[eIdx]);
            if (eType == 0)
            {
                RowEqualsRow(ref mat, cl, el);
                eType = 1;
            }
            else
            {
                RowEqualsCol(ref mat, cl, el);
                eType = 0;
                eIdx++;
            }
        }
        for (var c = 0; c < clear.Length; c++)
        {
            var cl = CharToIdx(clear[c]);
            var el = CharToIdx(enc[eIdx]);
            if (eType == 0)
            {
                ColEqualsRow(ref mat, cl, el);
                eType = 1;
            }
            else
            {
                ColEqualsCol(ref mat, cl, el);
                eType = 0;
                eIdx++;
            }
        }
        return mat;
    }

    private bool InheritRow(ref Matrix mat, int row, int from)
    {
        var changed = false;
        for (var i = 0; i < NR_VARIABLES; i++)
        {
            if (mat[from, i] == Same.TRUE)
            {
                if (mat[row, i] == Same.UNKNOWN)
                {
                    mat[row, i] = Same.TRUE;
                    changed = true;
                }
            }
            else if (mat[from, i] == Same.FALSE)
            {
                if (mat[row, i] == Same.UNKNOWN)
                {
                    mat[row, i] = Same.FALSE;
                    changed = true;
                }
            }
        }
        return changed;
    }

    private bool InheritInverseRow(ref Matrix mat, int row, int from)
    {
        var changed = false;
        for (var i = 0; i < NR_VARIABLES; i++)
        {
            if (mat[from, i] == Same.TRUE)
            {
                if (mat[row, i] == Same.UNKNOWN)
                {
                    mat[row, i] = Same.FALSE;
                    changed = true;
                }
            }
        }
        return changed;
    }

    private void CopyRowsToColumns(ref Matrix mat)
    {
        for (var r = 0; r < NR_VARIABLES; r++)
        {
            for (var c = 0; c < NR_VARIABLES; c++)
            {
                if (mat[r, c] != mat[c, r])
                {
                    if (mat[r, c] == Same.UNKNOWN)
                    {
                        mat[r, c] = mat[c, r];
                    }
                    else if (mat[c, r] == Same.UNKNOWN)
                    {
                        mat[c, r] = mat[r, c];
                    }
                }
            }
        }
    }

    private void SetRestToFalse(ref Matrix mat, int r, int offset)
    {
        for (var c = offset; c < offset + NR_LETTERS; c++)
        {
            if (mat[r, c] == Same.UNKNOWN)
            {
                mat[r, c] = Same.FALSE;
            }
        }
    }

    private void NoMoreThanFive(ref Matrix mat)
    {
        for (var r = 0; r < NR_VARIABLES; r++)
        {
            var cnt = 0;
            for (var c = 0; c < NR_LETTERS; c++)
            {
                if (mat[r, c] == Same.TRUE) cnt++;
            }
            if (cnt >= 5)
            {
                SetRestToFalse(ref mat, r, 0);
            }
            cnt = 0;
            for (var c = NR_LETTERS; c < NR_VARIABLES; c++)
            {
                if (mat[r, c] == Same.TRUE) cnt++;
            }
            if (cnt >= 5)
            {
                SetRestToFalse(ref mat, r, NR_LETTERS);
            }
        }
    }

    private bool SinglePass(ref Matrix mat)
    {
        var changed = false;
        for (var i = 0; i < NR_VARIABLES; i++)
        {
            for (var j = 0; j < NR_VARIABLES; j++)
            {
                if (mat[i, j] == Same.TRUE)
                {
                    changed = changed || InheritRow(ref mat, i, j);
                }
                if (mat[i, j] == Same.FALSE)
                {
                    changed = changed || InheritInverseRow(ref mat, i, j);
                }
            }
        }
        CopyRowsToColumns(ref mat);
        NoMoreThanFive(ref mat);
        return changed;
    }

    public void FindGroups(Matrix mat)
    {
        var changed = true;
        while (changed)
        {
            changed = SinglePass(ref mat);
        }
    }

    private char FindMatch(Matrix mat, int rowRow, int colRow)
    {
        for (var i = 0; i < NR_LETTERS; i++)
        {
            if (mat[rowRow, i] == Same.TRUE && mat[colRow, i + NR_LETTERS] == Same.TRUE)
            {
                return IdxToChar(i);
            }
        }
        throw new Exception("Can't find match");
    }

    public string Decrypt(Matrix mat, string enc)
    {
        var n = enc.Length;
        var decrypted = new StringBuilder();
        var idx1 = 0;
        var idx2 = n / 2;
        var useRow1 = true;
        var useRow2 = (n % 2 == 0);
        for (var i = 0; i < n; i++)
        {
            var row1 = CharToIdx(enc[idx1]) + (useRow1 ? 0 : NR_LETTERS);
            var row2 = CharToIdx(enc[idx2]) + (useRow2 ? 0 : NR_LETTERS);
            decrypted.Append(FindMatch(mat, row1, row2));
            if (!useRow1) { useRow1 = true; idx1++; }
            else useRow1 = false;
            if (!useRow2) { useRow2 = true; idx2++; }
            else useRow2 = false;
        }
        return decrypted.ToString();
    }
}
