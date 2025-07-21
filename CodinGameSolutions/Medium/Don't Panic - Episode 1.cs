using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    class Floor {
        
        public Floor(int floornumber){
            this.FloorNumber = floornumber;
        }
        public int FloorNumber { get; set; }
        public int ElavatorPosition { get; set;  }
        public bool HasBlocker { get; set; }
    }
    
    
    static void Main(string[] args)
    {
        Dictionary<int,Floor> building = new Dictionary<int,Floor>();
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int nbFloors = int.Parse(inputs[0]); // number of floors
        for(int j=0; j < nbFloors; j++){ building[j] = new Floor(j); }
        int width = int.Parse(inputs[1]); // width of the area
        int nbRounds = int.Parse(inputs[2]); // maximum number of rounds
        int exitFloor = int.Parse(inputs[3]); // floor on which the exit is found
        int exitPos = int.Parse(inputs[4]); // position of the exit on its floor
        int nbTotalClones = int.Parse(inputs[5]); // number of generated clones
        int nbAdditionalElevators = int.Parse(inputs[6]); // ignore (always zero)
        int nbElevators = int.Parse(inputs[7]); // number of elevators
        for (int i = 0; i < nbElevators; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int elevatorFloor = int.Parse(inputs[0]); // floor on which this elevator is found
            int elevatorPos = int.Parse(inputs[1]); // position of the elevator on its floor
            building[elevatorFloor].ElavatorPosition = elevatorPos;
        }

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int cloneFloor = int.Parse(inputs[0]); // floor of the leading clone
            int clonePos = int.Parse(inputs[1]); // position of the leading clone on its floor
            string direction = inputs[2]; // direction of the leading clone: LEFT or RIGHT
            
            var command = "WAIT";
            if(cloneFloor >= 0)
            {
                var target = cloneFloor == exitFloor ? exitPos : building[cloneFloor].ElavatorPosition;
            
                if((clonePos > target && direction == "RIGHT")||(clonePos < target && direction == "LEFT"))
                {
                    command="BLOCK";
                } 
            }
            Console.WriteLine(command);
        }
    }
}