using System;
using System.Linq;
using System.Text;

class Solution
{
    public static void Main()
    {
        var ip = Console.ReadLine().Split(':');
        var n = ip.Length;
        var trimmed = new string[n];
        for (var i = 0; i < n; i++)
        {
            var b = ip[i].TrimStart('0');
            trimmed[i] = b == string.Empty ? "0" : b;
        }
        var bestStart = -1;
        var bestLen = 0;
        for (var i = 0; i < n; i++)
        {
            if (ip[i] != "0000") continue;
            var j = i;
            while (j < n && ip[j] == "0000") j++;
            var len = j - i;
            if (len > bestLen)
            {
                bestLen = len;
                bestStart = i;
            }
            i = j;
        }
        var sb = new StringBuilder();
        if (bestLen < 2) bestStart = -1;
        for (var i = 0; i < n; i++)
        {
            if (i == bestStart)
            {
                sb.Append("::");
                i += bestLen - 1;
            }
            else
            {
                if (sb.Length > 0 && sb[sb.Length - 1] != ':') sb.Append(':');
                sb.Append(trimmed[i]);
            }
        }
        var result = sb.ToString();
        Console.WriteLine(result);
    }
}