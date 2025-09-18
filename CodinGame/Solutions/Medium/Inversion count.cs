using System;
using System.IO;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        int M = int.Parse(inputs[0]);
        long A = long.Parse(inputs[1]);
        long S = long.Parse(inputs[2]);
        int N = int.Parse(inputs[3]);

        var counter = new InversionCounter(M, A, S, N);
        long inversions = counter.Calculate();
        Console.WriteLine(inversions);
    }
}

public class InversionCounter
{
    private readonly int _M;
    private readonly long _A;
    private readonly long _S;
    private readonly int _N;
    private int[] _arr;

    public InversionCounter(int M, long A, long S, int N)
    {
        _M = M;
        _A = A;
        _S = S;
        _N = N;
        _arr = new int[N];
        GenerateSequence();
    }

    private void GenerateSequence()
    {
        long x = _S;
        for (int i = 0; i < _N; i++)
        {
            x = (_A * x) % _M;
            _arr[i] = (int)x;
        }
    }

    public long Calculate()
    {
        return SortCount(0, _N - 1);
    }

    private long SortCount(int left, int right)
    {
        if (left >= right) return 0;
        int mid = left + (right - left) / 2;
        long count = SortCount(left, mid);
        count += SortCount(mid + 1, right);
        count += MergeCount(left, mid, right);
        return count;
    }

    private long MergeCount(int left, int mid, int right)
    {
        int n1 = mid - left + 1;
        int n2 = right - mid;
        int[] L = new int[n1];
        int[] R = new int[n2];
        Array.Copy(_arr, left, L, 0, n1);
        Array.Copy(_arr, mid + 1, R, 0, n2);

        int i = 0, j = 0, k = left;
        long invCount = 0;

        while (i < n1 && j < n2)
        {
            if (L[i] <= R[j])
            {
                _arr[k++] = L[i++];
            }
            else
            {
                _arr[k++] = R[j++];
                invCount += n1 - i;
            }
        }

        while (i < n1) _arr[k++] = L[i++];
        while (j < n2) _arr[k++] = R[j++];

        return invCount;
    }
}
