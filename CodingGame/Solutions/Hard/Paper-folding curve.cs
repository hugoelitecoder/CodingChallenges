using System;
using System.Text;

class Solution
{
    static void Main(string[] args)
    {
        var order = int.Parse(Console.ReadLine());
        var inputs = Console.ReadLine().Split(' ');
        var S = long.Parse(inputs[0]);
        var E = long.Parse(inputs[1]);
        
        var resultBuilder = new StringBuilder((int)(E - S + 1));
        for (var i = S; i <= E; i++)
        {
            resultBuilder.Append(GetDragonCurveChar(order, i));
        }

        Console.WriteLine(resultBuilder.ToString());
    }

    private static char GetDragonCurveChar(int n, long k)
    {
        var isFlipped = false;
        var currentK = k;
        var powerOf2 = 1L << (n - 1);
        while (powerOf2 > 0)
        {
            var pivot = powerOf2 - 1;
            if (currentK == pivot)
            {
                break;
            }
            if (currentK > pivot)
            {
                currentK = (powerOf2 << 1) - 2 - currentK;
                isFlipped = !isFlipped;
            }
            powerOf2 >>= 1;
        }
        return isFlipped ? '0' : '1';
    }
}