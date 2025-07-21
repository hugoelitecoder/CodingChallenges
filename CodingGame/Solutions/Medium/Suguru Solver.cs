using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Linq;

class Solution
{
    private static int width, height, cellCount, emptyCount;
    private static char[] cageTags;
    private static int[] initialValues, gridValues;
    private static int[] regionId, regionSizes, regionBitmask;
    private static int[][] neighbors;
    private static int[] emptyCells;
    private static Dictionary<int, List<int>> regions;

    private static readonly (int dr, int dc)[] CardinalDirections = { (-1, 0), (1, 0), (0, -1), (0, 1) };
    private static readonly (int dr, int dc)[] AllDirections = {
        (-1, -1), (-1, 0), (-1, 1),
        (0, -1),           (0, 1),
        (1, -1),  (1, 0),  (1, 1)
    };

    static void Main()
    {
        var dims = Console.ReadLine().Split();
        width = int.Parse(dims[0]);
        height = int.Parse(dims[1]);
        cellCount = width * height;

        cageTags = new char[cellCount];
        initialValues = new int[cellCount];

        for (int r = 0; r < height; r++)
        {
            var inputs = Console.ReadLine().Split();
            for (int c = 0; c < width; c++)
            {
                int idx = r * width + c;
                var token = inputs[c];
                cageTags[idx] = token[0];
                initialValues[idx] = token[1] == '.' ? 0 : token[1] - '0';
            }
        }

        BuildNeighbors();
        FloodFillRegions(out int regionCount);

        regionSizes = new int[regionCount];
        regionBitmask = new int[regionCount];
        gridValues = (int[])initialValues.Clone();

        var empties = new List<int>(cellCount);
        for (int i = 0; i < cellCount; i++)
        {
            int rid = regionId[i];
            regionSizes[rid] = regions[rid].Count;
            if (gridValues[i] > 0)
            {
                regionBitmask[rid] |= 1 << (gridValues[i] - 1);
            }
            else
            {
                empties.Add(i);
            }
        }

        emptyCells = empties.ToArray();
        emptyCount = emptyCells.Length;

        Solve(0);
    }

    static void BuildNeighbors()
    {
        neighbors = new int[cellCount][];
        for (int i = 0; i < cellCount; i++)
        {
            int r = i / width, c = i % width;
            var list = new List<int>();
            foreach (var (dr, dc) in AllDirections)
            {
                int nr = r + dr, nc = c + dc;
                if (nr < 0 || nr >= height || nc < 0 || nc >= width)
                    continue;
                list.Add(nr * width + nc);
            }
            neighbors[i] = list.ToArray();
        }
    }

    static void FloodFillRegions(out int regionCount)
    {
        regionId = Enumerable.Repeat(-1, cellCount).ToArray();
        regions = new Dictionary<int, List<int>>();
        int nextId = 0;

        for (int i = 0; i < cellCount; i++)
        {
            if (regionId[i] >= 0) continue;
            char tag = cageTags[i];
            var stack = new Stack<int>();
            stack.Push(i);
            regionId[i] = nextId;
            regions[nextId] = new List<int>();

            while (stack.Count > 0)
            {
                int u = stack.Pop();
                regions[nextId].Add(u);
                int rr = u / width, cc = u % width;
                foreach (var (dr, dc) in CardinalDirections)
                {
                    int nr = rr + dr, nc = cc + dc;
                    if (nr < 0 || nr >= height || nc < 0 || nc >= width)
                        continue;
                    int v = nr * width + nc;
                    if (regionId[v] >= 0 || cageTags[v] != tag)
                        continue;
                    regionId[v] = nextId;
                    stack.Push(v);
                }
            }
            nextId++;
        }
        regionCount = nextId;
    }

    static bool Solve(int depth)
    {
        if (depth == emptyCount)
        {
            PrintSolution();
            return true;
        }

        int bestIdx = depth, minOptions = int.MaxValue, optionsMask = 0;
        for (int k = depth; k < emptyCount; k++)
        {
            int idx = emptyCells[k];
            int rid = regionId[idx];
            int mask = ((1 << regionSizes[rid]) - 1) & ~regionBitmask[rid];
            foreach (int n in neighbors[idx])
            {
                int val = gridValues[n];
                if (val > 0)
                    mask &= ~(1 << (val - 1));
            }
            int count = BitOperations.PopCount((uint)mask);
            if (count < minOptions)
            {
                minOptions = count;
                bestIdx = k;
                optionsMask = mask;
                if (count <= 1)
                    break;
            }
        }

        if (minOptions == 0)
            return false;

        Swap(emptyCells, depth, bestIdx);
        int cell = emptyCells[depth];
        int rid0 = regionId[cell];
        int maskCandidate = optionsMask;

        while (maskCandidate != 0)
        {
            int bit = maskCandidate & -maskCandidate;
            maskCandidate -= bit;
            int num = BitOperations.TrailingZeroCount((uint)bit) + 1;
            gridValues[cell] = num;
            regionBitmask[rid0] |= bit;

            if (Solve(depth + 1))
                return true;

            regionBitmask[rid0] &= ~bit;
            gridValues[cell] = 0;
        }

        Swap(emptyCells, depth, bestIdx);
        return false;
    }

    static void PrintSolution()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < cellCount; i++)
        {
            sb.Append(gridValues[i]);
            if ((i + 1) % width == 0)
                sb.AppendLine();
        }
        Console.Write(sb.ToString());
    }

    static void Swap(int[] arr, int i, int j)
    {
        (arr[i], arr[j]) = (arr[j], arr[i]);
    }
}
