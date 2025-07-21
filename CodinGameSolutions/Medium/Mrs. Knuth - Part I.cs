using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        var scheduler = new LessonScheduler(teacherAvailabilityString, studentLines);
        var finalSchedule = scheduler.GenerateSchedule();
        
        Console.Write(finalSchedule.Print());
    }
}

public record Lesson(string Name, string Inst, int Day, int Hour);
public class LessonScheduler
{
    private readonly Teacher _teacher;
    private readonly List<Student> _students;

    private readonly Dictionary<object, int> _reqMap = new Dictionary<object, int>();
    
    public LessonScheduler(string teacherAvailability, List<string> studentLines)
    {
        var parser = new ScheduleParser();
        _teacher = parser.ParseTeacherAvailability(teacherAvailability);
        _students = studentLines.Select(parser.ParseStudent).ToList();
    }

    public Schedule GenerateSchedule()
    {
        var (reqs, choices) = BuildRequirementsAndChoices();
        int numColumns = reqs.Count;
        int maxNodes = choices.Count * 3;
        int maxSolutionDepth = _students.Count;

        var solver = new AlgorithmXSolver<Lesson>(numColumns, maxNodes, maxSolutionDepth);

        foreach (var choice in choices)
        {
            solver.AddRow(GetColumnIndices(choice), choice);
        }
        var solution = solver.EnumerateSolutions().FirstOrDefault();
        
        return new Schedule(solution?.ToList());
    }

    private (List<object> reqs, List<Lesson> choices) BuildRequirementsAndChoices()
    {
        var reqs = new List<object>();
        var choices = new List<Lesson>();

        void AddReq(object req)
        {
            if (!_reqMap.ContainsKey(req))
            {
                _reqMap[req] = reqs.Count;
                reqs.Add(req);
            }
        }
        foreach (var day in _teacher.Avail.Keys)
        {
            foreach (var hour in _teacher.Avail[day]) AddReq(("slot filled", day, hour));
        }
        foreach (var s in _students)
        {
            AddReq(("student scheduled", s.Name));
        }
        var allInstruments = _students.Select(s => s.Inst).ToHashSet();
        foreach (var day in _teacher.Avail.Keys)
        {
            foreach (var inst in allInstruments) AddReq(("instrument on day", day, inst));
        }
        
        foreach (var student in _students)
        {
            foreach (var day in student.Avail.Keys)
            {
                if (!_teacher.Avail.ContainsKey(day)) continue;
                foreach (var hour in student.Avail[day])
                {
                    if (!_teacher.Avail[day].Contains(hour)) continue;
                    choices.Add(new Lesson(student.Name, student.Inst, day, hour));
                }
            }
        }
        return (reqs, choices);
    }
    
    private List<int> GetColumnIndices(Lesson lesson)
    {
        var indices = new List<int>
        {
            _reqMap[("slot filled", lesson.Day, lesson.Hour)],
            _reqMap[("student scheduled", lesson.Name)],
            _reqMap[("instrument on day", lesson.Day, lesson.Inst)]
        };
        return indices;
    }
}
public record Teacher(Dictionary<int, List<int>> Avail);
public record Student(string Name, string Inst, Dictionary<int, List<int>> Avail);

public class ScheduleParser
{
    private static readonly string[] DAYS = { "M", "Tu", "W", "Th", "F" };

    public Teacher ParseTeacherAvailability(string availability)
    {
        var avail = ParseSchedule(availability.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(kv => kv.Value.Count > 0)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
        return new Teacher(avail);
    }
    
    public Student ParseStudent(string studentData)
    {
        var parts = studentData.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var name = parts[0];
        var inst = parts[1];
        var avail = ParseSchedule(parts.Skip(2).ToArray());
        return new Student(name, inst, avail);
    }

    private Dictionary<int, List<int>> ParseSchedule(string[] tokens)
    {
        var d = new Dictionary<int, List<int>>();
        var day = -1;
        foreach (var t in tokens)
        {
            var di = Array.IndexOf(DAYS, t);
            if (di >= 0)
            {
                day = di;
                if (!d.ContainsKey(day)) d[day] = new List<int>();
            }
            else if (day != -1 && int.TryParse(t, out var hour))
            {
                d[day].Add(hour);
            }
        }
        return d;
    }
}
public class Schedule
{
    private static readonly string[] DAY_NAMES = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
    private static readonly int[] WORK_HOURS = { 8, 9, 10, 11, 1, 2, 3, 4 };
    private const int COL_WIDTH = 14;
    public readonly Dictionary<(int, int), Lesson> LessonsByTime;

    public Schedule(List<Lesson> solution)
    {
        LessonsByTime = new Dictionary<(int, int), Lesson>();
        if (solution == null) return;
        foreach (var lesson in solution)
        {
            LessonsByTime[(lesson.Day, lesson.Hour)] = lesson;
        }
    }

