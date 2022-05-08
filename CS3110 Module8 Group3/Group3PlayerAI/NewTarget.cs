using System.Collections.Generic;

namespace Module8
{
    public class NewTarget
    {

        public Position GridPosition { get; set; }
        private CardinalDirection _currentDirection = CardinalDirection.North;
        private int _nY,_sY,_eX,_wX = 0;
        private bool _validPosition = false;


        // Constructor that grabs the player index, the position reported as a hit, and the current state of the status grid.
        public NewTarget(Position position)
        {
            GridPosition = position;
        }

        // Will take the current attack direction and see if the next position is valid,
        // if not it will change to the next direction and repeat the process until a valid
        // point is found.
        public Position GetNextPosition(StatusType[,] statGrid)
        {
            _validPosition = false;
            
            Position result = null;

            do
            {
                if (_currentDirection == CardinalDirection.North)
                {
                    _nY--;
                    result = new Position(GridPosition.X, GridPosition.Y + _nY);
                }

                if (_currentDirection == CardinalDirection.East)
                {
                    _eX++;
                    result = new Position(GridPosition.X + _eX, GridPosition.Y);
                }

                if (_currentDirection == CardinalDirection.South)
                {
                    _sY++;
                    result = new Position(GridPosition.X, GridPosition.Y + _sY);
                }

                if (_currentDirection == CardinalDirection.West)
                {
                    _wX--;
                    result = new Position(GridPosition.X + _wX, GridPosition.Y);
                }

                if (statGrid[result.Y, result.X] != StatusType.Unknown)
                {
                    switch (_currentDirection)
                    {
                        case CardinalDirection.North:
                            _currentDirection = CardinalDirection.East;
                            break;
                        case CardinalDirection.East:
                            _currentDirection = CardinalDirection.South;
                            break;
                        case CardinalDirection.South:
                            _currentDirection = CardinalDirection.West;
                            break;
                        case CardinalDirection.West:
                            _currentDirection = CardinalDirection.North;
                            break;
                    }
                }

                if (statGrid[result.Y, result.X] == StatusType.Unknown)
                    _validPosition = true;


            } while (!_validPosition);

            return result;
        }
    }
}