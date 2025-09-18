using System;
using System.Text;

class Solution
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        var packet = Console.ReadLine().Trim();

        var response = new StringBuilder();
        int idx = 0;
        while (idx + 7 <= packet.Length)
        {
            var instrId = packet.Substring(idx, 3);
            idx += 3;

            var lenBits = packet.Substring(idx, 4);
            idx += 4;

            int length = Convert.ToInt32(lenBits, 2);

            if (idx + length > packet.Length) break;

            var info = packet.Substring(idx, length);
            idx += length;

            if (instrId == "101")
            {
                response.Append("001");
                response.Append(lenBits);
                response.Append(info);
            }
        }

        Console.WriteLine(response.ToString());
    }
}
