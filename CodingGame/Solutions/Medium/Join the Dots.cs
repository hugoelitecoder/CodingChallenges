using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    const char DOT = 'o';
    const char H   = '-';
    const char V   = '|';
    const char U   = '/';
    const char D   = '\\';
    const char HV  = '+';
    const char UD  = 'X';
    const char ALL = '*';

    static char Combine(char c, char k)
    {
        if (c == DOT || k == DOT) return DOT;
        if (c == ALL || k == ALL) return ALL;
        if ((c == H && k == V) || (c == V && k == H)) return HV;
        if ((c == U && k == D) || (c == D && k == U)) return UD;
        bool cHV = (c == H || c == V || c == HV);
        bool kHV = (k == H || k == V || k == HV);
        bool cUD = (c == U || c == D || c == UD);
        bool kUD = (k == U || k == D || k == UD);
        if ((cUD && kHV) || (kUD && cHV)) return ALL;
        if (c == ' ') return k;
        if (k == ' ') return c;
        // otherwise prefer k
        return k;
    }

    static IEnumerable<(int r,int c)> Path((int r,int c) a, (int r,int c) b)
    {
        int dr = Math.Sign(b.r - a.r);
        int dc = Math.Sign(b.c - a.c);
        int r = a.r, c = a.c;
        while (true)
        {
            yield return (r, c);
            if (r == b.r && c == b.c) break;
            r += dr; c += dc;
        }
    }

    static void Main()
    {
        var dims = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int h = dims[0], w = dims[1];
        var board = new char[h, w];

        for (int i = 0; i < h; i++)
        {
            var line = Console.ReadLine();
            for (int j = 0; j < w; j++)
                board[i,j] = line[j];
        }

        var pos = new Dictionary<char, (int r,int c)>();
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                char ch = board[i,j];
                if (ch != '.')
                    pos[ch] = (i,j);
            }

        var labels = Enumerable
            .Range('1','9'-'1'+1).Select(x=> (char)x)
            .Concat(Enumerable.Range('A','Z'-'A'+1).Select(x=>(char)x))
            .ToList();

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                if (board[i,j]=='.') board[i,j]=' ';

        for (int k = 0; k + 1 < labels.Count; k++)
        {
            var a = labels[k];
            var b = labels[k+1];
            if (!pos.ContainsKey(a) || !pos.ContainsKey(b)) continue;
            var p1 = pos[a];
            var p2 = pos[b];

            char lineChar;
            if (p1.r == p2.r)                     lineChar = H;
            else if (p1.c == p2.c)                lineChar = V;
            else if ((p1.r - p2.r) == (p1.c - p2.c)) lineChar = D;
            else                                  lineChar = U;

            foreach (var (r,c) in Path(p1,p2))
            {
                board[r,c] = Combine(board[r,c], lineChar);
            }
            board[p1.r,p1.c] = DOT;
            board[p2.r,p2.c] = DOT;
        }

        for (int i = 0; i < h; i++)
        {
            var row = new string(Enumerable.Range(0,w)
                .Select(j=>board[i,j]).ToArray());
            Console.WriteLine(row.TrimEnd());
        }
    }
}
