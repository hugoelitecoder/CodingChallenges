using System;
using System.Text;
using System.Collections.Generic;

class Solution
{
    private static Dictionary<string, Polynomial> _cache = new Dictionary<string, Polynomial>();

    private static Polynomial GetPolynomial(Graph graph)
    {
        var key = graph.GetCacheKey();
        if (_cache.ContainsKey(key))
        {
            return _cache[key];
        }

        var edge = graph.FindEdge();
        Polynomial result;

        if (edge == null)
        {
            var v = graph.V;
            if (v == 0)
            {
                result = new Polynomial(new long[0]);
            }
            else
            {
                var coeffs = new long[v];
                coeffs[v - 1] = 1;
                result = new Polynomial(coeffs);
            }
        }
        else
        {
            var (u, v) = edge.Value;
            var gMinusE = graph.DeleteEdge(u, v);
            var pMinusE = GetPolynomial(gMinusE);
            var gContractE = graph.ContractEdge(u, v);
            var pContractE = GetPolynomial(gContractE);
            result = Polynomial.Subtract(pMinusE, pContractE);
        }
        
        _cache[key] = result;
        return result;
    }
    
    static void Main(string[] args)
    {
        var nameToIndex = new Dictionary<string, int>();
        var links = new List<(int u, int v)>();
        var nextIndex = 0;
        
        var N = int.Parse(Console.ReadLine());
        for (var i = 0; i < N; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var countryA = inputs[0];
            var countryB = inputs[1];

            if (!nameToIndex.ContainsKey(countryA))
            {
                nameToIndex[countryA] = nextIndex++;
            }
            if (!nameToIndex.ContainsKey(countryB))
            {
                nameToIndex[countryB] = nextIndex++;
            }
            var u = nameToIndex[countryA];
            var v = nameToIndex[countryB];
            links.Add((u, v));
        }

        var vertexCount = nameToIndex.Count;
        var adjMatrix = new bool[vertexCount, vertexCount];
        foreach (var link in links)
        {
            adjMatrix[link.u, link.v] = true;
            adjMatrix[link.v, link.u] = true;
        }

        var initialGraph = new Graph(adjMatrix);
        var poly = GetPolynomial(initialGraph);

        var K = int.Parse(Console.ReadLine());
        var cValues = new int[K];
        for (var i = 0; i < K; i++)
        {
             cValues[i] = int.Parse(Console.ReadLine());
        }

        for (var i = 0; i < K; i++)
        {
            var C = cValues[i];
            var answer = poly.Evaluate(C);
            Console.WriteLine(answer);
        }
    }
}

class Polynomial
{
    private readonly long[] _coeffs;

    public Polynomial(long[] coeffs)
    {
        var lastNonZero = -1;
        for (var i = coeffs.Length - 1; i >= 0; i--)
        {
            if (coeffs[i] != 0)
            {
                lastNonZero = i;
                break;
            }
        }
        
        if (lastNonZero == -1)
        {
            _coeffs = new long[] { 0 };
        }
        else
        {
            _coeffs = new long[lastNonZero + 1];
            Array.Copy(coeffs, _coeffs, lastNonZero + 1);
        }
    }

    public long Evaluate(long k)
    {
        if (k == 0)
        {
            return 0;
        }
        
        var qValue = 0L;
        for (var i = _coeffs.Length - 1; i >= 0; i--)
        {
            qValue = qValue * k + _coeffs[i];
        }
        
        return qValue * k;
    }

    public static Polynomial Subtract(Polynomial p1, Polynomial p2)
    {
        var len1 = p1._coeffs.Length;
        var len2 = p2._coeffs.Length;
        var maxLen = Math.Max(len1, len2);
        var newCoeffs = new long[maxLen];

        for (var i = 0; i < maxLen; i++)
        {
            var c1 = (i < len1) ? p1._coeffs[i] : 0;
            var c2 = (i < len2) ? p2._coeffs[i] : 0;
            newCoeffs[i] = c1 - c2;
        }

        return new Polynomial(newCoeffs);
    }
}

class Graph
{
    public int V { get; }
    private readonly bool[,] _adj;
    private string _cacheKey;

    public Graph(bool[,] adj)
    {
        V = adj.GetLength(0);
        _adj = adj;
        _cacheKey = null;
    }

    public (int u, int v)? FindEdge()
    {
        for (var i = 0; i < V; i++)
        {
            for (var j = i + 1; j < V; j++)
            {
                if (_adj[i, j])
                {
                    return (i, j);
                }
            }
        }
        return null;
    }

    public Graph DeleteEdge(int u, int v)
    {
        var newAdj = (bool[,])_adj.Clone();
        newAdj[u, v] = false;
        newAdj[v, u] = false;
        return new Graph(newAdj);
    }

    public Graph ContractEdge(int u, int v)
    {
        if (u > v)
        {
            var temp = u;
            u = v;
            v = temp;
        }
        
        var tempAdj = (bool[,])_adj.Clone();

        for (var i = 0; i < V; i++)
        {
            if (tempAdj[v, i] && i != u)
            {
                tempAdj[u, i] = true;
                tempAdj[i, u] = true;
            }
        }
        
        var newAdj = new bool[V - 1, V - 1];
        for (var i = 0; i < V - 1; i++)
        {
            for (var j = 0; j < V - 1; j++)
            {
                var oldI = i < v ? i : i + 1;
                var oldJ = j < v ? j : j + 1;
                newAdj[i, j] = tempAdj[oldI, oldJ];
            }
        }

        return new Graph(newAdj);
    }

    public string GetCacheKey()
    {
        if (_cacheKey != null)
        {
            return _cacheKey;
        }
        
        var sb = new StringBuilder();
        sb.Append(V);
        sb.Append(':');
        for (var i = 0; i < V; i++)
        {
            for (var j = i + 1; j < V; j++)
            {
                sb.Append(_adj[i, j] ? '1' : '0');
            }
        }
        _cacheKey = sb.ToString();
        return _cacheKey;
    }
}

