using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;


class Solution
{
    static List<string> ReadDictionary(){
        List<string> result = new List<string>();
        int N = int.Parse(Console.ReadLine());
        for (int i = 0; i < N; i++)
        {
            result.Add(Console.ReadLine());
        }
        return result;
    }
    
    static Dictionary<char,int> ReadWeights(){
          Dictionary<char,int> weights = new Dictionary<char,int>();
          foreach(var c in "eaionrtlsu"){weights.Add(c,1);}
          foreach(var c in "dg"){weights.Add(c,2);}
          foreach(var c in "bcmp"){weights.Add(c,3);}
          foreach(var c in "fhvwy"){weights.Add(c,4);}
          foreach(var c in "k"){weights.Add(c,5);}
          foreach(var c in "jx"){weights.Add(c,8);}
          foreach(var c in "qz"){weights.Add(c,10);}
          return weights;
    }
    
    static int CalcScore(string word){
        var weights = ReadWeights();
        return word.Sum(e=>weights.Keys.Any(y=>y==e) ? weights[e]:0);
    }
    
     static bool SubsetSameFrequency(string letters, string word){
         
         Dictionary<char,int> letterfrequency = new Dictionary<char,int>();
         foreach(var group in letters.GroupBy(p => p).Select(p => new { Count = p.Count(), Char = p.Key })) {
             letterfrequency.Add(group.Char,group.Count);
         }
         
         Dictionary<char,int> wordfrequency = new Dictionary<char,int>();
         foreach(var group in word.GroupBy(p => p).Select(p => new { Count = p.Count(), Char = p.Key })) {
             wordfrequency.Add(group.Char,group.Count);
         }

         return word.All(e=> letters.Any(y=>e==y) && letterfrequency[e] >= wordfrequency[e]);
     }
    
    static void Main(string[] args)
    {
        var dictionary = ReadDictionary();
        Console.Error.WriteLine($"DICTIONARY = {String.Join(" " ,dictionary)}");
        string LETTERS = Console.ReadLine();
        var possible = dictionary.Where(word=> SubsetSameFrequency(LETTERS,word)).OrderByDescending(word=> CalcScore(word));
        Console.Error.WriteLine($"LETTERS = {LETTERS}");
        Console.Error.WriteLine($"POSSIBLE = {String.Join(" " ,possible)}");
        var anwser = possible.FirstOrDefault();
        Console.WriteLine($"{anwser}");
    }
}