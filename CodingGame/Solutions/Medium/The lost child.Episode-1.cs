using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

class Player {
    
    static readonly (int dx,int dy)[] Dirs = {(1,0),(-1,0),(0,1),(0,-1)};
    static void Main() {
        
        var raw = Enumerable.Range(0,10).Select(_ => Console.ReadLine()).ToArray();
        var grid = new int[12,12];
        for (int i = 0; i < 144; i++) grid[i/12, i%12] = -1;
        
        Point start = default, endp = default;
        raw.SelectMany((l,y) => l.Select((c,x) => {
            var p = new Point(x+1,y+1);
            grid[p.X,p.Y] = c=='#' ? -1 : 0;
            if (c=='C') start = p;
            if (c=='M') endp = p;
            return 0;
        })).ToList();

        var q = new Queue<Point>();
        q.Enqueue(endp);
        while (q.Count>0) {
            var p = q.Dequeue();
            foreach (var (dx,dy) in Dirs) {
                var n = new Point(p.X+dx, p.Y+dy);
                if (grid[n.X,n.Y]!=0) continue;
                grid[n.X,n.Y] = grid[p.X,p.Y] + 1;
                q.Enqueue(n);
            }
        }
        Console.WriteLine(grid[start.X,start.Y]*10 + "km");
    }
}
