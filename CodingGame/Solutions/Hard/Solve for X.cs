using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

public class Solution 
{
    public static void Main(string[] args) 
    {
        if (!int.TryParse(Console.ReadLine(), out var n)) return;
        for (var loopIdx = 0; loopIdx < n; loopIdx++) 
        {
            var line = Console.ReadLine(); if (string.IsNullOrEmpty(line)) continue;
            var parts = line.Split('='); if (parts.Length != 2) continue;
            var left = ExpressionParser.Parse(parts[0]) as Term;
            var right = ExpressionParser.Parse(parts[1]) as Term;
            if (right.HasX() && !left.HasX()) (left, right) = (right, left);
            var xVar = new Variable("x"); var iter = 0; const int MAX_ITER = 50;
            while (!left.Equals(xVar) && iter < MAX_ITER) 
            {
                iter++; 
                if (!(left is Expression currentLeftExp)) 
                { 
                    if (left.Equals(xVar)) break; 
                    break; 
                }
                if (!currentLeftExp.Terms.Any()) break; 
                Term termWithX = null; var xLoc = -1;
                for (var ti = 0; ti < currentLeftExp.Terms.Count; ti++) 
                { 
                    if (currentLeftExp.Terms[ti].HasX()) 
                    { 
                        xLoc = ti; termWithX = currentLeftExp.Terms[ti]; break; 
                    } 
                }
                if (xLoc == -1 || termWithX == null) break; 
                var newRight = new Expression(new List<Term>(), new List<char>());
                var currentRightExp = right as Expression ?? new Expression(new List<Term> { right });
                
                if (currentLeftExp.IsAddSub() || (!currentLeftExp.Ops.Any() && currentLeftExp.Terms.Count > 1 )) 
                {
                    if (currentRightExp.IsMulDiv() && !(currentRightExp.Terms.Count == 1 && !currentRightExp.Ops.Any())) 
                    {
                        currentRightExp = new Expression(new List<Term> { currentRightExp });
                    }

                    if (xLoc == 0 || (xLoc > 0 && currentLeftExp.Ops[xLoc - 1] == '+')) 
                    {
                        newRight.Terms.AddRange(currentRightExp.Terms); 
                        newRight.Ops.AddRange(currentRightExp.Ops);
                        for (var ti = 0; ti < currentLeftExp.Terms.Count; ti++) 
                        { 
                            if (ti == xLoc) continue;
                            var opToUse = (ti == 0 || (ti > 0 && currentLeftExp.Ops[ti - 1] == '+')) ? '-' : '+';
                            if (newRight.Terms.Any() || newRight.Ops.Any() || (newRight.Terms.Count == 0 && ti == 0 && currentLeftExp.Terms.Count > 1) ) 
                            {
                                newRight.Ops.Add(opToUse);
                            }  else if (newRight.Terms.Count == 0) { 
                                 newRight.Ops.Add(opToUse);
                            }
                            newRight.Terms.Add(currentLeftExp.Terms[ti]); 
                        }
                    } 
                    else 
                    {
                        newRight.Terms.Clear(); newRight.Ops.Clear(); 
                        if (currentLeftExp.Terms.Any()) newRight.Terms.Add(currentLeftExp.Terms[0]);
                        if (currentRightExp.Terms.Any()) 
                        { 
                            if (newRight.Terms.Any()) newRight.Ops.Add('-'); 
                            newRight.Terms.Add(currentRightExp.Terms[0]);
                            for(var ro=0; ro < currentRightExp.Ops.Count; ++ro) 
                            { 
                                newRight.Ops.Add(currentRightExp.Ops[ro] == '+' ? '-' : '+'); 
                                newRight.Terms.Add(currentRightExp.Terms[ro+1]); 
                            } 
                        }
                        for (var ti = 1; ti < currentLeftExp.Terms.Count; ti++) 
                        { 
                            if (ti == xLoc) continue;
                            if (ti - 1 < currentLeftExp.Ops.Count) newRight.Ops.Add(currentLeftExp.Ops[ti - 1]); 
                            newRight.Terms.Add(currentLeftExp.Terms[ti]); 
                        }
                    }
                } 
                else 
                {
                    if (currentRightExp.IsAddSub() && !(currentRightExp.Terms.Count == 1 && !currentRightExp.Ops.Any())) 
                    {
                        currentRightExp = new Expression(new List<Term> { currentRightExp });
                    }

                    if (xLoc == 0 || (xLoc > 0 && currentLeftExp.Ops[xLoc - 1] == '*')) 
                    {
                        newRight.Terms.AddRange(currentRightExp.Terms); 
                        newRight.Ops.AddRange(currentRightExp.Ops);
                        for (var ti = 0; ti < currentLeftExp.Terms.Count; ti++) 
                        { 
                            if (ti == xLoc) continue;
                            var opToUse = (ti == 0 || (ti > 0 && currentLeftExp.Ops[ti - 1] == '*')) ? '/' : '*';
                            if (newRight.Terms.Any() || newRight.Ops.Any() || (newRight.Terms.Count == 0 && ti == 0 && currentLeftExp.Terms.Count > 1) ) 
                            {
                                newRight.Ops.Add(opToUse);
                            } else if (newRight.Terms.Count == 0) {
                                 newRight.Ops.Add(opToUse);
                            }
                            newRight.Terms.Add(currentLeftExp.Terms[ti]); 
                        }
                    } 
                    else 
                    {
                        var lPrimeFull = currentLeftExp.CloneTerm() as Expression;
                        if (xLoc < lPrimeFull.Terms.Count) lPrimeFull.Terms.RemoveAt(xLoc); 
                        else lPrimeFull = new Expression(new List<Term> { new Constant(1L) });
                        
                        if (xLoc > 0 && (xLoc - 1) < lPrimeFull.Ops.Count) 
                        {
                            lPrimeFull.Ops.RemoveAt(xLoc - 1);
                        } 
                        else if (xLoc == 0 && lPrimeFull.Ops.Count > 0 && 
                                   (lPrimeFull.Ops.Count == lPrimeFull.Terms.Count || (lPrimeFull.Terms.Count == 0 && lPrimeFull.Ops.Count ==1 ))) 
                        {
                           lPrimeFull.Ops.RemoveAt(0);
                        }
                        
                        while(lPrimeFull.Ops.Count >= lPrimeFull.Terms.Count && lPrimeFull.Terms.Count > 0) 
                        {
                           lPrimeFull.Ops.RemoveAt(lPrimeFull.Ops.Count - 1);
                        }
                        if (lPrimeFull.Terms.Count == 0 && lPrimeFull.Ops.Count == 0) 
                        {
                            lPrimeFull.Terms.Add(new Constant(1L)); 
                        }
                        
                        var rhsToUse = currentRightExp;

                        newRight.Terms.Clear(); 
                        newRight.Ops.Clear();
                        if (!lPrimeFull.Terms.Any()) lPrimeFull.Terms.Add(new Constant(1L)); 
                        newRight.Terms.Add(lPrimeFull); 
                        newRight.Ops.Add('/');
                        if (!rhsToUse.Terms.Any()) rhsToUse = new Expression(new List<Term>{new Constant(1L)}); 
                        newRight.Terms.Add(rhsToUse);
                    }
                }
                left = termWithX; 
                right = newRight;
                if (left is Expression lE) lE.Standardize(false); 
                if (right is Expression rE) rE.Standardize(false);
            }
            
            if (right is Expression rFinal) rFinal.Standardize(true);
            else if (left.Equals(xVar) && !(right is Expression)) {  }
            else 
            { 
                if (right == null) right = new Expression(new List<Term> {new Constant(0L)});
                else if (!(right is Expression)) right = new Expression(new List<Term>{right});
                ((Expression)right).Standardize(true);
            }
            Console.WriteLine($"{left}={right}");
        }
    }
}

