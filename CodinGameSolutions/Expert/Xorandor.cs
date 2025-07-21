using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

public class Solution
{
    public static void Main(string[] args)
    {
        var firstLine = Console.ReadLine();
        var h = int.Parse(firstLine.Split(' ')[0]);
        var lines = new string[h + 1];
        lines[0] = firstLine;
        for (var i = 0; i < h; i++)
        {
            lines[i + 1] = Console.ReadLine();
        }
        var board = new Board(lines);
        var toggleState = new Dictionary<IToggleable, bool>();
        foreach (var t in board.Toggleables)
        {
            toggleState[t] = t.GetStatus();
        }
        var best = int.MaxValue;
        var bestStr = "";
        var bestToggles = new List<IToggleable>();
        var n = board.Toggleables.Count;
        var limit = 1L << n;
        for (var i = 0L; i < limit; i++)
        {
            var cnt = BitOperations.PopCount((ulong)i);
            if (cnt > best)
            {
                continue;
            }
            foreach (var kv in toggleState)
            {
                kv.Key.SetStatus(kv.Value);
            }
            var toggles = new SortedSet<IToggleable>();
            var sb = new StringBuilder();
            for (var j = 0; j < n; j++)
            {
                if ((i & (1L << j)) != 0)
                {
                    var t = board.Toggleables[j];
                    t.Toggle();
                    toggles.Add(t);
                    if (sb.Length > 0) sb.Append(' ');
                    sb.Append(t.GetName());
                }
            }
            if (board.OutputComponent.GetStatus())
            {
                var s = sb.ToString();
                if (cnt < best || (cnt == best && string.CompareOrdinal(s, bestStr) < 0))
                {
                    best = cnt;
                    bestStr = s;
                    bestToggles = toggles.ToList();
                }
            }
        }
        if (best == int.MaxValue)
        {
            Console.WriteLine("No solution");
        }
        else
        {
            foreach (var t in bestToggles.Where(t => t is Switch))
            {
                Console.WriteLine(t.GetName());
            }
            foreach (var t in bestToggles.Where(t => t is Input))
            {
                Console.WriteLine(t.GetName());
            }
        }
    }
}

enum ComponentSymbol
{
    UNKNOWN = 0,
    NOT = '~',
    AND = '&',
    OR = '|',
    XOR = '+',
    NAND = '^',
    NOR = '-',
    XNOR = '=',
    SWITCH_LEFT = '<',
    SWITCH_RIGHT = '>',
    INPUT_HI = '1',
    INPUT_LO = '0',
    OUTPUT = '@'
}

static class ComponentSymbolExtensions
{
    static readonly Dictionary<char, ComponentSymbol> _map = Enum.GetValues(typeof(ComponentSymbol)).Cast<ComponentSymbol>().ToDictionary(s => (char)s);
    public static ComponentSymbol FromChar(char c)
    {
        return _map.GetValueOrDefault(c, ComponentSymbol.UNKNOWN);
    }
}

class Direction
{
    public static readonly Direction UP = new Direction(0, -1);
    public static readonly Direction LEFT = new Direction(-1, 0);
    public static readonly Direction RIGHT = new Direction(1, 0);
    public static readonly Direction DOWN = new Direction(0, 1);
    public int Dx { get; }
    public int Dy { get; }
    private Direction(int dx, int dy)
    {
        Dx = dx;
        Dy = dy;
    }
}

abstract class Pin
{
    public Component Component { get; }
    public int X { get; }
    public int Y { get; }
    public int No { get; }
    public bool Status { get; internal set; }
    protected Pin(Component component, int no, int x, int y)
    {
        Component = component;
        No = no;
        X = x;
        Y = y;
    }
    public virtual void SetStatus(bool status)
    {
        Status = status;
        Component.Update();
    }
}

class InputPin : Pin
{
    public InputPin(Component component, int no, int x, int y) : base(component, no, x, y) { }
    public override string ToString() => $"InputPin @ ({X},{Y})";
}

