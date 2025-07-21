using System;
using System.Linq;
using System.Text;

class Solution
{
    static void Main()
    {
        var c1 = Convert.FromHexString(Console.ReadLine());
        var c2 = Convert.FromHexString(Console.ReadLine());
        var c3 = Convert.FromHexString(Console.ReadLine());

        var pt = c1
            .Select((b, i) => (byte)(b ^ c2[i] ^ c3[i]))
            .ToArray();

        Console.WriteLine(Encoding.ASCII.GetString(pt));
    }
}
