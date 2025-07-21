using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static List<int> SequenceNumbers(List<int> line)
    {
        var newLine = new List<int>();
        var R = line[0];
        int count = 1;
        for(int i = 1; i < line.Count();i++){
          if(line[i] != R){
              newLine.AddRange(new int[] {count,R});
              count=1;
              R = line[i];
          }else{
           count+=1;   
          }
        }
        newLine.AddRange(new int[] {count,R});
        return newLine;
    }
    
    static void Main(string[] args)
    {
        int R = int.Parse(Console.ReadLine());
        int L = int.Parse(Console.ReadLine());
        List<int> line = new List<int>() {R};
        for(int i=1; i < L; i++){
            line = SequenceNumbers(line);
        }
        Console.WriteLine($"{String.Join(" ",line)}");
    }
}