class OutputPin : Pin
{
    private HashSet<InputPin> _links = new HashSet<InputPin>();
    public OutputPin(Component component, int no, int x, int y) : base(component, no, x, y) { }
    public void LinkTo(InputPin pin) => _links.Add(pin);
    public override void SetStatus(bool status)
    {
        Status = status;
        foreach (var pin in _links)
        {
            pin.SetStatus(status);
        }
    }
    public override string ToString() => $"OutputPin @ ({X},{Y})";
}

interface IToggleable : IComparable<IToggleable>
{
    string GetName();
    void Toggle();
    void SetStatus(bool status);
    bool GetStatus();
}

abstract class Component
{
    public string Name { get; }
    public int X1 { get; }
    public int X2 { get; }
    public int Y { get; }
    public List<InputPin> Inputs { get; }
    public List<OutputPin> Outputs { get; }
    protected Component(string name, int x1, int x2, int y)
    {
        Name = name;
        X1 = x1;
        X2 = x2;
        Y = y;
        Inputs = new List<InputPin>();
        Outputs = new List<OutputPin>();
    }
    public abstract void CreatePins(char[][] layout);
    public abstract void Update();
    public override int GetHashCode() => Name.GetHashCode();
    public override bool Equals(object o)
    {
        if (this == o) return true;
        if (o is Component other)
        {
            return other.Name.Equals(Name) && other.X1 == X1 && other.X2 == X2 && other.Y == Y;
        }
        return false;
    }
}

abstract class Gate : Component
{
    public OutputPin Output { get; protected set; }
    protected Gate(string name, int x1, int x2, int y) : base(name, x1, x2, y) { }
    public override void CreatePins(char[][] layout)
    {
        var no = 0;
        for (var x = X1; x <= X2; x++)
        {
            if (layout[Y + 1][x] == '|')
            {
                Inputs.Add(new InputPin(this, no++, x, Y + 1));
            }
        }
        no = 0;
        for (var x = X1; x <= X2; x++)
        {
            if (layout[Y - 1][x] == '|')
            {
                Output = new OutputPin(this, no++, x, Y - 1);
                Outputs.Add(Output);
                break;
            }
        }
    }
    public override void Update()
    {
        var v = Op(Inputs);
        Output.SetStatus(v);
    }
    public override string ToString() => $"{Name} Gate at ({(X1 + X2) / 2},{Y})";
    public abstract bool Op(List<InputPin> inputs);
}

class NotGate : Gate
{
    public InputPin Input { get; }
    public NotGate(int x1, int x2, int y) : base("NOT", x1, x2, y)
    {
        Input = new InputPin(this, 0, (x1 + x2) / 2, y + 1);
        Output = new OutputPin(this, 0, (x1 + x2) / 2, y - 1);
        Inputs.Add(Input);
        Outputs.Add(Output);
    }
    public override void CreatePins(char[][] layout) { }
    public override bool Op(List<InputPin> inputs) => !Input.Status;
}

class AndGate : Gate
{
    public AndGate(int x1, int x2, int y) : base("AND", x1, x2, y) { }
    public override bool Op(List<InputPin> inputs) => inputs.All(input => input.Status);
}

class OrGate : Gate
{
    public OrGate(int x1, int x2, int y) : base("OR", x1, x2, y) { }
    public override bool Op(List<InputPin> inputs) => inputs.Any(input => input.Status);
}

class NandGate : Gate
{
    public NandGate(int x1, int x2, int y) : base("NAND", x1, x2, y) { }
    public override bool Op(List<InputPin> inputs) => !inputs.All(input => input.Status);
}

class NorGate : Gate
{
    public NorGate(int x1, int x2, int y) : base("NOR", x1, x2, y) { }
    public override bool Op(List<InputPin> inputs) => !inputs.Any(input => input.Status);
}

class XNorGate : Gate
{
    public XNorGate(int x1, int x2, int y) : base("XNOR", x1, x2, y) { }
    public override bool Op(List<InputPin> inputs) => (inputs.Count(input => input.Status) % 2) == 0;
}

class XorGate : Gate
{
    public XorGate(int x1, int x2, int y) : base("XOR", x1, x2, y) { }
    public override bool Op(List<InputPin> inputs) => (inputs.Count(input => input.Status) % 2) == 1;
}

