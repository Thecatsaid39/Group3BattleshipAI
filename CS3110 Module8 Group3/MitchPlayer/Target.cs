using System.Collections.Generic;

namespace Module8
{
    public class Target
    {
        public int PlayerIndex { get; set; }
        public Position GridPosition { get; set; }
        public List<Position> NorthAttackPositions { get; set; }
        public List<Position> EastAttackPositions { get; set; }
        public List<Position> SouthAttackPositions { get; set; }
        public List<Position> WestAttackPositions { get; set; }

        // Constructor that grabs the player index, the position reported as a hit, and the current state of the status grid.
        public Target(int index, Position position, StatusType[,] posStat)
        {

            PlayerIndex = index;
            GridPosition = position;
            NorthAttackPositions = new List<Position>();
            EastAttackPositions = new List<Position>();
            SouthAttackPositions = new List<Position>();
            WestAttackPositions = new List<Position>();

            // Generates a list of unknown positions for each direction direction based on the current state of the status grid.
            for (int i = position.Y - 1; i >= 0; i--)
            {

                if (posStat[position.X, i] == 0)
                    NorthAttackPositions.Add(new Position(position.X, i));
            }

            for (int i = position.Y + 1; i < posStat.GetLength(1); i++)
            {

                if (posStat[position.X, i] == 0)
                    SouthAttackPositions.Add(new Position(position.X, i));
            }

            for (int i = position.X - 1; i >= 0; i--)
            {

                if (posStat[i, position.Y] == 0)
                    WestAttackPositions.Add(new Position(i, position.Y));
            }

            for (int i = position.X + 1; i < posStat.GetLength(0); i++)
            {

                if (posStat[i, position.Y] == 0)
                    EastAttackPositions.Add(new Position(i, position.Y));
            }



        }
    }
}