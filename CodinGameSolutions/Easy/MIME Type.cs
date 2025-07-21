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
        int N = int.Parse(Console.ReadLine()); // Number of elements which make up the association table.
        int Q = int.Parse(Console.ReadLine()); // Number Q of file names to be analyzed.
        Dictionary<string,string> T = new Dictionary<string,string>();
        for (int i = 0; i < N; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            string EXT = inputs[0].ToUpper(); // file extension
            string MT = inputs[1]; // MIME type.
            T.Add(EXT,MT);
        }
        // For each of the Q filenames, display on a line the corresponding MIME type. If there is no corresponding type, then display UNKNOWN.
        for (int i = 0; i < Q; i++)
        {
            string FNAME = Console.ReadLine(); // One file name per line.
            if(!string.IsNullOrWhiteSpace(FNAME)){
                int l = FNAME.LastIndexOf('.');
                string REXT = l >=0 ? FNAME.Substring(l+1).ToUpper() : String.Empty;
                Console.Error.WriteLine("File: "+FNAME);
                if (!string.IsNullOrWhiteSpace(REXT) && T.Keys.Contains(REXT)){
                    Console.WriteLine(T[REXT]);
                } else {
                    Console.WriteLine("UNKNOWN");
                }
            }
        }

        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");


        
        
    }
}