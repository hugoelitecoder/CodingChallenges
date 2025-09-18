using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        int m = int.Parse(Console.ReadLine());

        for (int i = 0; i < m; i++)
        {
            string input = Console.ReadLine();
            var parts = input.Split('/');
            var ipParts = parts[0].Split('.').Select(int.Parse).ToArray();
            int suffix = int.Parse(parts[1]);

            uint ip = (uint)(ipParts[0] << 24 | ipParts[1] << 16 | ipParts[2] << 8 | ipParts[3]);
            uint mask = suffix == 0 ? 0 : ~((1u << (32 - suffix)) - 1);
            bool isValid = (ip & mask) == ip;
            if (isValid)
            {
                ulong count = 1UL << (32 - suffix);
                Console.WriteLine($"valid {count}");
            }
            else
            {
                int newSuffix = suffix + 1;
                while (newSuffix <= 32)
                {
                    uint newMask = newSuffix == 0 ? 0 : ~((1u << (32 - newSuffix)) - 1);
                    if ((ip & newMask) == ip)
                        break;
                    newSuffix++;
                }
                ulong newCount = 1UL << (32 - newSuffix);
                Console.WriteLine($"invalid {parts[0]}/{newSuffix} {newCount}");
            }
        }
    }
}
