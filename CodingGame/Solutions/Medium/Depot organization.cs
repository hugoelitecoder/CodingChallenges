using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    const int N = 7, F = 6;
    static readonly int[] DX = {1,1,0,-1,-1,0};
    static readonly int[] DY = {0,-1,-1,0,1,1};

    static readonly (int x,int y)[] POS = {
        (-1,  1), //0 NW
        ( 0,  1), //1 N
        ( 1,  0), //2 NE
        ( 0,  0), //3 center
        ( 1, -1), //4 SE
        ( 0, -1), //5 S
        (-1,  0)  //6 SW
    };

    static readonly List<(int p,int f,int q,int g)> ADJ = BuildAdj();
    static List<(int,int,int,int)> BuildAdj()
    {
        var L = new List<(int,int,int,int)>();
        for(int p=0;p<N;p++) for(int f=0;f<F;f++)
        {
            var nx = POS[p].x + DX[f];
            var ny = POS[p].y + DY[f];
            for(int q=0;q<N;q++) if(q!=p && POS[q].x==nx && POS[q].y==ny)
            {
                int g = (f+3)%6;
                L.Add((p,f,q,g));
            }
        }
        return L;
    }

    static char[][] tiles = new char[N][];
    static (int tile,int rot)[] assign = new (int,int)[N];
    static bool[] used = new bool[N];

    static char[] Rotate(char[] a,int r)
    {
        r = ((r%F)+F)%F;
        var b = new char[F];
        for(int i=0;i<F;i++) b[i] = a[(i+r)%F];
        return b;
    }

    static bool OK(int pos,int t,int r)
    {
        var face = Rotate(tiles[t],r);
        if(pos==3)
        {
            char mn = tiles[t].Min();
            if(face[0]!=mn) return false;
        }
        foreach(var (p,f,q,g) in ADJ)
        {
            if(p!=pos) continue;
            var (t2,r2) = assign[q];
            if(t2<0) continue;
            var face2 = Rotate(tiles[t2],r2);
            if(face[f]!=face2[g]) return false;
        }
        return true;
    }

    static bool Backtrack(int k)
    {
        if(k==N) return true;
        int[] order = {3,0,1,2,4,5,6};
        int pos = order[k];
        for(int t=0;t<N;t++) if(!used[t])
        {
            for(int r=0;r<F;r++)
            {
                if(OK(pos,t,r))
                {
                    assign[pos] = (t,r);
                    used[t] = true;
                    if(Backtrack(k+1)) return true;
                    used[t] = false;
                }
            }
        }
        assign[pos] = (-1,-1);
        return false;
    }

    static void Main()
    {
        for(int i=0;i<N;i++)
            tiles[i] = Console.ReadLine().Split().Select(s=>s[0]).ToArray();

        for(int i=0;i<N;i++) assign[i]=(-1,-1);

        Backtrack(0);

        var outp = new[]{0,1,6,3,2,5,4}
            .Select(p=>{
                var (t,r) = assign[p];
                var face = Rotate(tiles[t],r);
                return $"{t}{face[0]}";
            });
        Console.WriteLine(string.Join(" ",outp));
    }
}
