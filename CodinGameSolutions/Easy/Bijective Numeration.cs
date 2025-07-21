using System;
using System.Linq;

class Solution
{
    static int DecimaryToDecimal(string s)
    {
        int result = 0;
        foreach (char c in s)
        {
            int val = c == 'A' ? 10 : c - '0';
            result = result * 10 + val;
        }
        return result;
    }

    static string DecimalToDecimary(int n)
    {
        string result = "";
        while (n > 0)
        {
            n--;
            result = (n % 10 == 9 ? 'A' : (char)('1' + (n % 10))) + result;
            n /= 10;
        }
        return result;
    }

    static void Main()
    {
        int count = int.Parse(Console.ReadLine());
        var inputs = Console.ReadLine().Split();

        int sum = inputs.Select(DecimaryToDecimal).Sum();
        Console.WriteLine(DecimalToDecimary(sum));
    }
}
