using System;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        var n = int.Parse(Console.ReadLine().Trim());
        var people = new Dictionary<string, Person>(n);
        Person root = null;

        for (var i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            var name = parts[0];
            var parentName = parts[1];
            var birth = int.Parse(parts[2]);
            var death = parts[3];
            var religion = parts[4];
            var gender = parts[5];

            var p = new Person
            {
                Name = name,
                ParentName = parentName,
                BirthYear = birth,
                IsAlive = death == "-",
                IsCatholic = religion.Equals("Catholic", StringComparison.OrdinalIgnoreCase),
                Gender = gender
            };
            people[name] = p;
            if (parentName == "-") root = p;
        }

        foreach (var p in people.Values)
        {
            if (p.ParentName != "-")
                people[p.ParentName].Children.Add(p);
        }

        foreach (var p in people.Values)
            p.Children.Sort(CompareChildren);

        DFS(root);
    }

    private static void DFS(Person p)
    {
        if (p.IsAlive && !p.IsCatholic)
            Console.WriteLine(p.Name);
        foreach (var child in p.Children)
            DFS(child);
    }

    private static int CompareChildren(Person a, Person b)
    {
        if (a.Gender != b.Gender)
            return a.Gender == "M" ? -1 : 1;
        return a.BirthYear.CompareTo(b.BirthYear);
    }

    private class Person
    {
        public string Name;
        public string ParentName;
        public int BirthYear;
        public bool IsAlive;
        public bool IsCatholic;
        public string Gender;
        public List<Person> Children = new List<Person>();
    }
}
