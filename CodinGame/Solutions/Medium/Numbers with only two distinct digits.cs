using System;
using System.Numerics;

class Solution {
    static void Main(string[] args) {
        int n = int.Parse(Console.ReadLine());
        string[] inputs = Console.ReadLine().Split(' ');
        int a = int.Parse(inputs[0]);
        int b = int.Parse(inputs[1]);
        int evenDigit = a % 2 == 0 ? a : b;
        int oddDigit = a % 2 != 0 ? a : b;
        BigInteger current = evenDigit;
        BigInteger powerOf2 = 2;
        BigInteger powerOf10 = 10;
        for (int k = 1; k < n; k++) {
            int parity = (int)((current / powerOf2) % 2);
            int d = parity == 0 ? evenDigit : oddDigit;
            current += d * powerOf10;
            powerOf2 *= 2;
            powerOf10 *= 10;
        }
        Console.WriteLine(current.ToString());
    }
}