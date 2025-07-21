using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

#region Data Models
public abstract record Requirement;
public record StudentLessonRequirement(string Name, int LessonNumber) : Requirement;
public record TeacherSlotRequirement(int Day, int Hour) : Requirement;
public record InstrumentDayRequirement(string Instrument, int Day) : Requirement;

public abstract record Activity;
public record LessonAssignment(string Name, string Instrument, int LessonNumber, int Day, int Hour) : Activity;
public record SlackTeacherSlot(int Day, int Hour) : Activity;
public record SlackInstrumentDay(string Instrument, int Day) : Activity;

public record Student(string Name, string Inst, int Hours, Dictionary<int, List<int>> Avail);
#endregion

class Solution
{
    static void Main(string[] args)
    {
        var teacherAvailabilityString = Console.ReadLine();
        var numStudents = int.Parse(Console.ReadLine());
        var studentLines = new List<string>();
        for (var i = 0; i < numStudents; i++)
        {
            studentLines.Add(Console.ReadLine());
        }

        var firstStudentParts = studentLines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var hasMultipleHours = firstStudentParts.Length > 2 && int.TryParse(firstStudentParts[2], out _);

        var parser = new ScheduleParser();
        var teacherAvail = parser.ParseTeacherAvailability(teacherAvailabilityString);
        var students = new List<Student>();
        foreach (var sLine in studentLines)
        {
            students.Add(parser.ParseStudent(sLine, hasMultipleHours));
        }

        var troublesomePairs = new HashSet<(string, string)>();
        if (Console.In.Peek() != -1)
        {
            var line = Console.ReadLine();
            if (!string.IsNullOrEmpty(line))
            {
                var numTroublesomePairs = int.Parse(line);
                for (var i = 0; i < numTroublesomePairs; i++)
                {
                    var parts = Console.ReadLine().Split(' ');
                    troublesomePairs.Add((parts[0], parts[1]));
                    troublesomePairs.Add((parts[1], parts[0]));
                }
            }
        }

        var modeler = new ExactCoverModeler();
        var (reqs, acts) = modeler.BuildModel(students, teacherAvail);
        
        var solver = new SchedulingSolver(reqs, acts, troublesomePairs);
        var (bestSolution, bestScore, bestScoreDetails) = solver.Solve();

        var finalSchedule = new Schedule(bestSolution);
        Console.Write(finalSchedule.Print());

        if (hasMultipleHours)
        {
            Console.WriteLine();
            Console.Write(bestScoreDetails);
            Console.WriteLine(bestScore);
        }
    }
}

class SchedulingSolver
{
    private readonly AlgorithmXSolver<Activity> _solver;
    private readonly Scorer _scorer;
    private readonly HashSet<(string, string)> _troublesomePairs;
    private static readonly HashSet<string> LOUD_INST = new() { "Trumpet", "Drums", "Trombone" };
    private static readonly Dictionary<int, int> HourSortOrder = Schedule.HourSortOrder;
    
    public SchedulingSolver(List<Requirement> reqs, Dictionary<Activity, List<Requirement>> acts, HashSet<(string, string)> troublesomePairs)
    {
        _scorer = new Scorer();
        _troublesomePairs = troublesomePairs;

        var reqToIndexMap = new Dictionary<Requirement, int>();
        for (int i = 0; i < reqs.Count; i++)
        {
            reqToIndexMap[reqs[i]] = i;
        }

        int numColumns = reqs.Count;
        int maxNodes = acts.Values.Sum(v => v.Count);
        int maxSolutionDepth = reqs.Count;
        _solver = new AlgorithmXSolver<Activity>(numColumns, maxNodes, maxSolutionDepth);

        foreach (var act in acts)
        {
            var columns = act.Value.Select(req => reqToIndexMap[req]).ToList();
            _solver.AddRow(columns, act.Key);
        }
    }

