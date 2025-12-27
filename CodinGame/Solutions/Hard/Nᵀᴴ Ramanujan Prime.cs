using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Solution
{
    private class DebugLogger
    {
        private readonly Action<string> _writer;
        public DebugLogger(Action<string> writer)
        {
            _writer = writer;
        }
        public void Info(string msg)
        {
            _writer($"[DEBUG] {msg}");
        }
        public void Stat(string key, object value)
        {
            _writer($"[DEBUG] [STATS] {key}: {value}");
        }
        public void StartSection()
        {
            _writer($"[DEBUG] --- Start Statistics ---");
        }
        public void EndSection()
        {
            _writer("[DEBUG] --- End Statistics ---");
        }
        public void Check(string description, bool condition)
        {
            var status = condition ? "PASS" : "FAIL";
            _writer($"[DEBUG] [STATS] {description}: {status}");
        }
    }

    static void Main(string[] args)
    {
        var debug = new DebugLogger(Console.Error.WriteLine);
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var n = int.Parse(Console.ReadLine());
        debug.Info($"Input n: {n}");
        
        var solver = new RamanujanCalculator();
        var result = solver.FindRn(n);
        
        stopwatch.Stop();

        debug.Info($"Calculated sieve limit: {solver.Limit}");
        if (solver.IsSieved)
        {
            debug.Info($"Total primes found up to limit: {solver.GetPi(solver.Limit)}");
        }
        
        var targetCount = n - 1;
        debug.Info($"Searching for largest x where pi(x) - pi(x/2) == {targetCount}");
        
        if (solver.FoundX != -1)
        {
            debug.Info($"Found match at x = {solver.FoundX}. R_n is x + 1.");
        }
        debug.Info($"Found R_n: {result}");
        debug.Info($"Total time taken: {stopwatch.ElapsedMilliseconds} ms");

        PrintDebugStatistics(n, result, solver, debug);
        
        Console.WriteLine(result);
    }

    private static void PrintDebugStatistics(int n, int rn, RamanujanCalculator solver, DebugLogger log)
    {
        log.StartSection();
        var lowerBound = 2 * n * Math.Log(2 * n);
        var upperBound = 4 * n * Math.Log(4 * n);
        log.Stat("Theoretical Lower Bound (2n*ln(2n))", $"> {lowerBound:F2}");
        log.Stat("Theoretical Upper Bound (4n*ln(4n))", $"< {upperBound:F2}");
        log.Check($"R_n ({rn}) vs Bounds", rn > lowerBound && rn < upperBound);
        
        if (!solver.IsSieved)
        {
            log.Stat("Sieve data", "not available.");
            log.EndSection();
            return;
        }

        var limit = solver.Limit;
        var pntApprox = (limit > 1) ? limit / Math.Log(limit) : 0;
        log.Stat("Sieve Limit", limit);
        log.Stat("Primes found (pi[limit])", solver.GetPi(limit));
        log.Stat("PNT Approx (limit/ln(limit))", $"{pntApprox:F2}");
        
        if (rn > 0 && rn <= limit)
        {
            var countAtRnMinus1 = solver.GetPi(rn - 1) - solver.GetPi((rn - 1) / 2);
            log.Stat($"pi(R_n-1) - pi((R_n-1)/2)", $"{solver.GetPi(rn-1)} - {solver.GetPi((rn-1)/2)} = {countAtRnMinus1}");
            log.Check($"Check (should be n-1={n-1})", countAtRnMinus1 == n - 1);
            
            var countAtRn = solver.GetPi(rn) - solver.GetPi(rn / 2);
            log.Stat($"pi(R_n) - pi(R_n/2)", $"{solver.GetPi(rn)} - {solver.GetPi(rn/2)} = {countAtRn}");
            log.Check($"Check (should be >= n={n})", countAtRn >= n);
        }
        else
        {
            log.Info("R_n was not found or out of bounds, statistics skipped.");
        }
        
        log.Stat("5th prime (GetKthPrime(5))", solver.GetKthPrime(5));
        log.Stat("Prime count up to 100 (GetPi(100))", solver.GetPi(100));
        log.Stat("Is 97 prime? (IsPrime(97))", solver.IsPrime(97));
        log.Stat("Is 100 prime? (IsPrime(100))", solver.IsPrime(100));

        log.EndSection();
    }
}

internal class RamanujanCalculator
{
    private int _limit;
    private int[] _pi;
    private bool[] _isPrime;
    private List<int> _primes;
    
    public int FoundX { get; private set; } = -1;
    public int Limit => _limit;
    public bool IsSieved => _pi != null;

    public int GetPi(int x)
    {
        if (_pi == null) return 0;
        if (x < 0) return 0;
        if (x >= _pi.Length) return _pi[_pi.Length - 1];
        return _pi[x];
    }
    
    public bool IsPrime(int x)
    {
        if (_isPrime == null || x < 0 || x >= _isPrime.Length) return false;
        return _isPrime[x];
    }

    public int GetKthPrime(int k)
    {
        if (_primes == null || k < 1 || k > _primes.Count) return -1;
        return _primes[k - 1];
    }

    public int FindRn(int n)
    {
        this._limit = CalculateLimit(n);
        Sieve(this._limit);
        var x = SearchForRn(n);
        this.FoundX = x;
        return (x != -1) ? x + 1 : -1;
    }
    
    private int CalculateLimit(int n)
    {
        var limit = (int)(4.1 * n * Math.Log(4 * n)) + 50;
        return Math.Max(limit, 50);
    }

    private int SearchForRn(int n)
    {
        var targetCount = n - 1;
        for (var x = this._pi.Length - 1; x >= 1; x--)
        {
            var count = _pi[x] - _pi[x / 2];
            if (count == targetCount)
            {
                return x;
            }
        }
        return -1;
    }
    
    private void Sieve(int limit)
    {
        _isPrime = new bool[limit + 1];
        _pi = new int[limit + 1];
        _primes = new List<int>();

        if (limit < 2) return;
        
        for (var i = 2; i <= limit; i++)
        {
            _isPrime[i] = true;
        }
        
        for (var p = 2; (long)p * p <= limit; p++)
        {
            if (_isPrime[p])
            {
                for (long i = (long)p * p; i <= limit; i += p)
                {
                    _isPrime[(int)i] = false;
                }
            }
        }
        
        for (var i = 2; i <= limit; i++)
        {
            _pi[i] = _pi[i - 1];
            if (_isPrime[i])
            {
                _pi[i]++;
                _primes.Add(i);
            }
        }
    }
}