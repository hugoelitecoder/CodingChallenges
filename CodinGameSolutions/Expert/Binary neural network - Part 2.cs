using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var inputs = Console.ReadLine().Split();
        int tests = int.Parse(inputs[0]), trainingSets = int.Parse(inputs[1]);
        var testInputs = new double[tests][];
        for (int i = 0; i < tests; i++)
            testInputs[i] = Console.ReadLine().Select(c => (double)(c - '0')).ToArray();
        var trainInputs = new double[trainingSets][];
        var trainOutputs = new double[trainingSets][];
        for (int i = 0; i < trainingSets; i++)
        {
            var line = Console.ReadLine().Split();
            trainInputs[i] = line[0].Select(c => (double)(c - '0')).ToArray();
            trainOutputs[i] = line[1].Select(c => (double)(c - '0')).ToArray();
        }

        var nn = NeuralNetwork.BuildFor(trainInputs, trainOutputs);

        nn.Train(trainInputs, trainOutputs, nn.Epochs, nn.LearningRate);

        foreach (var test in testInputs)
            Console.WriteLine(string.Concat(nn.Predict(test).Select(x => Math.Round(x))));
    }
}

class NeuralNetwork
{
    List<Layer> _layers;
    public int Epochs { get; private set; }
    public double LearningRate { get; private set; }
    NeuralNetwork(List<Layer> layers, int epochs, double lr)
    {
        _layers = layers;
        Epochs = epochs;
        LearningRate = lr;
    }

    public static NeuralNetwork BuildFor(double[][] trainInputs, double[][] trainOutputs)
    {
        int inputSize = trainInputs[0].Length;
        int outputSize = trainOutputs[0].Length;

        var layers = new List<Layer>
        {
            new Layer(inputSize, inputSize*2, Activation.Sigmoid, new Random(1337)),
            new Layer(inputSize*2, outputSize*2, Activation.Sigmoid, new Random(1338)),
            new Layer(outputSize*2, outputSize, Activation.Sigmoid, new Random(1339))
        };
        return new NeuralNetwork(layers, 250, 0.5);
    }

    public void Train(double[][] x, double[][] y, int epochs, double lr)
    {
        int n = x.Length;
        var outputs = new double[_layers.Count][];
        for (int e = 0; e < epochs; e++)
        {
            for (int i = 0; i < n; i++)
            {
                var inp = x[i];
                for (int l = 0; l < _layers.Count; l++)
                {
                    inp = _layers[l].Forward(inp);
                    outputs[l] = inp;
                }
                var grad = new double[outputs[^1].Length];
                for (int j = 0; j < grad.Length; j++)
                    grad[j] = outputs[^1][j] - y[i][j];
                for (int li = _layers.Count - 1; li >= 0; li--)
                {
                    var prev = li == 0 ? x[i] : outputs[li - 1];
                    grad = _layers[li].Backward(prev, grad, lr);
                }
            }
        }
    }

    public double[] Predict(double[] x)
    {
        var inp = x;
        foreach (var layer in _layers)
            inp = layer.Forward(inp);
        return inp;
    }
}

class Layer
{
    int inputSize, outputSize;
    double[][] weights;
    double[] bias;
    Func<double, double> act;
    Func<double, double> dact;
    double[] lastZ, lastOutput;

    public Layer(int input, int output, Activation actFn, Random rnd)
    {
        inputSize = input;
        outputSize = output;
        weights = new double[input][];
        bias = new double[output];
        double wRange = Math.Sqrt(2.0 / input);
        for (int i = 0; i < input; i++)
        {
            weights[i] = new double[output];
            for (int j = 0; j < output; j++)
                weights[i][j] = (rnd.NextDouble() * 2 - 1) * wRange;
        }
        for (int i = 0; i < output; i++)
            bias[i] = (rnd.NextDouble() * 2 - 1) * 0.1;
        (act, dact) = ActivationMethod.Get(actFn);
        lastZ = new double[output];
        lastOutput = new double[output];
    }

    public double[] Forward(double[] input)
    {
        for (int j = 0; j < outputSize; j++)
        {
            lastZ[j] = bias[j];
            for (int i = 0; i < inputSize; i++)
                lastZ[j] += input[i] * weights[i][j];
            lastOutput[j] = act(lastZ[j]);
        }
        return lastOutput;
    }

    public double[] Backward(double[] prevInput, double[] gradOutput, double lr)
    {
        var gradInput = new double[inputSize];
        var delta = new double[outputSize];
        for (int j = 0; j < outputSize; j++)
            delta[j] = gradOutput[j] * dact(lastZ[j]);
        for (int i = 0; i < inputSize; i++)
        {
            double sum = 0;
            for (int j = 0; j < outputSize; j++)
            {
                sum += weights[i][j] * delta[j];
                weights[i][j] -= lr * prevInput[i] * delta[j];
            }
            gradInput[i] = sum;
        }
        for (int j = 0; j < outputSize; j++)
            bias[j] -= lr * delta[j];
        return gradInput;
    }
}

enum Activation { Sigmoid, Tanh, ReLU, LeakyReLU, ELU, Softplus, Swish, Linear }

static class ActivationMethod
{
    public static (Func<double, double>, Func<double, double>) Get(Activation a) =>
        a == Activation.Sigmoid ? (Sigmoid, SigmoidD) :
        a == Activation.Tanh ? (Tanh, TanhD) :
        a == Activation.ReLU ? (ReLU, ReLUD) :
        a == Activation.LeakyReLU ? (LeakyReLU, LeakyReLUD) :
        a == Activation.ELU ? (ELU, ELUD) :
        a == Activation.Softplus ? (Softplus, SoftplusD) :
        a == Activation.Swish ? (Swish, SwishD) :
        (Linear, LinearD);

    static double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));
    static double SigmoidD(double x)
    {
        var s = Sigmoid(x);
        return s * (1 - s);
    }
    static double Tanh(double x) => Math.Tanh(x);
    static double TanhD(double x)
    {
        var t = Math.Tanh(x);
        return 1 - t * t;
    }
    static double ReLU(double x) => x > 0 ? x : 0;
    static double ReLUD(double x) => x > 0 ? 1 : 0;
    static double LeakyReLU(double x) => x > 0 ? x : 0.01 * x;
    static double LeakyReLUD(double x) => x > 0 ? 1 : 0.01;
    static double ELU(double x) => x > 0 ? x : Math.Exp(x) - 1;
    static double ELUD(double x) => x > 0 ? 1 : Math.Exp(x);
    static double Softplus(double x) => Math.Log(1 + Math.Exp(x));
    static double SoftplusD(double x) => 1.0 / (1.0 + Math.Exp(-x));
    static double Swish(double x) => x * Sigmoid(x);
    static double SwishD(double x)
    {
        var s = Sigmoid(x);
        return s + x * s * (1 - s);
    }
    static double Linear(double x) => x;
    static double LinearD(double x) => 1.0;
}
