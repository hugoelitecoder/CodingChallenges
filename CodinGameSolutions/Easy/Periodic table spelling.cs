using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static HashSet<string> elements = new HashSet<string>{
        "H","He","Li","Be","B","C","N","O","F","Ne","Na","Mg","Al","Si","P","S","Cl","Ar",
        "K","Ca","Sc","Ti","V","Cr","Mn","Fe","Co","Ni","Cu","Zn","Ga","Ge","As","Se","Br","Kr",
        "Rb","Sr","Y","Zr","Nb","Mo","Tc","Ru","Rh","Pd","Ag","Cd","In","Sn","Sb","Te","I","Xe",
        "Cs","Ba","La","Ce","Pr","Nd","Pm","Sm","Eu","Gd","Tb","Dy","Ho","Er","Tm","Yb","Lu",
        "Hf","Ta","W","Re","Os","Ir","Pt","Au","Hg","Tl","Pb","Bi","Po","At","Rn","Fr","Ra",
        "Ac","Th","Pa","U","Np","Pu","Am","Cm","Bk","Cf","Es","Fm","Md","No","Lr","Rf","Db",
        "Sg","Bh","Hs","Mt","Ds","Rg","Cn","Nh","Fl","Mc","Lv","Ts","Og"
    };

    static List<string> results = new List<string>();

    static void Main(string[] args) {
        var word = Console.ReadLine().ToLower();
        Backtrack(word, 0, new StringBuilder());
        if (results.Count == 0) Console.WriteLine("none");
        else {
            results.Sort(StringComparer.Ordinal);
            foreach (var r in results) Console.WriteLine(r);
        }
    }

    static void Backtrack(string word, int idx, StringBuilder path) {
        if (idx == word.Length) {
            results.Add(path.ToString());
            return;
        }

        for (int len = 1; len <= 2 && idx + len <= word.Length; len++) {
            var slice = word.Substring(idx, len);
            var symbol = char.ToUpper(slice[0]) + (len == 2 ? slice[1].ToString() : "");
            if (elements.Contains(symbol)) {
                var lenBefore = path.Length;
                path.Append(symbol);
                Backtrack(word, idx + len, path);
                path.Length = lenBefore;
            }
        }
    }
}