    public (List<Activity> BestSolution, int BestScore, string BestScoreDetails) Solve()
    {
        int bestScore = -1;
        List<Activity> bestSolution = null;
        string bestScoreDetails = null;

        foreach (var solutionArray in _solver.EnumerateSolutions(IsValidStep))
        {
            var currentSolution = solutionArray.ToList();
            var (score, details) = _scorer.CalculateScore(new Schedule(currentSolution));

            if (score > bestScore)
            {
                bestScore = score;
                bestSolution = currentSolution;
                bestScoreDetails = details;
            }
        }
        
        return (bestSolution, bestScore, bestScoreDetails);
    }
    
    private bool IsValidStep(Activity newAction, Activity[] partialSolution, int depth)
    {
        if (newAction is not LessonAssignment lesson)
        {
            return true;
        }

        if (lesson.LessonNumber > 1)
        {
            bool foundPrevious = false;
            for (int i = 0; i < depth; i++)
            {
                if (partialSolution[i] is LessonAssignment psAction && psAction.Name == lesson.Name && psAction.LessonNumber == lesson.LessonNumber - 1)
                {
                    foundPrevious = true;
                    break;
                }
            }
            if (!foundPrevious) return false;
        }

        var sortOrder = HourSortOrder[lesson.Hour];
        
        for (int i = 0; i < depth; i++)
        {
            if (partialSolution[i] is LessonAssignment psAction && psAction.Day == lesson.Day)
            {
                var eSortOrder = HourSortOrder[psAction.Hour];
                if (Math.Abs(sortOrder - eSortOrder) == 1)
                {
                    if ((lesson.Hour == 11 && psAction.Hour == 1) || (lesson.Hour == 1 && psAction.Hour == 11))
                    {
                        continue;
                    }
                    if (IsConflict(lesson.Name, lesson.Instrument, psAction.Name, psAction.Instrument))
                    {
                        return false;
                    }
                }
            }
        }
        
        return true;
    }

    private bool IsConflict(string name1, string inst1, string name2, string inst2) => (LOUD_INST.Contains(inst1) && LOUD_INST.Contains(inst2)) || _troublesomePairs.Contains((name1, name2));
}

class ExactCoverModeler
{
    public (List<Requirement>, Dictionary<Activity, List<Requirement>>) BuildModel(List<Student> students, Dictionary<int, List<int>> teacherAvail)
    {
        var reqs = new List<Requirement>();
        var acts = new Dictionary<Activity, List<Requirement>>();
        var allInstruments = students.Select(s => s.Inst).Distinct().ToList();
        var teachingDays = teacherAvail.Keys.ToList();
        
        foreach (var s in students) for (var k = 1; k <= s.Hours; k++) reqs.Add(new StudentLessonRequirement(s.Name, k));
        foreach (var kvp in teacherAvail) foreach (var h in kvp.Value) reqs.Add(new TeacherSlotRequirement(kvp.Key, h));
        foreach (var inst in allInstruments) foreach (var day in teachingDays) reqs.Add(new InstrumentDayRequirement(inst, day));
        
        foreach (var s in students)
            for (var k = 1; k <= s.Hours; k++)
                foreach (var day in s.Avail.Keys)
                {
                    if (!teacherAvail.ContainsKey(day)) continue;
                    foreach (var h in s.Avail[day])
                        if (teacherAvail[day].Contains(h))
                        {
                            var assignment = new LessonAssignment(s.Name, s.Inst, k, day, h);
                            var requirements = new List<Requirement>
                            {
                                new StudentLessonRequirement(s.Name, k),
                                new TeacherSlotRequirement(day, h),
                                new InstrumentDayRequirement(s.Inst, day)
                            };
                            acts[assignment] = requirements;
                        }
                }
        
        foreach (var kvp in teacherAvail) foreach (var h in kvp.Value) acts[new SlackTeacherSlot(kvp.Key, h)] = new List<Requirement> { new TeacherSlotRequirement(kvp.Key, h) };
        foreach (var inst in allInstruments) foreach (var day in teachingDays) acts[new SlackInstrumentDay(inst, day)] = new List<Requirement> { new InstrumentDayRequirement(inst, day) };
        
        return (reqs, acts);
    }
}

