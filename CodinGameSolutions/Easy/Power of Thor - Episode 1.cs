using System;

class Player
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var lightX = int.Parse(inputs[0]);
        var lightY = int.Parse(inputs[1]);
        var initialTx = int.Parse(inputs[2]);
        var initialTy = int.Parse(inputs[3]);

        var solver = new Thor(initialTx, initialTy, lightX, lightY);

        while (true)
        {
            var remainingTurns = int.Parse(Console.ReadLine());
            var move = solver.ComputeNextMove();
            Console.WriteLine(move);
        }
    }
}

public class Thor
{
    private int _thorX;
    private int _thorY;
    private readonly int _lightX;
    private readonly int _lightY;

    public Thor(int initialTx, int initialTy, int lightX, int lightY)
    {
        _thorX = initialTx;
        _thorY = initialTy;
        _lightX = lightX;
        _lightY = lightY;
    }

    public string ComputeNextMove()
    {
        var directionY = "";
        if (_thorY > _lightY)
        {
            directionY = "N";
            _thorY--;
        }
        else if (_thorY < _lightY)
        {
            directionY = "S";
            _thorY++;
        }

        var directionX = "";
        if (_thorX > _lightX)
        {
            directionX = "W";
            _thorX--;
        }
        else if (_thorX < _lightX)
        {
            directionX = "E";
            _thorX++;
        }
        return directionY + directionX;
    }
}