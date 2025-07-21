using System;
using System.Collections.Generic;

class Solution
{
    private static readonly (char l,char r)[] _pairs = {
        ('0','0'),('1','1'),('2','2'),('5','5'),
        ('6','9'),('8','8'),('9','6')
    };
    private static readonly char[] _mids = { '0','1','2','5','8' };

    public static void Main()
    {
        var n = Console.ReadLine().Trim();
        var isStun = IsStunning(n);
        Console.WriteLine(isStun.ToString().ToLower());
        Console.WriteLine(NextStunning(n));
    }

    private static bool IsStunning(string s)
    {
        int i=0, j=s.Length-1;
        while(i<=j)
        {
            char c1 = s[i], c2 = s[j];
            if (!TryMap(c1, out char m) || m != c2) 
                return false;
            i++; j--;
        }
        return true;
    }

    private static bool TryMap(char c, out char mapped)
    {
        switch(c)
        {
            case '0': mapped='0'; return true;
            case '1': mapped='1'; return true;
            case '2': mapped='2'; return true;
            case '5': mapped='5'; return true;
            case '6': mapped='9'; return true;
            case '8': mapped='8'; return true;
            case '9': mapped='6'; return true;
            default: mapped = '?'; return false;
        }
    }

    private static string NextStunning(string n)
    {
        var L = n.Length;
        var builder = new char[L];
        var result = Backtrack(n, builder, 0, L-1, true);
        if (result != null) return result;
        var M = L+1;
        var ans = new char[M];
        ans[0] = '1'; ans[M-1] = '1';
        for(int k=1;k< M-1;k++) ans[k] = '0';
        return new string(ans);
    }

    private static string Backtrack(string n, char[] b, int posL, int posR, bool tight)
    {
        int L = n.Length;
        if (posL > posR)
        {
            return tight ? null : new string(b);
        }

        if (posL == posR)
        {
            foreach(var c in _mids)
            {
                if (tight)
                {
                    char nc = n[posL];
                    if (c < nc) continue;
                    bool nt = (c == nc);
                    b[posL] = c;
                    var r = Backtrack(n, b, posL+1, posR-1, nt);
                    if (r != null) return r;
                }
                else
                {
                    b[posL] = c;
                    var r = Backtrack(n, b, posL+1, posR-1, false);
                    if (r != null) return r;
                }
            }
            return null;
        }

        foreach(var (cl,cr) in _pairs)
        {
            if (posL==0 && L>1 && cl=='0') continue;
            if (tight)
            {
                char nc = n[posL];
                if (cl < nc) continue;
                bool nt = (cl == nc);
                b[posL] = cl; b[posR] = cr;
                var r = Backtrack(n, b, posL+1, posR-1, nt);
                if (r != null) return r;
            }
            else
            {
                b[posL] = cl; b[posR] = cr;
                var r = Backtrack(n, b, posL+1, posR-1, false);
                if (r != null) return r;
            }
        }
        return null;
    }
}
