using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string f = Console.ReadLine();
        int n = f.Length;
        int level = 0, levelMax = 0;
        var L = new int[n];

        for (int i = 0; i < n; i++)
        {
            if (f[i] == '(')
            {
                level++;
                if (level > levelMax) levelMax = level;
                L[i] = level;
            }
            else if (f[i] == ')')
            {
                L[i] = level;
                level--;
            }
        }

        var M = new int[n];
        for (int i = 0; i < n; i++)
        {
            if (f[i] == '(')
            {
                M[i] = -1;
                int add = -1;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (M[j] != 0)
                    {
                        if (f[j] == '(') add--;
                        if (f[j] == ')') add++;
                        if (M[j] < 0)
                            M[j] = add;
                    }
                }
            }
            else if (f[i] == ')')
            {
                int j = i - 1;
                while (j >= 0 && (M[j] >= 0)) j--;
                M[j] = -M[j];
                M[i] = M[j];
            }
        }

        Console.WriteLine(f);
        if (levelMax > 0)
        {
            var line = "";
            for (int i = 0; i < n; i++)
                line += L[i] != 0 ? '^' : ' ';
            Console.WriteLine(line);

            line = "";
            for (int i = 0; i < n; i++)
                line += L[i] != 0 ? '|' : ' ';
            Console.WriteLine(line);

            for (int lev = 1; lev <= levelMax; lev++)
            {
                line = "";
                bool inside = false;
                for (int i = 0; i < n; i++)
                {
                    if (M[i] != 0 && lev <= M[i])
                    {
                        if (lev < M[i])
                            line += '|';
                        else
                        {
                            line += L[i].ToString();
                            inside = (f[i] == '(');
                        }
                    }
                    else if (inside)
                    {
                        line += '-';
                    }
                    else
                    {
                        line += ' ';
                    }
                }
                Console.WriteLine(line);
            }
        }
    }
}