public abstract class Term : IComparable<Term>
{
    public abstract Term Eval();
    public abstract override string ToString();
    public abstract bool HasX();
    public abstract int CompareTo(Term other);
    public abstract override bool Equals(object obj);
    public abstract override int GetHashCode();
    public abstract Term CloneTerm();
}

public class Constant : Term, IEquatable<Constant>
{
    public long Num { get; private set; }
    public long Den { get; private set; }

    public Constant(long n, long d = 1L)
    {
        if (d == 0L)
        {
            Num = n > 0L ? 1L : (n < 0L ? -1L : 0L); 
            Den = 0L;
        }
        else if (n == 0L)
        {
            Num = 0L;
            Den = 1L;
        }
        else
        {
            if (d < 0L)
            {
                n = -n;
                d = -d;
            }
            var common = Gcd(Math.Abs(n), Math.Abs(d));
            Num = n / common;
            Den = d / common;
        }
    }

    private static long Gcd(long a, long b)
    {
        while (b != 0L) { var temp = b; b = a % b; a = temp; }
        return Math.Abs(a);
    }

    public override bool Equals(object obj) => Equals(obj as Constant);
    public bool Equals(Constant other)
    {
        if (other is null) return false;
        if (Den == 0L && other.Den == 0L) return Num == other.Num;
        if (Den == 0L || other.Den == 0L) return false;
        return Num == other.Num && Den == other.Den;
    }
    public override int GetHashCode() => HashCode.Combine(Num, Den);
    public override int CompareTo(Term other)
    {
        if (other is Variable) return 1;
        if (other is Constant c)
        {
            if (this.Den == 0L && c.Den == 0L) return this.Num.CompareTo(c.Num);
            if (this.Den == 0L) return 1;
            if (c.Den == 0L) return -1;
            double thisVal = (double)this.Num / this.Den;
            double otherVal = (double)c.Num / c.Den;
            return thisVal.CompareTo(otherVal);
        }
        if (other is Expression) return -1;
        return 0;
    }

