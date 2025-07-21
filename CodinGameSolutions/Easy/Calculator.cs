using System;
using System.Globalization;

class Solution{

    static void Main(){

        int n=int.Parse(Console.ReadLine());
        string dsp="0";
        double acc=0,lastNum=0;
        char? op=null;
        char lastOp='\0';
        char lk='C';
        for(int i=0;i<n;i++){
            var key=Console.ReadLine();
            if(key=="AC"){
                dsp="0"; acc=0; op=null; lastOp='\0'; lk='C';
            } else if(key=="+"||key=="-"||key=="x"||key=="/"){
                char o=key[0];
                if(lk=='O') op=o;
                else {
                    double cur=double.Parse(dsp,CultureInfo.InvariantCulture);
                    acc=op.HasValue?Calc(acc,cur,op.Value):cur;
                    dsp=Fmt(acc);
                    op=o;
                }
                lk='O';
            } else if(key=="="){
                if(lk=='D' && op.HasValue){
                    double cur=double.Parse(dsp,CultureInfo.InvariantCulture);
                    acc=Calc(acc,cur,op.Value);
                    dsp=Fmt(acc);
                    lastOp=op.Value; lastNum=cur; op=null;
                } else if(lk=='E' && lastOp!='\0'){
                    double cur=double.Parse(dsp,CultureInfo.InvariantCulture);
                    dsp=Fmt(Calc(cur,lastNum,lastOp));
                }
                lk='E';
            } else { 
                dsp=lk=='D'?(dsp=="0"?key:dsp+key):key;
                lk='D';
            }
            Console.WriteLine(dsp);
        }
    }
    static double Calc(double a,double b,char o){
        double r = o=='+'? a+b :
                   o=='-'? a-b :
                   o=='x'? a*b :
                            a/b;
        return Math.Round(r,3,MidpointRounding.AwayFromZero);
    }
    static string Fmt(double v){
        if(Math.Abs(v-Math.Round(v))<1e-9) 
            return ((long)Math.Round(v)).ToString();
        return v.ToString("0.###",CultureInfo.InvariantCulture);
    }
}
