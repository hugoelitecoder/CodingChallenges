using System;
using System.Collections.Generic;
using System.Numerics;

class Solution {

    abstract class Expr { public abstract Frac Eval(); }
    class Lit : Expr { BigInteger v; public Lit(BigInteger v)=>this.v=v; public override Frac Eval()=>new Frac(v,1); }
    class VarRef : Expr {
        string name;
        public VarRef(string n)=>name=n;
        public override Frac Eval(){
            if(cache.TryGetValue(name,out var f)) return f;
            var r = defs[name].Eval();
            cache[name] = r;
            return r;
        }
    }
    class BinOp : Expr {
        char op; Expr L, R;
        public BinOp(char op, Expr L, Expr R)=>(this.op,this.L,this.R)=(op,L,R);
        public override Frac Eval(){
            var a = L.Eval();
            var b = R.Eval();
            return op=='+' ? a+b
                 : op=='-' ? a-b
                 : op=='*' ? a*b
                 : a/b;
        }
    }
    struct Frac {
        public BigInteger n,d;
        public Frac(BigInteger a, BigInteger b){
            if(b.Sign<0){a=-a; b=-b;}
            var g=BigInteger.GreatestCommonDivisor(BigInteger.Abs(a), b);
            n=a/g; d=b/g;
        }
        public override string ToString()=> d.IsOne ? n.ToString() : $"{n}/{d}";
        public static Frac operator+(Frac A,Frac B)=> new Frac(A.n*B.d + B.n*A.d, A.d*B.d);
        public static Frac operator-(Frac A,Frac B)=> new Frac(A.n*B.d - B.n*A.d, A.d*B.d);
        public static Frac operator*(Frac A,Frac B)=> new Frac(A.n*B.n, A.d*B.d);
        public static Frac operator/(Frac A,Frac B)=> new Frac(A.n*B.d, A.d*B.n);
    }

    static string src; static int pos;
    static Dictionary<string,Expr> defs;
    static Dictionary<string,Frac> cache;

    static void SkipWs(){ while(pos<src.Length && char.IsWhiteSpace(src[pos])) pos++; }
    static string ReadStr(){
        pos++; int st=pos;
        while(src[pos]!='\'') pos++;
        var s=src.Substring(st, pos-st);
        pos++;
        return s;
    }
    static object ParseVal(){
        SkipWs();
        if(src[pos]=='\'') return ReadStr();
        if(src[pos]=='(')  return ParseBlk();
        var st=pos;
        if(src[pos]=='-') pos++;
        while(pos<src.Length && char.IsDigit(src[pos])) pos++;
        return BigInteger.Parse(src.Substring(st, pos-st));
    }
    static Dictionary<string,object> ParseBlk(){
        pos++; 
        var dict = new Dictionary<string,object>();
        for(;;){
            SkipWs();
            var key = ReadStr();
            SkipWs(); pos++;
            var val = ParseVal();
            dict[key]=val;
            SkipWs();
            if(src[pos]==';'){ pos++; continue; }
            pos++; 
            break;
        }
        return dict;
    }
    static Expr Build(object o){
        if(o is BigInteger bi) return new Lit(bi);
        if(o is string s)      return new VarRef(s);
        var d = (Dictionary<string,object>)o;
        if(d.ContainsKey("num1")){
            var L=Build(d["num1"]);
            var R=Build(d["num2"]);
            char op = ((string)d["operator"])[0];
            return new BinOp(op, L, R);
        }
        return new BinOp('/', Build(d["numerator"]), Build(d["denominator"]));
    }

    static void Main(){
        int n = int.Parse(Console.ReadLine());
        src = "";
        for(int i=0; i<n; i++) src += Console.ReadLine();
        pos = 0;
        defs = new Dictionary<string,Expr>();
        cache = new Dictionary<string,Frac>();

        var root = ParseBlk();
        if(root.TryGetValue("vars", out var rv)){
            var vd = (Dictionary<string,object>)rv;
            foreach(var kv in vd)
                defs[kv.Key] = Build(kv.Value);
        }

        var expr = Build(root["result"]);
        Console.WriteLine(expr.Eval());
    }
}
