using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    struct Digit {
        
        private int L;
        private int H;
        public char[,] Chars;
        public int Value;
        
        public Digit(int value, int l, int h){
            this.Value = value;
            this.L= l;
            this.H= h;
            this.Chars= new char[l,h];
        }
        
        public bool Match(List<string> lines){
            return this.Lines().SequenceEqual(lines);
        }
        
        public List<string> Lines(){
            List<string> result = new List<string>();
            foreach(var y in Enumerable.Range(0,H)) {
             string line = string.Empty;
             foreach(var x in Enumerable.Range(0,L)) {
                  line+= Chars[x,y];
             }
             result.Add(line);
          }
          return result;
        }
        
    }
    
    class Number {
        
        public NumberSystem SystemReference {get; set; }
        public List<Digit> Digits {get; set; } = new List<Digit>();
        
        public Number(NumberSystem systemreference, long value){
            this.SystemReference = systemreference;
            Value(value);
        }
        
        public long Value(){
            double number =0;
            int power = Digits.Count();
            foreach(var digit in Digits){
                number += digit.Value * Math.Pow(20,power-1);
                power--;
            }
            return (long)number;
        }
        
        private List<int> Factorize(long num, int numberbase)
        {
            List<int> result = new List<int>();
            if (num == 0)  { result.Add(0); return result; }
            int rem = 0;
            while (num > 0)
            {
                rem = (int)(num % numberbase);
                num = num / numberbase;
                result.Add(rem);
            }
            result.Reverse();
            return result;
        }
        
        public void Value(long value){
            this.Digits = new List<Digit>();
            var factors = Factorize(value,20);
            foreach(var factor in factors){
                var digit = this.SystemReference.GetDigit(factor);
                if (digit.HasValue)  {
                    this.Digits.Add(digit.Value);
                }
            }
        }
        
        public List<string> Lines(){
            List<string> lines  = new List<string>();
            foreach(var digit in this.Digits){
                lines.AddRange(digit.Lines());
            }
            return lines;
        }
        
         
        public void Print(){
            foreach(var line in this.Lines()){
                Console.WriteLine(line);
            }
        }
    }
    
    class NumberSystem {
        
        public int NumberLength { get; set; }
        public int NumberHeight { get; set ;}
        public List<Digit> Digits = new List<Digit>();
        
        public Number ReadNumber(List<string> numberdata){
            Number n = new Number(this,0);
            n.Digits = new List<Digit>();
            var count = numberdata.Count() / NumberHeight;
            for(int i=0; i< count; i++){
                var offset = i*NumberHeight;
                var data = Enumerable.Range(0,NumberHeight).Select(e=> numberdata[offset+e]).ToList();
                var digit = this.Digits.FirstOrDefault(e=> e.Match(data));
                n.Digits.Add(digit);
            }
            return n;
        }
        
        public Digit? GetDigit(int value) => this.Digits?.FirstOrDefault(e=>e.Value==value);
    }
    
     static NumberSystem ReadNumberSystem(){
        NumberSystem ns = new NumberSystem();
        string[] inputs = Console.ReadLine().Split(' ');
        int L = int.Parse(inputs[0]); ns.NumberLength =L;
        int H = int.Parse(inputs[1]); ns.NumberHeight =H;
        List<string> lines = Enumerable.Range(0,H).Select(e=> Console.ReadLine()).ToList();
        for(int i=0; i <20; i++){
            var digit = new Digit(i,L,H);
            foreach(var y in Enumerable.Range(0,H)) {
                foreach(var x in Enumerable.Range(0,L)) {
                    digit.Chars[x,y] = lines[y][i*L+x];
                }
            }
            ns.Digits.Add(digit);
        }
        return ns;
    }

    static void Main(string[] args)
    {
        NumberSystem numbersystem = ReadNumberSystem();
        Number number1 = numbersystem.ReadNumber(Enumerable.Range(0,int.Parse(Console.ReadLine())).Select(e=> Console.ReadLine()).ToList());
        Number number2 = numbersystem.ReadNumber(Enumerable.Range(0,int.Parse(Console.ReadLine())).Select(e=> Console.ReadLine()).ToList());
        string operation = Console.ReadLine();
        long resultvalue=0;
        long value1 = number1.Value();
        long value2 = number2.Value();
        switch(operation) {
            case "+" : resultvalue = value1 + value2; break;
            case "-" : resultvalue = value1 - value2; break;
            case "/" : resultvalue = value1 / value2; break;
            case "*" : resultvalue = value1 * value2; break;
        }
        Number result = new Number(numbersystem,resultvalue);
        Console.Error.WriteLine($"{value1} {operation} {value2} = {result.Value()}");
        result.Print();
       
    }
}