using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Module8
{
    public class PlayerData
    {
        public AttackResultType[,] StatusGrid { get; set; } // Used to store status values per grid point
        public int ShipsLeft { get; set; } // Counter from the value of ships left.
        public int Index { get; set; } // Store player index

        private List<Ship> _ships;

        public PlayerData(int gridSize, Ships ships, AttackResult result)
        {
            // Initialize variables with parameters 
            ShipsLeft = ships._ships.Count;
            Index = result.PlayerIndex;
            _ships = ships._ships;
            
            // Create probability/status grid
            StatusGrid = new AttackResultType[gridSize, gridSize];
            
            // Initialize values in status grid to 0
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                    StatusGrid[i, j] = 0;
            }

            // Log initial result
            StatusGrid[result.Position.X,result.Position.Y] = result.ResultType;

        }

        // In the event that a ship is sunk, mark related hit positions as sunk.
        public void SunkShip(ShipTypes shipType, Position position)
        {
            Debug.WriteLine("Called SunkShip()");
            
            int shipLength = 0; // Store lenght of sunk ship
            List<Position> possiblePositions = new List<Position>(); // List of the positions to be marked sunk
            bool valid = true; // Bool used as exit point for while loops
            
            // Switch case for figuring out lenght based on reported ship type

            switch (shipType)
            {
                case ShipTypes.AircraftCarrier:
                    shipLength = 5;
                    break;
                case ShipTypes.Battleship:
                    shipLength = 4;
                    break;
                case ShipTypes.Destroyer:
                case ShipTypes.Submarine:
                    shipLength = 3;
                    break;
                case ShipTypes.PatrolBoat:
                    shipLength = 2;
                    break;
            }

#if DEBUG

            DebugStatusGrid();

#endif
            
            Debug.WriteLine($"Length of ship sunk is {shipLength}");
            
            // Check for hit positions north of the sunk point and add them to possiblePositions
            // until a position isn't a hit or we have all the hit positions we need for the shipLength
            while (valid)
            {
                Debug.WriteLine($"PossiblePosition count before(north-while): {possiblePositions.Count}");
                
                for (int i = position.Y - 1; i >= 0; i--)
                {
                    Debug.WriteLine($"Checking position north of origin @ ({position.X},{i})");

                    if (StatusGrid[i, position.X] == AttackResultType.Hit)
                    {
                        Debug.WriteLine($"Adding hit position north of origin @ ({position.X},{i})");
                        possiblePositions.Add(new Position(position.X, i));
                    }

                    if (StatusGrid[i, position.X] != AttackResultType.Hit)
                    {
                        valid = false;
                        break;
                    }
                }
                
                if (possiblePositions.Count == shipLength - 1 || possiblePositions.Count == 0)
                {
                    valid = false;
                }
                
                Debug.WriteLine($"PossiblePosition count after(north-while): {possiblePositions.Count}");

            }

            valid = true; // Set back to true for next while

            // Check for hit positions south of the sunk point and add them to possiblePositions
            // until a position isn't a hit or we have all the hit positions we need for the shipLength
            while (valid && possiblePositions.Count < shipLength-1)
            {
                Debug.WriteLine($"PossiblePosition count before(south-while): {possiblePositions.Count}");
                
                for (int i = position.Y + 1; i < StatusGrid.GetLength(1); i++)
                {

                    Debug.WriteLine($"Checking position south of origin @ ({position.X},{i})");

                    if (StatusGrid[i, position.X] == AttackResultType.Hit)
                    {
                        Debug.WriteLine($"Adding hit position south of origin @ ({position.X},{i})");
                        possiblePositions.Add(new Position(position.X, i));
                    }
                    
                    if (StatusGrid[i, position.X] != AttackResultType.Hit)
                    {
                        valid = false;
                        break;
                    }
                    
                }
                
                if (possiblePositions.Count == shipLength - 1 || possiblePositions.Count == 0)
                {
                    valid = false;
                }
                Debug.WriteLine($"PossiblePosition count after(south-while): {possiblePositions.Count}");

            }

            valid = true; // Set back to true for next while

            
            if (possiblePositions.Count < shipLength-1)
            {
                possiblePositions.Clear();
                
                // Check for hit positions west of the sunk point and add them to possiblePositions
                // until a position isn't a hit or we have all the hit positions we need for the shipLength
                while (valid)
                {
                    
                    Debug.WriteLine($"PossiblePosition count before(left-while): {possiblePositions.Count}");
                    
                    for (int i = position.X - 1; i >= 0; i--)
                    {
                        Debug.WriteLine($"Checking position left of origin @ ({i},{position.Y})");

                        if (StatusGrid[position.Y, i] == AttackResultType.Hit)
                        {
                            Debug.WriteLine($"Adding hit position left of origin @ ({i},{position.Y})");
                            possiblePositions.Add(new Position(i, position.Y));
                        }
                        
                        if (StatusGrid[position.Y, i] != AttackResultType.Hit)
                        {
                            valid = false;
                            break;
                        }
                        
                    }
                    if (possiblePositions.Count == shipLength - 1 || possiblePositions.Count == 0)
                    {
                        valid = false;
                    }
                    Debug.WriteLine($"PossiblePosition count before(left-while): {possiblePositions.Count}");
                    
                }

                valid = true; // Set back to true for next while

                // Check for hit positions east of the sunk point and add them to possiblePositions
                // until a position isn't a hit or we have all the hit positions we need for the shipLength
                while (valid & possiblePositions.Count < shipLength-1)
                {
                    
                    Debug.WriteLine($"PossiblePosition count before(right-while): {possiblePositions.Count}");

                    for (int i = position.X + 1; i < StatusGrid.GetLength(0); i++)
                    {

                        Debug.WriteLine($"Checking position right of origin @ ({i},{position.Y})");

                        if (StatusGrid[position.Y, i] == AttackResultType.Hit)
                        {
                            Debug.WriteLine($"Adding hit position right of origin @ ({i},{position.Y})");
                            possiblePositions.Add(new Position(i, position.Y));
                        }
                        
                        if (StatusGrid[position.Y, i] != AttackResultType.Hit)
                        {
                            valid = false;
                            break;
                        }
                        
                    }
                    Debug.WriteLine($"PossiblePosition count after(right-while): {possiblePositions.Count}");
                }
            }
            
            // Change each position to Sunk
            foreach (var t in possiblePositions)
                StatusGrid[t.Y, t.X] = AttackResultType.Sank;
            
            // Change origin position to sunk
            StatusGrid[position.Y, position.X] = AttackResultType.Sank;

            ShipsLeft--;
        }

        public void DebugStatusGrid()
        {
            Debug.WriteLine($"Player {Index} Status Grid");
            Debug.WriteLine("________");
            
            for (int i = 0; i < StatusGrid.GetLength(0); i++)
            {
                for (int j = 0; j < StatusGrid.GetLength(1); j++)
                {
                    
                    if(StatusGrid[i,j] == AttackResultType.Hit)
                        Debug.Write("| H|");
                    if(StatusGrid[i,j] == AttackResultType.Miss)
                        Debug.Write("| M|");
                    if(StatusGrid[i,j] == AttackResultType.Sank)
                        Debug.Write("| S|");
                    if(StatusGrid[i,j] != AttackResultType.Hit && StatusGrid[i,j] != AttackResultType.Miss && StatusGrid[i,j] != AttackResultType.Sank)
                        Debug.Write("| U|");
                    
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("________");
        }

    }
}