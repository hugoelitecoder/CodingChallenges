using System;
using System.Collections.Generic;

class Solution
{
    class Pile { public int Value, Rate; }

    static void Main()
    {
        int k = int.Parse(Console.ReadLine());
        int n = int.Parse(Console.ReadLine());

        var avail = new PriorityQueue<Pile, (int negValue, int negRate)>();
        for (int i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split();
            int v = int.Parse(parts[0]);
            int r = int.Parse(parts[1]);
            avail.Enqueue(new Pile{Value = v, Rate = r}, (-v, -r));
        }

        var cooling = new PriorityQueue<Pile, int>();
        long resources = 0;

        for (int turn = 0; turn < k; turn++)
        {
            Pile sel;
            if (avail.Count > 0)
                sel = avail.Dequeue();
            else
                sel = new Pile { Value = 0, Rate = 0 };

            resources += sel.Value;

            while (cooling.Count > 0
                   && cooling.TryPeek(out var pHeld, out var ready) && ready == turn)
            {
                cooling.Dequeue();
                avail.Enqueue(pHeld, (-pHeld.Value, -pHeld.Rate));
            }

            int nv = sel.Value * sel.Rate / 100;
            if (nv > 0)
                cooling.Enqueue(new Pile { Value = nv, Rate = sel.Rate }, turn + 3);
        }

        Console.WriteLine(resources);
    }
}
