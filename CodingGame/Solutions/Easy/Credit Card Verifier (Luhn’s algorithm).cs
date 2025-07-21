using System;

class Solution
{
    static void Main()
    {
        var n = int.Parse(Console.ReadLine());
        for (int i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            var s = string.Concat(parts); 

            int sum = 0;
            for (int idx = s.Length - 1, pos = 1; idx >= 0; idx--, pos++)
            {
                int d = s[idx] - '0';
                if (pos % 2 == 0)
                {
                    d *= 2;
                    if (d > 9) d -= 9;
                }
                sum += d;
            }

            Console.WriteLine(sum % 10 == 0 ? "YES" : "NO");
        }
    }
}
