using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    
    static void Main(string[] args)
    {
        string T = Console.ReadLine();

        var cmds = T.Split(' ');
        var result = new StringBuilder();
        foreach(var cmd in cmds) {
            var abbr2 = cmd[^2..];
            if (abbr2 != "nl") {
                var repeatChar = cmd.TakeLast(1).First();
                var parsed = false;
                switch(abbr2) {
                    case "sp" : repeatChar = ' '; parsed = true;  break;
                    case "bS" : repeatChar = '\\'; parsed = true; break;
                    case "sQ" : repeatChar = '\''; parsed = true;  break;
                }
                if (parsed) {
                    var repeatNumber = int.Parse(cmd[..^2]);
                    result.Append(new string(repeatChar, repeatNumber));
                } else {
                    var repeatNumber = int.Parse(cmd[..^1]);
                    result.Append(new string(repeatChar, repeatNumber));
                }
            } else {
                Console.WriteLine(result.ToString());
                result = new StringBuilder();
            }
        }
        Console.WriteLine(result.ToString());
    }
}