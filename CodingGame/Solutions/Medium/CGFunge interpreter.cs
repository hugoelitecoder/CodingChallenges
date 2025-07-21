using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var lines = new List<string>();
        int maxC = 0;
        for (int i = 0; i < N; i++)
        {
            string l = Console.ReadLine();
            lines.Add(l);
            if (l.Length > maxC) maxC = l.Length;
        }
        char[,] grid = new char[N, maxC];
        for (int r = 0; r < N; r++)
        {
            var l = lines[r];
            for (int c = 0; c < maxC; c++)
                grid[r, c] = c < l.Length ? l[c] : ' ';
        }

        var stack = new Stack<int>();
        int rpos = 0, cpos = 0;
        int dr = 0, dc = 1;
        bool strMode = false;
        string output = "";

        while (true)
        {
            if (rpos < 0 || rpos >= N || cpos < 0 || cpos >= maxC)
                break;

            char cmd = grid[rpos, cpos];
            if (!strMode && cmd == 'E')
                break;

            if (strMode)
            {
                if (cmd == '"')
                    strMode = false;
                else
                    stack.Push((int)cmd);
            }
            else
            {
                switch (cmd)
                {
                    case ' ':
                        break;
                    case '>': dr = 0; dc = 1; break;
                    case '<': dr = 0; dc = -1; break;
                    case '^': dr = -1; dc = 0; break;
                    case 'v': dr = 1; dc = 0; break;
                    case 'S':
                        rpos += dr;
                        cpos += dc;
                        break;
                    case '"':
                        strMode = true;
                        break;
                    case '+':
                        {
                            int a = stack.Pop(), b = stack.Pop();
                            stack.Push((b + a) & 0xFF);
                        }
                        break;
                    case '-':
                        {
                            int a = stack.Pop(), b = stack.Pop();
                            stack.Push((b - a) & 0xFF);
                        }
                        break;
                    case '*':
                        {
                            int a = stack.Pop(), b = stack.Pop();
                            stack.Push((b * a) & 0xFF);
                        }
                        break;
                    case 'P':
                        stack.Pop();
                        break;
                    case 'X':
                        {
                            int a = stack.Pop(), b = stack.Pop();
                            stack.Push(a);
                            stack.Push(b);
                        }
                        break;
                    case 'D':
                        {
                            int a = stack.Peek();
                            stack.Push(a);
                        }
                        break;
                    case '_':
                        {
                            int v = stack.Pop();
                            if (v == 0) { dr = 0; dc = 1; }
                            else       { dr = 0; dc = -1; }
                        }
                        break;
                    case '|':
                        {
                            int v = stack.Pop();
                            if (v == 0) { dr = 1; dc = 0; }
                            else       { dr = -1; dc = 0; }
                        }
                        break;
                    case 'I':
                        {
                            int v = stack.Pop();
                            output += v.ToString();
                        }
                        break;
                    case 'C':
                        {
                            int v = stack.Pop();
                            output += (char)v;
                        }
                        break;
                    default:
                        if (cmd >= '0' && cmd <= '9')
                        {
                            stack.Push(cmd - '0');
                        }
                        break;
                }
            }
            rpos += dr;
            cpos += dc;
        }

        Console.Write(output);
    }
}
