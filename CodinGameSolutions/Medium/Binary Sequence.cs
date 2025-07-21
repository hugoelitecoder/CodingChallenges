using System;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        for (int i = 0; i < n; i++)
        {
            long idx = Convert.ToInt64(Console.ReadLine(), 2);
            Console.WriteLine(GetDigit(idx));
        }
    }

    static char GetDigit(long index)
    {
        if (index == 0)
            return '0';
        index--;
        int length = 1;
        while (index >= length * (1L << (length - 1)))
        {
            index -= length * (1L << (length - 1));
            length++;
        }
        long number = index / length;
        index %= length;
        number += 1L << (length - 1);
        string bits = Convert.ToString(number, 2);
        return bits[(int)index];
    }
}
