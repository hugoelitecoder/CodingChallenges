using System;

class Solution
{
    static void Main()
    {
        int height = int.Parse(Console.ReadLine());
        int width = int.Parse(Console.ReadLine());
        int shelves = int.Parse(Console.ReadLine());

        // Decorative Top
        if (width % 2 == 1)
        {
            int side = width / 2;
            Console.WriteLine(new string('/', side) + "^" + new string('\\', side));
        }
        else
        {
            Console.WriteLine(new string('/', width / 2) + new string('\\', width / 2));
        }

        int availableLines = height - 1;
        int spaceLines = availableLines - shelves;
        int[] gaps = new int[shelves];
        for (int i = 0; i < shelves; i++) gaps[i] = spaceLines / shelves;
        for (int i = shelves - spaceLines % shelves; i < shelves; i++) gaps[i]++;

        // Build body
        for (int i = 0; i < shelves; i++)
        {
            for (int j = 0; j < gaps[i]; j++)
                Console.WriteLine("|" + new string(' ', width - 2) + "|");
            Console.WriteLine("|" + new string('_', width - 2) + "|");
        }
    }
}
