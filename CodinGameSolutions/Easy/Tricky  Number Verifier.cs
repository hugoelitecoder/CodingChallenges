using System;
using System.Globalization;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
        for (int i = 0; i < N; i++)
        {
            string s = Console.ReadLine().Trim();
            if (s.Length != 10 || !ulong.TryParse(s, out _) || s[0] == '0')
            {
                Console.WriteLine("INVALID SYNTAX");
                continue;
            }
            int id, day, mon, yy;
            id  = int.Parse(s.Substring(0, 3), CultureInfo.InvariantCulture);
            day = int.Parse(s.Substring(4, 2), CultureInfo.InvariantCulture);
            mon = int.Parse(s.Substring(6, 2), CultureInfo.InvariantCulture);
            yy  = int.Parse(s.Substring(8, 2), CultureInfo.InvariantCulture);
            if (id < 100)
            {
                Console.WriteLine("INVALID SYNTAX");
                continue;
            }
            int year = (yy == 0 ? 2000 : 1900 + yy);
            bool dateOk = true;
            try {
                var dt = new DateTime(year, mon, day);
            }
            catch {
                dateOk = false;
            }
            if (!dateOk)
            {
                Console.WriteLine("INVALID DATE");
                continue;
            }
            string birth = s.Substring(4, 6);
            int[] factors = {3,7,9,5,8,4,2,1,6};
            int correctId = -1, correctX = -1;
            for (int cand = id; cand < 1000; cand++)
            {
                var digs = new int[9];
                digs[0] = cand / 100;
                digs[1] = (cand / 10) % 10;
                digs[2] = cand % 10;
                for (int j = 0; j < 6; j++)
                    digs[3 + j] = birth[j] - '0';
                int sum = 0;
                for (int k = 0; k < 9; k++)
                    sum += digs[k] * factors[k];
                int r = sum % 11;
                if (r == 10) continue;
                correctId = cand;
                correctX  = r;
                break;
            }
            var corrected = correctId.ToString("D3") + correctX + birth;
            if (corrected == s)
                Console.WriteLine("OK");
            else
                Console.WriteLine(corrected);
        }
    }
}
