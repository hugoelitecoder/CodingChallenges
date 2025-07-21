using System;
class Solution
{
    public static void Main(string[] args)
    {
        var input = Console.ReadLine();
        // Remove formatting separators
        var s = input.Replace(",", "").Replace(".", "");
        // Split into 9-digit integer part and 6-digit fractional part
        var intPart = s.Substring(0, 9);
        var fracPart = s.Substring(9, 6);
        // Replace 'x' with '0' to build numeric values
        var intDigits = intPart.Replace('x', '0');
        var fracDigits = fracPart.Replace('x', '0');
        var intVal = long.Parse(intDigits);
        var fracVal = int.Parse(fracDigits);
        // Combine into one integer representing micro-units
        var numerator = intVal * 1_000_000 + fracVal;
        // Divide by two
        var newNum = numerator / 2;
        var newIntVal = newNum / 1_000_000;
        var newFracVal = (int)(newNum % 1_000_000);
        // Format back to fixed-width strings
        var newIntStr = newIntVal.ToString("D9");
        var newFracStr = newFracVal.ToString("D6");
        // Determine first non-zero digit in integer part (or 9 if all zero)
        int firstNonZero = 9;
        for (var i = 0; i < 9; i++)
        {
            if (newIntStr[i] != '0')
            {
                firstNonZero = i;
                break;
            }
        }
        // Determine last non-zero digit in fractional part (or -1 if all zero)
        int lastNonZero = -1;
        for (var i = 0; i < 6; i++)
        {
            if (newFracStr[i] != '0')
                lastNonZero = i;
        }
        // Build output array of length 19
        var outArr = new char[19];
        // First three integer digits and comma
        for (var i = 0; i < 3; i++)
            outArr[i] = i < firstNonZero ? 'x' : newIntStr[i];
        outArr[3] = ',';
        // Next three integer digits and comma
        for (var i = 0; i < 3; i++)
            outArr[4 + i] = (i + 3) < firstNonZero ? 'x' : newIntStr[3 + i];
        outArr[7] = ',';
        // Last three integer digits and decimal point
        for (var i = 0; i < 3; i++)
            outArr[8 + i] = (i + 6) < firstNonZero ? 'x' : newIntStr[6 + i];
        outArr[11] = '.';
        // First three fractional digits and dot
        for (var i = 0; i < 3; i++)
            outArr[12 + i] = i > lastNonZero ? 'x' : newFracStr[i];
        outArr[15] = '.';
        // Last three fractional digits
        for (var i = 0; i < 3; i++)
            outArr[16 + i] = (i + 3) > lastNonZero ? 'x' : newFracStr[3 + i];
        Console.WriteLine(new string(outArr));
    }
}
