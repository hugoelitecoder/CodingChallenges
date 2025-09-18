using System;
class Solution
{
    public static void Main()
    {
        var pattern = Console.ReadLine();
        var parts = Console.ReadLine().Split(' ',StringSplitOptions.RemoveEmptyEntries);
        var h = int.Parse(parts[0]);
        var w = int.Parse(parts[1]);
        var pic = new string[h];
        for(var i=0;i<h;i++) pic[i]=Console.ReadLine();
        var m = pattern.Length;
        var H = h+2*(m+1);
        var W = w+2*(m+1);
        for(var r=0;r<H;r++)
        {
            var line = new char[W];
            for(var c=0;c<W;c++)
            {
                var k = r; if(c<k) k=c; var d = H-1-r; if(d<k) k=d; d=W-1-c; if(d<k) k=d;
                if(k<m) line[c]=pattern[k];
                else if(r>=m+1&&r<=m+h&&c>=m+1&&c<=m+w) line[c]=pic[r-(m+1)][c-(m+1)];
                else line[c]=' ';
            }
            Console.WriteLine(new string(line));
        }
    }
}