class ScheduleParser
{
    private static readonly string[] DAYS = { "M", "Tu", "W", "Th", "F" };
    public Dictionary<int, List<int>> ParseTeacherAvailability(string availability) => ParseSchedule(availability.Split(' ', StringSplitOptions.RemoveEmptyEntries)).Where(kv => kv.Value.Count > 0).ToDictionary(kv => kv.Key, kv => kv.Value);
    public Student ParseStudent(string studentData, bool hasMultipleHours)
    {
        var parts = studentData.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var name = parts[0]; var inst = parts[1];
        var hours = hasMultipleHours ? int.Parse(parts[2]) : 1;
        var availabilityTokens = parts.Skip(hasMultipleHours ? 3 : 2).ToArray();
        return new Student(name, inst, hours, ParseSchedule(availabilityTokens));
    }
    private Dictionary<int, List<int>> ParseSchedule(string[] tokens)
    {
        var d = new Dictionary<int, List<int>>(); var day = -1;
        foreach (var t in tokens)
        {
            var di = Array.IndexOf(DAYS, t);
            if (di >= 0) { day = di; if (!d.ContainsKey(day)) d[day] = new List<int>(); }
            else if (day != -1) d[day].Add(int.Parse(t));
        }
        return d;
    }
}

class Schedule
{
    public record Lesson(int Hour, string Name, string Inst);
    private static readonly string[] DAY_NAMES = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
    private static readonly int[] WORK_HOURS = { 8, 9, 10, 11, 1, 2, 3, 4 };
    public static readonly Dictionary<int, int> HourSortOrder = new() { { 8, 0 }, { 9, 1 }, { 10, 2 }, { 11, 3 }, { 1, 4 }, { 2, 5 }, { 3, 6 }, { 4, 7 } };
    private const int COL_WIDTH = 14;
    public readonly List<Lesson>[] DailySortedLessons;
    private readonly Dictionary<(int, int), (string, string)> _lessonMap;

    public Schedule(List<Activity> solutionActions)
    {
        DailySortedLessons = new List<Lesson>[5];
        for (var i = 0; i < 5; i++) DailySortedLessons[i] = new List<Lesson>();
        _lessonMap = new Dictionary<(int, int), (string, string)>();
        if (solutionActions == null) return;
        
        foreach (var lesson in solutionActions.OfType<LessonAssignment>())
        {
            DailySortedLessons[lesson.Day].Add(new Lesson(lesson.Hour, lesson.Name, lesson.Instrument));
            _lessonMap[(lesson.Day, lesson.Hour)] = (lesson.Name, lesson.Instrument);
        }
        for (var i = 0; i < 5; i++) DailySortedLessons[i].Sort((a, b) => HourSortOrder[a.Hour].CompareTo(HourSortOrder[b.Hour]));
    }

    public string Print()
    {
        var grid = new string[10, 6];
        for (var r = 0; r < 10; r++) { grid[r, 0] = "  "; for (var c = 1; c < 6; c++) grid[r, c] = ""; }
        for (var c = 1; c <= 5; c++) grid[0, c] = DAY_NAMES[c - 1];
        for (var c = 1; c <= 5; c++) grid[5, c] = "LUNCH";
        int[] rows = { 1, 2, 3, 4, 6, 7, 8, 9 };
        for (var i = 0; i < WORK_HOURS.Length; i++) grid[rows[i], 0] = WORK_HOURS[i].ToString().PadLeft(2);
        foreach (var kvp in _lessonMap)
        {
            var (day, h) = kvp.Key; var (name, inst) = kvp.Value;
            var hi = Array.IndexOf(WORK_HOURS, h);
            grid[rows[hi], day + 1] = $"{name}/{inst}";
        }
        var sb = new StringBuilder();
        foreach (var r in Enumerable.Range(0, 10))
        {
            var parts = new List<string>();
            foreach (var c in Enumerable.Range(0, 6))
            {
                var cell = grid[r, c];
                if (c == 0) parts.Add(cell);
                else if (string.IsNullOrEmpty(cell)) parts.Add(new string('-', COL_WIDTH));
                else { var pad = COL_WIDTH - cell.Length; parts.Add($"{new string(' ', pad / 2)}{cell}{new string(' ', pad - pad / 2)}"); }
            }
            sb.AppendLine(string.Join(" ", parts).TrimEnd());
        }
        return sb.ToString();
    }
}

