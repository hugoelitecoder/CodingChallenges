using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    class Point2D {
        public Point2D(int x, int y){
           this.X = x;
           this.Y = y;
        }
        
        public int X { get; set; }
        public int Y { get; set ; }
    }
    
    class Spaceship {
        
        public int X { get; set;}
        public int Y { get; set;}
        public int HSpeed { get; set;}
        public int VSpeed { get; set;}
        public int Fuel { get; set; }
        public int Rotate { get; set; }
        public int Power { get; set; }
        
        public Spaceship(){
        }
        
        public void ReadInputs() {
            string[] inputs = Console.ReadLine().Split(' ');
            this.X = int.Parse(inputs[0]);
            this.Y = int.Parse(inputs[1]);
            this.HSpeed = int.Parse(inputs[2]); // the horizontal speed (in m/s), can be negative.
            this.VSpeed = int.Parse(inputs[3]); // the vertical speed (in m/s), can be negative.
            this.Fuel = int.Parse(inputs[4]); // the quantity of remaining fuel in liters.
            this.Rotate = int.Parse(inputs[5]); // the rotation angle in degrees (-90 to 90).
            this.Power = int.Parse(inputs[6]); // the thrust power (0 to 4).
        }
        
    }
    
    struct SpaceshipAction {
        
        public SpaceshipAction(int rotate, int power){
            this.Rotate = rotate;
            this.Power = power;
        }
        
        public int Rotate;
        public int Power;
        
    }
    
    class Surfacespace {
        
        public Surfacespace(Point2D start, Point2D end)
        {
            this.Start = start;
            this.End = end;
        }
        
        public Point2D Start { get; set; }
        public Point2D End { get; set; }
        
    }
    
    static List<Point2D> ReadSurface() {
        string[] inputs;
        int surfaceN = int.Parse(Console.ReadLine()); // the number of points used to draw the surface of Mars.
        List<Point2D> surface = new List<Point2D>();
        for (int i = 0; i < surfaceN; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int landX = int.Parse(inputs[0]); 
            int landY = int.Parse(inputs[1]); 
            var point = new Point2D(landX,landY);
            Console.Error.WriteLine($"POINT => {point.X}, {point.Y}");
            surface.Add(point);
        }
        return surface;
    }
    
    static List<Surfacespace> FindLandingSpaces(List<Point2D> surface){
        List<Surfacespace> landingspaces = new List<Surfacespace>();
        for(int l=0; l < surface.Count(); l++){
            if (l < surface.Count()-1){
                Point2D first = surface.ElementAt(l);
                Point2D second = surface.ElementAt(l+1);
                if (first.Y == second.Y){
                    Surfacespace landingspace = new Surfacespace(first, second);
                    landingspaces.Add(landingspace);
                }
            }
        }
        return landingspaces;
    }
    static bool slowdown_left_to_right = false;
    static bool slowdown_right_to_left = false;
    static SpaceshipAction CalcNextAction(Spaceship spaceship, Surfacespace landingspace){
        var G = 3.711; // m/s^2  
        var A = 21.9; // angle
        var H = Math.Abs(Math.Sin(90-A)*G);
        var landingroom = Math.Abs(landingspace.Start.X - landingspace.End.X);
        var height_reduction = Math.Abs(spaceship.Y - landingspace.Start.Y);
        
        //cos λ = G / Pmax (to get vertical part of split vector)
        //λ = acos (G / Pmax)
        //λ = acos(3.711 / 4) = acos(0.9275) = 21.9
        
        Console.Error.WriteLine($"G={G}, A={A}, H={H}");
        
        SpaceshipAction command = new SpaceshipAction(0,0);
        //Position Ship with stable angle
        if (spaceship.Y <= landingspace.Start.Y+25 && spaceship.X > landingspace.Start.X && spaceship.X < landingspace.End.X){
            command.Rotate = 0;
            if (spaceship.VSpeed <= -40){
                command.Power = 4;
                command.Rotate = 0;
            }
            else {
                command.Power = 3; 
                command.Rotate = 0;
            }
            Console.Error.WriteLine($"Command 0 - Try to land vertically (possible fail)");
        } else if (spaceship.X < landingspace.Start.X){
            var distance_to_landingspace = Math.Abs(landingspace.Start.X - spaceship.X);
            var reduction_time = Math.Abs(spaceship.HSpeed) >= 0 ? Math.Abs(spaceship.HSpeed)/H : 0;
            var distance_reduction = (int)reduction_time*spaceship.HSpeed;
            Console.Error.WriteLine($"ReductionDistance={distance_reduction}, Distance2Middle={distance_to_landingspace}");
            if (  (distance_reduction > distance_to_landingspace || slowdown_left_to_right) && distance_to_landingspace >0 && spaceship.HSpeed > 0 ){
                slowdown_left_to_right = true;
                if (distance_reduction > landingroom*1.4 && height_reduction > 2000){
                   command.Rotate = +(int)(A*2);
                   Console.Error.WriteLine($"Command 1.5 - Slowing Spaceship down ---->>");
                } else {
                    command.Rotate = +(int)A;
                }
                command.Power  = 4;    
                 Console.Error.WriteLine($"Command 1 - Slowing Spaceship down ---->>");
            }  else {
                command.Rotate = -(int)A;
                command.Power  = 4;    
                Console.Error.WriteLine($"Command 2 - Move towards landingspace ---->>");
            }
        } else if (spaceship.X > landingspace.End.X){
            var distance_to_landingspace = Math.Abs(spaceship.X- landingspace.End.X);
            var reduction_time = Math.Abs(spaceship.HSpeed) >= 0 ? Math.Abs(spaceship.HSpeed)/H : 0;
            var distance_reduction = (int)reduction_time*Math.Abs(spaceship.HSpeed);
            Console.Error.WriteLine($"ReductionDistance={distance_reduction}, Distance2Middle={distance_to_landingspace}");
            if ( (distance_reduction > distance_to_landingspace || slowdown_right_to_left) && distance_to_landingspace >0 && spaceship.HSpeed < 0){
                 slowdown_right_to_left = true;

                if (distance_reduction > landingroom*1.4 && height_reduction > 2000 ){
                   command.Rotate = -(int)(A*2);
                   Console.Error.WriteLine($"Command 3.5 - Slowing Spaceship down <<----");
                } else {
                    command.Rotate = -(int)A;
                }
                
                
                command.Power  = 4;    
                Console.Error.WriteLine($"Command 3 - Slowing Spaceship down <<----");
            }  else {
                command.Rotate = +(int)A;
                command.Power  = 4;
                Console.Error.WriteLine($"Command 4 - Move towards landingspace <<----");
            }
        } else {
            slowdown_left_to_right = false;
            slowdown_right_to_left = false;
            if (spaceship.HSpeed >= 2 || spaceship.HSpeed <= -2) {
                if (spaceship.HSpeed > 0) {
                    command.Rotate = +(int)A;
                    command.Power  = 4;
                } else {
                   command.Rotate = -(int)A;
                   command.Power  = 4;
                }
                Console.Error.WriteLine($"Command 5 - Reduce speed to zero");
            } else {
                if (spaceship.VSpeed <= -40){
                    command.Power = 4;
                    command.Rotate = 0;
                }
                else {
                    command.Power = 3; 
                    command.Rotate = 0;
                }
                Console.Error.WriteLine($"Command 6 - Land vertically");
            }
        }
        return command;
    }
    
    static void Main(string[] args)
    {
        
        var surface = ReadSurface();
        var landingspaces = FindLandingSpaces(surface);
        var marslander = new Spaceship();
        
        // game loop
        while (true)
        {
            marslander.ReadInputs();
            
            if (landingspaces.Count() > 0){
                Console.Error.WriteLine($"{landingspaces.Count() } LANDINGSPACES FOUND! ");
                var land = landingspaces.First();
                Console.Error.WriteLine($"LAND => {land.Start?.X}, {land.End?.X}");
                var action = CalcNextAction(marslander,land);
                Console.WriteLine($"{action.Rotate} {action.Power} "); //Rotate Power
            } else {
                Console.WriteLine("0 4"); //Rotate Power
            }
        
            
        }
    }
}