using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    class Comment
    {
        public string Name, Date, Priority;
        public int Likes, PriorityRank, Index;
        public List<Comment> Replies = new();

        public string ToOutputLine() => $"{Name}|{Date}|{Likes}|{Priority}";
    }

    static void Main()
    {
        var n = int.Parse(Console.ReadLine());
        var topComments = new List<Comment>();
        Comment lastTop = null;

        for (var i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            var isReply = line.StartsWith("    ");
            var parts = (isReply ? line[4..] : line).Split('|');

            var com = new Comment
            {
                Name = parts[0],
                Date = parts[1],
                Likes = int.Parse(parts[2]),
                Priority = parts[3],
                Index = i,
                PriorityRank = parts[3].Equals("Pinned", StringComparison.OrdinalIgnoreCase) ? 0
                              : parts[3].Equals("Followed", StringComparison.OrdinalIgnoreCase) ? 1
                              : 2
            };

            if (isReply) lastTop.Replies.Add(com);
            else { topComments.Add(com); lastTop = com; }
        }

        List<Comment> Sort(List<Comment> list) =>
            list.OrderBy(c => c.PriorityRank)
                .ThenByDescending(c => c.Likes)
                .ThenByDescending(c => c.Date)
                .ThenBy(c => c.Index)
                .ToList();

        foreach (var tc in topComments)
            tc.Replies = Sort(tc.Replies);

        topComments = Sort(topComments);

        foreach (var tc in topComments)
        {
            Console.WriteLine(tc.ToOutputLine());
            foreach (var r in tc.Replies)
                Console.WriteLine("    " + r.ToOutputLine());
        }
    }
}
