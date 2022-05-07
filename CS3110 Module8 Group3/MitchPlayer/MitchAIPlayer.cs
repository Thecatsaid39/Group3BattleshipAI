using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace Module8
{
    public class MitchAIPlayer : IPlayer
    {

        //Instance Variables
        public string Name { get; set; }
        public int Index { get; set; }
        private static readonly Random Random = new Random(); // Randomize choices
        private int GridSize { get; set; }

        private List<PlayerData> _playersData;
        private int[,] ProbabilityGrid { get; set; } // Used to store probability values per grid point
        // private StatusType[,] StatusGrid { get; set; } // Used to store status values per grid point

        private int _turnCounter = 1;
        
        private Position _lastAttack; // Stores last position fired on

        private Stack<Target> _targetStack = new Stack<Target>();
        private Target _currentTarget;
        private CurrentTargetDirectionTypes _currentTargetDirection = CurrentTargetDirectionTypes.NotSet;
        private bool _eliminateMode = false;
        private Ships _ships;
        private int _lowestShipCount = 0;
        private bool _allowSelfDestruct = true;

        public MitchAIPlayer(string name)
        {
            Debug.WriteLine($"New Group3Player Created with name {name}");
            Name = name;
        }

        // StartNewGame is called when a game starts and the player's index,
        // the grid size, and the ships that will be placed have been set for the game.
        public void StartNewGame(int playerIndex, int gridSize, Ships ships)
        {
            Debug.WriteLine("Called StartNewGame()");
            // Set instance variables with parameters
            GridSize = gridSize;
            Index = playerIndex;
            _lowestShipCount = ships._ships.Count;
            
            
            Debug.WriteLine("Player index set to {0}, and the grid size is {1}x{2}",Index,GridSize,GridSize);
            
            // Use PlaceShipsRandomly to place ships
            //PlaceShipsRandomly(ships);
            
            // Use Ashley's method to place ships as mine was throwing exception
            AshleyPlaceShips(ships);

        }

        
        // GetAttackPosition will look to see if there is a current target(_eliminationMode is true)
        // If so, it will continue attacking the unknown spaces around the origina target point.
        // If not, it will look to the the _targetStack to grab the next target in the list.
        // If no target exists it will use MostProbablePosition to make a guess based on which spot has
        // the best probability of having a ship placed in it.
        public Position GetAttackPosition()
        {
            Debug.WriteLine("Called GetAttackPosition()");
            
            if (_eliminateMode)
                return AttackCurrentTarget();
            if (_targetStack.Count > 0)
            {
                _currentTarget = _targetStack.Pop();
                _eliminateMode = true;
                
                return AttackCurrentTarget();
            }
            
            // If it is my turn first, return position of (0,0) to start game
            if (_turnCounter == 1)
            {
                return new Position(0, 0);
            }

            // Find the most probable point for the most vulnerable player
            return MostProbablePosition();
        }
        
        // AttackCurrentTarget will rotate counterclockwise through the unknown spaces surrounding the origin
        // target position starting at above it. If all points have been checked before a sunk notification is
        // received(in the event a player was eliminated that was the original target), then it will set
        // _eliminationMode to false and revert back to MostProbablePosition()
        private Position AttackCurrentTarget()
        {
            Debug.WriteLine("Called AttackCurrentTarget()");
            
            // Remove excess capacity
            _currentTarget.NorthAttackPositions.TrimExcess();
            _currentTarget.EastAttackPositions.TrimExcess();
            _currentTarget.SouthAttackPositions.TrimExcess();
            _currentTarget.WestAttackPositions.TrimExcess();
            
            Debug.WriteLine("Continuing attack on target originating at ({0},{1})", _currentTarget.GridPosition.X, _currentTarget.GridPosition.Y);
            
            while(_currentTarget.NorthAttackPositions.Count > 0)
            {
                
                _currentTargetDirection = CurrentTargetDirectionTypes.North;
                _lastAttack = _currentTarget.NorthAttackPositions.First();
                Debug.WriteLine("Firing north of origin point at ({0},{1})",_lastAttack.X,_lastAttack.Y);
                _currentTarget.NorthAttackPositions.RemoveAt(0);
                if (_playersData[_currentTarget.PlayerIndex].StatusGrid[_lastAttack.X, _lastAttack.Y] == 0 & NotMyShip(_lastAttack))
                    return _lastAttack;
            }

            while(_currentTarget.EastAttackPositions.Count > 0)
            {
                _currentTargetDirection = CurrentTargetDirectionTypes.East;
                _lastAttack = _currentTarget.EastAttackPositions.First();
                Debug.WriteLine("Firing east of origin point at ({0},{1})",_lastAttack.X,_lastAttack.Y);
                _currentTarget.EastAttackPositions.RemoveAt(0);
                if (_playersData[_currentTarget.PlayerIndex].StatusGrid[_lastAttack.X, _lastAttack.Y] == 0 & NotMyShip(_lastAttack))
                    return _lastAttack;
            }

            while(_currentTarget.SouthAttackPositions.Count > 0)
            {
                _currentTargetDirection = CurrentTargetDirectionTypes.South;
                _lastAttack = _currentTarget.SouthAttackPositions.First();
                Debug.WriteLine("Firing south of origin point at ({0},{1})",_lastAttack.X,_lastAttack.Y);
                _currentTarget.SouthAttackPositions.RemoveAt(0);
                if (_playersData[_currentTarget.PlayerIndex].StatusGrid[_lastAttack.X, _lastAttack.Y] == 0 & NotMyShip(_lastAttack))
                    return _lastAttack;
            }
            
            while (_currentTarget.WestAttackPositions.Count > 0)
            {
                _currentTargetDirection = CurrentTargetDirectionTypes.West;
                _lastAttack = _currentTarget.WestAttackPositions.First();
                Debug.WriteLine("Firing west of origin point at ({0},{1})",_lastAttack.X,_lastAttack.Y);
                _currentTarget.WestAttackPositions.RemoveAt(0);
                if (_playersData[_currentTarget.PlayerIndex].StatusGrid[_lastAttack.X, _lastAttack.Y] == 0 & NotMyShip(_lastAttack))
                    return _lastAttack;
            }

            _eliminateMode = false;
            Debug.WriteLine("No other shots can be taken on this target originating at ({0},{1}), turning off elimination mode.",_currentTarget.GridPosition.X,_currentTarget.GridPosition.Y);

            // Find the most probable point for the most vulnerable player
            return MostProbablePosition();
        }

        // SetAttackResults 
        public void SetAttackResults(List<AttackResult> results)
        {
            Debug.WriteLine("Called SetAttackResults()");

            // On turn one when the attack results are received, create a list of PlayerData objects 
            // to store a status and probability grid
            if (_turnCounter == 1)
            {
                // Initialize _playersData to player count
                _playersData = new List<PlayerData>(results.Count);
                
                foreach (var r in results)
                {
                    _playersData.Insert(r.PlayerIndex,new PlayerData(GridSize,_ships, r));
                }
            }
            
            if (_eliminateMode)
            {
                foreach (AttackResult r in results)
                {
                    if (r.Position == _lastAttack && r.PlayerIndex == _currentTarget.PlayerIndex)
                    {
                        if (r.ResultType == AttackResultType.Sank)
                        {
                            if(_playersData[r.PlayerIndex].StatusGrid[r.Position.Y,r.Position.X] != AttackResultType.Sank)
                                _playersData[r.PlayerIndex].SunkShip(r.SunkShip,r.Position);
                            
                            Debug.WriteLine("Current target originating at ({0},{1}) has been sunk, turning off elimination mode.",_currentTarget.GridPosition.X, _currentTarget.GridPosition.Y);
                            
                            _eliminateMode = false;
                        }
                        
                        if (r.ResultType == AttackResultType.Hit)
                        {
                            _playersData[r.PlayerIndex].StatusGrid[r.Position.Y, r.Position.X] = AttackResultType.Hit;
                            Debug.WriteLine("Current target originating at ({0},{1}) has been hit at position ({2},{3}), continue attack {4} of the origin point.",_currentTarget.GridPosition.X, _currentTarget.GridPosition.Y, r.Position.X,r.Position.Y, _currentTargetDirection);

                        }
                        
                        if (r.ResultType == AttackResultType.Miss)
                        {
                            _playersData[r.PlayerIndex].StatusGrid[r.Position.Y, r.Position.X] = AttackResultType.Miss;
                            if (_currentTargetDirection == CurrentTargetDirectionTypes.North)
                            {
                                Debug.WriteLine("Shot fired at position ({0},{1}) missed, clearing remaining positions North of origin point ({2},{3}) and moving to points East of the origin.",r.Position.X,r.Position.Y,_currentTarget.GridPosition.X,_currentTarget.GridPosition.Y);
                                _currentTarget.NorthAttackPositions.Clear();
                            }

                            if (_currentTargetDirection == CurrentTargetDirectionTypes.East)
                            {
                                Debug.WriteLine("Shot fired at position ({0},{1}) missed, clearing remaining positions East of origin point ({2},{3}) and moving to points South of the origin.",r.Position.X,r.Position.Y,_currentTarget.GridPosition.X,_currentTarget.GridPosition.Y);
                                _currentTarget.EastAttackPositions.Clear();
                            }

                            if (_currentTargetDirection == CurrentTargetDirectionTypes.South)
                            {
                                Debug.WriteLine("Shot fired at position ({0},{1}) missed, clearing remaining positions South of origin point ({2},{3}) and moving to points West of the origin.",r.Position.X,r.Position.Y,_currentTarget.GridPosition.X,_currentTarget.GridPosition.Y);
                                _currentTarget.SouthAttackPositions.Clear();
                            }

                            if (_currentTargetDirection == CurrentTargetDirectionTypes.West)
                            {
                                Debug.WriteLine("Shot fired at position ({0},{1}) missed, clearing remaining positions West of origin point ({2},{3}) and turning off elimination mode.",r.Position.X,r.Position.Y,_currentTarget.GridPosition.X,_currentTarget.GridPosition.Y);
                                _currentTarget.WestAttackPositions.Clear();
                                _eliminateMode = false;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (AttackResult r in results)
                {
                    if (r.ResultType == AttackResultType.Sank)
                    {
                        Debug.WriteLine("Player {0} reported a sunk ship at ({1},{2}), marking the status grid with a hit.",r.PlayerIndex,r.Position.X,r.Position.Y);
                        
                        if(_playersData[r.PlayerIndex].StatusGrid[r.Position.Y,r.Position.X] != AttackResultType.Sank)
                            _playersData[r.PlayerIndex].SunkShip(r.SunkShip,r.Position);
                    }

                    if (r.ResultType == AttackResultType.Hit)
                    {
                        if (r.PlayerIndex != Index)
                        {
                            Debug.WriteLine("Player {0} reported a hit at ({1},{2}), adding a new target to the target stack.",r.PlayerIndex,r.Position.X,r.Position.Y);
                            // Check if position is already marked Hit in the status grid to ensure duplicate targets aren't created.
                            // Happens when dumb player shoots on a position that has already been fired upon.
                            if(_playersData[r.PlayerIndex].StatusGrid[r.Position.Y,r.Position.X] != AttackResultType.Hit)
                                _targetStack.Push(new Target(r.PlayerIndex, r.Position, _playersData[r.PlayerIndex].StatusGrid));
                        }
                        _playersData[r.PlayerIndex].StatusGrid[r.Position.X, r.Position.Y] = AttackResultType.Hit;
                    }

                    if (r.ResultType == AttackResultType.Miss)
                    {
                        _playersData[r.PlayerIndex].StatusGrid[r.Position.Y, r.Position.X] = AttackResultType.Miss;
                    }

                }
            }

            Debug.WriteLine($"Turn: {_turnCounter}");
            _turnCounter++;
            

        }

        private void AshleyPlaceShips(Ships ships)
        {
            
            var availableColumns = new List<int>();
            for (int i = 0; i < GridSize; i++)
            {
                availableColumns.Add(i);
            }

            _ships = ships;
            foreach (var ship in ships._ships)
            {
                // Pick an open X from the remaining columns
                var x = availableColumns[Random.Next(availableColumns.Count)];
                availableColumns.Remove(x); //Make sure we can't pick it again

                // Pick a Y that fits
                var y = Random.Next(GridSize - ship.Length);
                ship.Place(new Position(x, y), Direction.Vertical);
            }
             
        }
        
        private void PlaceShipsRandomly(Ships ships)
        {   
            
            Debug.WriteLine("Called PlaceShipsRandomly()");
            
            List<Ship> shipList = ships._ships;
            
            int totalLength = shipList.Sum(ship => ship.Length);
            
            Debug.WriteLine("Total Length of ships to place: " + totalLength);
            Debug.WriteLine("Total number of ships to place: " + shipList.Count);
            
            bool[,] occupied = new bool[GridSize, GridSize];
                
            // Initialize oll positions of occupied with false
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                    occupied[i, j] = false;
            }

            while (shipList.Count > 0)
            {

                bool fits = false;

                var ship = shipList[Random.Next(shipList.Count - 1)];
                shipList.Remove(ship);
                Debug.WriteLine("{0} was selected for placement",ship.ShipType);

                while (!fits)
                {
                    
                    var direction = (Direction) Random.Next(0, 2);
                    Debug.WriteLine("{0} was selected for placement direction",direction);
                    
                    bool valid = true;
                    var x = Random.Next(GridSize - 1);
                    var y = Random.Next(GridSize - 1);
                    
                    Debug.WriteLine($"Proposed position is ({x},{y})");
                    
                    if (direction == Direction.Horizontal)
                    {
                        
                        // CHeck only for spaces the size of the ship
                        for (int i = x; i < GridSize; i++)
                        {
                            if (occupied[i, y])
                            {
                                valid = false;
                            }
                        }

                        if (valid)
                        {

                            ship.Place(new Position(x, y), direction);
                            Debug.WriteLine("{0} selected for placement {1} starting at position ({2},{3}) and was successful",ship.ShipType,direction,x,y);
                            
                            // Occupy only for spaces the size of the ship
                            for (int i = x; i < GridSize; i++)
                            {
                                occupied[i, y] = true;
                            }

                            fits = true;
                        }
                        else
                            Debug.WriteLine("{0} selected for placement {1} starting at position ({2},{3}) and failed because it was an occupied space", ship.ShipType, direction, x, y);

                    }

                    if (direction == Direction.Vertical)
                    {
                        // CHeck only for spaces the size of the ship
                        for (int i = x; i < GridSize; i++)
                        {
                            if (occupied[x, i])
                            {
                                valid = false;
                            }
                        }

                        if (valid)
                        {

                            ship.Place(new Position(x, y), direction);
                            Debug.WriteLine("{0} selected for placement {1} starting at position ({2},{3}) and was successful",ship.ShipType,direction,x,y);
                            // Occupy only for spaces the size of the ship
                            for (int i = x; i < GridSize; i++)
                            {
                                occupied[x, i] = true;
                            }

                            fits = true;
                        }
                        else
                            Debug.WriteLine("{0} selected for placement {1} starting at position ({2},{3}) and failed because it was an occupied space", ship.ShipType, direction, x, y);
                    }
                }
            }

        } // End of PlaceShipsRandomly

        
        //    MostProbablePosition(): 
        //    Start at (0,0) and check if the largest ship can fit vertically against
        //    current list of misses/sunk ships. If it fits, then add a point to all
        //    positions the ship fits in in ProbabilityGrid. Iterate through each position Then start over
        //    at (0,0) and check horizontally if the ship fits and if it does add a point
        //    to all positions the ship fits in in ProbabilityGrid.
        //
        //    DO not apply points to positions that hold our ship until
        //
        //    Do this process again for the next size ship that the player should have left.
        //
        //    Return position from the list of positions with the highest score on the ProbabilityGrid.

        private Position MostProbablePosition()
        {
            Debug.WriteLine("Called MostProbablePosition");
            
            // Stores player index
            int playerIndex = -1;
            
            // Figure out most vulnerable player based on number of ships left
            foreach (var player in _playersData)
            {
                if (player.ShipsLeft < _lowestShipCount && player.Index != Index)
                    playerIndex = player.Index;
                Debug.WriteLine($"Most vulnerable player identified as {playerIndex}");
            }
            
            ProbabilityGrid = new int[GridSize,GridSize];
            
            Position mostProbable = new Position(0, 0); // Initialize to origin at the beginning of each execute
            int largestScore = 0;
            
            
            #if DEBUG
            Debug.WriteLine($"Player {playerIndex} Status Grid");
            Debug.WriteLine("________");
            
            for (int i = 0; i < _playersData[playerIndex].StatusGrid.GetLength(0); i++)
            {
                for (int j = 0; j < _playersData[playerIndex].StatusGrid.GetLength(1); j++)
                {
                    
                    if(_playersData[playerIndex].StatusGrid[i,j] == AttackResultType.Hit)
                        Debug.Write("| H|");
                    if(_playersData[playerIndex].StatusGrid[i,j] == AttackResultType.Miss)
                        Debug.Write("| M|");
                    if(_playersData[playerIndex].StatusGrid[i,j] == AttackResultType.Sank)
                        Debug.Write("| S|");
                    if(_playersData[playerIndex].StatusGrid[i,j] == 0)
                        Debug.Write("| U|");
                    
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("________");

            #endif
            
                // Assign horizontal probability points
                for (int i = 0; i < _playersData[playerIndex].StatusGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < _playersData[playerIndex].StatusGrid.GetLength(1); j++)
                    {
                        for (int s = 5; s > 1; s--)
                        {
                            bool fits = true;
                            for (int p = 0; p < s; p++)
                            {
                                if (i + p >= GridSize)
                                {
                                    fits = false;
                                    break;
                                }

                                if (_playersData[playerIndex].StatusGrid[i + p, j] != 0 && _playersData[playerIndex].StatusGrid[i + p, j] != AttackResultType.Hit)
                                    fits = false;
                            }

                            if (fits)
                            {
                                for (int p = 0; p < s; p++)
                                {
                                    ProbabilityGrid[i + p, j] += 1;
                                }

                            }
                        }
                    }
                } // End of horizontal probability point assignment

                // Assign vertical probability points
                for (int i = 0; i < _playersData[playerIndex].StatusGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < _playersData[playerIndex].StatusGrid.GetLength(1); j++)
                    {
                        for (int s = 5; s > 1; s--)
                        {
                            bool fits = true;
                            for (int p = 0; p < s; p++)
                            {
                                if (j + p >= GridSize)
                                {
                                    fits = false;
                                    break;
                                }
                                if (_playersData[playerIndex].StatusGrid[i, j + p] != 0 && _playersData[playerIndex].StatusGrid[i, j + p] != AttackResultType.Hit)
                                    fits = false;
                            }

                            if (fits)
                            {
                                for (int p = 0; p < s; p++)
                                {
                                    ProbabilityGrid[i, j + p] += 1;
                                }
                            }

                        }
                    }
                } // End of vertical probability point assignment

                // Find position with highest score 
                for (int i = 0; i < ProbabilityGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < ProbabilityGrid.GetLength(1); j++)
                    {
                        if (NotMyShip(new Position(j,i)))
                        {
                        
                            if (ProbabilityGrid[i, j] > largestScore)
                            {
                                largestScore = ProbabilityGrid[i, j];
                                mostProbable = new Position(j, i);
                            }
                        }
                    }
                }
            
#if DEBUG
            Debug.WriteLine($"Player {playerIndex} Probability Grid");
            Debug.WriteLine("________");
                for (int i = 0; i < ProbabilityGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < ProbabilityGrid.GetLength(1); j++)
                    {
                        Debug.Write("|" + ProbabilityGrid[i,j].ToString("00") + "|");
                    }
                    Debug.WriteLine("");
                }
                Debug.WriteLine("________");
#endif
                Debug.WriteLine($"Most probable position for player {playerIndex} was ({mostProbable.X},{mostProbable.Y})");
                return mostProbable;
        }

        private bool NotMyShip(Position p)
        {
            // Check to see if spot contains the AI's ship.
            if (!_allowSelfDestruct)
            {
                foreach (Ship s in _ships._ships)
                {
                    foreach (Position shipPosition in s.Positions)
                    {
                        if (shipPosition.X == p.X && shipPosition.Y == p.Y)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}