
using System;

public class Solution {
    public static void Main() {
        string[] input = Console.ReadLine().Split(' ');
        int sinExp = int.Parse(input[0]);
        int cosExp = int.Parse(input[1]);
        int factor = int.Parse(input[2]);
        TrigSimplifier simplifier = new TrigSimplifier();
        string result = simplifier.Simplify(sinExp, cosExp, factor);
        Console.WriteLine(result);
    }
}

public class TrigSimplifier {
    private long Factorial(long k) {
        long result = 1;
        for (long i = 2; i <= k; i++) {
            result *= i;
        }
        return result;
    }
    private long NCK(long n, long k) {
        return Factorial(n) / (Factorial(k) * Factorial(n - k));
    }
    public string Simplify(int sinExp, int cosExp, int factor) {
        if (sinExp + cosExp == 0) {
            return factor.ToString();
        }
        int isCos = 1 - sinExp % 2;
        int numTerms = sinExp + cosExp + 1;
        long[] sinEn = new long[numTerms];
        long[] sinEp = new long[numTerms];
        long[] cosEn = new long[numTerms];
        long[] cosEp = new long[numTerms];
        for (int i = 0; i <= sinExp; i++) {
            int exp = sinExp - 2 * i;
            long sign = (long)Math.Pow(-1, (sinExp - exp) / 2);
            if (exp < 0) {
                sinEp[-exp] = sign * NCK(sinExp, i);
            } else {
                sinEn[exp] = sign * NCK(sinExp, i);
            }
        }
        for (int i = 0; i <= cosExp; i++) {
            int exp = cosExp - 2 * i;
            if (exp < 0) {
                cosEp[-exp] = NCK(cosExp, i);
            } else {
                cosEn[exp] = NCK(cosExp, i);
            }
        }
        long[] resEp = new long[2 * numTerms];
        long[] resEn = new long[2 * numTerms];
        if (sinExp == 0) {
            Array.Copy(cosEn, resEn, numTerms);
            Array.Copy(cosEp, resEp, numTerms);
        } else if (cosExp == 0) {
            Array.Copy(sinEn, resEn, numTerms);
            Array.Copy(sinEp, resEp, numTerms);
        } else {
            for (int sinI = -numTerms + 1; sinI < numTerms; sinI++) {
                for (int cosI = -numTerms + 1; cosI < numTerms; cosI++) {
                    int exp = sinI + cosI;
                    long sinCoef = sinI < 0 ? sinEp[-sinI] : sinEn[sinI];
                    long cosCoef = cosI < 0 ? cosEp[-cosI] : cosEn[cosI];
                    if (exp < 0) {
                        resEp[-exp] += sinCoef * cosCoef;
                    } else {
                        resEn[exp] += sinCoef * cosCoef;
                    }
                }
            }
        }
        double pow2 = Math.Pow(2, numTerms - 2) * Math.Pow(-1, sinExp / 2);
        double[] ansEp = new double[2 * numTerms];
        double[] ansEn = new double[2 * numTerms];
        for (int i = 0; i < numTerms; i++) {
            if (resEp[i] == resEn[i]) {
                ansEp[i] = resEp[i] / pow2;
            }
            if (-resEp[i] == resEn[i]) {
                ansEn[i] = resEn[i] / pow2;
            }
        }
        string result = "";
        for (int i = 0; i < numTerms; i++) {
            double coef = ansEp[i] + ansEn[i];
            if (i > 0) {
                if (Math.Abs(coef) > 1e-9) {
                    long f = (long)Math.Round(coef * factor);
                    string signStr = coef > 0 ? "+" : "-";
                    string fStr = Math.Abs(f) != 1 ? Math.Abs(f).ToString() : "";
                    string funcStr = isCos == 1 ? "cos" : "sin";
                    string xStr = i > 1 ? i.ToString() : "";
                    result += $"{signStr}{fStr}{funcStr}({xStr}x)";
                }
            } else {
                long constTerm = (long)Math.Round((factor * (resEn[0] - resEp[0])) / (2 * pow2));
                result = constTerm.ToString();
                if (result == "0") {
                    result = "";
                }
            }
        }
        if (result.Length > 0 && result[0] == '+') {
            result = result.Substring(1);
        }
        return result;
    }
}