class Scorer
{
    private static readonly int[] WORK_SLOTS = { 8, 9, 10, 11, 12, 1, 2, 3, 4 };
    private static readonly int[,] SCHEDULING_POINTS = { { 15, 10 }, { 12, 8 }, { 9, 6 }, { 6, 4 }, { 3, 2 } };
    
    public (int, string) CalculateScore(Schedule schedule)
    {
        var LOUD_INST = new HashSet<string>() { "Trumpet", "Drums", "Trombone" };
        var (f, l, s, a) = (new long[5], new int[5], new int[5], new int[5]);
        for (var d = 0; d < 5; d++)
        {
            var isFree = WORK_SLOTS.ToDictionary(h => h, h => true);
            var dailyLessons = schedule.DailySortedLessons[d];
            foreach (var lesson in dailyLessons) isFree[lesson.Hour] = false;
            var freeStreak = 0;
            foreach (var h in WORK_SLOTS)
            {
                if (isFree[h]) freeStreak++; else { if (freeStreak > 0) f[d] += (freeStreak == 1) ? 2 : (long)Math.Pow(2, freeStreak); freeStreak = 0; }
            }
            if (freeStreak > 0) f[d] += (freeStreak == 1) ? 2 : (long)Math.Pow(2, freeStreak);
            for (var i = 0; i < dailyLessons.Count; i++)
            {
                var lesson = dailyLessons[i]; var isMorning = lesson.Hour >= 8 && lesson.Hour <= 11;
                if (LOUD_INST.Contains(lesson.Inst) && isMorning) l[d] += 50;
                s[d] += SCHEDULING_POINTS[d, isMorning ? 0 : 1];
                if (i > 0 && lesson.Name.CompareTo(dailyLessons[i - 1].Name) > 0) a[d] += 15;
            }
        }
        var details = new StringBuilder();
        details.AppendLine($"{string.Join("+", f)}={f.Sum()}").AppendLine($"{string.Join("+", l)}={l.Sum()}").AppendLine($"{string.Join("+", s)}={s.Sum()}").AppendLine($"{string.Join("+", a)}={a.Sum()}");
        return ((int)(f.Sum() + l.Sum() + s.Sum() + a.Sum()), details.ToString());
    }
}

public class AlgorithmXSolver<T> where T : class
{
    private struct DlxNode
    {
        public int Left, Right, Up, Down;
        public int ColHeader;
        public T RowPayload;
        public int Size;
    }

    private readonly DlxNode[] _nodes;
    private int _nodeCount;
    private readonly T[] _solution;

    public AlgorithmXSolver(int numColumns, int maxNodes, int maxSolutionDepth)
    {
        var poolSize = numColumns + 1 + maxNodes;
        _nodes = new DlxNode[poolSize];
        _solution = new T[maxSolutionDepth];
        for (int i = 0; i <= numColumns; i++)
        {
            _nodes[i] = new DlxNode { 
                Left = i - 1, 
                Right = i + 1, 
                Up = i, 
                Down = i, 
                ColHeader = i, 
                Size = 0 
            };
        }
        _nodes[0].Left = numColumns;
        _nodes[numColumns].Right = 0;
        _nodeCount = numColumns + 1;
    }