class Input : Component, IToggleable
{
    public readonly OutputPin Output;
    public Input(int id, int x, int y, bool status) : base("I" + id, x, x, y)
    {
        Output = new OutputPin(this, 0, x, y - 1);
        Outputs.Add(Output);
        Output.Status = status;
    }
    public string GetName() => Name;
    public override void CreatePins(char[][] layout) { }
    public void Toggle() => Output.SetStatus(!Output.Status);
    public bool GetStatus() => Output.Status;
    public void SetStatus(bool status) => Output.SetStatus(status);
    public override void Update() => Output.SetStatus(Output.Status);
    public override int GetHashCode() => Name.GetHashCode();
    public override bool Equals(object obj) => this == obj || obj is Input other && other.Name.Equals(Name);
    public int CompareTo(IToggleable other) => Name.CompareTo(other.GetName());
}

class Output : Component
{
    public bool Status { get; private set; }
    public Output(int x1, int x2, int y) : base("OUT", x1, x2, y) { }
    public override void CreatePins(char[][] layout)
    {
        var no = 0;
        for (var x = X1; x <= X2; x++)
        {
            if (layout[Y + 1][x] == '|')
            {
                Inputs.Add(new InputPin(this, no++, x, Y + 1));
            }
        }
    }
    public override void Update()
    {
        Status = true;
        if (Inputs.Count > 0)
        {
            foreach (var inputPin in Inputs)
            {
                Status &= inputPin.Status;
            }
        }
        else
        {
            Status = false;
        }
    }
    public bool GetStatus() => Status;
}

class Switch : Component, IToggleable
{
    private InputPin _input;
    private bool _left;
    public Switch(int id, int x1, int x2, int y, bool left) : base("K" + id, x1, x2, y)
    {
        _input = new InputPin(this, 0, (x1 + x2) / 2, y + 1);
        Inputs.Add(_input);
        _left = left;
    }
    public string GetName() => Name;
    public void Toggle()
    {
        _left = !_left;
        Update();
    }
    public void SetStatus(bool status)
    {
        _left = status;
        Update();
    }
    public bool GetStatus() => _left;
    public override void CreatePins(char[][] layout)
    {
        Outputs.Add(new OutputPin(this, 0, X1 + 1, Y - 1));
        Outputs.Add(new OutputPin(this, 1, X2 - 1, Y - 1));
    }
    public override void Update()
    {
        if (_left)
        {
            Outputs[0].SetStatus(_input.Status);
            Outputs[1].SetStatus(false);
        }
        else
        {
            Outputs[0].SetStatus(false);
            Outputs[1].SetStatus(_input.Status);
        }
    }
    public override int GetHashCode() => Name.GetHashCode();
    public override bool Equals(object obj) => this == obj || obj is Switch other && other.Name.Equals(Name);
    public int CompareTo(IToggleable other) => Name.CompareTo(other.GetName());
}

