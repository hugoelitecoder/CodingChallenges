using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int c = int.Parse(Console.ReadLine());
        int p = int.Parse(Console.ReadLine());

        var inventory = new List<(string category, string size, int price)>();
        for (int i = 0; i < c; i++)
        {
            var parts = Console.ReadLine().Split();
            inventory.Add((parts[0], parts[1], int.Parse(parts[2])));
        }

        for (int i = 0; i < p; i++)
        {
            var order = Console.ReadLine().Split();
            string cat = order[0], size = order[1];
            var match = inventory
                .Where(item => item.category == cat && item.size == size)
                .OrderBy(item => item.price)
                .FirstOrDefault();

            if (match.price == 0 && match.category == null)
            {
                Console.WriteLine("NONE");
            }
            else
            {
                Console.WriteLine(match.price);
                inventory.Remove(match);
            }
        }
    }
}
