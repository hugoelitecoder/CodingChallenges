using System;
using System.Linq;

class Solution
{
    static int n;
    static string[] people;
    static string[] constraints;
    const string AT_FLOOR     = " is at floor ";
    const string NOT_AT_FLOOR = " is NOT at floor ";
    const string NOBODY       = "There's nobody at floor ";
    const string EXACTLY_TWO  = "There are exactly two customers at floor ";
    const string ALONE        = " is alone at his/her floor";
    const string WITH_TWO     = " is with two other customers at his/her floor";
    const string JUST_ABOVE   = " is just above ";
    const string HIGHER       = " is higher than ";
    const string SAME         = " is at the same floor as ";
    const string NOT_SAME     = " is NOT at the same floor as ";

    static void Main()
    {
        n = int.Parse(Console.ReadLine());
        people = new string[n];
        for (int i = 0; i < n; i++)
            people[i] = Console.ReadLine();

        int m = int.Parse(Console.ReadLine());
        constraints = new string[m];
        for (int i = 0; i < m; i++)
            constraints[i] = Console.ReadLine();

        var floorOf = new int[n];
        if (DFS(0, floorOf))
        {
            for (int i = 0; i < n; i++)
                Console.WriteLine($"{people[i]} {floorOf[i]}");
        }
        else
        {
            Console.WriteLine("No solution found");
        }
    }

    static bool DFS(int idx, int[] floorOf)
    {
        if (idx == n)
        {
            foreach (var rule in constraints)
            {
                if (rule.Contains(AT_FLOOR))
                {
                    var tok = rule.Split(new[] { AT_FLOOR }, StringSplitOptions.None);
                    if (floorOf[Find(tok[0])] != int.Parse(tok[1])) return false;
                }
                else if (rule.Contains(NOT_AT_FLOOR))
                {
                    var tok = rule.Split(new[] { NOT_AT_FLOOR }, StringSplitOptions.None);
                    if (floorOf[Find(tok[0])] == int.Parse(tok[1])) return false;
                }
                else if (rule.StartsWith(NOBODY))
                {
                    int fl = int.Parse(rule.Substring(NOBODY.Length));
                    if (CountAt(fl, floorOf) != 0) return false;
                }
                else if (rule.StartsWith(EXACTLY_TWO))
                {
                    int fl = int.Parse(rule.Substring(EXACTLY_TWO.Length));
                    if (CountAt(fl, floorOf) != 2) return false;
                }
                else if (rule.Contains(ALONE))
                {
                    var name = rule.Substring(0, rule.IndexOf(ALONE));
                    if (CountAt(floorOf[Find(name)], floorOf) != 1) return false;
                }
                else if (rule.Contains(WITH_TWO))
                {
                    var name = rule.Substring(0, rule.IndexOf(WITH_TWO));
                    if (CountAt(floorOf[Find(name)], floorOf) != 3) return false;
                }
                else if (rule.Contains(JUST_ABOVE))
                {
                    var tok = rule.Split(new[] { JUST_ABOVE }, StringSplitOptions.None);
                    if (floorOf[Find(tok[0])] != floorOf[Find(tok[1])] + 1) return false;
                }
                else if (rule.Contains(HIGHER))
                {
                    var tok = rule.Split(new[] { HIGHER }, StringSplitOptions.None);
                    if (floorOf[Find(tok[0])] <= floorOf[Find(tok[1])]) return false;
                }
                else if (rule.Contains(SAME) && !rule.Contains(NOT_SAME))
                {
                    var tok = rule.Split(new[] { SAME }, StringSplitOptions.None);
                    if (floorOf[Find(tok[0])] != floorOf[Find(tok[1])]) return false;
                }
                else if (rule.Contains(NOT_SAME))
                {
                    var tok = rule.Split(new[] { NOT_SAME }, StringSplitOptions.None);
                    if (floorOf[Find(tok[0])] == floorOf[Find(tok[1])]) return false;
                }
            }
            return true;
        }

        for (int fl = 0; fl < 4; fl++)
        {
            floorOf[idx] = fl;
            if (DFS(idx + 1, floorOf)) return true;
        }
        return false;
    }

    static int Find(string name) => Array.IndexOf(people, name);
    static int CountAt(int floor, int[] floorOf) => floorOf.Count(x => x == floor);
}
