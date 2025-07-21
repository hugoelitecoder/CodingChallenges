using System;
using System.Collections.Generic;
using System.Linq;

public class Solution
{
    public static void Main(string[] args)
    {
        var N = int.Parse(Console.ReadLine());
        var addends = new List<string>();
        for (var i = 0; i < N; i++)
        {
            var word = Console.ReadLine();
            addends.Add(word);
        }
        var total = Console.ReadLine();
        var solver = new CryptarithmeticSolver(addends, total);
        if (solver.Solve())
        {
            var sortedResult = solver.ResultMap.OrderBy(kvp => kvp.Key);
            foreach (var entry in sortedResult)
            {
                Console.WriteLine(entry.Key + " " + entry.Value);
            }
        }
    }
}

class CryptarithmeticSolver
{
    private readonly List<string> _addendWords;
    private readonly string _totalWord;
    private readonly char[] _uniqueChars;
    private readonly HashSet<char> _firstChars;
    private readonly int[] _assignedDigits;
    private readonly bool[] _digitIsUsed;
    public Dictionary<char, int> ResultMap { get; private set; }

    public CryptarithmeticSolver(List<string> addends, string total)
    {
        _addendWords = addends;
        _totalWord = total;
        var uniqueCharsSet = new HashSet<char>();
        foreach (var word in _addendWords)
        {
            foreach (var c in word)
            {
                uniqueCharsSet.Add(c);
            }
        }
        foreach (var c in _totalWord)
        {
            uniqueCharsSet.Add(c);
        }
        _uniqueChars = uniqueCharsSet.ToArray();
        Array.Sort(_uniqueChars);
        _firstChars = new HashSet<char>();
        foreach (var word in _addendWords)
        {
            if (word.Length > 0)
            {
                _firstChars.Add(word[0]);
            }
        }
        if (_totalWord.Length > 0)
        {
            _firstChars.Add(_totalWord[0]);
        }
        _assignedDigits = new int[_uniqueChars.Length];
        _digitIsUsed = new bool[10];
        ResultMap = null;
    }

    public bool Solve()
    {
        return SolveRecursive(0);
    }

    private bool SolveRecursive(int charIdx)
    {
        if (charIdx == _uniqueChars.Length)
        {
            return CheckCurrentAssignment();
        }
        var curChar = _uniqueChars[charIdx];
        for (var digit = 0; digit <= 9; ++digit)
        {
            if (_digitIsUsed[digit])
            {
                continue;
            }
            if (digit == 0 && _firstChars.Contains(curChar))
            {
                continue;
            }
            _assignedDigits[charIdx] = digit;
            _digitIsUsed[digit] = true;
            if (SolveRecursive(charIdx + 1))
            {
                return true;
            }
            _digitIsUsed[digit] = false;
        }
        return false;
    }

    private bool CheckCurrentAssignment()
    {
        var currentMap = new Dictionary<char, int>();
        for (var i = 0; i < _uniqueChars.Length; ++i)
        {
            currentMap[_uniqueChars[i]] = _assignedDigits[i];
        }
        var maxAddendLen = 0;
        if (_addendWords.Any()) 
        {
             maxAddendLen = _addendWords.Max(w => w.Length);
        }
       
        if (_totalWord.Length > maxAddendLen + 1 && maxAddendLen > 0) return false;
        if (_totalWord.Length < maxAddendLen) return false;
        
        long carry = 0;
        for (var i = 0; i < _totalWord.Length; ++i)
        {
            long sumForCol = carry;
            foreach (var word in _addendWords)
            {
                if (i < word.Length)
                {
                    sumForCol += currentMap[word[word.Length - 1 - i]];
                }
            }
            var totalDigitForCol = currentMap[_totalWord[_totalWord.Length - 1 - i]];
            if (sumForCol % 10 != totalDigitForCol)
            {
                return false;
            }
            carry = sumForCol / 10;
        }
        if (carry != 0) return false;
        ResultMap = currentMap;
        return true;
    }
}