    public string Print()
    {
        var grid = new string[10, 6];
        for (var r = 0; r < 10; r++)
        {
            grid[r, 0] = "  ";
            for (var c = 1; c < 6; c++) grid[r, c] = "";
        }

        for (var c = 1; c <= 5; c++) grid[0, c] = DAY_NAMES[c - 1];
        for (var c = 1; c <= 5; c++) grid[5, c] = "LUNCH";
        int[] rows = { 1, 2, 3, 4, 6, 7, 8, 9 };
        for (var i = 0; i < WORK_HOURS.Length; i++) grid[rows[i], 0] = WORK_HOURS[i].ToString().PadLeft(2);
        
        foreach(var lesson in LessonsByTime.Values)
        {
            var hi = Array.IndexOf(WORK_HOURS, lesson.Hour);
            if (hi == -1) continue;
            var row = rows[hi];
            var col = lesson.Day + 1;
            grid[row,col] = $"{lesson.Name}/{lesson.Inst}";
        }

        var sb = new StringBuilder();
        foreach (var r in Enumerable.Range(0, 10))
        {
            var parts = new List<string>();
            foreach (var c in Enumerable.Range(0, 6))
            {
                var cell = grid[r, c];
                if (c == 0) parts.Add(cell);
                else if (string.IsNullOrEmpty(cell))
                    parts.Add(new string('-', COL_WIDTH));
                else
                {
                    var pad = COL_WIDTH - cell.Length;
                    var left = pad / 2;
                    var right = pad - left;
                    parts.Add(new string(' ', left) + cell + new string(' ', right));
                }
            }
            sb.AppendLine(string.Join(" ", parts).TrimEnd());
        }
        return sb.ToString();
    }
}

public class AlgorithmXSolver<T>
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
    private int _solutionDepth;

    public AlgorithmXSolver(int numColumns, int maxNodes, int maxSolutionDepth)
    {
        var poolSize = numColumns + 1 + maxNodes;
        _nodes = new DlxNode[poolSize];
        _solution = new T[maxSolutionDepth];
        for (int i = 0; i <= numColumns; i++)
        {
            _nodes[i] = new DlxNode { Left = i - 1, Right = i + 1, Up = i, Down = i, ColHeader = i, Size = 0 };
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
            if (firstNode == -1)
            {
                firstNode = _nodeCount;
                _nodes[_nodeCount].Left = _nodeCount;
                _nodes[_nodeCount].Right = _nodeCount;
            }
            else
            {
                _nodes[_nodeCount].Left = _nodes[firstNode].Left;
                _nodes[_nodeCount].Right = firstNode;
                _nodes[_nodes[firstNode].Left].Right = _nodeCount;
                _nodes[firstNode].Left = _nodeCount;
            }
            _nodeCount++;
        }
    }
    
    public IEnumerable<T[]> EnumerateSolutions()
    {
        if (_nodes[0].Right == 0)
        {
            var result = new T[_solutionDepth];
            Array.Copy(_solution, result, _solutionDepth);
            yield return result;
            yield break;
        }

        int c = ChooseColumn();
        Cover(c);

        for (int r_node = _nodes[c].Down; r_node != c; r_node = _nodes[r_node].Down)
        {
            _solution[_solutionDepth] = _nodes[r_node].RowPayload;
            _solutionDepth++;
            for (int j_node = _nodes[r_node].Right; j_node != r_node; j_node = _nodes[j_node].Right)
            {
                Cover(_nodes[j_node].ColHeader);
            }

            foreach (var sol in EnumerateSolutions())
            {
                yield return sol;
            }
            
            _solutionDepth--;
            for (int j_node = _nodes[r_node].Left; j_node != r_node; j_node = _nodes[j_node].Left)
            {
                Uncover(_nodes[j_node].ColHeader);
            }
        }
        Uncover(c);
    }

    private int ChooseColumn()
    {
        int minSize = int.MaxValue;
        int bestCol = 0;
        for (int c_header = _nodes[0].Right; c_header != 0; c_header = _nodes[c_header].Right)
        {
            if (_nodes[c_header].Size < minSize)
            {
                minSize = _nodes[c_header].Size;
                bestCol = c_header;
            }
        }
        return bestCol;
    }

    private void Cover(int c)
    {
        _nodes[_nodes[c].Right].Left = _nodes[c].Left;
        _nodes[_nodes[c].Left].Right = _nodes[c].Right;
        for (int i = _nodes[c].Down; i != c; i = _nodes[i].Down)
        {
            for (int j = _nodes[i].Right; j != i; j = _nodes[j].Right)
            {
                _nodes[_nodes[j].Up].Down = _nodes[j].Down;
                _nodes[_nodes[j].Down].Up = _nodes[j].Up;
                _nodes[_nodes[j].ColHeader].Size--;
            }
        }
    }

    private void Uncover(int c)
    {
        for (int i = _nodes[c].Up; i != c; i = _nodes[i].Up)
        {
            for (int j = _nodes[i].Left; j != i; j = _nodes[j].Left)
            {
                _nodes[_nodes[j].ColHeader].Size++;
                _nodes[_nodes[j].Up].Down = j;
                _nodes[_nodes[j].Down].Up = j;
            }
        }
        _nodes[_nodes[c].Right].Left = c;
        _nodes[_nodes[c].Left].Right = c;
    }
}