    public static Constant operator +(Constant a, Constant b)
    {
        if (a.Den == 0L || b.Den == 0L) return new Constant(1L, 0L);
        var num = a.Num * b.Den + b.Num * a.Den;
        var den = a.Den * b.Den;
        return new Constant(num, den);
    }
    public static Constant operator -(Constant a, Constant b)
    {
        if (a.Den == 0L || b.Den == 0L) return new Constant(1L, 0L);
        var num = a.Num * b.Den - b.Num * a.Den;
        var den = a.Den * b.Den;
        return new Constant(num, den);
    }
    public static Constant operator *(Constant a, Constant b)
    {
        if (a.Den == 0L || b.Den == 0L) return new Constant(1L, 0L);
        var num = a.Num * b.Num;
        var den = a.Den * b.Den;
        return new Constant(num, den);
    }
    public static Constant operator /(Constant a, Constant b)
    {
        if (b.Num == 0L && b.Den != 0L) return new Constant(1L, 0L);
        if (a.Den == 0L || b.Den == 0L) return new Constant(1L, 0L);
        var num = a.Num * b.Den;
        var den = a.Den * b.Num;
        if (den == 0L && b.Num != 0L) return new Constant(1L, 0L);
        return new Constant(num, den);
    }
    
    public override Term Eval() => this;
    public override string ToString() => Den == 0L ? "undefined" : ((Den == 1L || Num == 0L) ? Num.ToString() : ((double)Num / Den).ToString("F2", CultureInfo.InvariantCulture));
    public override bool HasX() => false;
    public override Term CloneTerm() => new Constant(this.Num, this.Den);
}

public class Variable : Term, IEquatable<Variable> 
{
    public string Name { get; }
    public Variable(string name) { Name = name; }
    public override Term Eval() => this;
    public override string ToString() => Name;
    public override bool Equals(object obj) { if (obj is Variable v) return Equals(v); if (obj is string s) return Name == s; return false; }
    public bool Equals(Variable other) => other != null && Name == other.Name;
    public override int GetHashCode() => Name.GetHashCode();
    public override int CompareTo(Term other) { if (other is Variable v) return string.Compare(Name, v.Name, StringComparison.Ordinal); return -1; } // Variable is smallest
    public override bool HasX() => Name == "x";
    public override Term CloneTerm() => new Variable(this.Name);
}

public class Expression : Term, IEquatable<Expression> 
{
    public List<Term> Terms { get; set; }
    public List<char> Ops { get; set; }