class Board
{
    private int _height;
    private int _width;
    private char[][] _layout;
    public Output OutputComponent { get; private set; }
    public List<Input> Inputs { get; }
    public List<Component> Components { get; }
    public List<IToggleable> Toggleables { get; }
    public Board(string[] lines)
    {
        var dims = lines[0].Split(' ');
        _height = int.Parse(dims[0]);
        _width = int.Parse(dims[1]);
        _layout = new char[_height][];
        for (var i = 0; i < _height; i++)
        {
            _layout[i] = lines[i + 1].ToCharArray();
        }
        Inputs = new List<Input>();
        Components = new List<Component>();
        Toggleables = new List<IToggleable>();
        ParseLayout();
        CreatePins();
        CreateLinks();
        InitializeStates();
    }
    private void ParseLayout()
    {
        var inputId = 0;
        var switchId = 0;
        for (var y = 0; y < _height; y++)
        {
            var line = _layout[y];
            int gx1, gx2;
            for (var x = 0; x < line.Length; x++)
            {
                var c = line[x];
                if (c == '.') continue;
                if (c == '[')
                {
                    gx1 = x;
                    while (line[++x] == ' ') { }
                    var g = line[x];
                    while (line[x] != ']') x++;
                    gx2 = x;
                    var component = CreateComponentFromChar(g, gx1, gx2, y, switchId);
                    if (component != null)
                    {
                        AddComponent(component);
                        if (component is Switch) switchId++;
                    }
                }
                else if (c == (char)ComponentSymbol.INPUT_HI || c == (char)ComponentSymbol.INPUT_LO)
                {
                    AddComponent(new Input(++inputId, x, y, c == (char)ComponentSymbol.INPUT_HI));
                }
            }
        }
    }
    private Component CreateComponentFromChar(char g, int gx1, int gx2, int y, int switchId)
    {
        var s = ComponentSymbolExtensions.FromChar(g);
        if (s == ComponentSymbol.NOT) return new NotGate(gx1, gx2, y);
        if (s == ComponentSymbol.AND) return new AndGate(gx1, gx2, y);
        if (s == ComponentSymbol.OR) return new OrGate(gx1, gx2, y);
        if (s == ComponentSymbol.XOR) return new XorGate(gx1, gx2, y);
        if (s == ComponentSymbol.NAND) return new NandGate(gx1, gx2, y);
        if (s == ComponentSymbol.NOR) return new NorGate(gx1, gx2, y);
        if (s == ComponentSymbol.XNOR) return new XNorGate(gx1, gx2, y);
        if (s == ComponentSymbol.OUTPUT) return new Output(gx1, gx2, y);
        if (s == ComponentSymbol.SWITCH_LEFT) return new Switch(switchId + 1, gx1, gx2, y, true);
        if (s == ComponentSymbol.SWITCH_RIGHT) return new Switch(switchId + 1, gx1, gx2, y, false);
        return null;
    }
    private void AddComponent(Component component)
    {
        Components.Add(component);
        if (component is IToggleable t) Toggleables.Add(t);
        if (component is Input i) Inputs.Add(i);
        if (component is Output o) OutputComponent = o;
    }
    private void CreatePins()
    {
        foreach (var c in Components) c.CreatePins(_layout);
    }
    private void CreateLinks()
    {
        foreach (var c in Components)
        {
            foreach (var o in c.Outputs)
            {
                Link(o, o.X, o.Y, Direction.UP);
            }
        }
    }
    private void InitializeStates()
    {
        foreach (var i in Inputs) i.Update();
    }
    private void Link(OutputPin o, int x, int y, Direction d)
    {
        while (true)
        {
            if (y < 0 || y >= _height || x < 0 || x >= _width) return;
            var c = _layout[y][x];
            if (c == '|')
            {
                y--;
                d = Direction.UP;
            }
            else if (c == '-')
            {
                x += d.Dx;
            }
            else if (c == '+')
            {
                var cLeft = x > 0 ? (_layout[y][x - 1] == '+' ? '-' : _layout[y][x - 1]) : '.';
                var cRight = x < _width - 1 ? (_layout[y][x + 1] == '+' ? '-' : _layout[y][x + 1]) : '.';
                var cTop = y > 0 ? (_layout[y - 1][x] == '+' ? '|' : _layout[y - 1][x]) : '.';
                var forks = new List<(int, int, Direction)>();
                if (d != Direction.RIGHT && cLeft == '-') forks.Add((x - 1, y, Direction.LEFT));
                if (d != Direction.LEFT && cRight == '-') forks.Add((x + 1, y, Direction.RIGHT));
                if (d != Direction.DOWN && cTop == '|') forks.Add((x, y - 1, Direction.UP));
                if (forks.Count > 1)
                {
                    foreach (var f in forks) Link(o, f.Item1, f.Item2, f.Item3);
                    return;
                }
                if (forks.Count == 1)
                {
                    x = forks[0].Item1;
                    y = forks[0].Item2;
                    d = forks[0].Item3;
                }
                else return;
            }
            else
            {
                foreach (var comp in Components)
                {
                    foreach (var pin in comp.Inputs)
                    {
                        if (pin.X == x && pin.Y == y + 1)
                        {
                            o.LinkTo(pin);
                            return;
                        }
                    }
                }
                return;
            }
        }
    }
}
