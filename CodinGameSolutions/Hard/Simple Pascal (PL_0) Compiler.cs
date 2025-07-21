using System;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        var n = int.Parse(Console.ReadLine());
        var src = new List<string>();
        for (var i = 0; i < n; i++) src.Add(Console.ReadLine());
        new Solution().Compile(src);
    }

    struct Tok { public string T; public int L; public int P; public Tok(string t, int l, int p) { T = t; L = l; P = p; } }
    class Sym { public string N; public string K; public int Lev; public int Val; public int Addr; public int Line; public Sym(string n, string k, int lev, int val, int addr, int line) { N = n; K = k; Lev = lev; Val = val; Addr = addr; Line = line; } }
    class Instr { public string T; public int L; public int A; public Instr(string t, int l, int a) { T = t; L = l; A = a; } }
    List<Tok> toks = new List<Tok>();
    int tp = 0;
    List<List<Sym>> syms = new List<List<Sym>>();
    int level = 0;
    Stack<int> stk = new Stack<int>();
    List<Instr> insts = new List<Instr>();
    string err;
    HashSet<string> kws = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "const","var","procedure","begin","end","if","then","while","do","call","odd" };
    HashSet<string> ops = new HashSet<string> { "+","-","*","/","=","#","<",">","<=",">=" };
    HashSet<string> spec = new HashSet<string> { "=", ",", ";", "(", ")", ".", ":=", "+", "-", "*", "/", "#", "<", ">", "<=", ">=", "?", "!" };
    class E : Exception { }
    void Compile(List<string> src)
    {
        Tokn(src);
        if (err != null) { Console.WriteLine(err); return; }
        syms.Add(new List<Sym>());
        stk.Push(3);
        try { Prog(); }
        catch (E) { Console.WriteLine(err); return; }
        if (tp < toks.Count) { Console.WriteLine($"Line {toks[tp].L}: Invalid statement"); return; }
        for (var i = 0; i < insts.Count; i++) if (insts[i].T != "nop") Console.WriteLine($"{insts[i].T} {insts[i].L}, {insts[i].A}");
    }
    void Tokn(List<string> src)
    {
        for (var i = 0; i < src.Count; i++)
        {
            var s = src[i]; var l = i + 1; var p = 1; var j = 0;
            while (j < s.Length)
            {
                while (j < s.Length && char.IsWhiteSpace(s[j])) { j++; p++; }
                if (j >= s.Length) break;
                if (char.IsLetter(s[j]))
                {
                    var st = j;
                    while (j < s.Length && char.IsLetterOrDigit(s[j])) j++;
                    toks.Add(new Tok(s.Substring(st, j - st), l, p)); p += j - st;
                }
                else if (char.IsDigit(s[j]))
                {
                    var st = j;
                    while (j < s.Length && char.IsDigit(s[j])) j++;
                    toks.Add(new Tok(s.Substring(st, j - st), l, p)); p += j - st;
                }
                else
                {
                    var t = ""; if (j + 1 < s.Length && (s[j] == '<' || s[j] == '>') && s[j + 1] == '=') { t = s.Substring(j, 2); j += 2; p += 2; }
                    else if (j + 1 < s.Length && s[j] == ':' && s[j + 1] == '=') { t = ":="; j += 2; p += 2; }
                    else { t = s[j].ToString(); j++; p++; }
                    toks.Add(new Tok(t, l, p - t.Length));
                }
            }
        }
    }
    Tok Peek(int k = 0) { if (tp + k < toks.Count) return toks[tp + k]; return new Tok("", toks.Count > 0 ? toks[^1].L : 1, 0); }
    bool Ac(string t) { if (tp < toks.Count && string.Equals(toks[tp].T, t, StringComparison.OrdinalIgnoreCase)) { tp++; return true; } return false; }
    bool AcSp(string t) { if (tp < toks.Count && toks[tp].T == t) { tp++; return true; } return false; }
    void Ex(string t, string m) { if (!Ac(t)) { var line = tp > 0 ? toks[tp - 1].L : 1; Error(line, $"{m} missing"); throw new E(); } }
    void Error(int l, string m) { if (err == null) err = $"Line {l}: {m}"; throw new E(); }
    void Prog() { Blk(); if (!AcSp(".")) Error(Peek().L, "Invalid program"); }
    void Blk()
    {
        var jmp = insts.Count; insts.Add(new Instr("jmp", 0, 0));
        var vcnt = 0; level++; syms.Add(new List<Sym>()); stk.Push(3);
        if (Ac("const"))
        {
            do
            {
                var t = Peek(); if (!char.IsLetter(t.T.Length > 0 ? t.T[0] : '\0')) Error(t.L, "Invalid const");
                var n = t.T; tp++; if (FindCurr(n) != null) Error(t.L, "const already defined");
                Ex("=", ";");
                t = Peek(); if (!int.TryParse(t.T, out var val)) Error(t.L, "Invalid const value"); tp++;
                syms[^1].Add(new Sym(n, "const", level - 1, val, 0, t.L));
            } while (AcSp(","));
            if (!AcSp(";")) Error(tp > 0 ? toks[tp - 1].L : 1, "; missing");
        }
        if (Ac("var"))
        {
            do
            {
                var t = Peek(); if (!char.IsLetter(t.T.Length > 0 ? t.T[0] : '\0')) Error(t.L, "Invalid var");
                var n = t.T; tp++; if (FindCurr(n) != null) Error(t.L, "var already defined");
                vcnt++; syms[^1].Add(new Sym(n, "var", level - 1, 0, stk.Peek(), t.L)); stk.Pop(); stk.Push(syms[^1][^1].Addr + 1);
            } while (AcSp(","));
            if (!AcSp(";")) Error(tp > 0 ? toks[tp - 1].L : 1, "; missing");
        }
        while (Ac("procedure"))
        {
            var t = Peek(); if (!char.IsLetter(t.T.Length > 0 ? t.T[0] : '\0')) Error(t.L, "Invalid procedure");
            var n = t.T; tp++; if (FindCurr(n) != null) Error(t.L, "procedure already defined");
            if (!AcSp(";")) Error(tp > 0 ? toks[tp - 1].L : 1, "; missing");
            var ipos = insts.Count; syms[^1].Add(new Sym(n, "proc", level - 1, 0, ipos, t.L));
            Blk();
            if (!AcSp(";")) Error(tp > 0 ? toks[tp - 1].L : 1, "; missing");
        }
        var ip = insts.Count;
        insts.Add(new Instr("int", 0, stk.Peek()));
        insts[jmp].A = ip;
        Stmt();
        insts.Add(new Instr("opr", 0, 0));
        level--; syms.RemoveAt(syms.Count - 1); stk.Pop();
    }
    Sym Find(string n) { for (var i = syms.Count - 1; i >= 0; i--) for (var j = 0; j < syms[i].Count; j++) if (syms[i][j].N == n) return syms[i][j]; return null; }
    Sym FindCurr(string n) { for (var j = 0; j < syms[^1].Count; j++) if (syms[^1][j].N == n) return syms[^1][j]; return null; }
    void Stmt()
    {
        var t = Peek();
        if (char.IsLetter(t.T.Length > 0 ? t.T[0] : '\0') && !kws.Contains(t.T))
        {
            var n = t.T; tp++; var s = Find(n); if (s == null) Error(t.L, "Unknown var");
            if (!AcSp(":=")) Error(Peek().L, ":= missing");
            var stmtLine = toks[tp - 2].L;
            Expr();
            if (s.K == "const") Error(stmtLine, "Invalid statement");
            Emit("sto", level - 1 - s.Lev, s.Addr);
        }
        else if (Ac("call"))
        {
            var t2 = Peek(); var n = t2.T; tp++; var s = Find(n);
            if (s == null || s.K != "proc") Error(t2.L, "Unknown var");
            Emit("cal", level - 1 - s.Lev, s.Addr);
        }
        else if (AcSp("?"))
        {
            var t2 = Peek(); var n = t2.T; tp++; var s = Find(n);
            if (s == null || s.K != "var") Error(t2.L, "Unknown var");
            Emit("opr", 0, 14); Emit("sto", level - 1 - s.Lev, s.Addr);
        }
        else if (AcSp("!"))
        {
            Expr(); Emit("opr", 0, 13);
        }
        else if (Ac("begin"))
        {
            Stmt(); while (AcSp(";")) { if (Peek().T == "end") break; Stmt(); }
            if (!Ac("end")) Error(tp > 0 ? toks[tp - 1].L : 1, "end missing");
        }
        else if (Ac("if"))
        {
            Cond(); if (!Ac("then")) Error(tp > 0 ? toks[tp - 1].L : 1, "then missing");
            var p = insts.Count; Emit("jpc", 0, 0); Stmt(); insts[p].A = insts.Count;
        }
        else if (Ac("while"))
        {
            var s = insts.Count; Cond(); if (!Ac("do")) Error(tp > 0 ? toks[tp - 1].L : 1, "do missing");
            var p = insts.Count; Emit("jpc", 0, 0); Stmt(); Emit("jmp", 0, s); insts[p].A = insts.Count;
        }
    }
    void Cond()
    {
        if (Ac("odd")) { Expr(); Emit("opr", 0, 6); }
        else
        {
            Expr();
            var op = "";
            if (AcSp("=")) op = "="; else if (AcSp("#")) op = "#";
            else if (AcSp("<")) op = "<"; else if (AcSp("<=")) op = "<=";
            else if (AcSp(">")) op = ">"; else if (AcSp(">=")) op = ">=";
            else Error(Peek().L, "Invalid condition");
            Expr();
            if (op == "=") Emit("opr", 0, 7);
            else if (op == "#") Emit("opr", 0, 8);
            else if (op == "<") Emit("opr", 0, 9);
            else if (op == ">=") Emit("opr", 0, 10);
            else if (op == ">") Emit("opr", 0, 11);
            else if (op == "<=") Emit("opr", 0, 12);
        }
    }
    void Expr()
    {
        var neg = false; if (AcSp("+")) { } else if (AcSp("-")) neg = true;
        Term(); if (neg) Emit("opr", 0, 1);
        while (true)
        {
            if (AcSp("+")) { Term(); Emit("opr", 0, 2); }
            else if (AcSp("-")) { Term(); Emit("opr", 0, 3); }
            else break;
        }
    }
    void Term()
    {
        Factor();
        while (true)
        {
            if (AcSp("*")) { Factor(); Emit("opr", 0, 4); }
            else if (AcSp("/")) { Factor(); Emit("opr", 0, 5); }
            else break;
        }
    }
    void Factor()
    {
        var t = Peek();
        if (char.IsLetter(t.T.Length > 0 ? t.T[0] : '\0') && !kws.Contains(t.T))
        {
            var n = t.T; tp++; var s = Find(n); if (s == null) Error(t.L, "Unknown var");
            if (s.K == "const") Emit("lit", 0, s.Val);
            else if (s.K == "var") Emit("lod", level - 1 - s.Lev, s.Addr);
            else Error(t.L, "Unknown var");
        }
        else if (int.TryParse(t.T, out var v)) { tp++; Emit("lit", 0, v); }
        else if (AcSp("(")) { Expr(); if (!AcSp(")")) Error(Peek().L, ") missing"); }
        else Error(t.L, "Invalid expr");
    }
    void Emit(string t, int l, int a) { insts.Add(new Instr(t, l, a)); }
}
