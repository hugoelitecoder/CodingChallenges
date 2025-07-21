using System;
using System.Collections.Generic;
using System.Linq;

namespace DollPairingSolver
{
    class Program
    {
        static Stack<Num> stack = new Stack<Num>();

        static void Main(string[] args)
        {
            if (!int.TryParse(Console.ReadLine(), out int n)) return;

            for (int i = 0; i < n; i++)
            {
                stack.Clear();
                string s = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    int result = Process(s);
                    Console.WriteLine(result);
                }
            }
        }

        private static int Process(string s)
        {
            var heights = new List<int>();

            string[] tokens = s.Split((char[])null);
            Num[] arr = new Num[tokens.Length];

            for (int i = 0; i < tokens.Length; i++)
            {
                if (!int.TryParse(tokens[i], out int a))
                    return -1;
                arr[i] = new Num(a);

                if (stack.Count > 0 && stack.Peek().IsPairWith(arr[i]))
                {
                    heights.Add(stack.Count);
                    Num x = stack.Pop();

                    if (!x.MotherIsBigEnough())
                        return -1;

                    if (stack.Count > 0)
                    {
                        stack.Peek().Contained.Add(x.Value);
                    }
                    else
                    {
                        if (i > 0 && i < tokens.Length - 1)
                            return -1;
                    }
                }
                else
                {
                    stack.Push(arr[i]);
                }
            }

            return stack.Count == 0 ? CountSolidDolls(heights) : -1;
        }

        private static int CountSolidDolls(List<int> list)
        {
            if (list.Count == 0)
                return 0;

            int count = 1;
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i] >= list[i - 1])
                    count++;
            }
            return count;
        }
    }

    class Num
    {
        public int Value { get; }
        public List<int> Contained { get; } = new List<int>();

        public Num(int value)
        {
            Value = value;
        }

        public bool MotherIsBigEnough()
        {
            return Value < Contained.Sum();
        }

        public bool IsPairWith(Num other)
        {
            return this.Value == -other.Value;
        }
    }
}