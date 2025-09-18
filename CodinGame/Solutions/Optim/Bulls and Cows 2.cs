using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Numerics;
using System.Linq;

public class Solution
{
    public static class Debug
    {
        public static bool Enabled { get; set; } = false;

        public static void WriteLine(string message)
        {
            if (Enabled)
            {
                Console.Error.WriteLine($"[DEBUG] {message}");
            }
        }

        public static void WriteLine(FormattableString message)
        {
            if (Enabled)
            {
                Console.Error.WriteLine($"[DEBUG] {message}");
            }
        }
    }

    public static void Main(string[] args)
    {
        var sw = new Stopwatch();
        var numberLength = int.Parse(Console.ReadLine());
        
        Debug.WriteLine($"Starting game with numberLength = {numberLength}");
        sw.Start();
        var solver = new BullsAndCowsSolver(numberLength);
        sw.Stop();
        Debug.WriteLine($"Init in {sw.ElapsedMilliseconds}ms.");
        var currentGuess = "";
        while (true)
        {
            var inputs = Console.ReadLine().Split(' ');
            var bulls = int.Parse(inputs[0]);
            var cows = int.Parse(inputs[1]);
            Debug.WriteLine($"INPUT: Bulls={bulls}, Cows={cows}");
            
            sw.Restart();
            if (bulls != -1 || cows != -1)
            {
                solver.ProcessFeedback(currentGuess, bulls, cows);
            }
            
            currentGuess = solver.GetNextGuess();
            sw.Stop();
            
            var elapsedMs = sw.ElapsedMilliseconds;
            Debug.WriteLine($"Turn took {elapsedMs}ms. Guessing: {currentGuess}");
            
            if (elapsedMs > 50)
            {
                Debug.WriteLine("FAILED: Time limit of 50ms exceeded.");
            }
            
            Console.WriteLine(currentGuess);
        }
    }

    private class BullsAndCowsSolver
    {
        private readonly int _len;
        private readonly List<(int[] guess, int bulls, int cows)> _history;
        private readonly HashSet<string> _pastGuesses;
        private readonly bool[,] _possibleAt;
        private int _mustNotContainMask;
        private readonly Random _rng;

        private const int TIME_LIMIT_MS = 45;

        public BullsAndCowsSolver(int len)
        {
            _len = len;
            _history = new List<(int[], int, int)>();
            _pastGuesses = new HashSet<string>();
            _rng = new Random();
            _mustNotContainMask = 0;
            
            _possibleAt = new bool[len, 10];
            for (var i = 0; i < len; i++)
            for (var j = 0; j < 10; j++)
            {
                _possibleAt[i, j] = true;
            }
            _possibleAt[0, 0] = false;
        }

        public void ProcessFeedback(string guessStr, int bulls, int cows)
        {
            var guessDigits = StringToDigits(guessStr);
            _pastGuesses.Add(guessStr);
            _history.Add((guessDigits, bulls, cows));
            
            if (bulls == 0)
            {
                for(var i = 0; i < _len; i++)
                {
                   _possibleAt[i, guessDigits[i]] = false;
                }
            }
            
            if (bulls + cows == 0)
            {
                foreach(var digit in guessDigits)
                {
                    _mustNotContainMask |= (1 << digit);
                }
            }
            
            if (bulls + cows == _len)
            {
                var guessMask = 0;
                foreach(var digit in guessDigits) guessMask |= (1 << digit);
                _mustNotContainMask |= (~guessMask & 0x3FF);
            }

            for(var d = 0; d < 10; d++)
            {
                if ((_mustNotContainMask & (1 << d)) != 0)
                {
                    for(var i = 0; i < _len; i++)
                    {
                        _possibleAt[i, d] = false;
                    }
                }
            }
        }
        
        public string GetNextGuess()
        {
            var deadline = Stopwatch.StartNew();
            var p = new int[_len];
            var guess = FindConsistentGuess(0, new bool[10], p, deadline, checkHistory: true);

            if (guess == null)
            {
                Debug.WriteLine("Primary search failed. Trying without history.");
                guess = FindConsistentGuess(0, new bool[10], p, deadline, checkHistory: false);
            }

            if (guess == null) {
                Debug.WriteLine("All searches failed. Generating emergency guess.");
                return GenerateEmergencyGuess();
            }
            
            return DigitsToString(guess);
        }

