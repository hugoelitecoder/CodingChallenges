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
class Player
{
    
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int W = int.Parse(inputs[0]); // width of the building.
        int H = int.Parse(inputs[1]); // height of the building.
        int N = int.Parse(Console.ReadLine()); // maximum number of turns before game over.
        Console.Error.WriteLine($" Building : {W}x{H}");
        inputs = Console.ReadLine().Split(' ');
        int X0 = int.Parse(inputs[0]);
        int Y0 = int.Parse(inputs[1]);
        Console.Error.WriteLine($"Starting  : {X0} {Y0}");
        //0 ≤ X, X0 < W
        //0 ≤ Y, Y0 < H
        int X = X0;
        int Y = Y0;
        int MINY = 0;
        int MINX = 0;
        int MAXY = H;
        int MAXX = W;
        // game loop
        while (true)
        {
            string bombDir = Console.ReadLine(); // the direction of the bombs from batman's current location (U, UR, R, DR, D, DL, L or UL)
            Console.Error.WriteLine($" {bombDir}");
              foreach(var dir in bombDir){
                switch(dir){
                    case 'U' : { MAXY=Y; Y = Y + (int)Math.Floor((double)(MINY-Y)/2); break; } 
                    case 'D' : { MINY=Y; Y = Y + (int)Math.Floor((double)(MAXY-Y)/2); break; } 
                    case 'L' : { MAXX=X; X = X + (int)Math.Floor((double)(MINX-X)/2); break; } 
                    case 'R' : { MINX=X; X = X + (int)Math.Floor((double)(MAXX-X)/2); break; } 
                }
            }
            Console.WriteLine($"{X} {Y}");
        }
    }
}