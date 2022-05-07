using System.Collections.Generic;

namespace Module8
{
    public class NewTarget
    {
        public int PlayerIndex { get; set; }
        public Position GridPosition { get; set; }


        // Constructor that grabs the player index, the position reported as a hit, and the current state of the status grid.
        public NewTarget(int index, Position position)
        {

            PlayerIndex = index;
            GridPosition = position;
        }
    }
}