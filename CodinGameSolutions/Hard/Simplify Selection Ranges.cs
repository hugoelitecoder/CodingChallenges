using System;
using System.Linq;
using System.Text;

class Solution
{
    public static void Main(string[] args)
    {
        var s = Console.ReadLine();
        var nums = s.Trim('[',']').Split(',').Select(x => int.Parse(x.Trim())).OrderBy(x => x).ToArray();
        var n = nums.Length;
        var res = new StringBuilder();
        var i = 0;
        while (i < n)
        {
            var j = i;
            while (j + 1 < n && nums[j + 1] == nums[j] + 1) j++;
            if (j - i >= 2)
            {
                res.Append(nums[i]);
                res.Append("-");
                res.Append(nums[j]);
                i = j + 1;
            }
            else
            {
                res.Append(nums[i]);
                i++;
            }
            if (i < n) res.Append(",");
        }
        Console.WriteLine(res.ToString());
    }
}
