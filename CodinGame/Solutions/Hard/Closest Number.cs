using System;
using System.Text;
using System.Numerics;


class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var nStr = inputs[0];
        var mStr = inputs[1];
        var handler = new PermutationHandler();
        var result = handler.FindBestPermutation(nStr, mStr);
        Console.WriteLine(result);
    }
}

public class PermutationHandler
{
    public string FindBestPermutation(string nStr, string mStr)
    {
        var nOriginalBigInt = BigInteger.Parse(nStr);
        var mCountsInitial = new int[10];
        foreach (var c in mStr)
        {
            mCountsInitial[c - '0']++;
        }
        var mLen = mStr.Length;
        var nLen = nStr.Length;
        string bestPermutationRaw;

        if (mLen < nLen)
        {
            var sb = new StringBuilder(mLen);
            for (var d = 9; d >= 0; d--)
            {
                sb.Append(new string((char)('0' + d), mCountsInitial[d]));
            }
            bestPermutationRaw = sb.ToString();
        }
        else
        {
            var targetNStrPadded = nStr;
            if (mLen > nLen)
            {
                targetNStrPadded = new string('0', mLen - nLen) + nStr;
            }
            
            string lowerPermStr = GetPermutation(targetNStrPadded, mCountsInitial, true, mLen);
            string upperPermStr = GetPermutation(targetNStrPadded, mCountsInitial, false, mLen);

            BigInteger? lowerVal = null;
            if (lowerPermStr != null)
            {
                lowerVal = BigInteger.Parse(lowerPermStr);
            }

            BigInteger? upperVal = null;
            if (upperPermStr != null)
            {
                upperVal = BigInteger.Parse(upperPermStr);
            }

            if (lowerVal == null && upperVal == null)
            {
                var sb = new StringBuilder(mLen);
                var firstDigitFilled = false;
                for (var d = 1; d <= 9; d++)
                {
                     if (mCountsInitial[d] > 0) {
                        sb.Append((char)('0' + d));
                        mCountsInitial[d]--;
                        firstDigitFilled = true;
                        break;
                     }
                }
                if (!firstDigitFilled && mCountsInitial[0] > 0)
                {
                     sb.Append('0');
                     mCountsInitial[0]--;
                }
                FillRemainingGreedy(sb, mLen - sb.Length, mCountsInitial, false);
                bestPermutationRaw = sb.ToString();


            }
            else if (lowerVal == null)
            {
                bestPermutationRaw = upperPermStr;
            }
            else if (upperVal == null)
            {
                bestPermutationRaw = lowerPermStr;
            }
            else
            {
                var diffLower = BigInteger.Abs(nOriginalBigInt - lowerVal.Value);
                var diffUpper = BigInteger.Abs(nOriginalBigInt - upperVal.Value);

                if (diffLower < diffUpper)
                {
                    bestPermutationRaw = lowerPermStr;
                }
                else if (diffUpper < diffLower)
                {
                    bestPermutationRaw = upperPermStr;
                }
                else
                {
                    bestPermutationRaw = lowerVal.Value < upperVal.Value ? lowerPermStr : upperPermStr;
                }
            }
        }
        var finalVal = BigInteger.Parse(bestPermutationRaw);
        return finalVal.ToString();
    }

    private static string GetPermutation(string targetNStrPadded, int[] mInitialCounts, bool findLower, int mLen)
    {
        var currentCounts = (int[])mInitialCounts.Clone();
        var resChars = new char[mLen];
        var resLen = 0;
        var needsBacktrack = false;

        for (var i = 0; i < mLen; i++)
        {
            var targetDigit = targetNStrPadded[i] - '0';
            if (currentCounts[targetDigit] > 0)
            {
                resChars[resLen++] = targetNStrPadded[i];
                currentCounts[targetDigit]--;
            }
            else
            {
                var nextBest = FindImmediateNextBestDigit(currentCounts, targetDigit, findLower);
                if (nextBest != -1)
                {
                    resChars[resLen++] = (char)('0' + nextBest);
                    currentCounts[nextBest]--;
                    FillRemainingGreedy(resChars, resLen, mLen, currentCounts, findLower);
                    return new string(resChars, 0, mLen);
                }
                else
                {
                    needsBacktrack = true;
                    break; 
                }
            }
        }

        if (!needsBacktrack)
        {
            return new string(resChars, 0, mLen);
        }
        
        for (var i = resLen - 1; i >= 0; i--)
        {
            var currentDigitInRes = resChars[i] - '0';
            currentCounts[currentDigitInRes]++;
            var replacementDigit = FindImmediateNextBestDigit(currentCounts, currentDigitInRes, findLower);
            if (replacementDigit != -1)
            {
                resChars[i] = (char)('0' + replacementDigit);
                currentCounts[replacementDigit]--;
                FillRemainingGreedy(resChars, i + 1, mLen, currentCounts, findLower);
                return new string(resChars, 0, mLen);
            }
        }
        return null;
    }

    private static void FillRemainingGreedy(char[] permChars, int startIndex, int permLength, int[] counts, bool findLower)
    {
        for (var k = startIndex; k < permLength; k++)
        {
            var digitToPlace = -1;
            if (findLower)
            {
                for (var d = 9; d >= 0; d--)
                {
                    if (counts[d] > 0) { digitToPlace = d; break; }
                }
            }
            else
            {
                for (var d = 0; d <= 9; d++)
                {
                    if (counts[d] > 0) { digitToPlace = d; break; }
                }
            }
            permChars[k] = (char)('0' + digitToPlace);
            counts[digitToPlace]--;
        }
    }
    
    private static void FillRemainingGreedy(StringBuilder sb, int numDigitsToFill, int[] counts, bool findLower)
    {
        for (var k = 0; k < numDigitsToFill; k++)
        {
            var digitToPlace = -1;
            if (findLower)
            {
                for (var d = 9; d >= 0; d--)
                {
                    if (counts[d] > 0) { digitToPlace = d; break; }
                }
            }
            else
            {
                for (var d = 0; d <= 9; d++)
                {
                    if (counts[d] > 0) { digitToPlace = d; break; }
                }
            }
            sb.Append((char)('0' + digitToPlace));
            counts[digitToPlace]--;
        }
    }


    private static int FindImmediateNextBestDigit(int[] counts, int thresholdDigit, bool findLower)
    {
        if (findLower)
        {
            for (var d = thresholdDigit - 1; d >= 0; d--)
            {
                if (counts[d] > 0) return d;
            }
        }
        else
        {
            for (var d = thresholdDigit + 1; d <= 9; d++)
            {
                if (counts[d] > 0) return d;
            }
        }
        return -1;
    }
}
