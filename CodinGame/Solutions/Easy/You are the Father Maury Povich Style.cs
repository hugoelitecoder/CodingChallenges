using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var motherLine = Console.ReadLine().Replace("Mother ", "").Trim();
        var childLine = Console.ReadLine().Replace("Child ", "").Trim();
        int numFathers = int.Parse(Console.ReadLine());

        // Safely parse mother and child
        var mSplit = motherLine.Split(':', 2);
        var cSplit = childLine.Split(':', 2);
        string motherName = mSplit[0].Trim();
        string[] motherChroms = mSplit[1].Trim().Split();
        string childName = cSplit[0].Trim();
        string[] childChroms = cSplit[1].Trim().Split();

        for (int i = 0; i < numFathers; i++)
        {
            string line = Console.ReadLine();
            var split = line.Split(':', 2);
            if (split.Length < 2) continue;

            string fatherName = split[0].Trim();
            string[] fatherChroms = split[1].Trim().Split();

            bool isFather = true;
            for (int j = 0; j < motherChroms.Length; j++)
            {
                var m = motherChroms[j];
                var c = childChroms[j];
                var f = fatherChroms[j];

                var cSet = c.ToHashSet();
                var mSet = m.ToHashSet();
                var fSet = f.ToHashSet();

                bool valid = false;
                foreach (var a in cSet)
                {
                    foreach (var b in cSet)
                    {
                        if ((mSet.Contains(a) && fSet.Contains(b)) || (mSet.Contains(b) && fSet.Contains(a)))
                        {
                            valid = true;
                            break;
                        }
                    }
                    if (valid) break;
                }

                if (!valid)
                {
                    isFather = false;
                    break;
                }
            }

            if (isFather)
            {
                Console.WriteLine($"{fatherName}, you are the father!");
                return;
            }
        }
    }
}
