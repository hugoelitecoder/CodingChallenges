using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;


class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        int n = int.Parse(inputs[0]);
        double c = double.Parse(inputs[1], CultureInfo.InvariantCulture);

        var mcts = new MCTS(c);

        for (int i = 0; i < n; i++)
        {
            var playoutInput = Console.ReadLine().Split(' ');
            string playout = playoutInput[0];
            double score = double.Parse(playoutInput[1], CultureInfo.InvariantCulture);
            mcts.AddPlayout(playout, score);
        }

        Console.WriteLine(mcts.FindBestPath());
    }
}


class MCTSNode
{
    public int Visits { get; private set; }
    public double TotalScore { get; private set; }
    public MCTSNode Parent { get; }
    public SortedDictionary<char, MCTSNode> Children { get; }

    public MCTSNode(MCTSNode parent = null)
    {
        Visits = 0;
        TotalScore = 0.0;
        Parent = parent;
        Children = new SortedDictionary<char, MCTSNode>();
    }

    public void Update(double score)
    {
        Visits++;
        TotalScore += score;
    }
}

class MCTS
{
    private readonly MCTSNode _root;
    private readonly double _explorationConstant;

    public MCTS(double explorationConstant)
    {
        _root = new MCTSNode();
        _explorationConstant = explorationConstant;
    }

    public void AddPlayout(string playout, double score)
    {
        var expandedNode = Expand(_root, playout, score);
        Backpropagate(expandedNode, score);
    }

    private MCTSNode Expand(MCTSNode node, string playout, double score)
    {
        foreach (char move in playout)
        {
            if (node.Children.TryGetValue(move, out var child))
            {
                node = child;
            }
            else
            {
                var newNode = new MCTSNode(node);
                node.Children.Add(move, newNode);
                node = newNode;
                break;
            }
        }
        return node;
    }

    private void Backpropagate(MCTSNode node, double score)
    {
        while (node != null)
        {
            node.Update(score);
            node = node.Parent;
        }
    }

    public string FindBestPath()
    {
        var path = new StringBuilder();
        var node = _root;

        while (node.Children.Count > 0)
        {
            char bestMove = '\0';
            double bestUcb1 = double.MinValue;
            MCTSNode bestChild = null;

            foreach (var kv in node.Children)
            {
                var child = kv.Value;
                double exploitation = child.TotalScore / child.Visits;
                double exploration = _explorationConstant * Math.Sqrt(Math.Log(node.Visits) / child.Visits);
                double ucb1 = exploitation + exploration;

                if (ucb1 > bestUcb1)
                {
                    bestUcb1 = ucb1;
                    bestMove = kv.Key;
                    bestChild = child;
                }
            }

            if (bestChild == null)
                break;

            path.Append(bestMove);
            node = bestChild;
        }
        return path.ToString();
    }

    public MCTSNode Root => _root;
}
