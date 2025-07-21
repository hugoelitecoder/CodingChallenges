using System;

class Solution
{
    static void Main()
    {
        var parts = Console.ReadLine().Split();
        int n1 = int.Parse(parts[0]);
        int n2 = int.Parse(parts[1]);
        var s1 = Console.ReadLine();
        var s2 = Console.ReadLine();
        int t = int.Parse(Console.ReadLine());
        int L = n1 + n2;
        var res = new char[L];

        for (int i = 0; i < n1; i++)
        {
            int pos = (n1 - i - 1) + Math.Min(Math.Max(t - i, 0), n2);
            res[pos] = s1[i];
        }
        for (int i = 0; i < n2; i++)
        {
            int pos = (n1 + i) - Math.Min(Math.Max(t - i, 0), n1);
            res[pos] = s2[i];
        }

        Console.WriteLine(new string(res));
    }
}
