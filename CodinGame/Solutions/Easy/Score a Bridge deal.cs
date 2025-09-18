using System;
using System.Globalization;

class Solution
{
    static void Main()
    {
        int nbTests = int.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
        for (int ti = 0; ti < nbTests; ti++)
        {
            var parts = Console.ReadLine().Split(' ');
            bool vulnerable = parts[0] == "V";
            string contract = parts[1];
            if (contract == "Pass")
            {
                Console.WriteLine(0);
                continue;
            }
            int tricksWon = int.Parse(parts[2], CultureInfo.InvariantCulture);

            int level = contract[0] - '0';
            string suit;
            int idx = 1;
            if (contract[idx] == 'N')
            {
                suit = "NT";
                idx += 2;
            }
            else
            {
                suit = contract[idx].ToString();
                idx += 1;
            }
            bool doubled = false, redoubled = false;
            if (contract.Length >= idx + 2 && contract.Substring(idx, 2) == "XX")
                redoubled = true;
            else if (contract.Length >= idx + 1 && contract[idx] == 'X')
                doubled = true;

            Console.WriteLine(
                ComputeScore(vulnerable, level, suit, doubled, redoubled, tricksWon)
            );
        }
    }

    static long ComputeScore(bool vulnerable,
                             int level,
                             string suit,
                             bool doubled,
                             bool redoubled,
                             int tricks)
    {
        int contractTricks = level + 6;
        if (tricks < contractTricks)
        {
            int under = contractTricks - tricks;
            if (!doubled && !redoubled)
            {
                return under * -50L * (vulnerable ? 2L : 1L);
            }
            long penalty = -300L * under;
            if (vulnerable)
            {
                long basePen = penalty + 100;
                return basePen * (redoubled ? 2L : 1L);
            }
            penalty += 200; 
            penalty += 100 * Math.Min(under - 1, 2);
            return penalty * (redoubled ? 2L : 1L);
        }

        int trickValue = (suit == "C" || suit == "D") ? 20 : 30;
        long contracted = level * trickValue;
        if (suit == "NT")
            contracted += 10;
        long factor = redoubled ? 4L : (doubled ? 2L : 1L);
        long score = contracted * factor;
        if (score >= 100)
            score += vulnerable ? 500 : 300;
        else
            score += 50;
        if (level == 6)
            score += vulnerable ? 750 : 500;
        else if (level == 7)
            score += vulnerable ? 1500 : 1000;
        int over = tricks - contractTricks;
        if (over <= 0)
        {
            if (doubled)    score += 50;
            else if (redoubled) score += 100;
            return score;
        }

        if (!doubled && !redoubled)
        {
            score += (long)over * trickValue;
            return score;
        }

        if (redoubled) score += 100;
        else           score += 50;

        long overValue = vulnerable ? 200 : 100;
        if (redoubled) overValue *= 2;
        score += over * overValue;
        return score;
    }
}
