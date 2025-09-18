using System;
using System.Collections.Generic;

class Solution
{
    private static readonly HashSet<string> _stopCodons = new HashSet<string> { "UAA", "UAG", "UGA" };
    private static readonly Dictionary<string, char> _codonMap = new Dictionary<string, char>
    {
        {"UUU",'F'},{"UUC",'F'},{"UUA",'L'},{"UUG",'L'},
        {"UCU",'S'},{"UCC",'S'},{"UCA",'S'},{"UCG",'S'},
        {"UAU",'Y'},{"UAC",'Y'},{"UGU",'C'},{"UGC",'C'},
        {"UGG",'W'},
        {"CUU",'L'},{"CUC",'L'},{"CUA",'L'},{"CUG",'L'},
        {"CCU",'P'},{"CCC",'P'},{"CCA",'P'},{"CCG",'P'},
        {"CAU",'H'},{"CAC",'H'},{"CAA",'Q'},{"CAG",'Q'},
        {"CGU",'R'},{"CGC",'R'},{"CGA",'R'},{"CGG",'R'},
        {"AUU",'I'},{"AUC",'I'},{"AUA",'I'},{"AUG",'M'},
        {"ACU",'T'},{"ACC",'T'},{"ACA",'T'},{"ACG",'T'},
        {"AAU",'N'},{"AAC",'N'},{"AAA",'K'},{"AAG",'K'},
        {"AGU",'S'},{"AGC",'S'},{"AGA",'R'},{"AGG",'R'},
        {"GUU",'V'},{"GUC",'V'},{"GUA",'V'},{"GUG",'V'},
        {"GCU",'A'},{"GCC",'A'},{"GCA",'A'},{"GCG",'A'},
        {"GAU",'D'},{"GAC",'D'},{"GAA",'E'},{"GAG",'E'},
        {"GGU",'G'},{"GGC",'G'},{"GGA",'G'},{"GGG",'G'}
    };

    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        for (var caseIdx = 0; caseIdx < n; caseIdx++)
        {
            var rna = Console.ReadLine();
            var bestTotal = -1;
            var bestSeqs = new List<string>();
            for (var frame = 0; frame < 3; frame++)
            {
                var sequences = new List<string>();
                var stateOpened = false;
                var curr = new List<char>();
                for (var i = frame; i + 3 <= rna.Length; i += 3)
                {
                    var codon = rna.Substring(i, 3);
                    if (!stateOpened)
                    {
                        if (codon == "AUG")
                        {
                            stateOpened = true;
                            curr.Clear();
                            curr.Add(_codonMap[codon]);
                        }
                    }
                    else
                    {
                        if (_stopCodons.Contains(codon))
                        {
                            sequences.Add(new string(curr.ToArray()));
                            stateOpened = false;
                            curr.Clear();
                        }
                        else
                        {
                            curr.Add(_codonMap[codon]);
                        }
                    }
                }
                var total = 0;
                foreach (var s in sequences) total += s.Length;
                if (total > bestTotal)
                {
                    bestTotal = total;
                    bestSeqs = sequences;
                }
            }
            Console.WriteLine(string.Join("-", bestSeqs));
        }
    }
    
}
