using System;
using System.Collections.Generic;
using System.Text;


class Solution
{
    static void Main(string[] args)
    {
        var N = Console.ReadLine();
        var maximizer = new IntegerMaximizer();
        var maximizedInteger = maximizer.CreateLargestPossibleInteger(N);
        Console.WriteLine(maximizedInteger);
    }
}

public class IntegerMaximizer
{
    public string CreateLargestPossibleInteger(string nStr)
    {
        var oddDigitsList = new List<char>();
        var evenDigitsList = new List<char>();

        foreach (var digitChar in nStr)
        {
            var digitVal = digitChar - '0';
            if (digitVal % 2 == 1)
            {
                oddDigitsList.Add(digitChar);
            }
            else
            {
                evenDigitsList.Add(digitChar);
            }
        }

        var resultOutputBuilder = new StringBuilder(nStr.Length);
        var oddIdx = 0;
        var evenIdx = 0;
        var totalDigits = nStr.Length;

        for (var i = 0; i < totalDigits; i++)
        {
            var isOddAvailable = oddIdx < oddDigitsList.Count;
            var isEvenAvailable = evenIdx < evenDigitsList.Count;

            if (isOddAvailable && isEvenAvailable)
            {
                if (oddDigitsList[oddIdx] > evenDigitsList[evenIdx])
                {
                    resultOutputBuilder.Append(oddDigitsList[oddIdx]);
                    oddIdx++;
                }
                else
                {
                    resultOutputBuilder.Append(evenDigitsList[evenIdx]);
                    evenIdx++;
                }
            }
            else if (isOddAvailable)
            {
                resultOutputBuilder.Append(oddDigitsList[oddIdx]);
                oddIdx++;
            }
            else if (isEvenAvailable)
            {
                resultOutputBuilder.Append(evenDigitsList[evenIdx]);
                evenIdx++;
            }
        }
        return resultOutputBuilder.ToString();
    }
}
