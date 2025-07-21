using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        for (int i = 0; i < N; i++)
        {
            string cargo = Console.ReadLine();
            Console.WriteLine(MinStacksRequired(cargo));
        }
    }

    static int MinStacksRequired(string cargo)
    {
        var stacks = new List<Stack<char>>();

        foreach (char container in cargo)
        {
            bool placed = false;

            for (int i = 0; i < stacks.Count; i++)
            {
                if (container <= stacks[i].Peek())
                {
                    stacks[i].Push(container);
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                var newStack = new Stack<char>();
                newStack.Push(container);
                stacks.Add(newStack);
            }
        }

        return stacks.Count;
    }
}
