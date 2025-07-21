using System;
using System.Collections.Generic;

class Solution
{
    struct Cell : IComparable<Cell>
    {
        public int Height, R, C;
        public Cell(int h, int r, int c) { Height = h; R = r; C = c; }
        public int CompareTo(Cell other) => Height - other.Height;
    }

    class MinHeap
    {
        List<Cell> data = new List<Cell>();
        public int Count => data.Count;
        public void Push(Cell x)
        {
            data.Add(x);
            int i = data.Count - 1;
            while (i > 0)
            {
                int p = (i - 1) >> 1;
                if (data[p].CompareTo(data[i]) <= 0) break;
                var tmp = data[p]; data[p] = data[i]; data[i] = tmp;
                i = p;
            }
        }
        public Cell Pop()
        {
            var ret = data[0];
            int last = data.Count - 1;
            data[0] = data[last];
            data.RemoveAt(last);
            int i = 0;
            while (true)
            {
                int l = 2*i + 1, r = 2*i + 2, smallest = i;
                if (l < data.Count && data[l].CompareTo(data[smallest]) < 0) smallest = l;
                if (r < data.Count && data[r].CompareTo(data[smallest]) < 0) smallest = r;
                if (smallest == i) break;
                var tmp = data[i]; data[i] = data[smallest]; data[smallest] = tmp;
                i = smallest;
            }
            return ret;
        }
    }

    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var grid = new int[N, N];
        for (int r = 0; r < N; r++)
        {
            var line = Console.ReadLine();
            for (int c = 0; c < N; c++)
                grid[r, c] = line[c] - 'A' + 1;
        }

        bool[,] vis = new bool[N, N];
        var heap = new MinHeap();

        // Push boundary
        for (int i = 0; i < N; i++)
        {
            vis[0, i] = true; heap.Push(new Cell(grid[0, i], 0, i));
            vis[N-1, i] = true; heap.Push(new Cell(grid[N-1, i], N-1, i));
            vis[i, 0] = true; heap.Push(new Cell(grid[i, 0], i, 0));
            vis[i, N-1] = true; heap.Push(new Cell(grid[i, N-1], i, N-1));
        }

        int[] dr = { -1, 1, 0, 0 }, dc = { 0, 0, -1, 1 };
        int volume = 0;

        while (heap.Count > 0)
        {
            var cell = heap.Pop();
            for (int d = 0; d < 4; d++)
            {
                int nr = cell.R + dr[d], nc = cell.C + dc[d];
                if (nr < 0 || nr >= N || nc < 0 || nc >= N || vis[nr, nc])
                    continue;
                vis[nr, nc] = true;
                int nh = grid[nr, nc];
                // If neighbor lower, it traps water up to cell.Height
                if (nh < cell.Height)
                {
                    volume += cell.Height - nh;
                    nh = cell.Height;
                }
                heap.Push(new Cell(nh, nr, nc));
            }
        }

        Console.WriteLine(volume);
    }
}
