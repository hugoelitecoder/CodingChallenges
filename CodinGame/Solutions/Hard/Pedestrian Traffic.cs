using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static int n;
    static char[][] path;
    static char[][] prevPath;
    static int[] dir = new int[128];
    static List<Person> moved, notChecked, current;

    static void Main()
    {
        n = int.Parse(Console.ReadLine());
        path = new char[2][];
        path[0] = Console.ReadLine().ToCharArray();
        path[1] = Console.ReadLine().ToCharArray();
        dir['R'] = 1; dir['L'] = -1;
        int seconds = 0;

        while (true)
        {
            moved = new List<Person>();
            notChecked = new List<Person>();
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < n; j++)
                    if (path[i][j] == 'R' || path[i][j] == 'L')
                        notChecked.Add(new Person(i, j, path[i][j]));

            prevPath = new char[2][];
            prevPath[0] = (char[])path[0].Clone();
            prevPath[1] = (char[])path[1].Clone();
            path = new char[2][];
            path[0] = new string('o', n).ToCharArray();
            path[1] = new string('o', n).ToCharArray();

            while (notChecked.Count > 0)
            {
                current = new List<Person>();
                Move(notChecked[0]);
            }

            if (SameState(path, prevPath)) break;
            
            seconds++;
        }
        Console.WriteLine(AllExited() ? seconds.ToString() : "Congestion");
    }

    struct Person : IEquatable<Person>
    {
        public int Lane, Pos;
        public char Kind;
        public Person(int lane, int pos, char kind) { Lane = lane; Pos = pos; Kind = kind; }
        public bool Equals(Person other) => Lane == other.Lane && Pos == other.Pos && Kind == other.Kind;
        public override int GetHashCode() => Lane * 10000 + Pos * 10 + Kind;
    }

    static bool Move(Person p)
    {
        if (notChecked.Contains(p))
        {
            notChecked.Remove(p);
            current.Add(p);
            if (SwitchLane(p) || MoveForward(p))
            {
                moved.Add(p);
                return true;
            }
            path[p.Lane][p.Pos] = prevPath[p.Lane][p.Pos];
            return false;
        }
        else
        {
            return moved.Contains(p);
        }
    }

    static bool MoveForward(Person p)
    {
        int next = p.Pos + dir[p.Kind];
        if (next < 0 || next >= n)
            return true;
        char other = prevPath[p.Lane][next];
        var dep = new Person(p.Lane, next, other);
        if (current.Contains(dep))
        {
            if (dep.Equals(current[0]) && current.Count > 2)
            {
                path[p.Lane][next] = p.Kind;
                return true;
            }
            return false;
        }
        if (other == 'o' || Move(dep))
        {
            if (!((p.Kind == 'R' && p.Lane == 0) || (p.Kind == 'L' && p.Lane == 1)))
            {
                int otherSide = p.Pos + 2 * dir[p.Kind];
                if (otherSide >= 0 && otherSide < n)
                {
                    char op2 = prevPath[p.Lane][otherSide];
                    if (op2 != 'o' && dir[p.Kind] == -dir[op2])
                        return false;
                }
            }
            path[p.Lane][next] = p.Kind;
            return true;
        }
        return false;
    }

    static bool SwitchLane(Person p)
    {
        if ((p.Kind == 'R' && p.Lane == 0) || (p.Kind == 'L' && p.Lane == 1))
            return false;
        int otherLane = 1 - p.Lane;
        char other = prevPath[otherLane][p.Pos];
        var dep = new Person(otherLane, p.Pos, other);
        if (other != 'o' && current.Contains(dep))
        {
            if (dep.Equals(current[0]))
            {
                int idx = p.Pos - dir[p.Kind];
                if (idx >= 0 && idx < n && prevPath[otherLane][idx] == p.Kind)
                    return false;
                path[otherLane][p.Pos] = p.Kind;
                return true;
            }
            return false;
        }
        int next1 = p.Pos - dir[p.Kind];
        if (next1 >= 0 && next1 < n && prevPath[otherLane][next1] == p.Kind)
            return false;
        if (other == 'o' || Move(dep))
        {
            int next2 = p.Pos + dir[p.Kind];
            if (next2 >= 0 && next2 < n)
            {
                char contender = prevPath[otherLane][next2];
                if (contender == 'R' || contender == 'L')
                {
                    Move(new Person(otherLane, next2, contender));
                    if (path[otherLane][p.Pos] != 'o')
                        return false;
                }
            }
            path[otherLane][p.Pos] = p.Kind;
            return true;
        }
        return false;
    }

    static bool AllExited()
    {
        return !path[0].Any(x => x == 'R' || x == 'L') && !path[1].Any(x => x == 'R' || x == 'L');
    }

    static bool SameState(char[][] a, char[][] b)
    {
        return Enumerable.SequenceEqual(a[0], b[0]) && Enumerable.SequenceEqual(a[1], b[1]);
    }
}
