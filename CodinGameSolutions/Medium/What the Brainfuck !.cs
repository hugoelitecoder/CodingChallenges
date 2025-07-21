using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Solution {
    static void Main() {
        var p = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int L = p[0], S = p[1], N = p[2];

        var src = new StringBuilder();
        for(int i=0;i<L;i++) src.Append(Console.ReadLine());
        var code = src.ToString().Where(c=>"<>+-.,[]".Contains(c)).ToArray();
        int M = code.Length;

        var jump = new int[M];
        var st = new Stack<int>();
        for(int i=0;i<M;i++) {
            if(code[i]=='[') st.Push(i);
            else if(code[i]==']') {
                if(st.Count==0) { Console.WriteLine("SYNTAX ERROR"); return; }
                int j=st.Pop();
                jump[i]=j; jump[j]=i;
            }
        }
        if(st.Count>0) { Console.WriteLine("SYNTAX ERROR"); return; }

        var input = Enumerable.Range(0,N).Select(_=>int.Parse(Console.ReadLine())).ToArray();
        int ip=0, dp=0, ipt=0;
        var cells = new int[S];
        var output = new StringBuilder();

        while(ip<M) {
            switch(code[ip]) {
                case '>': if(++dp>=S) { Console.WriteLine("POINTER OUT OF BOUNDS"); return; } break;
                case '<': if(--dp<0)   { Console.WriteLine("POINTER OUT OF BOUNDS"); return; } break;
                case '+': if(++cells[dp]>255) { Console.WriteLine("INCORRECT VALUE"); return; } break;
                case '-': if(--cells[dp]<0)   { Console.WriteLine("INCORRECT VALUE"); return; } break;
                case '.': output.Append((char)cells[dp]); break;
                case ',': {
                        if(ipt>=N||input[ipt]<0||input[ipt]>255) { Console.WriteLine("INCORRECT VALUE"); return; }
                        cells[dp]=input[ipt++]; 
                    } break;
                case '[': if(cells[dp]==0) ip=jump[ip]; break;
                case ']': if(cells[dp]!=0) ip=jump[ip]; break;
            }
            ip++;
        }

        Console.Write(output.ToString());
    }
}
