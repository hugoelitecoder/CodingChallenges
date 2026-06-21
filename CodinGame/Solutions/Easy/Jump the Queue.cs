using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

class Solution
{
    public static void Main(string[] args)
    {
        var totalWatch = Stopwatch.StartNew();
        var readWatch = Stopwatch.StartNew();
        var firstLine = ParseInts(Console.ReadLine() ?? "");
        var groupCount = firstLine[0];
        var eventCount = firstLine[1];
        var groups = new int[groupCount][];
        for (var i = 0; i < groupCount; i++)
            groups[i] = ParseInts(Console.ReadLine() ?? "");
        var events = new int[eventCount];
        var eventIndex = 0;
        while (eventIndex < eventCount)
        {
            var values = ParseInts(Console.ReadLine() ?? "");
            for (var i = 0; i < values.Length && eventIndex < eventCount; i++)
                events[eventIndex++] = values[i];
        }
        readWatch.Stop();
        var solveWatch = Stopwatch.StartNew();
        var solver = new LunchQueueSolver();
        QueueStats stats;
        var leavingStudents = solver.Process(groups, events, out stats);
        solveWatch.Stop();
        var outputWatch = Stopwatch.StartNew();
        var output = new StringBuilder();
        for (var i = 0; i < leavingStudents.Count; i++)
            output.AppendLine(leavingStudents[i].ToString());
        Console.Write(output.ToString());
        outputWatch.Stop();
        totalWatch.Stop();
        PrintDebug(groupCount, eventCount, stats, readWatch.Elapsed, solveWatch.Elapsed, outputWatch.Elapsed, totalWatch.Elapsed);
    }

    private static int[] ParseInts(string text)
    {
        var values = new List<int>();
        var index = 0;
        while (index < text.Length)
        {
            while (index < text.Length && text[index] == ' ')
                index++;
            if (index >= text.Length)
                break;
            var sign = 1;
            if (text[index] == '-')
            {
                sign = -1;
                index++;
            }
            var value = 0;
            while (index < text.Length && text[index] >= '0' && text[index] <= '9')
            {
                value = value * 10 + text[index] - '0';
                index++;
            }
            values.Add(value * sign);
        }
        return values.ToArray();
    }

    private static void PrintDebug(int groupCount, int eventCount, QueueStats stats, TimeSpan readTime, TimeSpan solveTime, TimeSpan outputTime, TimeSpan totalTime)
    {
        Console.Error.WriteLine("[DEBUG] ========================================");
        Console.Error.WriteLine("[DEBUG] School Canteen Queue Report");
        Console.Error.WriteLine("[DEBUG] ========================================");
        Console.Error.WriteLine("[DEBUG] Input:");
        Console.Error.WriteLine("[DEBUG]   Read " + groupCount + " friend group(s).");
        Console.Error.WriteLine("[DEBUG]   Listed students with friends: " + stats.GroupStudents + ".");
        Console.Error.WriteLine("[DEBUG]   Read " + eventCount + " queue event(s).");
        Console.Error.WriteLine("[DEBUG]   Reading input took " + FormatTime(readTime) + ".");
        Console.Error.WriteLine("[DEBUG] Simulation:");
        Console.Error.WriteLine("[DEBUG]   Students joining behind an active friend group: " + stats.GroupJoins + ".");
        Console.Error.WriteLine("[DEBUG]   Students joining at the end of the queue: " + stats.TailJoins + ".");
        Console.Error.WriteLine("[DEBUG]   Students leaving after buying lunch: " + stats.Leaves + ".");
        Console.Error.WriteLine("[DEBUG]   Longest queue length: " + stats.MaxQueue + ".");
        Console.Error.WriteLine("[DEBUG]   Simulating the queue took " + FormatTime(solveTime) + ".");
        Console.Error.WriteLine("[DEBUG] Output:");
        Console.Error.WriteLine("[DEBUG]   Wrote " + stats.Leaves + " departing student ID(s).");
        Console.Error.WriteLine("[DEBUG]   Writing output took " + FormatTime(outputTime) + ".");
        Console.Error.WriteLine("[DEBUG] Total execution time: " + FormatTime(totalTime) + ".");
        Console.Error.WriteLine("[DEBUG] ========================================");
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture) + " ms";
    }
}

class LunchQueueSolver
{
    public List<int> Process(int[][] groups, int[] events, out QueueStats stats)
    {
        var groupOfStudent = new Dictionary<int, int>();
        var groupStudentCount = 0;
        for (var group = 0; group < groups.Length; group++)
        {
            var members = groups[group];
            for (var i = 0; i < members.Length; i++)
            {
                groupOfStudent[members[i]] = group;
                groupStudentCount++;
            }
        }
        var queue = new LinkedList<int>();
        var groupTail = new LinkedListNode<int>[groups.Length];
        var leavingStudents = new List<int>();
        var groupJoins = 0;
        var tailJoins = 0;
        var maxQueue = 0;
        for (var i = 0; i < events.Length; i++)
        {
            var currentEvent = events[i];
            if (currentEvent == -1)
            {
                var leavingNode = queue.First;
                var leavingStudent = leavingNode.Value;
                leavingStudents.Add(leavingStudent);
                queue.RemoveFirst();
                int group;
                if (groupOfStudent.TryGetValue(leavingStudent, out group) && groupTail[group] == leavingNode)
                    groupTail[group] = null;
                continue;
            }
            var student = currentEvent;
            int studentGroup;
            if (groupOfStudent.TryGetValue(student, out studentGroup) && groupTail[studentGroup] != null)
            {
                groupTail[studentGroup] = queue.AddAfter(groupTail[studentGroup], student);
                groupJoins++;
            }
            else
            {
                var node = queue.AddLast(student);
                if (groupOfStudent.TryGetValue(student, out studentGroup))
                    groupTail[studentGroup] = node;
                tailJoins++;
            }
            if (queue.Count > maxQueue)
                maxQueue = queue.Count;
        }
        stats = new QueueStats(groupStudentCount, groupJoins, tailJoins, leavingStudents.Count, maxQueue);
        return leavingStudents;
    }
}

struct QueueStats
{
    public int GroupStudents;
    public int GroupJoins;
    public int TailJoins;
    public int Leaves;
    public int MaxQueue;

    public QueueStats(int groupStudents, int groupJoins, int tailJoins, int leaves, int maxQueue)
    {
        GroupStudents = groupStudents;
        GroupJoins = groupJoins;
        TailJoins = tailJoins;
        Leaves = leaves;
        MaxQueue = maxQueue;
    }
}