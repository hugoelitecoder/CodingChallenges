using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int P = int.Parse(Console.ReadLine());
        var propNames = new List<string>();
        for (int i = 0; i < P; i++)
            propNames.Add(Console.ReadLine());

        int N = int.Parse(Console.ReadLine());
        var people = new List<Dictionary<string, string>>();

        for (int i = 0; i < N; i++)
        {
            var tokens = Console.ReadLine().Split();
            var person = new Dictionary<string, string>();
            for (int j = 0; j < P; j++)
                person[propNames[j]] = tokens[j + 1]; // skip name
            people.Add(person);
        }

        int F = int.Parse(Console.ReadLine());
        for (int i = 0; i < F; i++)
        {
            var formula = Console.ReadLine();
            var filters = formula.Split(new[] { " AND " }, StringSplitOptions.None)
                                 .Select(f => f.Split('='))
                                 .ToList();

            if (filters.Any(f => !propNames.Contains(f[0])))
            {
                Console.WriteLine(0);
                continue;
            }

            int count = people.Count(p => filters.All(f => p[f[0]] == f[1]));
            Console.WriteLine(count);
        }
    }
}
