using System;
using System.Collections.Generic;
using System.Globalization;

class Solution
{
    static int N, M;
    static int[] invoices;
    static int[] payments;
    static bool[] used;
    static List<int>[] matches;

    static void Main()
    {
        N = int.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
        M = int.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
        invoices = new int[N];
        for (int i = 0; i < N; i++)
            invoices[i] = int.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
        payments = new int[M];
        for (int j = 0; j < M; j++)
            payments[j] = int.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

        used = new bool[N];
        matches = new List<int>[M];
        DFS(0);

        for (int j = 0; j < M; j++)
        {
            char letter = (char)('A' + j);
            Console.Write(letter);
            Console.Write(" ");
            Console.Write(payments[j]);
            Console.Write(" - ");
            var list = matches[j];
            for (int k = 0; k < list.Count; k++)
            {
                if (k > 0) Console.Write(" ");
                Console.Write(invoices[list[k]]);
            }
            Console.WriteLine();
        }
    }

    static bool DFS(int idx)
    {
        if (idx == M) return true;
        return FindComb(idx, payments[idx], 0, new List<int>());
    }

    static bool FindComb(int pIdx, int rem, int start, List<int> current)
    {
        if (rem == 0)
        {
            matches[pIdx] = new List<int>(current);
            if (DFS(pIdx + 1)) return true;
            return false;
        }
        for (int i = start; i < N; i++)
        {
            if (used[i]) continue;
            int v = invoices[i];
            if (v > rem) continue;
            used[i] = true;
            current.Add(i);
            if (FindComb(pIdx, rem - v, i + 1, current)) return true;
            current.RemoveAt(current.Count - 1);
            used[i] = false;
        }
        return false;
    }
}
