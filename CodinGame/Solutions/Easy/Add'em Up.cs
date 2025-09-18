using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine()!);
        var parts = Console.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var heap = new MinHeap(N);
        for (int i = 0; i < N; i++)
            heap.Push(long.Parse(parts[i]));

        long totalCost = 0;
        while (heap.Count > 1)
        {
            long a = heap.Pop();
            long b = heap.Pop();
            long s = a + b;
            totalCost += s;
            heap.Push(s);
        }

        Console.WriteLine(totalCost);
    }

    // simple binary min-heap for longs
    class MinHeap
    {
        List<long> data;
        public int Count => data.Count;
        public MinHeap(int capacity) { data = new List<long>(capacity); }
        public void Push(long val)
        {
            data.Add(val);
            int i = data.Count - 1;
            while (i > 0)
            {
                int p = (i - 1) >> 1;
                if (data[p] <= data[i]) break;
                Swap(p, i);
                i = p;
            }
        }
        public long Pop()
        {
            long ret = data[0];
            int last = data.Count - 1;
            data[0] = data[last];
            data.RemoveAt(last);
            int i = 0, n = data.Count;
            while (true)
            {
                int l = (i << 1) + 1, r = l + 1, smallest = i;
                if (l < n && data[l] < data[smallest]) smallest = l;
                if (r < n && data[r] < data[smallest]) smallest = r;
                if (smallest == i) break;
                Swap(i, smallest);
                i = smallest;
            }
            return ret;
        }
        void Swap(int i, int j)
        {
            long t = data[i];
            data[i] = data[j];
            data[j] = t;
        }
    }
}
