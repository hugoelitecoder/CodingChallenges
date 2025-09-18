using System;
using System.Collections.Generic;
using System.Text;

class Solution
{
    public static void Main()
    {
        var offerList = new List<string>();
        var numOffers = int.Parse(Console.ReadLine());
        for (var i = 0; i < numOffers; i++)
            offerList.Add(Console.ReadLine());
        var gardenHeight = int.Parse(Console.ReadLine());
        var sbGarden = new StringBuilder();
        for (var i = 0; i < gardenHeight; i++)
            sbGarden.Append(Console.ReadLine());
        var stripped = StripWeeds(sbGarden.ToString());
        var freq = new Dictionary<string, int>();
        for (var i = 0; i < stripped.Length; i += 2)
        {
            var emoji = stripped.Substring(i, 2);
            if (freq.TryGetValue(emoji, out var cnt))
                freq[emoji] = cnt + 1;
            else
                freq[emoji] = 1;
        }
        var totalCost = 0;
        foreach (var offer in offerList)
        {
            var parts = offer.Split(' ');
            var perUnit = int.Parse(parts[0].Substring(1));
            var plants = parts[2];
            for (var i = 0; i < plants.Length; i += 2)
            {
                var emoji = plants.Substring(i, 2);
                if (freq.TryGetValue(emoji, out var count))
                    totalCost += count * perUnit;
            }
        }
        Console.WriteLine(string.Format("{0:C0}", totalCost));
    }
    private static string StripWeeds(string s)
    {
        var sb = new StringBuilder();
        foreach (var c in s)
            if (c < 32 || c >= 200)
                sb.Append(c);
        return sb.ToString();
    }
}
