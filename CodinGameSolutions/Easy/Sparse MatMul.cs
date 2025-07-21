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
        var m=int.Parse(d[0]);
        var n=int.Parse(d[1]);
        var p=int.Parse(d[2]);
        d=Console.ReadLine().Split();
        var ca=int.Parse(d[0]);
        var cb=int.Parse(d[1]);
        var a=new Dictionary<int,List<(int,double)>>();
        var b=new Dictionary<int,List<(int,double)>>();
        for(var i=0;i<ca;i++)
        {
            d=Console.ReadLine().Split();
            var r=int.Parse(d[0]);
            var c=int.Parse(d[1]);
            var v=double.Parse(d[2]);
            if(!a.ContainsKey(r)) a[r]=new List<(int,double)>();
            a[r].Add((c,v));
        }
        for(var i=0;i<cb;i++)
        {
            d=Console.ReadLine().Split();
            var r=int.Parse(d[0]);
            var c=int.Parse(d[1]);
            var v=double.Parse(d[2]);
            if(!b.ContainsKey(r)) b[r]=new List<(int,double)>();
            b[r].Add((c,v));
        }
        var res=new SortedDictionary<(int,int),double>();
        foreach(var ai in a)
        {
            var i=ai.Key;
            foreach(var (j,av) in ai.Value)
            {
                if(!b.ContainsKey(j)) continue;
                foreach(var (k,bv) in b[j])
                {
                    var key=(i,k);
                    if(!res.ContainsKey(key)) res[key]=0;
                    res[key]+=av*bv;
                }
            }
        }
        foreach(var e in res) if(e.Value!=0) Console.WriteLine($"{e.Key.Item1} {e.Key.Item2} {e.Value:0.0#############################}");
    }
}