    public void AddRow(List<int> columns, T rowPayload)
    {
        if (columns.Count == 0) return;
        int firstNode = -1;
        foreach (int c_idx in columns)
        {
            int headerIdx = c_idx + 1;
            _nodes[headerIdx].Size++;
            _nodes[_nodeCount].RowPayload = rowPayload;
            _nodes[_nodeCount].ColHeader = headerIdx;
            _nodes[_nodeCount].Up = _nodes[headerIdx].Up; 
            _nodes[_nodeCount].Down = headerIdx;
            _nodes[_nodes[headerIdx].Up].Down = _nodeCount; 
            _nodes[headerIdx].Up = _nodeCount;
            if (firstNode == -1) { 
                firstNode = _nodeCount; 
                _nodes[_nodeCount].Left = _nodeCount;
                _nodes[_nodeCount].Right = _nodeCount; 
            }
            else {
                 _nodes[_nodeCount].Left = _nodes[firstNode].Left;
                 _nodes[_nodeCount].Right = firstNode;
                 _nodes[_nodes[firstNode].Left].Right =
                 _nodeCount; _nodes[firstNode].Left = _nodeCount; 
            }
            _nodeCount++;
        }
    }

    public IEnumerable<T[]> EnumerateSolutions(Func<T, T[], int, bool> canSelectRow)
    {
        return Search(0, canSelectRow ?? ((_,__,___) => true));
    }
    
    private IEnumerable<T[]> Search(int k, Func<T, T[], int, bool> canSelectRow)
    {
        if (_nodes[0].Right == 0)
        {
            var result = new T[k]; Array.Copy(_solution, result, k);
            yield return result; yield break;
        }
        int c = ChooseColumn(); Cover(c);
        for (int r_node = _nodes[c].Down; r_node != c; r_node = _nodes[r_node].Down)
        {
            T rowPayload = _nodes[r_node].RowPayload;
            if (!canSelectRow(rowPayload, _solution, k)) { continue; }
            _solution[k] = rowPayload;
            for (int j_node = _nodes[r_node].Right; j_node != r_node; j_node = _nodes[j_node].Right) 
            {
                Cover(_nodes[j_node].ColHeader); 
            }
            foreach(var solution in Search(k + 1, canSelectRow)) { yield return solution; }
            for (int j_node = _nodes[r_node].Left; j_node != r_node; j_node = _nodes[j_node].Left) 
            {
                Uncover(_nodes[j_node].ColHeader); 
            }
        }
        Uncover(c);
    }

    private int ChooseColumn()
    {
        int minSize = int.MaxValue; int bestCol = 0;
        for (int c_header = _nodes[0].Right; c_header != 0; c_header = _nodes[c_header].Right)
        {
            if (_nodes[c_header].Size < minSize) { 
                minSize = _nodes[c_header].Size; 
                bestCol = c_header; 
            }
        }
        return bestCol;
    }

    private void Cover(int c)
    {
        _nodes[_nodes[c].Right].Left = _nodes[c].Left; _nodes[_nodes[c].Left].Right = _nodes[c].Right;
        for (int i = _nodes[c].Down; i != c; i = _nodes[i].Down)
            for (int j = _nodes[i].Right; j != i; j = _nodes[j].Right)
            { 
                _nodes[_nodes[j].Up].Down = _nodes[j].Down;
                _nodes[_nodes[j].Down].Up = _nodes[j].Up;
                _nodes[_nodes[j].ColHeader].Size--; 
            }
    }

    private void Uncover(int c)
    {
        for (int i = _nodes[c].Up; i != c; i = _nodes[i].Up)
            for (int j = _nodes[i].Left; j != i; j = _nodes[j].Left)
            {
                _nodes[_nodes[j].ColHeader].Size++;
                _nodes[_nodes[j].Up].Down = j;
                _nodes[_nodes[j].Down].Up = j; 
            }
            _nodes[_nodes[c].Right].Left = c;
            _nodes[_nodes[c].Left].Right = c;
    }
}
