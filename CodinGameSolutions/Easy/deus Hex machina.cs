using System;
using System.Linq;

class Solution
{
    private static readonly char[] HMap = {
        '0','1','5','x','x','2','a','x','8','e','6','d','x','b','9','x'
    };
    private static readonly char[] VMap = {
        '0','1','5','3','x','2','e','x','8','a','9','x','c','x','6','x'
    };
    private static readonly int[] BitCount = {
        0,1,1,2,1,2,2,3,1,2,2,3,2,3,3,4
    };

    static void Main()
    {
        var hex = Console.ReadLine().Trim().ToLower();
        int n = hex.Length;

        int firstVal = Convert.ToInt32(hex[0].ToString(), 16);
        int bitsInFirst = firstVal == 0 ? 1 : (int)Math.Floor(Math.Log(firstVal, 2)) + 1;
        long totalBits = bitsInFirst + (n - 1) * 4L;
        long ones = BitCount[firstVal];
        for (int i = 1; i < n; i++)
            ones += BitCount[Convert.ToInt32(hex[i].ToString(), 16)];
        long zeros = totalBits - ones;

        bool applyH = (zeros & 1) != 0;
        bool applyV = (ones  & 1) != 0;
        if ((applyH && hex.Any(c => HMap[HexVal(c)] == 'x'))
         || (applyV && hex.Any(c => VMap[HexVal(c)] == 'x')))
        {
            Console.WriteLine("Not a number");
            return;
        }
        string result = hex;
        if (applyH)
            result = new string(result.Reverse()
                                  .Select(c => HMap[HexVal(c)])
                                  .ToArray());
        if (applyV)
            result = new string(result
                                  .Select(c => VMap[HexVal(c)])
                                  .ToArray());

        if (result.Length > 1000)
            result = result.Substring(0, 1000);

        Console.WriteLine(result);
    }

    private static int HexVal(char c)
    {
        return c <= '9' ? c - '0' : c - 'a' + 10;
    }
}
