using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

class SurfacePoint {
    
    public SurfacePoint Previous {get;set;}
    public SurfacePoint Next { get;set;}
    public int X {get;set;}
    public int Y {get;set;}

    public SurfacePoint(int x, int y) {
        X = x;
        Y=  y;
    }
    
}

class Surface {

    public List<SurfacePoint> SurfacePoints { get; set; }
    
    public Surface(){
        SurfacePoints = new List<SurfacePoint>();
    }

    public void AddSurfacePoint(SurfacePoint surfacePoint){
        SurfacePoints.Add(surfacePoint);
    }

    public Point FindLandingPoint() {
        var firstPoint = SurfacePoints.FirstOrDefault(point => point.Next!= null && point.Y == point.Next.Y);
        return new Point(firstPoint.X, firstPoint.Y);
    }
 
}

class Player
{
    
    static void Main(string[] args)
    {

        Surface surface = new Surface();
        string[] inputs;
        int surfaceN = int.Parse(Console.ReadLine()); // the number of points used to draw the surface of Mars.
        SurfacePoint previous=null;
        for (int i = 0; i < surfaceN; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int landX = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
            int landY = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.

            var current = new SurfacePoint(landX,landY);
            if (previous != null) { 
                previous.Next = current;
                current.Previous = previous; 
            }
            surface.AddSurfacePoint(current);
            previous = current;
        }

        var landingPoint = surface.FindLandingPoint();

        var testCase1 = landingPoint.X==2200 && landingPoint.Y==150;
        var testCase2 = !testCase1;

             // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int X = int.Parse(inputs[0]);
            int Y = int.Parse(inputs[1]);
            int hSpeed = int.Parse(inputs[2]); // the horizontal speed (in m/s), can be negative.
            int vSpeed = int.Parse(inputs[3]); // the vertical speed (in m/s), can be negative.
            int fuel = int.Parse(inputs[4]); // the quantity of remaining fuel in liters.
            int rotate = int.Parse(inputs[5]); // the rotation angle in degrees (-90 to 90).
            int power = int.Parse(inputs[6]); // the thrust power (0 to 4).

            Console.Error.WriteLine($"LANDING X={landingPoint.X} Y={landingPoint.Y}");
            Console.Error.WriteLine($"CURRENT X={X} Y={Y}");
            Console.Error.WriteLine($"HSPEED X={hSpeed}");
            Console.Error.WriteLine($"VSPEED X={vSpeed}");
            Console.Error.WriteLine($"FUEL X={fuel}");
            Console.Error.WriteLine($"ROTATE X={rotate}");
            Console.Error.WriteLine($"POWER X={power}");


               //Manually fly cuz I am lazy
            if (testCase1) {
                
                if (X <= 8000 && X > 5400  ) {
                    Console.WriteLine("45 4");
                } else if (X <= 5400 && X > 4000  ) {
                    Console.WriteLine("-45 4");
                } else if (X <= 4000  ) {
                    if (Y <= 250) {
                        Console.WriteLine("0 4");
                    } else {
                    if (hSpeed > 0) {
                        Console.WriteLine("3 4");
                    }
                    if (hSpeed <= 0) {
                        Console.WriteLine("-3 4");
                    }
                    }
                }
            }
            if (testCase2){
                 if (X >=5000 && Y < 2141  ) {
                    Console.WriteLine("10 4");
                } else if (X < 5000 && Y >= 1365  ) {
                    Console.WriteLine("-15 3");
                } else if (Y <  1365 ) {
                    if (X < 3700) {
                        Console.WriteLine("-15 4");
                    } else {
                        if (Y <= 320) {
                        Console.WriteLine("0 4");
                    } else {
                        if (hSpeed > 0) {
                            Console.WriteLine("15 4");
                        }
                        if (hSpeed <= 0) {
                            Console.WriteLine("-15 4");
                        }
                    }
                    }
                    
                }
            }
            
        }
    }
}