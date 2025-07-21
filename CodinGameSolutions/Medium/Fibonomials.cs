// File: Program.cs
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string definedBy = Console.ReadLine().Trim();
        var aTokens = Console.ReadLine()
                             .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                             .Select(BigInteger.Parse);
        var bTokens = Console.ReadLine()
                             .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                             .Select(BigInteger.Parse);

        BigInteger x0 = BigInteger.Parse(Console.ReadLine().Trim());
        int n = int.Parse(Console.ReadLine().Trim());

        var seq = new FibonomialSequence(definedBy, aTokens, bTokens);
        var results = seq.Compute(x0, n);

        foreach (var line in results)
            Console.WriteLine(line);
    }
}

 public class FibonomialSequence
    {
        private readonly bool _isCoefs;
        private readonly List<BigInteger> _coef0;
        private readonly List<BigInteger> _coef1;
        private readonly List<BigInteger> _roots0;
        private readonly List<BigInteger> _roots1;
        private readonly Dictionary<(int, BigInteger), BigInteger> _memo;

        public FibonomialSequence(string definedBy,
                                  IEnumerable<BigInteger> aTokens,
                                  IEnumerable<BigInteger> bTokens)
        {
            _isCoefs = definedBy == "COEFS";

            if (_isCoefs)
            {
                _coef0 = aTokens.ToList();
                _coef1 = bTokens.ToList();
            }
            else
            {
                _roots0 = aTokens.ToList();
                _roots1 = bTokens.ToList();
            }

            _memo = new Dictionary<(int, BigInteger), BigInteger>();
        }

        public IReadOnlyList<string> Compute(BigInteger x0, int n)
        {
            var output = new List<string>(n);
            for (int k = 0; k < n; k++)
            {
                var y = Evaluate(k, x0);
                output.Add(Format(y));
            }
            return output;
        }

        private BigInteger Evaluate(int k, BigInteger z)
        {
            if (k == 0) return Eval0(z);
            if (k == 1) return Eval1(z);

            var key = (k, z);
            if (_memo.TryGetValue(key, out var v)) return v;

            var a = Evaluate(k - 1, Evaluate(k - 2, z));
            var b = Evaluate(k - 2, Evaluate(k - 1, z));
            var res = a + b;

            _memo[key] = res;
            return res;
        }

        private BigInteger Eval0(BigInteger z)
        {
            if (_isCoefs)
                return Horner(_coef0, z);

            return _roots0.Aggregate(BigInteger.One, (prod, r) => prod * (z - r));
        }

        private BigInteger Eval1(BigInteger z)
        {
            if (_isCoefs)
                return Horner(_coef1, z);

            return _roots1.Aggregate(BigInteger.One, (prod, r) => prod * (z - r));
        }

        private static BigInteger Horner(IList<BigInteger> coeffs, BigInteger x)
        {
            BigInteger acc = 0;
            for (int i = coeffs.Count - 1; i >= 0; i--)
                acc = acc * x + coeffs[i];
            return acc;
        }

        private string Format(BigInteger v)
        {
            var absV = BigInteger.Abs(v);
            var threshold = BigInteger.Pow(10, 12);
            if (absV < threshold)
                return v.ToString();

            var s = absV.ToString();
            int exp = s.Length - 1;
            const int sig = 7;
            string p = s.Length > sig + 1
                ? s.Substring(0, sig + 1)
                : s.PadRight(sig + 1, '0');

            var digs = p.Substring(0, sig).ToCharArray();
            int next = p[sig] - '0';
            if (next >= 5)
            {
                int i = sig - 1;
                while (i >= 0 && digs[i] == '9')
                {
                    digs[i] = '0';
                    i--;
                }
                if (i >= 0)
                    digs[i]++;
                else
                {
                    exp++;
                    digs = Enumerable.Repeat('0', sig).ToArray();
                    digs[0] = '1';
                }
            }

            string mant = $"{digs[0]}.{new string(digs, 1, sig - 1)}";
            string sign = v.Sign < 0 ? "-" : "";
            string eSign = exp >= 0 ? "+" : "-";
            return $"{sign}{mant}E{eSign}{Math.Abs(exp)}";
        }
    }
