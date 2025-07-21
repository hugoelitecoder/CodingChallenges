using System;
using System.Globalization;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
        for (int i = 0; i < N; i++)
        {
            var tok = Console.ReadLine().Split(' ');
            string L1 = tok[0], L2 = tok[3], L3 = tok[6];
            double x1 = double.Parse(tok[1], CultureInfo.InvariantCulture),
                   y1 = double.Parse(tok[2], CultureInfo.InvariantCulture),
                   x2 = double.Parse(tok[4], CultureInfo.InvariantCulture),
                   y2 = double.Parse(tok[5], CultureInfo.InvariantCulture),
                   x3 = double.Parse(tok[7], CultureInfo.InvariantCulture),
                   y3 = double.Parse(tok[8], CultureInfo.InvariantCulture);
            
            double d12 = Dist2(x1,y1,x2,y2),
                   d23 = Dist2(x2,y2,x3,y3),
                   d31 = Dist2(x3,y3,x1,y1);
            
            string sideArt, sideNat;
            if (!Equal(d12,d23) && !Equal(d23,d31) && !Equal(d31,d12))
            {
                sideArt = "a"; sideNat = "scalene";
            }
            else
            {
                if (Equal(d12,d31))
                    { sideArt="an"; sideNat = $"isosceles in {L1}"; }
                else if (Equal(d12,d23))
                    { sideArt="an"; sideNat = $"isosceles in {L2}"; }
                else 
                    { sideArt="an"; sideNat = $"isosceles in {L3}"; }
            }
            var v12 = (x2-x1,y2-y1); var v13 = (x3-x1,y3-y1);
            double dot1 = v12.Item1*v13.Item1 + v12.Item2*v13.Item2;
            var v21 = (x1-x2,y1-y2); var v23 = (x3-x2,y3-y2);
            double dot2 = v21.Item1*v23.Item1 + v21.Item2*v23.Item2;
            var v31 = (x1-x3,y1-y3); var v32 = (x2-x3,y2-y3);
            double dot3 = v31.Item1*v32.Item1 + v31.Item2*v32.Item2;

            string angleArt, angleNat;
            if (Equal(dot1,0) || Equal(dot2,0) || Equal(dot3,0))
            {
                string V = Equal(dot1,0)? L1 : Equal(dot2,0)? L2 : L3;
                angleArt = "a"; angleNat = $"right in {V}";
            }
            else if (dot1<0 || dot2<0 || dot3<0)
            {
                string V; double dot, a2, b2, c2, ux,uy,vx,vy;
                if (dot1<0) { V=L1; dot=dot1; ux=v12.Item1; uy=v12.Item2; vx=v13.Item1; vy=v13.Item2; }
                else if (dot2<0) { V=L2; dot=dot2; ux=v21.Item1; uy=v21.Item2; vx=v23.Item1; vy=v23.Item2; }
                else { V=L3; dot=dot3; ux=v31.Item1; uy=v31.Item2; vx=v32.Item1; vy=v32.Item2; }
                double magu = Math.Sqrt(ux*ux+uy*uy), magv = Math.Sqrt(vx*vx+vy*vy);
                double cos = dot/(magu*magv);
                double deg = Math.Round(Math.Acos(cos)*180/Math.PI, 0, MidpointRounding.AwayFromZero);
                angleArt = "an"; angleNat = $"obtuse in {V} ({deg:0}°)";
            }
            else
            {
                angleArt = "an"; angleNat = "acute";
            }

            Console.WriteLine(
                $"{L1}{L2}{L3} is {sideArt} {sideNat} and {angleArt} {angleNat} triangle."
            );
        }
    }

    static double Dist2(double x1,double y1,double x2,double y2)
        => (x1-x2)*(x1-x2)+(y1-y2)*(y1-y2);

    static bool Equal(double a,double b)
        => Math.Abs(a-b) < 1e-9;
}
