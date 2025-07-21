using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        int PN = int.Parse(Console.ReadLine());
        int FN = int.Parse(Console.ReadLine());
        int FM = int.Parse(Console.ReadLine());

        int[] species = new int[PN];
        int[][] feats = new int[PN][];

        for (int i = 0; i < PN; i++)
        {
            var parts = Console.ReadLine().Split().Select(int.Parse).ToArray();
            species[i] = parts[1];
            feats[i] = parts.Skip(2).ToArray();
        }

        var selector = new DecisionTreeSelector(PN, FN, FM, species, feats);
        int[] best = selector.FindBestFeatures();

        Console.WriteLine(string.Join(" ", best.Select(f => f + 1)));
    }
}

class DecisionTreeSelector
{
    private int PN, FN, FM;
    private int[] species;
    private int[][] feats;

    public DecisionTreeSelector(int pn, int fn, int fm, int[] species, int[][] feats)
    {
        PN = pn;
        FN = fn;
        FM = fm;
        this.species = species;
        this.feats = feats;
    }

    public int[] FindBestFeatures()
    {
        double bestEntropy = double.MaxValue;
        int[] bestFeatures = new int[FM];

        int maxMask = 1 << FN;
        for (int mask = 0; mask < maxMask; mask++)
        {
            if (CountBits(mask) != FM) continue;
            var sel = GetSelectedFeatures(mask).ToList();
            double ent = ComputeGlobalEntropy(sel);

            if (IsBetter(ent, bestEntropy, sel, bestFeatures))
            {
                bestEntropy = ent;
                bestFeatures = sel.ToArray();
            }
        }

        Array.Sort(bestFeatures);
        return bestFeatures;
    }

    private IEnumerable<int> GetSelectedFeatures(int mask)
    {
        for (int f = 0; f < FN; f++)
            if (((mask >> f) & 1) == 1)
                yield return f;
    }

    private bool IsBetter(double ent, double bestEntropy, List<int> sel, int[] bestFeatures)
    {
        if (ent < bestEntropy - 1e-12) return true;
        if (Math.Abs(ent - bestEntropy) < 1e-12)
        {
            for (int i = 0; i < FM; i++)
            {
                if (sel[i] < bestFeatures[i]) return true;
                if (sel[i] > bestFeatures[i]) return false;
            }
        }
        return false;
    }

    private double ComputeGlobalEntropy(List<int> features)
    {
        var allIdx = Enumerable.Range(0, PN).ToList();
        return RecurseNode(allIdx, features);
    }

    private double RecurseNode(List<int> idxs, List<int> features)
    {
        int n = idxs.Count;
        if (n == 0) return 0.0;

        double nodeEntropy = Entropy(idxs);
        double bestChildEntropy = nodeEntropy;
        List<int> bestLeft = null;
        List<int> bestRight = null;

        foreach (int f in features)
        {
            var vals = GetValues(idxs, f);
            foreach (int v in vals)
            {
                var (left, right) = Split(idxs, f, v);
                double weighted = ((double)left.Count / n) * Entropy(left)
                                + ((double)right.Count / n) * Entropy(right);
                if (weighted < bestChildEntropy - 1e-12)
                {
                    bestChildEntropy = weighted;
                    bestLeft = left;
                    bestRight = right;
                }
            }
        }

        if (bestLeft == null)
            return ((double)n / PN) * nodeEntropy;

        return RecurseNode(bestLeft, features)
             + RecurseNode(bestRight, features);
    }

    private HashSet<int> GetValues(List<int> idxs, int f)
    {
        var set = new HashSet<int>();
        foreach (var i in idxs)
            set.Add(feats[i][f]);
        return set;
    }

    private (List<int> left, List<int> right) Split(List<int> idxs, int f, int v)
    {
        var left = new List<int>();
        var right = new List<int>();
        foreach (var i in idxs)
        {
            if (feats[i][f] == v) left.Add(i);
            else right.Add(i);
        }
        return (left, right);
    }

    private double Entropy(List<int> idxs)
    {
        int n = idxs.Count;
        if (n <= 1) return 0.0;
        var cnt = new Dictionary<int,int>();
        foreach (var i in idxs)
        {
            int s = species[i];
            if (!cnt.ContainsKey(s)) cnt[s] = 0;
            cnt[s]++;
        }
        double h = 0.0;
        foreach (var kv in cnt)
        {
            double p = (double)kv.Value / n;
            h -= p * Math.Log(p, 2);
        }
        return h;
    }

    private int CountBits(int x)
    {
        int c = 0;
        while (x > 0) { c += x & 1; x >>= 1; }
        return c;
    }
}