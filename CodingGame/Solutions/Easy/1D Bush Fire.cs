using System;

class Solution
{
    public static void Main()
    {
        var N = int.Parse(Console.ReadLine().Trim());
        for (var t = 0; t < N; t++)
        {
            var s = Console.ReadLine();
            var drops = 0;
            for (var i = 0; i < s.Length; i++)
            {
                if (s[i] == 'f')
                {
                    drops++;
                    i += 2;
                }
            }
            Console.WriteLine(drops);
        }
    }
}
