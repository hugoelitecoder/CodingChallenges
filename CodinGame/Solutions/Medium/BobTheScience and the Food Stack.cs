using System;
using System.Linq;
using System.Collections.Generic;

namespace PancakeStacks
{
    class Program
    {
        static void Main(string[] args)
        {
            var header = Console.ReadLine().Split();
            int m = int.Parse(header[0]);
            int n = int.Parse(header[1]);
            int d = int.Parse(header[2]);
            var weights = Console.ReadLine().Split().Select(int.Parse).ToArray();

            var assignment = new StackAssigner(n).Assign(weights);

            Console.WriteLine(string.Join(" ", assignment));
        }
    }

    class StackAssigner
    {
        private readonly SortedSet<PancakeStack> stacks;

        public StackAssigner(int stackCount)
        {
            stacks = new SortedSet<PancakeStack>();
            for (int i = 1; i <= stackCount; i++)
                stacks.Add(new PancakeStack(i));
        }

        public int[] Assign(int[] pancakeWeights)
        {
            var indexed = pancakeWeights
                .Select((w, i) => new Pancake(w, i))
                .OrderByDescending(p => p.Weight);

            int[] assignment = new int[pancakeWeights.Length];

            foreach (var pancake in indexed)
            {
                var lightest = stacks.Min;
                stacks.Remove(lightest);
                assignment[pancake.Index] = lightest.Id;
                lightest.Add(pancake.Weight);
                stacks.Add(lightest);
            }

            return assignment;
        }
    }

    class Pancake
    {
        public int Weight { get; }
        public int Index { get; }

        public Pancake(int weight, int index)
        {
            Weight = weight;
            Index = index;
        }
    }

    class PancakeStack : IComparable<PancakeStack>
    {
        public int Id { get; }
        public long TotalWeight { get; private set; }

        public PancakeStack(int id)
        {
            Id = id;
            TotalWeight = 0;
        }

        public void Add(int weight)
        {
            TotalWeight += weight;
        }

        public int CompareTo(PancakeStack other)
        {
            int result = TotalWeight.CompareTo(other.TotalWeight);
            return result != 0 ? result : Id.CompareTo(other.Id);
        }
    }
}