    public Expression(List<Term> terms, List<char> ops = null) 
    {
        Terms = terms ?? new List<Term>();
        Ops = ops ?? new List<char>();
    }
    
    public override Term CloneTerm() 
    {
        var clonedTerms = new List<Term>();
        foreach(var t in Terms) clonedTerms.Add(t.CloneTerm());
        return new Expression(clonedTerms, new List<char>(this.Ops));
    }

    public override bool HasX() => Terms.Any(t => t.HasX());
    public override int CompareTo(Term other) 
    {
        if (other is Variable) return 1;  
        if (other is Constant) return 1;  
        if (other is Expression otherExpr) 
        {
            var thisHasX = HasX(); 
            var otherHasX = otherExpr.HasX();
            if (!thisHasX && !otherHasX) 
            {
                try 
                {
                    var thisVal = (Constant)this.Eval();
                    var otherVal = (Constant)otherExpr.Eval();
                    var valComparison = thisVal.CompareTo(otherVal);
                    if (valComparison != 0) return valComparison;
                } 
                catch { } 
            } 
            else if (thisHasX && !otherHasX) return 1; 
            else if (!thisHasX && otherHasX) return -1; 
            return string.Compare(this.ToString(), otherExpr.ToString(), StringComparison.Ordinal);
        }
        return 0; 
    }

    public bool IsDiv() => Ops.Count == 1 && Ops[0] == '/';
    public bool IsMul() => Ops.Any() && Ops.All(op => op == '*');
    public bool IsMulDiv() => Ops.Any() && Ops.All(op => op == '*' || op == '/');
    public bool IsAddSub() => Ops.Any() && Ops.All(op => op == '+' || op == '-');

    public override Term Eval() 
    {
        if (!Terms.Any()) return new Constant(0L);
        if (HasX()) throw new InvalidOperationException("Expression contains 'x'.");
        var evalTerms = Terms.Select(t => t.Eval() as Constant ?? throw new InvalidOperationException("Non-constant term in Eval().")).ToList();
        if (!evalTerms.Any()) return new Constant(0L);
        var currentResult = evalTerms[0];
        for (var i = 0; i < Ops.Count; i++) 
        {
            var nextTerm = evalTerms[i + 1];
            switch (Ops[i]) 
            {
                case '+': currentResult += nextTerm; break;
                case '-': currentResult -= nextTerm; break;
                case '*': currentResult *= nextTerm; break;
                case '/': currentResult /= nextTerm; break;
                default: throw new InvalidOperationException($"Unknown operator: {Ops[i]}");
            }
        }
        return currentResult;
    }

    public void RewriteDiv() 
    {
        foreach(var term in Terms) if(term is Expression exp) exp.RewriteDiv();
        if (IsMulDiv()) 
        {
            var top = new List<Term>(); 
            var bottom = new List<Term>();
            for (var i = 0; i < Terms.Count; i++) 
            {
                var term = Terms[i];
                var isNum = (i == 0 || Ops[i - 1] == '*');
                if (term is Expression et && et.IsDiv()) 
                {
                    (isNum ? top : bottom).Add(et.Terms[0]);
                    (isNum ? bottom : top).Add(et.Terms[1]);
                } 
                else (isNum ? top : bottom).Add(term);
            }
            if (!bottom.Any()) 
            {
                Terms = top; 
                Ops = new List<char>(Enumerable.Repeat('*', Math.Max(0, top.Count - 1)));
            } 
            else 
            {
                var t1 = top.Count > 1 ? new Expression(top, Enumerable.Repeat('*', top.Count - 1).ToList()) : (top.Any() ? top[0] : new Constant(1L));
                var t2 = bottom.Count > 1 ? new Expression(bottom, Enumerable.Repeat('*', bottom.Count - 1).ToList()) : (bottom.Any() ? bottom[0] : new Constant(1L));
                Terms = new List<Term> { t1, t2 }; 
                Ops = new List<char> { '/' };
            }
        }
    }
    
    private static void SortRange(List<Term> list, int startIdx, int count) 
    {
        if (count <= 1) return;
        var subList = list.GetRange(startIdx, count); 
        subList.Sort();
        for (var i = 0; i < count; i++) list[startIdx + i] = subList[i];
    }

