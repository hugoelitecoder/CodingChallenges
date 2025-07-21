using System;

class Solution
{
    public static void Main()
    {
        int total = int.Parse(Console.ReadLine());
        int maxLen = (int)((Math.Sqrt(8L * total + 1) - 1) / 2);
        int startCount = 1, rowCount = 1;
        for (int currLen = maxLen; currLen >= 1; currLen--)
        {
            long remainder = (long)total - (long)currLen * (currLen - 1) / 2;
            if (remainder % currLen == 0)
            {
                startCount = (int)(remainder / currLen);
                rowCount = currLen;
                break;
            }
        }
        for (int i = 0; i < rowCount; i++)
            Console.WriteLine(new string('*', startCount + i));
    }
}
