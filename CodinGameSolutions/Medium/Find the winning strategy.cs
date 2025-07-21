using System;
using System.Linq;

class Player
{
    static void Main()
    {
        int rows    = int.Parse(Console.ReadLine());
        int columns = int.Parse(Console.ReadLine());
        while (true)
        {
            int[] xP = new int[rows], xB = new int[rows];
            for (int i = 0; i < rows; i++)
            {
                var parts = Console.ReadLine().Split();
                xP[i] = int.Parse(parts[0]);
                xB[i] = int.Parse(parts[1]);
            }

            int nimSum = 0;
            int[] heap = new int[rows];
            for (int i = 0; i < rows; i++)
            {
                heap[i] = xB[i] - xP[i] - 1;
                nimSum ^= heap[i];
            }

            int chosenRow = -1, newX = 0;
            if (nimSum != 0)
            {
                for (int i = 0; i < rows; i++)
                {
                    int h = heap[i];
                    int target = h ^ nimSum;
                    if (target < h)
                    {
                        int delta = h - target;
                        chosenRow = i;
                        newX = xP[i] + delta;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < rows; i++)
                {
                    if (heap[i] > 0)
                    {
                        chosenRow = i;
                        newX = xP[i] + 1;
                        break;
                    }
                }
            }

            Console.WriteLine($"{chosenRow} {newX}");
        }
    }
}
