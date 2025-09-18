using System;

class Solution
{
    static long Dot((long x, long y) u, (long x, long y) v) => u.x * v.x + u.y * v.y;
    static long Cross((long x, long y) u, (long x, long y) v) => u.x * v.y - u.y * v.x;
    static long Len2((long x, long y) u) => u.x * u.x + u.y * u.y;

    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        for (int i = 0; i < n; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            string name = inputs[0] + inputs[3] + inputs[6] + inputs[9];

            var A = (x: long.Parse(inputs[1]), y: long.Parse(inputs[2]));
            var B = (x: long.Parse(inputs[4]), y: long.Parse(inputs[5]));
            var C = (x: long.Parse(inputs[7]), y: long.Parse(inputs[8]));
            var D = (x: long.Parse(inputs[10]), y: long.Parse(inputs[11]));

            // edge vectors
            var AB = (x: B.x - A.x, y: B.y - A.y);
            var BC = (x: C.x - B.x, y: C.y - B.y);
            var CD = (x: D.x - C.x, y: D.y - C.y);
            var DA = (x: A.x - D.x, y: A.y - D.y);

            // side lengths squared
            long ab2 = Len2(AB), bc2 = Len2(BC), cd2 = Len2(CD), da2 = Len2(DA);

            // parallelogram if AB ∥ CD and BC ∥ DA
            bool parallelOpp = (Cross(AB, CD) == 0) && (Cross(BC, DA) == 0);

            // right angles: AB ⟂ BC and BC ⟂ CD
            bool rightAngles = (Dot(AB, BC) == 0) && (Dot(BC, CD) == 0);

            // all sides equal
            bool allEqual = (ab2 == bc2) && (bc2 == cd2) && (cd2 == da2);

            string kind;
            if (parallelOpp && rightAngles && allEqual)
                kind = "square";
            else if (parallelOpp && rightAngles)
                kind = "rectangle";
            else if (parallelOpp && allEqual)
                kind = "rhombus";
            else if (parallelOpp)
                kind = "parallelogram";
            else
                kind = "quadrilateral";

            Console.WriteLine($"{name} is a {kind}.");
        }
    }
}