    public void DoSort() 
    {
        foreach(var term in Terms) if(term is Expression exp) exp.DoSort();
        if (IsAddSub()) 
        {
            bool changed; 
            do 
            { 
                changed = false;
                for (var i = 0; i < Ops.Count; i++) 
                {
                    if (Ops[i] == '+' && i > 0 && Ops[i-1] == '-') 
                    {
                        var tempT = Terms[i+1]; Terms[i+1] = Terms[i]; Terms[i] = tempT;
                        var tempO = Ops[i]; Ops[i] = Ops[i-1]; Ops[i-1] = tempO;
                        changed = true;
                    } 
                } 
            } while (changed);
            var plusIndices = Ops.Select((o, i) => o == '+' ? i : -1).Where(i => i != -1).ToList();
            if (plusIndices.Any()) SortRange(Terms, 0, plusIndices.Last() + 2);
            else if (!Ops.Any() && Terms.Count > 1) SortRange(Terms, 0, Terms.Count);
            var minusIndices = Ops.Select((o, i) => o == '-' ? i : -1).Where(i => i != -1).ToList();
            if (minusIndices.Any()) SortRange(Terms, minusIndices.First() + 1, Terms.Count - (minusIndices.First() + 1));
        } 
        else if (IsMulDiv()) 
        {
            var multIndices = Ops.Select((o, i) => o == '*' ? i : -1).Where(i => i != -1).ToList();
            if (multIndices.Any()) SortRange(Terms, 0, multIndices.Last() + 2);
            else if (!Ops.Any() && Terms.Count > 1) SortRange(Terms, 0, Terms.Count);
            var divIndices = Ops.Select((o, i) => o == '/' ? i : -1).Where(i => i != -1).ToList();
            if (divIndices.Any()) SortRange(Terms, divIndices.First() + 1, Terms.Count - (divIndices.First() + 1));
        }
    }

    public void Standardize(bool doSort = true) 
    {
        RewriteDiv();
        foreach(var term in Terms) if(term is Expression exp) exp.Standardize(false);
        
        var newTerms = new List<Term>();
        var newOps = new List<char>();
        var sourceExpr = this;

        if (!Ops.Any() && Terms.Count == 1 && Terms[0] is Expression singleChild)
        {
            if ((IsMulDiv() && singleChild.IsMulDiv()) || 
                (IsAddSub() && singleChild.IsAddSub()) ||
                (!IsMulDiv() && !IsAddSub() && (singleChild.IsMulDiv() || singleChild.IsAddSub())))
            {
                 sourceExpr = singleChild;
            }
        }

        if (sourceExpr.IsMulDiv() || (!sourceExpr.IsAddSub() && !sourceExpr.Ops.Any() && sourceExpr.Terms.Any()))
        {
            for (var idx = 0; idx < sourceExpr.Terms.Count; idx++)
            {
                var term = sourceExpr.Terms[idx];
                if (!(term is Expression expTerm) || expTerm.IsAddSub()) newTerms.Add(term);
                else if (idx > 0 && newOps.Any() && newOps.Last() == '/' && expTerm.IsMul())
                {
                    newTerms.AddRange(expTerm.Terms); newOps.AddRange(Enumerable.Repeat('/', expTerm.Ops.Count));
                }
                else { newTerms.AddRange(expTerm.Terms); newOps.AddRange(expTerm.Ops); }
                if (idx < sourceExpr.Ops.Count) newOps.Add(sourceExpr.Ops[idx]);
            }
        }
        else if (sourceExpr.IsAddSub())
        {
            for (var idx = 0; idx < sourceExpr.Terms.Count; idx++)
            {
                var term = sourceExpr.Terms[idx];
                if (!(term is Expression expTerm) || expTerm.IsMulDiv()) newTerms.Add(term);
                else 
                {
                    var precededByMinus = (idx > 0 && newOps.Any() && newOps.Last() == '-');
                    newTerms.AddRange(expTerm.Terms);
                    newOps.AddRange(precededByMinus ? expTerm.Ops.Select(op => op == '+' ? '-' : '+') : expTerm.Ops);
                }
                if (idx < sourceExpr.Ops.Count) newOps.Add(sourceExpr.Ops[idx]);
            }
        } else { 
            newTerms.AddRange(sourceExpr.Terms); 
            newOps.AddRange(sourceExpr.Ops); 
        } 
        Terms = newTerms; Ops = newOps;
        if (doSort) DoSort();
    }

