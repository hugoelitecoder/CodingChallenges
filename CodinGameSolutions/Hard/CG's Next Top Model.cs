using System;
using System.Globalization;

class Solution
{
    public static void Main(string[] args)
    {
        var train = ReadDataLine(Console.ReadLine());
        var test = ReadDataLine(Console.ReadLine());
        var models = new IModel[]
        {
            new LinearModel(),
            new ParabolaModel(),
            new SineModel(),
            new ExponentialModel()
        };
        var bestModel = models[0];
        var bestError = bestModel.CalcError(test, bestModel.Fit(train));
        foreach (var model in models)
        {
            var parameters = model.Fit(train);
            var error = model.CalcError(test, parameters);
            if (error < bestError)
            {
                bestError = error;
                bestModel = model;
            }
        }
        Console.WriteLine(bestModel.Name);
    }

    static double[][] ReadDataLine(string line)
    {
        var arr = line.Split(' ');
        var n = arr.Length / 2;
        var data = new double[n][];
        for (var i = 0; i < n; ++i)
        {
            var x = double.Parse(arr[2 * i], CultureInfo.InvariantCulture);
            var y = double.Parse(arr[2 * i + 1], CultureInfo.InvariantCulture);
            data[i] = new[] { x, y };
        }
        return data;
    }
}

interface IModel
{
    string Name { get; }
    double[] Fit(double[][] data);
    double Predict(double x, double[] parameters);
    double CalcError(double[][] data, double[] parameters);
}

abstract class BaseModel : IModel
{
    public abstract string Name { get; }
    public abstract double Predict(double x, double[] parameters);

    public double CalcError(double[][] data, double[] parameters)
    {
        double sum = 0;
        foreach (var pt in data)
        {
            var err = pt[1] - Predict(pt[0], parameters);
            sum += err * err;
        }
        return sum;
    }

    public virtual double[] Fit(double[][] data)
    {
        var rng = new Random(42);
        var bestParams = InitialParams();
        var bestError = CalcError(data, bestParams);
        for (var iter = 10; iter < 10000; ++iter)
        {
            var p1 = RandomParams(rng);
            var err1 = CalcError(data, p1);
            if (err1 < bestError)
            {
                bestParams = p1;
                bestError = err1;
            }
            var p2 = MutateParams(bestParams, rng, iter);
            var err2 = CalcError(data, p2);
            if (err2 < bestError)
            {
                bestParams = p2;
                bestError = err2;
            }
        }
        return bestParams;
    }

    protected abstract double[] InitialParams();
    protected abstract double[] RandomParams(Random rng);
    protected abstract double[] MutateParams(double[] best, Random rng, int iter);
}

class LinearModel : BaseModel
{
    public override string Name => "LINEAR";
    public override double Predict(double x, double[] p) => p[0] * x + p[1];

    protected override double[] InitialParams() => new double[] { 0.1, 0.1 };

    protected override double[] RandomParams(Random rng)
    {
        return new double[] { rng.NextDouble() * 20 - 10, rng.NextDouble() * 20 - 10 };
    }

    protected override double[] MutateParams(double[] best, Random rng, int iter)
    {
        return new double[]
        {
            best[0] + (rng.NextDouble() * 20 - 10) * 10.0 / iter,
            best[1] + (rng.NextDouble() * 20 - 10) * 10.0 / iter
        };
    }
}

class ParabolaModel : BaseModel
{
    public override string Name => "PARABOLA";
    public override double Predict(double x, double[] p) => p[0] * x * x + p[1] * x + p[2];

    protected override double[] InitialParams() => new double[] { 0.1, 0.1, 0.1 };

    protected override double[] RandomParams(Random rng)
    {
        return new double[]
        {
            rng.NextDouble() * 20 - 10,
            rng.NextDouble() * 20 - 10,
            rng.NextDouble() * 20 - 10
        };
    }

    protected override double[] MutateParams(double[] best, Random rng, int iter)
    {
        return new double[]
        {
            best[0] + (rng.NextDouble() * 20 - 10) * 10.0 / iter,
            best[1] + (rng.NextDouble() * 20 - 10) * 10.0 / iter,
            best[2] + (rng.NextDouble() * 20 - 10) * 10.0 / iter
        };
    }
}

class SineModel : BaseModel
{
    public override string Name => "SINE";
    public override double Predict(double x, double[] p) => p[0] * Math.Sin(p[1] * x + p[2]) + p[3];

    protected override double[] InitialParams() => new double[] { 0.1, 0.1, 0.1, 0.1 };

    protected override double[] RandomParams(Random rng)
    {
        return new double[]
        {
            rng.NextDouble() * 20 - 10,
            rng.NextDouble() * 20 - 10,
            rng.NextDouble() * 20 - 10,
            rng.NextDouble() * 20 - 10
        };
    }

    protected override double[] MutateParams(double[] best, Random rng, int iter)
    {
        return new double[]
        {
            best[0] + (rng.NextDouble() * 20 - 10) * 10.0 / iter,
            best[1] + (rng.NextDouble() * 20 - 10) * 10.0 / iter,
            best[2] + (rng.NextDouble() * 20 - 10) * 10.0 / iter,
            best[3] + (rng.NextDouble() * 20 - 10) * 10.0 / iter
        };
    }
}

class ExponentialModel : BaseModel
{
    public override string Name => "EXPONENTIAL";
    public override double Predict(double x, double[] p)
    {
        return Math.Pow(Math.Abs(p[0] / 5.0), x + p[1]) + p[2];
    }

    protected override double[] InitialParams() => new double[] { 0.1, 0.1, 0.1 };

    protected override double[] RandomParams(Random rng)
    {
        return new double[]
        {
            rng.NextDouble() * 20 - 10,
            rng.NextDouble() * 20 - 10,
            rng.NextDouble() * 20 - 10
        };
    }

    protected override double[] MutateParams(double[] best, Random rng, int iter)
    {
        return new double[]
        {
            best[0] + (rng.NextDouble() * 20 - 10) * 10.0 / iter,
            best[1] + (rng.NextDouble() * 20 - 10) * 10.0 / iter,
            best[2] + (rng.NextDouble() * 20 - 10) * 10.0 / iter
        };
    }
}
