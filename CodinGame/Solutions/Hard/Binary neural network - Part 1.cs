using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Solution
{
    public static void Main(string[] args)
    {
        var config = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
        var inputs = config[0];
        var outputs = config[1];
        var layersCount = config[2];
        var validators = config[3];
        var examples = config[4];
        var iterations = config[5];

        var hiddenNodesLine = Console.ReadLine();
        var numHiddenNodes = string.IsNullOrWhiteSpace(hiddenNodesLine)
            ? Array.Empty<int>()
            : hiddenNodesLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(int.Parse)
                             .ToArray();

        var testInput = new int[validators][];
        for (var j = 0; j < validators; j++)
        {
            testInput[j] = Console.ReadLine().Select(c => c - '0').ToArray();
        }

        var trainData = new Tuple<int[], int[]>[examples];
        for (var j = 0; j < examples; j++)
        {
            var parts = Console.ReadLine().Split(' ');
            var trainIn = parts[0].Select(c => c - '0').ToArray();
            var trainOut = parts[1].Select(c => c - '0').ToArray();
            trainData[j] = Tuple.Create(trainIn, trainOut);
        }

        var network = new NeuralNetwork(inputs, outputs, numHiddenNodes);

        for (var i = 0; i < iterations; i++)
        {
            foreach (var data in trainData)
            {
                network.ForwardPass(data.Item1);
                network.BackPropagate(data.Item2);
            }
        }
        
        var sb = new StringBuilder();
        foreach (var data in testInput)
        {
            network.ForwardPass(data);
            foreach(var o in network.GetResults())
            {
                sb.Append(Math.Round(o));
            }
            Console.WriteLine(sb.ToString());
            sb.Clear();
        }
    }
}

public class NeuralNetwork
{
    private readonly int _inputsCount;
    private readonly int _outputsCount;
    private readonly List<InputNode> _inNodes;
    private readonly List<Node> _outNodes;
    private readonly List<List<Node>> _allProcessingLayers;
    private readonly InputNode _biasNode;

    public NeuralNetwork(int inputs, int outputs, int[] numHiddenNodes)
    {
        _inputsCount = inputs;
        _outputsCount = outputs;
        _outNodes = new List<Node>(outputs);
        for(var i = 0; i < outputs; i++) _outNodes.Add(new Node());

        var hiddenNodes = new List<List<Node>>(numHiddenNodes.Length);
        foreach(var count in numHiddenNodes)
        {
            var layer = new List<Node>(count);
            for(var i = 0; i < count; i++) layer.Add(new Node());
            hiddenNodes.Add(layer);
        }
        
        _inNodes = new List<InputNode>(inputs);
        for(var i = 0; i < inputs; i++) _inNodes.Add(new InputNode());
        
        _biasNode = new InputNode();
        _biasNode.SetInput(1);

        _allProcessingLayers = new List<List<Node>>();
        _allProcessingLayers.AddRange(hiddenNodes);
        _allProcessingLayers.Add(_outNodes);

        var l1 = _inNodes.Cast<INode>().ToList();
        foreach (var l2Nodes in _allProcessingLayers)
        {
            foreach (var o in l2Nodes)
            {
                foreach (var i in l1)
                {
                    o.AddInput(i);
                }
                o.AddInput(_biasNode);
            }
            l1 = l2Nodes.Cast<INode>().ToList();
        }
    }

    public void ForwardPass(int[] inData)
    {
        for (var i = 0; i < _inputsCount; i++)
        {
            _inNodes[i].SetInput(inData[i]);
        }
        foreach (var layer in _allProcessingLayers)
        {
            foreach (var node in layer)
            {
                node.CalcOutput();
            }
        }
    }

    public void BackPropagate(int[] outData)
    {
        for (var i = 0; i < _outputsCount; i++)
        {
            _outNodes[i].SetExpected(outData[i]);
        }
        for (var i = _allProcessingLayers.Count - 1; i >= 0; i--)
        {
            var layer = _allProcessingLayers[i];
            foreach (var node in layer)
            {
                node.CalcNewWeights();
            }
        }
        foreach (var layer in _allProcessingLayers)
        {
            foreach (var node in layer)
            {
                node.ApplyNewWeights();
            }
        }
    }
    
    public IEnumerable<double> GetResults()
    {
        foreach(var n in _outNodes)
        {
            yield return n.GetOutput();
        }
    }
}

public interface INode
{
    double GetOutput();
    void AddOutput(Node node);
}

public class Node : INode
{
    private readonly List<Connection> _inputs = new List<Connection>();
    private readonly List<Node> _outputs = new List<Node>();
    private double _out;
    private double _expected;
    private double _delta;
    private List<double> _newWeights;

    public void AddInput(INode node)
    {
        _inputs.Add(new Connection { Weight = Lcg.NextDouble(), Source = node });
        node.AddOutput(this);
    }

    public void AddOutput(Node node)
    {
        _outputs.Add(node);
    }

    public void CalcOutput()
    {
        var sum = 0.0;
        foreach (var i in _inputs)
        {
            sum += i.Weight * i.Source.GetOutput();
        }
        _out = Activation(sum);
    }

    public double GetOutput()
    {
        return _out;
    }

    public void SetExpected(double val)
    {
        _expected = val;
    }

    public void CalcNewWeights()
    {
        _delta = _out * (1 - _out);
        if (_outputs.Any())
        {
            _delta *= SumWeightedDeltas();
        }
        else
        {
            _delta *= (_out - _expected);
        }
        _newWeights = new List<double>(_inputs.Count);
        foreach(var i in _inputs)
        {
            _newWeights.Add(NewWeight(i));
        }
    }

    public void ApplyNewWeights()
    {
        for (var i = 0; i < _inputs.Count; i++)
        {
            _inputs[i].Weight = _newWeights[i];
        }
    }

    private double SumWeightedDeltas()
    {
        var sum = 0.0;
        foreach (var n in _outputs)
        {
            sum += n.GetWeightedDelta(this);
        }
        return sum;
    }

    private double NewWeight(Connection connection)
    {
        return connection.Weight - 0.5 * _delta * connection.Source.GetOutput();
    }
    
    internal double GetWeightedDelta(INode other)
    {
        var connection = _inputs.First(c => c.Source == other);
        return connection.Weight * _delta;
    }

    private static double Activation(double t)
    {
        return 1.0 / (1.0 + Math.Exp(-t));
    }
}

public class InputNode : INode
{
    private double _val = 0;
    public void SetInput(double val)
    {
        _val = val;
    }
    public double GetOutput()
    {
        return _val;
    }
    public void AddOutput(Node node) { }
}

public class Connection
{
    public double Weight { get; set; }
    public INode Source { get; set; }
}

public static class Lcg
{
    private static uint _seed = 1103527590;
    public static double NextDouble()
    {
        var currentVal = _seed;
        _seed = (uint)((1103515245L * _seed + 12345) & 0x7fffffff);
        return (double)currentVal / 0x7fffffff;
    }
}