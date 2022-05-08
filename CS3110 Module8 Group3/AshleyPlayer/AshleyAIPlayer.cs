using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace CS3110.Module8.Group3
{
    internal class AshleyAIPlayer : IPlayer
    {
        private List<Position> HitPositions = new List<Position>(); // stores all ‘hit’ guesses
        private List<Position> MissPositions = new List<Position>(); // stores all ‘miss’ guesses
        private List<Position> SankPositions = new List<Position>(); // stores all ‘sank’ guesses
        private int _index; // player's index in turn order
        private int _gridSize; // size of grid
        private Ships _ships; // size of grid
        private static readonly Random Random = new Random(); // used to randomize choices
        private char[] directions = { 'N', 'E', 'S', 'W' }; //represents north, east, south, west


        // Constructor:
        public AshleyAIPlayer(string name)
        {
            Name = name;
        }

        // Property that returns player's name:
        public string Name { get; }

        // Propery that reutn's player's indexin turn order.
        public int Index => _index;


        // Logic to start new game
        // **** TBD ****
        // TBD: Does this properly reset if more than 1 game is played during runtime?
        // (arrays need to be reset, etc.)
        // **** TBD ****
        public void StartNewGame(int playerIndex, int gridSize, Ships ships)
        {
            _gridSize = gridSize;
            _index = playerIndex;
           

    _ships = ships;
            foreach (var ship in ships._ships)
            {
                //while(ship != null)
                {


                    // Pick an open X from the remaining columns
                    var x = availableColumns[Random.Next(availableColumns.Count)];
                    availableColumns.Remove(x); //Make sure we can't pick it again

                    // Pick a Y that fits
                    var y = Random.Next(gridSize - ship.Length);
                    ship.Place(new Position(x, y), Direction.Vertical);
                }
            }
        }


        // Method to intelligently find best spot to atack.
        public Position GetAttackPosition()
        {
            Position guess = null;

            // - (1) Look at the spaces to the north, east, south, or west of each hit (reference the HitPositions array here).
            // - (2) If the it finds a spot on the grid that doesn’t contain the AI’s own ships, it will shoot at it.
            foreach (Position position in HitPositions)
            {
                foreach (char direction in directions)
                {
                    guess = GetAdjacent(position, direction);
                    if (guess != null)
                        break;
                }
                if (guess != null)
                    break;
            }

            // If guess is null by now, that means nothing has been found.
          
            if (guess == null)
                guess = new Position(0, 0); // ( This is a placeholder that just guesses 0, 0. )

            return guess;

        }

        // Method to find adjacent spot to a given position, if provided the direction.
        // Returns null if the spot is somehow invalid (off the grid or has already been shot at)
        internal Position GetAdjacent(Position p, char direction)
        {
            // initialize x & y
            int x = p.X;
            int y = p.Y;

            // shift in the desired adjacent direction
            if (direction == 'N')
                y++;
            else if (direction == 'E')
                x++;
            else if (direction == 'S')
                y--;
            else if (direction == 'W')
                x--;
            else
                return null;

            // save result
            Position result = new Position(x, y);

            // return result if valid
            if (IsValid(result))
                return result;

            // return null otherwise
            else
                return null;

        }

        

        // This method, given a position, checks if it is a valid spot at which to fire.
        // Valid spots do not contain the player's own ships, have not already been shot at, and
        // are on the grid.
        internal bool IsValid(Position p)
        {
            // Check to see if spot contains the AI's ship.
            //foreach (Ship s in _ships._ships)
            //{


               // foreach (Position ShipPosition in s.Positions)
                //{
                    //while (!Ships.ShipType.Battleship)

                   // if (ShipPosition.X == p.X && ShipPosition.Y == p.Y)

                   // {
                      //  return false;
                   // }
                //}
           // }
           // IEnumerable<ShipTypes> results;

            //foreach (Ship s in _ships._ships)
            //{
              //  results = from position in s.Positions
                          //where position.X == p.X && position.Y == p.Y
                          //select s.ShipType;
            //}


               // foreach (var r in results)
                    //if (r == ShipTypes.Battleship)
                       // return false;
            

            // Check to see if spot has already been shot at
            foreach (List<Position> LoggedPositions in new[] { HitPositions, MissPositions, SankPositions })
            {
                foreach (Position LoggedPosition in LoggedPositions)
                {
                    if (LoggedPosition.X == p.X && LoggedPosition.Y == p.Y)
                    {
                        return false;
                    }
                }
            }

            // Check to see if spot is on the grid
            if (p.X < 0 || p.X >= _gridSize || p.Y < 0 || p.Y >= _gridSize)
            {
                return false;
            }

            // If all the checks have passed, this spot is valid.
            return true;

        }

        // Method to log results throughout the game.
        // GreyPlayer will separately keep track of each guess that results in a hit or a miss.
        // It does not track misses, as those require no follow up.
        public void SetAttackResults(List<AttackResult> results)
        {
            foreach (AttackResult r in results)
            {
                if (r.ResultType == AttackResultType.Miss)
                {
                    if (MissPositions.Contains(r.Position) == false)
                        MissPositions.Add(r.Position);
                }
                else if (r.ResultType == AttackResultType.Hit)
                {
                    if (HitPositions.Contains(r.Position) == false)
                        HitPositions.Add(r.Position);
                }
                else if (r.ResultType == AttackResultType.Sank)
                {
                    if (SankPositions.Contains(r.Position) == false)
                        SankPositions.Add(r.Position);
                }
            }
        }

   //_ships = ships;

            //returns true if the ship will fit in the horizontal direction
       bool availablerow(int x, int y, int ships)
       {
            try
            {
                for (int i = 0; i < ships; i++)
                {
                    if (gridSize[x, y + i] != '.')
                    {
                        return false;
                    }
                }
                return true;
            }

            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }
        bool availableColumns(int x, int y, int ships)
        {
            try
            {
                for (int i = 0; i < ships; i++)

                {

                    if (gridSize[x + i, y] != '.')
                    {
                        return false;

                    }
                }

                return true;
            }

            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }


        public void populate(int shipCount)
        {
            Reset();
            Random rng = new Random(); //ship placement will be random.
            int row = 0; //variables for the row and column
            int col = 0;


            bool vertHor = false;
            int shipNum = 0;
            int shipSize = 1;
            char shipChar = 'B';


            do
            {
                //get a random location and direction for the ship
                row = rng.Next(0, size);
                col = rng.Next(0, size);
                //generate a random true or false.
                vertHor = (rng.Next(100) >= 50);

                switch (shipNum + 1)
                {
                    case 1: shipSize = 5; shipChar = 'C'; break;
                    case 2: shipSize = 2; shipChar = 'A'; break;
                    case 3: shipSize = 5; shipChar = 'T'; break;
                    case 4: shipSize = 2; shipChar = 'Z'; break;
                    case 5: shipSize = 3; shipChar = 'S'; break;
                    default: shipSize = 2; shipChar = 'B'; break;
                }

                //Check if you can add it to the location and not overlap
                if (gridSize[row, col] == '.')
                {

                    if (availablerow(row, col, shipSize) == true & availableColumns(row, col, shipSize) == true)
                    {


                        for (int i = 0; i < shipSize; i++)
                        {
                            if (vertHor)
                            {
                                gridSize[row, col + i] = shipChar;
                            }
                            else
                            {
                                _gridSize[row + i, col] = shipChar;
                            }
                        }
                        shipNum++;
                    }
                }
            } while (shipNum < shipCount);
        }
    }
}
    



