using System;
using System.Collections.Generic;
using System.Text;

class Solution
{
    static void Main()
    {
        string movesInput = Console.ReadLine();
        var cube = new RubiksCube();
        cube.Apply(movesInput);
        Console.WriteLine(cube.Front());
    }
}

class RubiksCube
{
    private Dictionary<char,char[]> faces;

    public RubiksCube()
    {
        faces = new Dictionary<char,char[]>();
        foreach (char face in new char[]{'U','R','F','D','L','B'})
            faces[face] = new char[]{face,face,face,face};
    }

    private static string Normalize(string moves)
    {
        var builder = new StringBuilder(moves);
        int idx = 0;
        while (idx < builder.Length)
        {
            char ch = builder[idx];
            if (ch == '\'')
            {
                char prev = builder[idx - 1];
                builder.Remove(idx, 1);
                builder.Insert(idx, new string(prev, 2));
                idx += 2;
            }
            else if (ch == '2')
            {
                char prev = builder[idx - 1];
                builder.Remove(idx, 1);
                builder.Insert(idx, prev.ToString());
                idx++;
            }
            else if ("RLUDFB".IndexOf(ch) < 0)
                builder.Remove(idx, 1);
            else
                idx++;
        }
        return builder.ToString();
    }

    public void Apply(string moves)
    {
        string seq = Normalize(moves);
        foreach (char move in seq)
        {
            Rotate(move);
            char[] front, up, back, down, left, right;
            switch (move)
            {
                case 'R':
                    front = faces['F']; up = faces['U']; back = faces['B']; down = faces['D'];
                    (down[1], back[3], up[1], front[1]) = (back[3], up[1], front[1], down[1]);
                    (down[2], back[0], up[2], front[2]) = (back[0], up[2], front[2], down[2]);
                    break;
                case 'F':
                    left = faces['L']; up = faces['U']; right = faces['R']; down = faces['D'];
                    (down[0], right[3], up[2], left[1]) = (right[3], up[2], left[1], down[0]);
                    (down[1], right[0], up[3], left[2]) = (right[0], up[3], left[2], down[1]);
                    break;
                case 'U':
                    left = faces['L']; back = faces['B']; right = faces['R']; front = faces['F'];
                    (front[1], right[1], back[1], left[1]) = (right[1], back[1], left[1], front[1]);
                    (front[0], right[0], back[0], left[0]) = (right[0], back[0], left[0], front[0]);
                    break;
                case 'L':
                    front = faces['F']; down = faces['D']; back = faces['B']; up = faces['U'];
                    (up[0], back[2], down[0], front[0]) = (back[2], down[0], front[0], up[0]);
                    (up[3], back[1], down[3], front[3]) = (back[1], down[3], front[3], up[3]);
                    break;
                case 'B':
                    left = faces['L']; down = faces['D']; right = faces['R']; up = faces['U'];
                    (up[1], right[2], down[3], left[0]) = (right[2], down[3], left[0], up[1]);
                    (up[0], right[1], down[2], left[3]) = (right[1], down[2], left[3], up[0]);
                    break;
                case 'D':
                    left = faces['L']; front = faces['F']; right = faces['R']; back = faces['B'];
                    (back[3], right[3], front[3], left[3]) = (right[3], front[3], left[3], back[3]);
                    (back[2], right[2], front[2], left[2]) = (right[2], front[2], left[2], back[2]);
                    break;
            }
        }
    }

    private void Rotate(char face)
    {
        var s = faces[face];
        faces[face] = new char[]{ s[3], s[0], s[1], s[2] };
    }

    public string Front()
    {
        var f = faces['F'];
        return $"{f[0]}{f[1]}\n{f[3]}{f[2]}";
    }
}