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
    static void Main(string[] args)
    {
        string MESSAGE = Console.ReadLine();
        List<List<short>> bits = new List<List<short>>();
        List<short> bitpart = new List<short>();
        foreach(char letter in MESSAGE){
            int num = (int)letter;
            for (short i=6; i>=0; i--){
                short bit = (num & (1<< i)) > 0 ? (short)1 : (short)0;
                if (bitpart.Count == 0 || bitpart[0] == bit){
                    bitpart.Add(bit);
                } else {
                    bits.Add(bitpart);
                    bitpart = new List<short>();
                    bitpart.Add(bit);
                }
            }
        }
        bits.Add(bitpart); // last
        String NEWMESSAGE= String.Empty;
        foreach(List<short> bitseq in bits){
            if (bitseq.Count > 0){
               NEWMESSAGE += bitseq[0] == 1 ? "0" : "00";
               NEWMESSAGE += " " + new String('0', bitseq.Count) + " ";
            }
        }
        NEWMESSAGE = NEWMESSAGE.Substring(0,NEWMESSAGE.Length-1);
         Console.WriteLine(NEWMESSAGE);
        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");

        
    }
}