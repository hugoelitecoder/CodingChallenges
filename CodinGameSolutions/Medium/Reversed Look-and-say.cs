using System;
using System.Text;

class Solution
{
    static void Main()  {
        Console.WriteLine(Reverse(Console.ReadLine()));
    }

    static string Reverse(string s)
    {
        while (s.Length % 2 == 0)
        {
            var decoded = Decode(s);
            if (decoded == s || LengthEncode(decoded) != s)
                break;
            s = decoded;
        }
        return s;
    }

    static string Decode(string s)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < s.Length; i += 2)
            sb.Append(s[i + 1], s[i] - '0');
        return sb.ToString();
    }

    static string LengthEncode(string s)
    {
        var sb = new StringBuilder();
        for (int i = 0, j; i < s.Length; i = j)
        {
            char c = s[i];
            j = i + 1;
            while (j < s.Length && s[j] == c) j++;
            sb.Append(j - i).Append(c);
        }
        return sb.ToString();
    }
}
