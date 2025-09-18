using System;
using System.Collections.Generic;

public class Solution
{
    public static void Main(string[] args)
    {
        var tStr = Console.ReadLine();
        if (!int.TryParse(tStr, out var t)) return;
        
        while (t-- > 0)
        {
            var line = Console.ReadLine().Split(' ');
            var p = ulong.Parse(line[0]);
            var r = ulong.Parse(line[1]);
            var c = ulong.Parse(line[2]);
            
            var calculator = new FractalGridSumCalculator(p, r, c);
            var result = calculator.Calculate();
            Console.WriteLine(result);
        }
    }
}

public class FractalGridSumCalculator
{
    private const ulong MAX = 10000000000000000000UL;
    private const long MOD = 1000000007;

    private readonly ulong _p;
    private readonly ulong _r;
    private readonly ulong _c;

    private readonly ulong[] _powers;
    private readonly long[] _values;
    private readonly Dictionary<(ulong, ulong), long> _memo;

    public FractalGridSumCalculator(ulong p, ulong r, ulong c)
    {
        _p = p;
        _r = r;
        _c = c;
        _powers = new ulong[65];
        _values = new long[65];
        _memo = new Dictionary<(ulong, ulong), long>();
    }

    public long Calculate()
    {
        if (_r == 0 || _c == 0 || _p == 0)
        {
            return 0;
        }
        if (_p == 1)
        {
            return Multiply((long)(_r % MOD), (long)(_c % MOD));
        }

        var maxLevel = Precompute();
        return Solve(maxLevel, _r, _c);
    }

    private int Precompute()
    {
        _powers[0] = 1;
        _values[0] = 1;
        var pSum = SumToN(_p);

        var maxLevel = 0;
        for (var i = 1; i < _powers.Length; i++)
        {
            if (MAX / _p < _powers[i - 1])
            {
                break;
            }
            _powers[i] = _powers[i - 1] * _p;
            _values[i] = Multiply(_values[i - 1], pSum);
            maxLevel = i;
        }
        return maxLevel;
    }
    
    private long Solve(int level, ulong r, ulong cParam)
    {
        var c = Math.Min(r, cParam);
        if (level < 0)
        {
            return 0;
        }
        
        var key = (r, c);
        if (_memo.ContainsKey(key))
        {
            return _memo[key];
        }

        var power = _powers[level];
        var val = _values[level];
        var res = 0L;
        var rBlocks = r / power;
        var cBlocks = c / power;
        var rightColSum = 0L;
        var rightColComputed = false;

        for (ulong i = 1; i <= _p; i++)
        {
            var hMult = Math.Min(i, cBlocks);
            var vMult = Math.Min(i, rBlocks);

            if (i > vMult)
            {
                var finalVMult = rBlocks;
                var finalHMult = Math.Min(i, cBlocks);
                var rRem = r - finalVMult * power;
                var bottomRowSum = 0L;
                if (rRem > 0)
                {
                    bottomRowSum = Multiply((long)finalHMult, Solve(level - 1, rRem, power));
                }
                
                var cornerSum = 0L;
                if (i > finalHMult)
                {
                     var cRem = c - finalHMult * power;
                     if (rRem > 0 && cRem > 0)
                     {
                        cornerSum = Solve(level - 1, rRem, cRem);
                     }
                }
                res = (res + bottomRowSum + cornerSum) % MOD;
                break;
            }
            
            res = (res + Multiply((long)hMult, val)) % MOD;

            if (i > hMult)
            {
                if (!rightColComputed)
                {
                    var cRem = c - hMult * power;
                    if (cRem > 0)
                    {
                        rightColSum = Solve(level - 1, power, cRem);
                    }
                    rightColComputed = true;
                }
                res = (res + rightColSum) % MOD;
            }
        }

        if (res < 0)
        {
            res += MOD;
        }

        _memo[key] = res;
        return res;
    }

    private long SumToN(ulong n)
    {
        var term1 = n;
        var term2 = n + 1;
        if (term1 % 2 == 0)
        {
            term1 /= 2;
        }
        else
        {
            term2 /= 2;
        }
        var term1Mod = (long)(term1 % MOD);
        var term2Mod = (long)(term2 % MOD);
        return Multiply(term1Mod, term2Mod);
    }
    
    private long Multiply(long a, long b)
    {
        a %= MOD;
        b %= MOD;
        var res = (a * b) % MOD;
        return res < 0 ? res + MOD : res;
    }
}