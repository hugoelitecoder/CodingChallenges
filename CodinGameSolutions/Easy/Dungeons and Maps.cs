using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var d=Console.ReadLine().Split();
        var w=int.Parse(d[0]);
        var h=int.Parse(d[1]);
        d=Console.ReadLine().Split();
        var sr=int.Parse(d[0]);
        var sc=int.Parse(d[1]);
        var n=int.Parse(Console.ReadLine());
        var maps=new List<char[,]>();
        for(var i=0;i<n;i++)
        {
            var map=new char[h,w];
            for(var j=0;j<h;j++)
            {
                var line=Console.ReadLine();
                for(var k=0;k<w;k++) map[j,k]=line[k];
            }
            maps.Add(map);
        }
        var minLen=int.MaxValue;
        var res=-1;
        for(var idx=0;idx<n;idx++)
        {
            var map=maps[idx];
            var seen=new HashSet<(int,int)>();
            var r=sr;
            var c=sc;
            var steps=1;
            var trap=false;
            while(true)
            {
                if(r<0||r>=h||c<0||c>=w||map[r,c]=='#'||seen.Contains((r,c)))
                {
                    trap=true;
                    break;
                }
                if(map[r,c]=='T') break;
                seen.Add((r,c));
                var dir=map[r,c];
                if(dir=='^') r--;
                else if(dir=='v') r++;
                else if(dir=='<') c--;
                else if(dir=='>') c++;
                else
                {
                    trap=true;
                    break;
                }
                steps++;
            }
            if(!trap && steps<minLen)
            {
                minLen=steps;
                res=idx;
            }
        }
        Console.WriteLine(res==-1?"TRAP":res.ToString());
    }
}
