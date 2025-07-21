using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        double[] nums = new double[4];
        for (int i = 0; i < 4; i++)
            nums[i] = double.Parse(Console.ReadLine());
        Console.WriteLine(CanMake(nums) ? "true" : "false");
    }

    static bool CanMake(double[] numbers)
    {
        const double eps = 1e-6;
        if (numbers.Length == 1)
            return Math.Abs(numbers[0] - 24) < eps;
        for (int i = 0; i < numbers.Length; i++)
        {
            for (int j = i + 1; j < numbers.Length; j++)
            {
                var rest = numbers.Where((_, idx) => idx != i && idx != j).ToList();
                foreach (var val in Combine(numbers[i], numbers[j]))
                {
                    rest.Add(val);
                    if (CanMake(rest.ToArray()))
                        return true;
                    rest.RemoveAt(rest.Count - 1);
                }
            }
        }
        return false;
    }

    static double[] Combine(double a, double b)
    {
        return new double[]
        {
            a + b,
            a - b,
            b - a,
            a * b,
            Math.Abs(b) > 1e-6 ? a / b : double.NaN,
            Math.Abs(a) > 1e-6 ? b / a : double.NaN
        };
    }
}
