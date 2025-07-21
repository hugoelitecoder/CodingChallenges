using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution
{
    static string ALFA = "ABCDEFGHIJKLMNOPQRSTUVWXYZ?";
    static List<string> lines = new List<string>();
    
    static List<string> createLetter(char letter, int width, int height){
        List<string> result = new List<string>();
        int indexAlfa = ALFA.IndexOf(letter);
        int offsetX = indexAlfa* width;
        if (offsetX >= 0){
            for(int y=0; y < height; y++){
                result.Add(lines[y].Substring(offsetX,width));
            }
        }
        return result;
    }
    
    static string standardize(string T){
         string result = String.Empty;
         foreach(char ch in T.ToUpper()){
             if (ALFA.IndexOf(ch) < 0){
                 result+='?';
             } else {
                 result+=ch;
             }
         }
         return result;
    }
    
    static void Main(string[] args)
    {
        int L = int.Parse(Console.ReadLine());
        int H = int.Parse(Console.ReadLine());
        string T = Console.ReadLine();
        for (int i = 0; i < H; i++)
        {
            
            string rowline = Console.ReadLine();
            //Console.Error.WriteLine(rowline);
            lines.Add(rowline);
        }
        List<string> asciiLetters = new List<String>();
        foreach(var ch in standardize(T)){
            var asciiLetter = createLetter(ch,L,H);
            for(int j=0; j < asciiLetter.Count;  j++){
                if (asciiLetters.Count > j){
                    asciiLetters[j] += asciiLetter[j];
                } else {
                    asciiLetters.Add(asciiLetter[j]);
                }
            }
        }
        for(int l=0; l < asciiLetters.Count; l++){
            Console.WriteLine(asciiLetters[l]);
        }
        
        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");

        
    }
}