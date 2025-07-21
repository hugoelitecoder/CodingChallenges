using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

class Solution
{
    static void Main()
    {
        var line = Console.ReadLine().Trim();
        var toks = Regex.Matches(line, @"d\d+|\d+|[+\-*>]|\(|\)")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .ToList();

        var prec = new Dictionary<char,int>{{'*',3},{'+',2},{'-',2},{'>',1}};
        var output = new List<string>();
        var ops = new Stack<string>();
        foreach(var t in toks)
        {
            if (Regex.IsMatch(t, @"^\d+$") || t[0]=='d')
                output.Add(t);
            else if (t=="(")
                ops.Push(t);
            else if (t==")")
            {
                while(ops.Peek()!="(") output.Add(ops.Pop());
                ops.Pop();
            }
            else
            {
                while(ops.Count>0 && ops.Peek()!="(" &&
                      prec[ops.Peek()[0]] >= prec[t[0]])
                    output.Add(ops.Pop());
                ops.Push(t);
            }
        }
        while(ops.Count>0) output.Add(ops.Pop());

        var stack = new Stack<Dictionary<int,double>>();
        foreach(var t in output)
        {
            if (Regex.IsMatch(t, @"^\d+$"))
            {
                var v = int.Parse(t);
                stack.Push(new Dictionary<int,double>{{v,1.0}});
            }
            else if (t[0]=='d')
            {
                int n = int.Parse(t.Substring(1));
                var d = new Dictionary<int,double>();
                for(int i=1;i<=n;i++) d[i]=1.0/n;
                stack.Push(d);
            }
            else
            {
                var B = stack.Pop();
                var A = stack.Pop();
                Dictionary<int,double> R = new();
                switch(t[0])
                {
                  case '+':
                    foreach(var (a,pa) in A)
                      foreach(var (b,pb) in B)
                        R[a+b] = R.GetValueOrDefault(a+b,0)+pa*pb;
                    break;
                  case '-':
                    foreach(var (a,pa) in A)
                      foreach(var (b,pb) in B)
                        R[a-b] = R.GetValueOrDefault(a-b,0)+pa*pb;
                    break;
                  case '*':
                    foreach(var (a,pa) in A)
                      foreach(var (b,pb) in B)
                        R[a*b] = R.GetValueOrDefault(a*b,0)+pa*pb;
                    break;
                  case '>':
                    double p=0;
                    foreach(var (a,pa) in A)
                      foreach(var (b,pb) in B)
                        if(a>b) p+=pa*pb;
                    R[0]=1-p; R[1]=p;
                    break;
                }
                stack.Push(R);
            }
        }

        var dist = stack.Pop();
        foreach(var kv in dist.OrderBy(k=>k.Key))
            if(kv.Value>1e-12)
                Console.WriteLine($"{kv.Key} { (kv.Value*100).ToString("F2",CultureInfo.InvariantCulture) }");
    }
}