        private int[] FindConsistentGuess(int k, bool[] used, int[] p, Stopwatch deadline, bool checkHistory)
        {
            if (deadline.ElapsedMilliseconds > TIME_LIMIT_MS) return null;

            if (k == _len)
            {
                var candidateStr = DigitsToString(p);
                if (_pastGuesses.Contains(candidateStr)) return null;

                if (checkHistory)
                {
                    foreach (var (prevGuess, prevBulls, prevCows) in _history)
                    {
                        if (Compare(p, prevGuess) != (prevBulls, prevCows)) return null;
                    }
                }
                return (int[])p.Clone();
            }
            
            var digitsToTry = Enumerable.Range(0, 10).ToArray();
            for (var i = digitsToTry.Length - 1; i > 0; i--) {
                var j = _rng.Next(i + 1);
                (digitsToTry[i], digitsToTry[j]) = (digitsToTry[j], digitsToTry[i]);
            }

            foreach (var digit in digitsToTry)
            {
                if (used[digit] || !_possibleAt[k, digit] || (_mustNotContainMask & (1 << digit)) != 0) continue;
                
                used[digit] = true;
                p[k] = digit;
                var result = FindConsistentGuess(k + 1, used, p, deadline, checkHistory);
                if (result != null) return result;
                used[digit] = false;
            }
            return null;
        }
        
        private string GenerateEmergencyGuess()
        {
            const int maxAttempts = 100;

            var possibleDigitsPerPosition = new List<int>[_len];
            for (int i = 0; i < _len; i++)
            {
                possibleDigitsPerPosition[i] = new List<int>();
                for (int d = 0; d < 10; d++)
                {
                    if (_possibleAt[i, d])
                    {
                        possibleDigitsPerPosition[i].Add(d);
                    }
                }
            }

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var p = new int[_len];
                var used = new bool[10];
                bool success = true;

                for (int k = 0; k < _len; k++)
                {
                    var candidates = possibleDigitsPerPosition[k];
                    for (int i = candidates.Count - 1; i > 0; i--) {
                        int j = _rng.Next(i + 1);
                        (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
                    }

                    int? chosenDigit = null;
                    foreach (var digit in candidates)
                    {
                        if (!used[digit])
                        {
                            chosenDigit = digit;
                            break;
                        }
                    }

                    if (chosenDigit.HasValue)
                    {
                        p[k] = chosenDigit.Value;
                        used[chosenDigit.Value] = true;
                    }
                    else
                    {
                        success = false;
                        break;
                    }
                }

                if (success)
                {
                    var guessStr = DigitsToString(p);
                    if (!_pastGuesses.Contains(guessStr))
                    {
                        Debug.WriteLine($"Emergency guess (smart): {guessStr}");
                        return guessStr;
                    }
                }
            }
            
            Debug.WriteLine("Emergency fallback: generating any new unique permutation.");
            while(true)
            {
                var digits = Enumerable.Range(0, 10).OrderBy(d => _rng.Next()).Take(_len).ToArray();

                if (_possibleAt[0, 0] == false && digits.Length > 0 && digits[0] == 0)
                {
                    int nonZeroIndex = -1;
                    for(int i = 1; i < digits.Length; i++)
                    {
                        if(digits[i] != 0) {
                            nonZeroIndex = i;
                            break;
                        }
                    }
                    
                    if (nonZeroIndex != -1)
                    {
                        (digits[0], digits[nonZeroIndex]) = (digits[nonZeroIndex], digits[0]);
                    } else {
                        continue;
                    }
                }

                var guessStr = DigitsToString(digits);
                if (!_pastGuesses.Contains(guessStr))
                {
                    Debug.WriteLine($"Emergency guess (fallback): {guessStr}");
                    return guessStr;
                }
            }
        }

        private (int bulls, int cows) Compare(int[] guess, int[] candidate)
        {
            var bulls = 0;
            var guessMask = 0;
            var candidateMask = 0;
            for (var i = 0; i < _len; i++)
            {
                if (guess[i] == candidate[i]) bulls++;
                guessMask |= (1 << guess[i]);
                candidateMask |= (1 << candidate[i]);
            }
            var commonDigits = BitOperations.PopCount((uint)(guessMask & candidateMask));
            return (bulls, commonDigits - bulls);
        }

        private int[] StringToDigits(string s)
        {
            var digits = new int[_len];
            for (var i = 0; i < _len; i++) digits[i] = s[i] - '0';
            return digits;
        }

        private string DigitsToString(int[] digits)
        {
            if(digits.Length == 0) {
                return "1023456789".Substring(0, _len);
            }
            var sb = new StringBuilder(_len);
            foreach (var d in digits) sb.Append(d);
            return sb.ToString();
        }
    }
}