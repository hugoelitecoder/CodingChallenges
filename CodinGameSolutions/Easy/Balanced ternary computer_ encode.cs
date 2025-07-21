using System;
using System.Text;
class Solution
{
    public static void Main(string[] args)
    {
        var value0 = int.Parse(Console.ReadLine());
        if (value0 == 0) { Console.WriteLine("0"); return; }
        var value = value0;
        var sb = new StringBuilder();
        while (value != 0)
        {
            var q = Math.DivRem(value, 3, out var r);
            if (r == 2) { r = -1; q++; }
            else if (r == -2) { r = 1; q--; }
            sb.Append(r == -1 ? 'T' : (r == 0 ? '0' : '1'));
            value = q;
        }
        var arr = sb.ToString().ToCharArray();
        Array.Reverse(arr);
        Console.WriteLine(new string(arr));
    }
}