    public override string ToString()
    {
        if (!Terms.Any()) return "";
        var sb = new StringBuilder();
        for (var i = 0; i < Terms.Count; i++)
        {
            var term = Terms[i];
            if (term is Expression expTerm)
            {
                var needsParen = false;
                if (i > 0 && Ops.Count > i - 1)
                {
                    var pOp = Ops[i - 1];
                    if ((expTerm.IsMulDiv() && pOp == '/') || (expTerm.IsAddSub() && (pOp == '*' || pOp == '/'))) needsParen = true;
                }
                else if (i == 0 && Ops.Any() && i < Ops.Count)
                {
                    if (expTerm.IsAddSub() && (Ops[i] == '*' || Ops[i] == '/')) needsParen = true;
                }
                if (needsParen) sb.Append('('); sb.Append(expTerm.ToString()); if (needsParen) sb.Append(')');
            }
            else sb.Append(term.ToString());
            if (i < Ops.Count) sb.Append(Ops[i]);
        }
        return sb.ToString();
    }
    public override bool Equals(object obj) => Equals(obj as Expression); 
    public bool Equals(Expression other) => other != null && ToString() == other.ToString(); 
    public override int GetHashCode() => ToString().GetHashCode();
}

public static class ExpressionParser
{
    public static Expression Parse(string expStr)
    {
        expStr = expStr.Replace(" ", ""); if (string.IsNullOrEmpty(expStr)) return new Expression(new List<Term>());
        var terms = new List<Term>(); var operators = new List<char>(); var i = 0;
        while (i < expStr.Length)
        {
            if (expStr[i] == '(')
            {
                var pCount = 1; var j = i; while (pCount > 0) { j++; if (j >= expStr.Length) throw new ArgumentException("Mismatched parens"); if (expStr[j] == '(') pCount++; else if (expStr[j] == ')') pCount--; }
                terms.Add(Parse(expStr.Substring(i + 1, j - i - 1))); i = j;
            }
            else if (char.IsDigit(expStr[i]))
            {
                var start = i; while (i < expStr.Length - 1 && char.IsDigit(expStr[i + 1])) i++;
                terms.Add(new Constant(long.Parse(expStr.Substring(start, i - start + 1))));
            }
            else if (char.IsLetter(expStr[i])) terms.Add(new Variable(expStr[i].ToString()));
            else operators.Add(expStr[i]);
            i++;
        }
        var hasAddSub = operators.Any(o => o == '+' || o == '-');
        var hasMultDiv = operators.Any(o => o == '*' || o == '/');
        if (hasAddSub && hasMultDiv)
        {
            var finalTerms = new List<Term>(); var finalOps = new List<char>();
            var currentMDTerms = new List<Term>(); var currentMDOps = new List<char>();
            if (terms.Any()) currentMDTerms.Add(terms[0]);
            for (var k = 0; k < operators.Count; k++)
            {
                var op = operators[k]; var next = terms[k + 1];
                if (op == '*' || op == '/') { currentMDTerms.Add(next); currentMDOps.Add(op); }
                else
                {
                    if (currentMDOps.Any()) finalTerms.Add(new Expression(new List<Term>(currentMDTerms), new List<char>(currentMDOps)));
                    else if (currentMDTerms.Any()) finalTerms.Add(currentMDTerms[0]);
                    currentMDTerms.Clear(); currentMDOps.Clear(); currentMDTerms.Add(next); finalOps.Add(op);
                }
            }
            if (currentMDOps.Any()) finalTerms.Add(new Expression(new List<Term>(currentMDTerms), new List<char>(currentMDOps)));
            else if (currentMDTerms.Any()) finalTerms.Add(currentMDTerms[0]);
            return new Expression(finalTerms, finalOps);
        }
        return new Expression(terms, operators);
    }
}

