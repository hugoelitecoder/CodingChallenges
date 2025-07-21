using System;
using System.Linq;
using System.Text;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split();
        int w = int.Parse(inputs[0]);
        int h = int.Parse(inputs[1]);
        var bits = new StringBuilder();
        for (int i = 0; i < h; i++)
        {
            var row = Console.ReadLine().Split().Select(int.Parse);
            foreach (var pixel in row)
                bits.Append(pixel & 1); // LSB
        }
        var output = new StringBuilder();
        for (int i = 0; i < bits.Length; i += 8)
        {
            var byteString = bits.ToString(i, 8);
            int value = Convert.ToInt32(byteString, 2);
            output.Append((char)value);
        }

        Console.WriteLine(output.ToString());
    }
}
