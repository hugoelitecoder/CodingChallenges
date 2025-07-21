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
    
    public struct DEFIB {
        public string Number;
        public string Name;
        public string Address;
        public string PhoneNumber;
        public double LON;
        public double LAT;
        public double distance;
             
        public void Parse(string fieldsinstring){
            var fields = fieldsinstring.Split(';');
            int c=0;
            foreach(var field in fields){
                if (c==0){
                    this.Number = field;
                } else if (c==1){
                    this.Name= field;
                } else if (c==2){
                    this.Address=field;
                } else if (c==3){
                    this.PhoneNumber=field;
                } else if (c==4){
                    if (field != null){
                        this.LON = double.Parse(field.Replace(',', '.'));
                   }
                } else if(c==5){
                     if (field != null){
                        this.LAT = double.Parse(field.Replace(',', '.'));
                     }
                }
                c++;
            }
        }
        
        public double GetDistance(string LON,string LAT){
            double LON1 = double.Parse(LON)*(Math.PI / 180);
            double LAT1 = double.Parse(LAT)*(Math.PI / 180.0);
            double LON2 = this.LON*(Math.PI / 180);
            double LAT2 = this.LAT*(Math.PI / 180);
            return GetDistanceF(LON1,LAT1,LON2,LAT2);
        }
        
    }
    
    static double GetDistanceF(double LON1,double LAT1, double LON2, double LAT2){
        double result = 0.0;
        double x = (LON2 - LON1) * Math.Cos((LAT1+LAT2)/2);
        double y = (LAT2 - LAT1);
        result = Math.Sqrt ((x*x)+(y*y)) * (double)6371.0;
        return result;
    }
    
    static void Main(string[] args)
    {
        string LON = Console.ReadLine().Replace(',','.');
        string LAT = Console.ReadLine().Replace(',','.');
        int N = int.Parse(Console.ReadLine());
        List<DEFIB> defibs = new List<DEFIB>();
        for (int i = 0; i < N; i++)
        {
            DEFIB defib = new DEFIB();
            defib.Parse(Console.ReadLine());
            defibs.Add(defib);
        }
        Console.WriteLine(defibs.OrderBy(e=>e.GetDistance(LON,LAT)).FirstOrDefault().Name);
    }
}